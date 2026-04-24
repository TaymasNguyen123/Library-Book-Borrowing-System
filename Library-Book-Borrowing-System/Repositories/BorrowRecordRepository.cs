using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Data;
using Microsoft.EntityFrameworkCore;

namespace Library_Book_Borrowing_System.Repositories;

public class BorrowRecordRepository(Database database) : IBorrowRecordRepository
{
    public BorrowRecord? Borrow(BorrowRecord borrowRecord)
    {
        int rowsAffected = database.Database.ExecuteSqlRaw(@"
            UPDATE Books
            SET AvailableCopies = AvailableCopies - 1
            WHERE Id = {0} AND AvailableCopies > 0
        ", borrowRecord.BookId);

        if (rowsAffected == 0) return null;

        database.BorrowRecords.Add(borrowRecord);
        database.SaveChanges();
        return borrowRecord;
    }

    public BorrowRecord Return(BorrowRecord borrowRecord)
    {
        IEnumerable<BorrowRecord>? _records = GetByMemberId(borrowRecord.MemberId)
            ?.Where(record =>
                record.BookId == borrowRecord.BookId &&
                record.Status == "Borrowed"
            );
        if (_records is not null && _records.Count() > 0)
        {
            BorrowRecord _record = _records.ElementAt(0);

            _record.ReturnDate = borrowRecord.ReturnDate;
            _record.Status = "Returned";
            
            database.SaveChanges();
        }
        return borrowRecord;
    }

    public IEnumerable<BorrowRecord> GetAll()
    {
        return database.BorrowRecords.AsNoTracking().ToList();
    }

    public IEnumerable<BorrowRecord>? GetByMemberId(Guid id)
    {
        return database.BorrowRecords.AsNoTracking().Where(record => record.MemberId == id).ToList();
    }

    public int? CountByBorrowed(Guid bookId)
    {
        return database.Books.AsNoTracking().FirstOrDefault(book => book.Id == bookId)?.BorrowedCount;
    }
    
    public Task<bool> ExistsAsync(Guid recordId)
    {
        return database.BorrowRecords.AnyAsync(borrowRecord => borrowRecord.Id == recordId);
    }
}