using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Repositories;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore.Storage;

namespace Library_Book_Borrowing_System.Services;

public class BookService: IBookService
{ 
    private readonly IBookRepository _bookRepository;
    private readonly IBorrowRecordRepository _borrowRecordRepository;

    public BookService(IBookRepository bookRepository, IBorrowRecordRepository borrowRecordRepository)
    {
        _bookRepository = bookRepository;
        _borrowRecordRepository = borrowRecordRepository;
    }

    public GetBookResponse CreateBook(CreateBookRequest book)
    {
        if (book.AvailableCopies > book.TotalCopies)
        {
            throw new Exception("Total copies must be greater than or equal to available copies");
        }
        if (_bookRepository.GetByTitle(book.Title) is not null)
        {
            throw new Exception("Book with that title already exists");
        }

        var bk = new Book
        {
            Id = new Guid(),
            Title = book.Title,
            Author = book.Author,
            Isbn = book.Isbn,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies,
            BorrowedCount = 0
        };

        var created = _bookRepository.Add(bk);
        return new GetBookResponse
        {
            Id = created.Id,
            Title = created.Title,
            Author = created.Author,
            Isbn = created.Isbn,
            TotalCopies = created.TotalCopies,
            AvailableCopies = created.AvailableCopies
        };
    }
    public IEnumerable<GetBookResponse> GetAllBooks()
    {
        return _bookRepository.GetAll()
            .Select(bk => new GetBookResponse
            {
                Id = bk.Id,
                Title = bk.Title,
                Author = bk.Author,
                Isbn = bk.Isbn,
                TotalCopies = bk.TotalCopies,
                AvailableCopies = bk.AvailableCopies,
                BorrowedCount = bk.BorrowedCount
            });
    }
    public async Task<GetBookDetailsResponse> GetBookById(Guid id)
    {
        var bk = _bookRepository.GetById(id);
        if (bk is null)
        {
            throw new Exception("Book does not exist");
        }
        var totalBorrowedCount = _borrowRecordRepository.CountByBorrowed(id);
        var remainingAvailable = bk.TotalCopies - bk.AvailableCopies;

        return new GetBookDetailsResponse
        {
            Id = bk.Id,
            Title = bk.Title,
            Author = bk.Author,
            Isbn = bk.Isbn,
            TotalCopies = bk.TotalCopies,
            AvailableCopies = bk.AvailableCopies,
            TotalBorrowedCount = (int) totalBorrowedCount,
            RemainingAvailable = remainingAvailable
        };
    }
    public GetBookResponse UpdateBook(Guid id, UpdateBookRequest book)
    {
        Book? bookUpdating = _bookRepository.GetById(id);
        if (bookUpdating is null)
        {
            throw new Exception("Book does not exist");
        }

        bookUpdating.Title = book.Title ?? bookUpdating.Title;
        bookUpdating.Author = book.Author ?? bookUpdating.Author;
        bookUpdating.Isbn = book.Isbn ?? bookUpdating.Isbn;
        bookUpdating.TotalCopies = book.TotalCopies ?? bookUpdating.TotalCopies;
        bookUpdating.AvailableCopies = book.AvailableCopies ?? bookUpdating.AvailableCopies;

        if (bookUpdating.AvailableCopies > bookUpdating.TotalCopies)
        {
            throw new Exception("Total copies must be greater than or equal to available copies");
        }

        var updated = _bookRepository.Update(id, bookUpdating);

        return new GetBookResponse
        {
            Id = id,
            Title = updated.Title,
            Author = updated.Author,
            Isbn = updated.Isbn,
            TotalCopies = updated.TotalCopies,
            AvailableCopies = updated.AvailableCopies
        };
    }
    public void DeleteBook(Guid id)
    {
        _bookRepository.Delete(id);
    }
}