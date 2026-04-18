using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;

namespace Library_Book_Borrowing_System.Services;

public interface IBorrowRecordService
{
    GetBorrowRecordResponse BorrowBook(CreateBorrowRecordRequest borrowRecord);
    GetBorrowRecordResponse ReturnBook(UpdateBorrowRecordRequest borrowRecord);
    IEnumerable<GetBorrowRecordResponse> GetAllRecords();
    IEnumerable<GetBorrowRecordResponse>? GetAllRecordsByMember(Guid memberId);
}