using Library_Book_Borrowing_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Library_Book_Borrowing_System.Data;

public class Database : DbContext
{
    public Database(DbContextOptions<Database> options) : base(options){}

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<BorrowRecord> BorrowRecords => Set<BorrowRecord>();
}