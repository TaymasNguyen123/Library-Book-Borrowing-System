using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Repositories;
using Library_Book_Borrowing_System.GlobalException;
using Microsoft.Extensions.Caching.Memory;
using System.Data.Common;
using System.Security.Cryptography.X509Certificates;
using System.Globalization;

namespace Library_Book_Borrowing_System.Services;

public class BookService: IBookService
{ 
    private readonly IBookRepository _bookRepository;
    private readonly IBorrowRecordRepository _borrowRecordRepository;
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
        SlidingExpiration = TimeSpan.FromSeconds(30)
    };

    public BookService(IBookRepository bookRepository, IBorrowRecordRepository borrowRecordRepository, IMemoryCache cache)
    {
        _bookRepository = bookRepository;
        _borrowRecordRepository = borrowRecordRepository;
        _cache = cache;
    }

    public GetBookResponse CreateBook(CreateBookRequest book)
    {
        if (book.AvailableCopies > book.TotalCopies)
        {
            throw new HttpRequestException(GlobalExceptionHandler.MORE_AVAILABLE_THAN_TOTAL, null, System.Net.HttpStatusCode.NotFound);
        }

        if (!Helper.IsValidIsbn(book.Isbn))
        {
            throw new HttpRequestException(GlobalExceptionHandler.INVALID_ISBN, null, System.Net.HttpStatusCode.NotFound);
        }

        if (GetAllBooks().Any(x => x.Isbn == book.Isbn))
        {
            throw new HttpRequestException(GlobalExceptionHandler.DUPLICATE_ISBN, null, System.Net.HttpStatusCode.BadRequest);
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
        _cache.Remove("book:list");

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
        if (_cache.TryGetValue("book:list", out IEnumerable<GetBookResponse>? list)) {
            return list;
        }

        IEnumerable<GetBookResponse> bookList = _bookRepository.GetAll()
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

        _cache.Set("book:list", bookList, _cacheOptions);
        
        return bookList;
    }
    public async Task<GetBookDetailsResponse> GetBookById(Guid id)
    {
        if (_cache.TryGetValue($"book:{id}", out GetBookDetailsResponse? value))
        {
            return value;
        }
        
        Book bk = _bookRepository.GetById(id)
                ?? throw new HttpRequestException(GlobalExceptionHandler.MISSING_BOOK_ID, null, System.Net.HttpStatusCode.NotFound);

        GetBookDetailsResponse response = new GetBookDetailsResponse
        {
            Id = bk.Id,
            Title = bk.Title,
            Author = bk.Author,
            Isbn = bk.Isbn,
            TotalCopies = bk.TotalCopies,
            AvailableCopies = bk.AvailableCopies,
            BorrowedCount = bk.BorrowedCount
        };

        _cache.Set($"book:{response.Id}", response, _cacheOptions);

        return response;
    }
    public GetBookResponse UpdateBook(Guid id, UpdateBookRequest book)
    {
        Book? bookUpdating = _bookRepository.GetById(id)
            ?? throw new HttpRequestException(GlobalExceptionHandler.MISSING_BOOK_ID, null, System.Net.HttpStatusCode.NotFound);
        
        if (book.AvailableCopies > book.TotalCopies)
        {
            throw new HttpRequestException(GlobalExceptionHandler.MORE_AVAILABLE_THAN_TOTAL, null, System.Net.HttpStatusCode.NotFound);
        }

        if (book.Isbn is not null && !Helper.IsValidIsbn(book.Isbn))
        {
            throw new HttpRequestException(GlobalExceptionHandler.INVALID_ISBN, null, System.Net.HttpStatusCode.NotFound);
        }

        
        if (GetAllBooks().Any(x => x.Isbn == book.Isbn && x.Id != bookUpdating.Id))
        {
            throw new HttpRequestException(GlobalExceptionHandler.DUPLICATE_ISBN, null, System.Net.HttpStatusCode.BadRequest);   
        }

        bookUpdating.Title = book.Title ?? bookUpdating.Title;
        bookUpdating.Author = book.Author ?? bookUpdating.Author;
        bookUpdating.Isbn = book.Isbn ?? bookUpdating.Isbn;
        bookUpdating.TotalCopies = book.TotalCopies ?? bookUpdating.TotalCopies;
        bookUpdating.AvailableCopies = book.AvailableCopies ?? bookUpdating.AvailableCopies;
        bookUpdating.BorrowedCount = book.BorrowedCount ?? bookUpdating.BorrowedCount;

        if (bookUpdating.AvailableCopies > bookUpdating.TotalCopies)
        {
            throw new HttpRequestException(GlobalExceptionHandler.MORE_AVAILABLE_THAN_TOTAL, null, System.Net.HttpStatusCode.BadRequest);
        }

        Book? updated = _bookRepository.Update(id, bookUpdating);
        if (updated is null)
        {
            throw new HttpRequestException(GlobalExceptionHandler.MISSING_BOOK_ID, null, System.Net.HttpStatusCode.NotFound);
        }

        GetBookResponse response = new GetBookResponse
        {
            Id = id,
            Title = updated.Title,
            Author = updated.Author,
            Isbn = updated.Isbn,
            TotalCopies = updated.TotalCopies,
            AvailableCopies = updated.AvailableCopies,
            BorrowedCount = updated.BorrowedCount
        };
        _cache.Remove($"book:{response.Id}");
        _cache.Remove("book:list");
        
        return response;
    }
    public void DeleteBook(Guid id)
    {
        var isBeingBorrowed = _borrowRecordRepository.GetAll()
            .Any(r => r.BookId == id && r.Status == "Borrowed");
        
        if(isBeingBorrowed)
        {
            throw new HttpRequestException(GlobalExceptionHandler.BOOK_IS_BORROWED, null, System.Net.HttpStatusCode.Conflict);
        }
        _cache.Remove($"book:{id}");
        _cache.Remove("book:list");

        _bookRepository.Delete(id);
    }
    public async Task<IEnumerable<GetBookDetailsResponse>> SearchBooksAsync(string? title, string? author, string? isbn)
    {
        if (string.IsNullOrWhiteSpace(title) && 
            string.IsNullOrWhiteSpace(author) && 
            string.IsNullOrWhiteSpace(isbn))
        {
            throw new HttpRequestException(
                GlobalExceptionHandler.NOT_FOUND, 
                null, 
                System.Net.HttpStatusCode.BadRequest
            );
            
        }
        
        var books = _bookRepository.GetAll(); 
        var query = books.AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(b => b.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(author))
            query = query.Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(isbn))
            query = query.Where(b => b.Isbn == isbn);

        var results = query.Select(bk => new GetBookDetailsResponse
        {
            Id = bk.Id,
            Title = bk.Title,
            Author = bk.Author,
            Isbn = bk.Isbn,
            TotalCopies = bk.TotalCopies,
            AvailableCopies = bk.AvailableCopies,
            BorrowedCount = bk.BorrowedCount
        }).ToList();

        if (!results.Any())
        {
            string searchDetails = string.Join(", ", new[] {
                !string.IsNullOrEmpty(title) ? $"Title: {title}" : null,
                !string.IsNullOrEmpty(author) ? $"Author: {author}" : null,
                !string.IsNullOrEmpty(isbn) ? $"ISBN: {isbn}" : null
            }.Where(s => s != null));

            throw new HttpRequestException(
                $"No books found matching the search criteria: {searchDetails}", 
                null, 
                System.Net.HttpStatusCode.BadRequest
            );
        }

        return results;
    }
}