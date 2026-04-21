using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Repositories;
using Library_Book_Borrowing_System.GlobalException;
using Microsoft.Extensions.Caching.Memory;

namespace Library_Book_Borrowing_System.Services;

public class BookService: IBookService
{ 
    private readonly IBookRepository _bookRepository;
    private readonly IBorrowRecordRepository _borrowRecordRepository;
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        SlidingExpiration = TimeSpan.FromMinutes(2)
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
        Book? bookUpdating = _bookRepository.GetById(id);
        if (bookUpdating is null)
        {
            throw new HttpRequestException(GlobalExceptionHandler.MISSING_BOOK_ID, null, System.Net.HttpStatusCode.NotFound);
        }
        
        if (book.AvailableCopies > book.TotalCopies)
        {
            throw new HttpRequestException(GlobalExceptionHandler.MORE_AVAILABLE_THAN_TOTAL, null, System.Net.HttpStatusCode.NotFound);
        }

        if (!Helper.IsValidIsbn(book.Isbn))
        {
            throw new HttpRequestException(GlobalExceptionHandler.INVALID_ISBN, null, System.Net.HttpStatusCode.NotFound);
        }

        
        if (GetAllBooks().Any(x => x.Isbn == book.Isbn && x.Id != bookUpdating.Id))
        {
            throw new HttpRequestException(GlobalExceptionHandler.DUPLICATE_ISBN, null, System.Net.HttpStatusCode.BadRequest);   
        }

        bookUpdating.Title = book.Title;
        bookUpdating.Author = book.Author;
        bookUpdating.Isbn = book.Isbn;
        bookUpdating.TotalCopies = (int) book.TotalCopies;
        bookUpdating.AvailableCopies = (int) book.AvailableCopies;
        bookUpdating.BorrowedCount = (int) book.BorrowedCount;

        if (bookUpdating.AvailableCopies > bookUpdating.TotalCopies)
        {
            throw new HttpRequestException(GlobalExceptionHandler.MORE_AVAILABLE_THAN_TOTAL, null, System.Net.HttpStatusCode.BadRequest);
        }

        var updated = _bookRepository.Update(id, bookUpdating);

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
        _cache.Remove($"book:{id}");
        _cache.Remove("book:list");

        _bookRepository.Delete(id);
    }
}