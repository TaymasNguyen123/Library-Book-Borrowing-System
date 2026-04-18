using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Repositories;
using System.Collections.Immutable;

namespace Library_Book_Borrowing_System.Services;

public class BorrowRecordService: IBorrowRecordService
{
    private readonly IBorrowRecordRepository _borrowRecordRepository;
    private readonly IBookRepository _bookRepository;

    public BorrowRecordService(IBorrowRecordRepository borrowRecordRepository, IBookRepository bookRepository)
    {
        _borrowRecordRepository = borrowRecordRepository;
        _bookRepository = bookRepository;
    }
    public GetBorrowRecordResponse BorrowBook(CreateBorrowRecordRequest borrowRecord)
    {
        Guid bookId = borrowRecord.BookId;
        Guid memberId = borrowRecord.MemberId;

        Book? _book = _bookRepository.GetById(bookId);

        if (_book is null)
        {
            throw new Exception("Book does not exist");
        }

        if (_book.AvailableCopies <= 0)
        {
            throw new Exception("Book has no copies available");
        }

        BorrowRecord _borrowRecord = new BorrowRecord
        {
            Id = new Guid(),
            BookId = bookId,
            MemberId = memberId,
            BorrowDate = new DateTime(),
            ReturnDate = null,
            Status = "Borrowed"
        };
        _borrowRecordRepository.Borrow(_borrowRecord);

        _bookRepository.Update(
            _book,
            bookId,
            _book.Title,
            _book.Author,
            _book.Isbn,
            _book.TotalCopies,
            --_book.AvailableCopies
        );

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
            throw new Exception("Book does not exist");
        }

        // Get all borrowing records that match the book and are currently borrowed
        IEnumerable<BorrowRecord>? _records = _borrowRecordRepository.GetByMemberId(memberId)
            ?.Where(
                record => record.BookId == bookId && 
                record.MemberId == memberId &&
                record.Status == "Borrowed"
            );

        var _record = (_records?.ElementAt(0)) ?? throw new Exception("Member has not borrowed this book");
        BorrowRecord _newRecord = new BorrowRecord
        {
            Id = _record.Id,
            BookId = bookId,
            MemberId = memberId,
            BorrowDate = _record.BorrowDate,
            ReturnDate = new DateTime(),
            Status = "Returned"
        };
        _borrowRecordRepository.Return(_newRecord);

        _bookRepository.Update(
            _book,
            bookId,
            _book.Title,
            _book.Author,
            _book.Isbn,
            _book.TotalCopies,
            ++_book.AvailableCopies
        );

        return new GetBorrowRecordResponse
        {
            Id = _newRecord.Id,
            BookId = bookId,
            MemberId = memberId,
            BorrowDate = _newRecord.BorrowDate,
            ReturnDate = _newRecord.ReturnDate,
            Status = _newRecord.Status
        };
    }
    public IEnumerable<GetBorrowRecordResponse> GetAllRecords()
    {
        return _borrowRecordRepository.GetAll()
            .Select(record => new GetBorrowRecordResponse
            {
                Id = record.Id,
                BookId = record.BookId,
                MemberId = record.MemberId,
                BorrowDate = record.BorrowDate,
                ReturnDate = record.ReturnDate,
                Status = record.Status
            });
    }

    public IEnumerable<GetBorrowRecordResponse>? GetAllRecordsByMember(Guid memberId)
    {
        return _borrowRecordRepository.GetByMemberId(memberId)
            ?.Select(record => new GetBorrowRecordResponse
            {
                Id = record.Id,
                BookId = record.BookId,
                MemberId = record.MemberId,
                BorrowDate = record.BorrowDate,
                ReturnDate = record.ReturnDate,
                Status = record.Status
            });
    }
}