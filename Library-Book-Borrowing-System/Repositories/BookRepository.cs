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
    public Book Update(
        Book oldBook,
        Guid? updateId = null,
        string? updateTitle = null,
        string? updateAuthor = null,
        string? updateIsbn = null,
        int? updateTotalCopies = null,
        int? updateAvailableCopies = null
    )
    {
        Book? findBook = database.Books.AsNoTracking().FirstOrDefault(book => book.Id == oldBook.Id);

        findBook.Id = updateId == null ? oldBook.Id : (Guid)updateId;
        findBook.Title = updateTitle == null ? oldBook.Title : updateTitle;
        findBook.Author = updateAuthor == null ? oldBook.Author : updateAuthor;
        findBook.Isbn = updateIsbn == null ? oldBook.Isbn : updateIsbn;
        findBook.TotalCopies = updateTotalCopies == null ? oldBook.TotalCopies : (int)updateTotalCopies;
        findBook.AvailableCopies = updateAvailableCopies == null ? oldBook.AvailableCopies : (int)updateAvailableCopies;

        database.SaveChanges();

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