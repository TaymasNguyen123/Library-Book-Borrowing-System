using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Data;
using Microsoft.EntityFrameworkCore;

namespace Library_Book_Borrowing_System.Repositories;

public class BorrowRecordRepository(Database database) : IBorrowRecordRepository
{
    public BorrowRecord Borrow(BorrowRecord borrowRecord)
    {
        database.BorrowRecords.Add(borrowRecord);
        database.SaveChanges();
        return borrowRecord;
    }

    public BorrowRecord Return(BorrowRecord borrowRecord)
    {
        database.BorrowRecords.Add(borrowRecord);
        database.SaveChanges();
        return borrowRecord;
    }

    public IEnumerable<BorrowRecord> GetAll()
    {
        return database.BorrowRecords.AsNoTracking().ToList();
    }

    public IEnumerable<BorrowRecord>? GetByMemberId(Guid id)
    {
        return database.Members.AsNoTracking().FirstOrDefault(member => member.Id == id).BorrowRecords;
    }

    public int CountByBorrowed(Guid bookId)
    {
        return database.Books.AsNoTracking().FirstOrDefault(book => book.Id == bookId).BorrowedCount;
    }
    
    public Task<bool> ExistsAsync(Guid recordId)
    {
        return database.BorrowRecords.AnyAsync(borrowRecord => borrowRecord.Id == recordId);
    }
}