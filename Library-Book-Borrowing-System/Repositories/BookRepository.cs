using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Data;
using Microsoft.EntityFrameworkCore;

namespace Library_Book_Borrowing_System.Repositories;

public class BookRepository(Database database) : IBookRepository
{
    public Book Add(Book book)
    {
        database.Books.Add(book);
        database.SaveChanges();
        return book;
    }

    public IEnumerable<Book> GetAll()
    {
        return database.Books.AsNoTracking().ToList();
    }

    public Book? GetById(Guid id)
    {
        return database.Books.AsNoTracking().FirstOrDefault(book => book.Id == id);
    }

    public Book? GetByTitle(string title)
    {
        return database.Books.AsNoTracking().FirstOrDefault(book => book.Title == title);
    }
    public Book? Update(Guid id, Book book)
    {
        Book? findBook = GetById(id);
        if (findBook is not null)
        {
            findBook.Title = book.Title;
            findBook.Author = book.Author;
            findBook.Isbn = book.Isbn;
            findBook.TotalCopies = book.TotalCopies;
            findBook.AvailableCopies = book.AvailableCopies;
            findBook.BorrowedCount = book.BorrowedCount;

            database.Entry(findBook).State = EntityState.Modified;
            database.SaveChanges();
        }

        return findBook;
    }

    public void Delete(Guid id)
    {
        database.Books.Where(book => book.Id == id).ExecuteDeleteAsync();
        database.SaveChanges();
    }

    public Task<bool> ExistsAsync(Guid bookId)
    {
        return database.Books.AnyAsync(book => book.Id == bookId);
    }
}