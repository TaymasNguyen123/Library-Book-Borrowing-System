using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Repositories;
using System.Collections.Immutable;
using Library_Book_Borrowing_System.GlobalException;
using Microsoft.Extensions.Caching.Memory;

namespace Library_Book_Borrowing_System.Services;

public class BorrowRecordService: IBorrowRecordService
{
    private readonly IBorrowRecordRepository _borrowRecordRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        SlidingExpiration = TimeSpan.FromMinutes(2)    
    };

    public BorrowRecordService(
        IBorrowRecordRepository borrowRecordRepository, 
        IBookRepository bookRepository,
        IMemoryCache cache
    )
    {
        _borrowRecordRepository = borrowRecordRepository;
        _bookRepository = bookRepository;
        _cache = cache;
    }
    public GetBorrowRecordResponse BorrowBook(CreateBorrowRecordRequest borrowRecord)
    {
        Guid bookId = borrowRecord.BookId;
        Guid memberId = borrowRecord.MemberId;

        Book? _book = _bookRepository.GetById(bookId);
        if (_book is null)
        {
            throw new HttpRequestException(GlobalExceptionHandler.MISSING_BOOK_ID, null, System.Net.HttpStatusCode.NotFound);
        }

        if (_book.AvailableCopies <= 0)
        {
            throw new HttpRequestException(GlobalExceptionHandler.MORE_AVAILABLE_THAN_TOTAL, null, System.Net.HttpStatusCode.Conflict);
        }


        // Get all records of requested book to check if user is already borrowing
        IEnumerable<BorrowRecord>? _records = _borrowRecordRepository.GetByMemberId(memberId)
            ?.Where(record => 
                record.BookId == bookId && 
                record.MemberId == memberId &&
                record.Status == "Borrowed"
            );

        if (_records is not null && _records.Count() != 0)
        {
            throw new HttpRequestException(GlobalExceptionHandler.DUPLICATE_RECORD, null, System.Net.HttpStatusCode.Conflict);
        }

        BorrowRecord _borrowRecord = new BorrowRecord
        {
            Id = new Guid(),
            BookId = bookId,
            MemberId = memberId,
            BorrowDate = DateTime.Now,
            ReturnDate = null,
            Status = "Borrowed"
        };
        _borrowRecordRepository.Borrow(_borrowRecord);

        _book.AvailableCopies--;
        _bookRepository.Update(bookId, _book);

        return new GetBorrowRecordResponse
        {
            Id = _borrowRecord.Id,
            BookId = bookId,
            MemberId = memberId,
            BorrowDate = _borrowRecord.BorrowDate,
            ReturnDate = null,
            Status = _borrowRecord.Status
        };
    }
    public GetBorrowRecordResponse ReturnBook(UpdateBorrowRecordRequest borrowRecord)
    {
        Guid bookId = borrowRecord.BookId;
        Guid memberId = borrowRecord.MemberId;

        Book? _book = _bookRepository.GetById(bookId);
        if (_book is null)
        {
            throw new HttpRequestException(GlobalExceptionHandler.MISSING_BOOK_ID, null, System.Net.HttpStatusCode.NotFound);
        }

        // Get all borrowing records that match the book and are currently borrowed
        IEnumerable<BorrowRecord>? _records = _borrowRecordRepository.GetByMemberId(memberId)
            ?.Where(
                record => record.BookId == bookId && 
                record.MemberId == memberId &&
                record.Status == "Borrowed"
            );
        if (_records is null || _records.Count() > 0)
        {
            throw new HttpRequestException(GlobalExceptionHandler.MISSING_BORROW_RECORD, null, System.Net.HttpStatusCode.Conflict);
        }
        var _record = _records.ElementAt(0);
        
        _record.ReturnDate = DateTime.Now;
        _record.Status = "Returned";

        BorrowRecord returned = _borrowRecordRepository.Return(_record);

        _book.AvailableCopies++;
        _bookRepository.Update(bookId, _book);

        return new GetBorrowRecordResponse
        {
            Id = returned.Id,
            BookId = bookId,
            MemberId = memberId,
            BorrowDate = returned.BorrowDate,
            ReturnDate = returned.ReturnDate,
            Status = returned.Status
        };
    }
    public IEnumerable<GetBorrowRecordResponse> GetAllRecords()
    {
        if (_cache.TryGetValue("record:list", out IEnumerable<GetBorrowRecordResponse>? records))
        {
            return records;
        }

        IEnumerable<GetBorrowRecordResponse>? responses = _borrowRecordRepository.GetAll()
            .Select(record => new GetBorrowRecordResponse
            {
                Id = record.Id,
                BookId = record.BookId,
                MemberId = record.MemberId,
                BorrowDate = record.BorrowDate,
                ReturnDate = record.ReturnDate,
                Status = record.Status
            });
        
        _cache.Set("record:list", responses, _cacheOptions);
        
        return responses;
    }

    public IEnumerable<GetBorrowRecordResponse>? GetAllRecordsByMember(Guid memberId)
    {
        if (_cache.TryGetValue($"record:{memberId}", out IEnumerable<GetBorrowRecordResponse>? records))
        {
            return records;
        }

        IEnumerable<GetBorrowRecordResponse>? responses = _borrowRecordRepository.GetByMemberId(memberId)
            ?.Select(record => new GetBorrowRecordResponse
            {
                Id = record.Id,
                BookId = record.BookId,
                MemberId = record.MemberId,
                BorrowDate = record.BorrowDate,
                ReturnDate = record.ReturnDate,
                Status = record.Status
            });

        _cache.Set($"record:{memberId}", responses, _cacheOptions);

        return responses;
    }
}