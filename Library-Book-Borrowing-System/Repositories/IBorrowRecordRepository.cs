using Library_Book_Borrowing_System.Models;

namespace Library_Book_Borrowing_System.Repositories;

public interface IBorrowRecordRepository
{
    BorrowRecord BorrowBook(BorrowRecord borrowRecord);
    BorrowRecord ReturnBook(BorrowRecord borrowRecord);
    IEnumerable<BorrowRecord> GetAllBorrowRecords();
    IEnumerable<BorrowRecord> GetMemberBorrowRecord(Guid id);
}