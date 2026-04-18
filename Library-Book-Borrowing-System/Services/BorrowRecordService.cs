using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Repositories;
using Microsoft.AspNetCore.Components.Web;

namespace Library_Book_Borrowing_System.Services;

public class BorrowRecordService(IBorrowRecordRepository _borrowRecordRepository): IBorrowRecordService
{
    public GetBorrowRecordResponse BorrowBook(CreateBorrowRecordRequest borrowRecord)
    {
        GetBorrowRecordResponse BorrowRecord_ = new GetBorrowRecordResponse
        {
            Id = new Guid(),
            BookId = borrowRecord.BookId,
            MemberId = borrowRecord.MemberId,
            BorrowDate = borrowRecord.BorrowDate,
            ReturnDate = borrowRecord.ReturnDate,
            Status = borrowRecord.Status
        };
        return BorrowRecord_;
    }
    public GetBorrowRecordResponse ReturnBook(CreateBorrowRecordRequest borrowRecord)
    {
        GetBorrowRecordResponse BorrowRecord_ = new GetBorrowRecordResponse
        {
            Id = new Guid(),
            BookId = borrowRecord.BookId,
            MemberId = borrowRecord.MemberId,
            BorrowDate = borrowRecord.BorrowDate,
            ReturnDate = borrowRecord.ReturnDate,
            Status = borrowRecord.Status
        };
        return BorrowRecord_;
    }
    public IEnumerable<GetBorrowRecordResponse> GetAllBorrowRecords()
    {
        return _borrowRecordRepository.GetAll()
            .Select(borrowRecord => new GetBorrowRecordResponse
            {
                Id = borrowRecord.Id,
                BookId = borrowRecord.BookId,
                MemberId = borrowRecord.MemberId,
                BorrowDate = borrowRecord.BorrowDate,
                ReturnDate = borrowRecord.ReturnDate,
                Status = borrowRecord.Status
            });
    }
    public IEnumerable<GetBorrowRecordResponse> GetMemberBorrowHistory(Guid memberId)
    {
        return _borrowRecordRepository.GetByMemberId(memberId)
            .Select(borrowRecord => new GetBorrowRecordResponse
            {
                Id = borrowRecord.Id,
                BookId = borrowRecord.BookId,
                MemberId = borrowRecord.MemberId,
                BorrowDate = borrowRecord.BorrowDate,
                ReturnDate = borrowRecord.ReturnDate,
                Status = borrowRecord.Status
            });
    }
    public int CountBookBorrow(Guid bookId)
    {
        return _borrowRecordRepository.CountByBorrowed(bookId);
    }
}