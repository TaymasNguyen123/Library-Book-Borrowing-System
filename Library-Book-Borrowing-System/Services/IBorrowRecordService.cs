using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;

namespace Library_Book_Borrowing_System.Services;
public interface IBorrowRecordService
{
    GetBorrowRecordResponse BorrowBook(CreateBorrowRecordRequest borrowRecord);
    GetBorrowRecordResponse ReturnBook(CreateBorrowRecordRequest borrowRecord);
    IEnumerable<GetBorrowRecordResponse> GetAllBorrowRecords();
    IEnumerable<GetBorrowRecordResponse> GetMemberBorrowHistory(Guid memberId);
    int CountBookBorrow(Guid bookId);
}