using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.GlobalException;
using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace Library_Book_Borrowing_System.Services;

public class BorrowRecordService : IBorrowRecordService
{
    private const int MaxActiveBorrowedBooks = 3;

    private readonly IBorrowRecordRepository _borrowRecordRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BorrowRecordService> _logger;
    private readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
        SlidingExpiration = TimeSpan.FromSeconds(30)
    };

    public BorrowRecordService(
        IBorrowRecordRepository borrowRecordRepository,
        IBookRepository bookRepository,
        IMemberRepository memberRepository,
        IMemoryCache cache,
        ILogger<BorrowRecordService> logger)
    {
        _borrowRecordRepository = borrowRecordRepository;
        _bookRepository = bookRepository;
        _memberRepository = memberRepository;
        _cache = cache;
        _logger = logger;
    }

    public GetBorrowRecordResponse BorrowBook(CreateBorrowRecordRequest borrowRecord)
    {
        Guid bookId = borrowRecord.BookId;
        Guid memberId = borrowRecord.MemberId;

        Book? book = _bookRepository.GetById(bookId);
        Member? member = _memberRepository.GetById(memberId);

        if (book is null)
        {
            _logger.LogWarning("Invalid book id provided: {BookId}", bookId);
            throw new HttpRequestException(GlobalExceptionHandler.MISSING_BOOK_ID, null, System.Net.HttpStatusCode.NotFound);
        }

        if (member is null)
        {
            _logger.LogWarning("Invalid member id provided: {MemberId}", memberId);
            throw new HttpRequestException(GlobalExceptionHandler.MISSING_MEMBER_ID, null, System.Net.HttpStatusCode.NotFound);
        }

        if (book.AvailableCopies <= 0)
        {
            _logger.LogWarning(
                "Cannot borrow book {BookId} for member {MemberId}: zero available copies",
                bookId,
                memberId);
            throw new HttpRequestException(GlobalExceptionHandler.ZERO_COPIES_AVAILABLE, null, System.Net.HttpStatusCode.Conflict);
        }

        List<BorrowRecord> activeBorrowRecords = _borrowRecordRepository.GetByMemberId(memberId)
            ?.Where(record => record.Status == "Borrowed")
            .ToList()
            ?? new List<BorrowRecord>();

        if (activeBorrowRecords.Any(record => record.BookId == bookId))
        {
            _logger.LogWarning(
                "Cannot borrow book {BookId} for member {MemberId}: duplicate borrowing attempted",
                bookId,
                memberId);
            throw new HttpRequestException(GlobalExceptionHandler.DUPLICATE_RECORD, null, System.Net.HttpStatusCode.Conflict);
        }

        if (activeBorrowRecords.Count >= MaxActiveBorrowedBooks)
        {
            _logger.LogWarning(
                "Cannot borrow book for member {MemberId}: active borrow limit reached at {ActiveBorrowCount}",
                memberId,
                activeBorrowRecords.Count);
            throw new HttpRequestException(
                GlobalExceptionHandler.MEMBER_BORROW_LIMIT_EXCEEDED,
                null,
                System.Net.HttpStatusCode.Conflict);
        }

        BorrowRecord newBorrowRecord = new BorrowRecord
        {
            Id = new Guid(),
            BookId = bookId,
            MemberId = memberId,
            BorrowDate = DateTime.Now.ToString("MM/dd/yyyy"),
            ReturnDate = null,
            Status = "Borrowed"
        };

        BorrowRecord? record = _borrowRecordRepository.Borrow(newBorrowRecord);
        if (record is null)
        {
            _logger.LogWarning(
                "Cannot borrow book {BookId} for member {MemberId}: zero available copies",
                bookId,
                memberId);
            throw new HttpRequestException(GlobalExceptionHandler.ZERO_COPIES_AVAILABLE, null, System.Net.HttpStatusCode.Conflict);
        }

        _cache.Remove("record:list");

        book.AvailableCopies--;
        book.BorrowedCount++;
        _bookRepository.Update(bookId, book);
        _cache.Remove($"book:{bookId}");
        _cache.Remove("book:list");

        member.BorrowRecords ??= new List<BorrowRecord>();
        member.BorrowRecords.Add(new BorrowRecord
        {
            Id = newBorrowRecord.Id,
            BookId = bookId,
            MemberId = memberId,
            BorrowDate = newBorrowRecord.BorrowDate,
            ReturnDate = null,
            Status = newBorrowRecord.Status
        });

        _memberRepository.Update(memberId, new Member
        {
            Id = member.Id,
            FullName = member.FullName,
            Email = member.Email,
            MembershipDate = member.MembershipDate,
            BorrowRecords = member.BorrowRecords
        });

        _cache.Remove("member:list");
        _cache.Remove($"member:{memberId}");
        _cache.Remove($"record:{memberId}");

        GetBorrowRecordResponse response = new GetBorrowRecordResponse
        {
            Id = newBorrowRecord.Id,
            BookId = bookId,
            MemberId = memberId,
            BorrowDate = newBorrowRecord.BorrowDate,
            ReturnDate = null,
            Status = newBorrowRecord.Status
        };

        _logger.LogInformation(
            "Book borrowed successfully. BorrowRecordId: {BorrowRecordId}, BookId: {BookId}, MemberId: {MemberId}",
            response.Id,
            response.BookId,
            response.MemberId);

        return response;
    }

    public GetBorrowRecordResponse ReturnBook(UpdateBorrowRecordRequest borrowRecord)
    {
        Guid bookId = borrowRecord.BookId;
        Guid memberId = borrowRecord.MemberId;

        Book? book = _bookRepository.GetById(bookId);
        Member? member = _memberRepository.GetById(memberId);

        if (book is null)
        {
            _logger.LogWarning("Invalid book id provided: {BookId}", bookId);
            throw new HttpRequestException(GlobalExceptionHandler.MISSING_BOOK_ID, null, System.Net.HttpStatusCode.NotFound);
        }

        if (member is null)
        {
            _logger.LogWarning("Invalid member id provided: {MemberId}", memberId);
            throw new HttpRequestException(GlobalExceptionHandler.MISSING_MEMBER_ID, null, System.Net.HttpStatusCode.NotFound);
        }

        List<BorrowRecord> records = _borrowRecordRepository.GetByMemberId(memberId)
            ?.Where(r => r.BookId == bookId && r.Status == "Borrowed")
            .ToList()
            ?? new List<BorrowRecord>();

        if (!records.Any())
        {
            throw new HttpRequestException(GlobalExceptionHandler.MISSING_BORROW_RECORD, null, System.Net.HttpStatusCode.Conflict);
        }

        BorrowRecord record = records.First();
        record.ReturnDate = DateTime.Now.ToString("MM/dd/yyyy");
        record.Status = "Returned";

        BorrowRecord returned = _borrowRecordRepository.Return(record);

        if (book.AvailableCopies < book.TotalCopies)
        {
            book.AvailableCopies++;
            _bookRepository.Update(bookId, book);
        }

        _cache.Remove($"book:{bookId}");
        _cache.Remove("book:list");

        BorrowRecord? recordInMember = member.BorrowRecords?.FirstOrDefault(r => r.Id == record.Id);
        if (recordInMember is not null)
        {
            recordInMember.Status = "Returned";
            recordInMember.ReturnDate = record.ReturnDate;
        }

        _memberRepository.Update(memberId, member);
        _cache.Remove("record:list");
        _cache.Remove($"record:{memberId}");
        _cache.Remove("member:list");
        _cache.Remove($"member:{memberId}");

        GetBorrowRecordResponse response = new GetBorrowRecordResponse
        {
            Id = returned.Id,
            BookId = bookId,
            MemberId = memberId,
            BorrowDate = returned.BorrowDate,
            ReturnDate = returned.ReturnDate,
            Status = returned.Status
        };

        _logger.LogInformation(
            "Book returned successfully. BorrowRecordId: {BorrowRecordId}, BookId: {BookId}, MemberId: {MemberId}",
            response.Id,
            response.BookId,
            response.MemberId);

        return response;
    }

    public IEnumerable<GetBorrowRecordResponse> GetAllRecords()
    {
        if (_cache.TryGetValue("record:list", out IEnumerable<GetBorrowRecordResponse>? list) && list is not null)
        {
            return list;
        }

        List<GetBorrowRecordResponse> responses = _borrowRecordRepository.GetAll()
            .Select(record => new GetBorrowRecordResponse
            {
                Id = record.Id,
                BookId = record.BookId,
                MemberId = record.MemberId,
                BorrowDate = record.BorrowDate,
                ReturnDate = record.ReturnDate,
                Status = record.Status
            })
            .ToList();

        _cache.Set("record:list", responses, _cacheOptions);

        return responses;
    }

    public PaginatedResponse<GetBorrowRecordResponse> GetAllRecords(int pageNumber, int pageSize)
    {
        return Helper.ToPaginatedResponse(GetAllRecords(), pageNumber, pageSize);
    }

    public IEnumerable<GetBorrowRecordResponse>? GetAllRecordsByMember(Guid memberId)
    {
        Member? member = _memberRepository.GetById(memberId);
        if (member is null)
        {
            _logger.LogWarning("Invalid member id provided: {MemberId}", memberId);
            throw new HttpRequestException(GlobalExceptionHandler.MISSING_MEMBER_ID, null, System.Net.HttpStatusCode.NotFound);
        }

        if (_cache.TryGetValue($"record:{memberId}", out IEnumerable<GetBorrowRecordResponse>? records) && records is not null)
        {
            return records;
        }

        List<GetBorrowRecordResponse> responses = _borrowRecordRepository.GetByMemberId(memberId)
            ?.Select(record => new GetBorrowRecordResponse
            {
                Id = record.Id,
                BookId = record.BookId,
                MemberId = record.MemberId,
                BorrowDate = record.BorrowDate,
                ReturnDate = record.ReturnDate,
                Status = record.Status
            })
            .ToList()
            ?? new List<GetBorrowRecordResponse>();

        _cache.Set($"record:{memberId}", responses, _cacheOptions);

        return responses;
    }

    public PaginatedResponse<GetBorrowRecordResponse> GetAllRecordsByMember(Guid memberId, int pageNumber, int pageSize)
    {
        return Helper.ToPaginatedResponse(
            GetAllRecordsByMember(memberId) ?? Enumerable.Empty<GetBorrowRecordResponse>(),
            pageNumber,
            pageSize);
    }
}
