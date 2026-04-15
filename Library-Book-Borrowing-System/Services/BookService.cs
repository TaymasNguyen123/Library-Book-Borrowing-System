using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Repositories;
using Microsoft.AspNetCore.Components.Web;

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
            AvailableCopies = book.AvailableCopies
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
                AvailableCopies = bk.AvailableCopies
            });
    }
    public async Task<GetBookDetailsResponse> GetBookById(Guid id)
    {
        var bk = _bookRepository.GetById(id);
        if (bk is null)
        {
            throw new Exception("Book does not exist");
        }
        var totalBorrowedCount = await _borrowRecordRepository.CountByBorrowed(id);
        var remainingAvailable = bk.TotalCopies - bk.AvailableCopies;

        return new GetBookDetailsResponse
        {
            Id = bk.Id,
            Title = bk.Title,
            Author = bk.Author,
            Isbn = bk.Isbn,
            TotalCopies = bk.TotalCopies,
            AvailableCopies = bk.AvailableCopies,
            TotalBorrowedCount = totalBorrowedCount,
            RemainingAvailable = remainingAvailable
        };
    }
    public GetBookResponse UpdateBook(UpdateBookRequest book)
    {
        if (book.AvailableCopies > book.TotalCopies)
        {
            throw new Exception("Total copies must be greater than or equal to available copies");
        }

        var updated = _bookRepository.Update(
            book.Id,
            book.Title,
            book.Author,
            book.Isbn,
            book.TotalCopies,
            book.AvailableCopies
        );

        return new GetBookResponse
        {
            Id = updated.Id,
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