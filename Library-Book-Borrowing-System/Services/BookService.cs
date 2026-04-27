using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.GlobalException;
using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace Library_Book_Borrowing_System.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IBorrowRecordRepository _borrowRecordRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BookService> _logger;
    private readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
        SlidingExpiration = TimeSpan.FromSeconds(30)
    };

    public BookService(
        IBookRepository bookRepository,
        IBorrowRecordRepository borrowRecordRepository,
        IMemoryCache cache,
        ILogger<BookService> logger)
    {
        _bookRepository = bookRepository;
        _borrowRecordRepository = borrowRecordRepository;
        _cache = cache;
        _logger = logger;
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

        Book bk = new Book
        {
            Id = new Guid(),
            Title = book.Title,
            Author = book.Author,
            Isbn = book.Isbn,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies,
            BorrowedCount = 0
        };

        Book created = _bookRepository.Add(bk);
        _cache.Remove("book:list");

        GetBookResponse response = new GetBookResponse
        {
            Id = created.Id,
            Title = created.Title,
            Author = created.Author,
            Isbn = created.Isbn,
            TotalCopies = created.TotalCopies,
            AvailableCopies = created.AvailableCopies,
            BorrowedCount = created.BorrowedCount
        };

        _logger.LogInformation("Book created successfully. BookId: {BookId}", response.Id);

        return response;
    }

    public IEnumerable<GetBookResponse> GetAllBooks()
    {
        if (_cache.TryGetValue("book:list", out IEnumerable<GetBookResponse>? list) && list is not null)
        {
            return list;
        }

        List<GetBookResponse> bookList = _bookRepository.GetAll()
            .Select(bk => new GetBookResponse
            {
                Id = bk.Id,
                Title = bk.Title,
                Author = bk.Author,
                Isbn = bk.Isbn,
                TotalCopies = bk.TotalCopies,
                AvailableCopies = bk.AvailableCopies,
                BorrowedCount = bk.BorrowedCount
            })
            .ToList();

        _cache.Set("book:list", bookList, _cacheOptions);

        return bookList;
    }

    public PaginatedResponse<GetBookResponse> GetAllBooks(int pageNumber, int pageSize)
    {
        return Helper.ToPaginatedResponse(GetAllBooks(), pageNumber, pageSize);
    }

    public Task<GetBookDetailsResponse> GetBookById(Guid id)
    {
        if (_cache.TryGetValue($"book:{id}", out GetBookDetailsResponse? value) && value is not null)
        {
            return Task.FromResult(value);
        }

        Book? bk = _bookRepository.GetById(id);
        if (bk is null)
        {
            _logger.LogWarning("Invalid book id provided: {BookId}", id);
            throw new HttpRequestException(GlobalExceptionHandler.MISSING_BOOK_ID, null, System.Net.HttpStatusCode.NotFound);
        }

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

        return Task.FromResult(response);
    }

    public GetBookResponse UpdateBook(Guid id, UpdateBookRequest book)
    {
        Book? bookUpdating = _bookRepository.GetById(id);
        if (bookUpdating is null)
        {
            _logger.LogWarning("Invalid book id provided: {BookId}", id);
            throw new HttpRequestException(GlobalExceptionHandler.MISSING_BOOK_ID, null, System.Net.HttpStatusCode.NotFound);
        }

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
            _logger.LogWarning("Invalid book id provided: {BookId}", id);
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

        _logger.LogInformation("Book updated successfully. BookId: {BookId}", response.Id);

        return response;
    }

    public void DeleteBook(Guid id)
    {
        Book? book = _bookRepository.GetById(id);
        if (book is null)
        {
            _logger.LogWarning("Invalid book id provided: {BookId}", id);
            throw new HttpRequestException(GlobalExceptionHandler.MISSING_BOOK_ID, null, System.Net.HttpStatusCode.NotFound);
        }

        bool isBeingBorrowed = _borrowRecordRepository.GetAll()
            .Any(r => r.BookId == id && r.Status == "Borrowed");

        if (isBeingBorrowed)
        {
            throw new HttpRequestException(GlobalExceptionHandler.BOOK_IS_BORROWED, null, System.Net.HttpStatusCode.Conflict);
        }

        _cache.Remove($"book:{id}");
        _cache.Remove("book:list");
        _bookRepository.Delete(id);

        _logger.LogInformation("Book deleted successfully. BookId: {BookId}", id);
    }

    public Task<IEnumerable<GetBookDetailsResponse>> SearchBooksAsync(string? title, string? author, string? isbn)
    {
        if (string.IsNullOrWhiteSpace(title) &&
            string.IsNullOrWhiteSpace(author) &&
            string.IsNullOrWhiteSpace(isbn))
        {
            throw new HttpRequestException(
                GlobalExceptionHandler.NOT_FOUND,
                null,
                System.Net.HttpStatusCode.BadRequest);
        }

        IQueryable<Book> query = _bookRepository.GetAll().AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(b => b.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(author))
        {
            query = query.Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(isbn))
        {
            query = query.Where(b => b.Isbn == isbn);
        }

        List<GetBookDetailsResponse> results = query
            .Select(bk => new GetBookDetailsResponse
            {
                Id = bk.Id,
                Title = bk.Title,
                Author = bk.Author,
                Isbn = bk.Isbn,
                TotalCopies = bk.TotalCopies,
                AvailableCopies = bk.AvailableCopies,
                BorrowedCount = bk.BorrowedCount
            })
            .ToList();

        if (!results.Any())
        {
            string searchDetails = string.Join(", ", new[]
            {
                !string.IsNullOrEmpty(title) ? $"Title: {title}" : null,
                !string.IsNullOrEmpty(author) ? $"Author: {author}" : null,
                !string.IsNullOrEmpty(isbn) ? $"ISBN: {isbn}" : null
            }.Where(s => s is not null));

            throw new HttpRequestException(
                $"No books found matching the search criteria: {searchDetails}",
                null,
                System.Net.HttpStatusCode.BadRequest);
        }

        _logger.LogInformation(
            "Book search performed. Title: {Title}, Author: {Author}, Isbn: {Isbn}, ResultCount: {ResultCount}",
            title,
            author,
            isbn,
            results.Count);

        return Task.FromResult<IEnumerable<GetBookDetailsResponse>>(results);
    }
}
