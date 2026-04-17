using Library_Book_Borrowing_System.Models;

namespace Library_Book_Borrowing_System.Repositories;

public interface IBorrowRecordRepository
{
    BorrowRecord Borrow(BorrowRecord borrowRecord);
    BorrowRecord Return(BorrowRecord borrowRecord);
    IEnumerable<BorrowRecord> GetAll();
    IEnumerable<BorrowRecord>? GetByMemberId(Guid id);
    int CountByBorrowed(Guid bookId);
    Task<bool> ExistsAsync(Guid recordId);
}