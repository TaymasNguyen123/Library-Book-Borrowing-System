using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Library_Book_Borrowing_System.Data;
public class DatabaseFactory: IDesignTimeDbContextFactory<Database>
{
    public Database CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<Database>();
        optionsBuilder.UseSqlite("Data Source=librarymanagement.db");
        return new Database(optionsBuilder.Options);
    }
}