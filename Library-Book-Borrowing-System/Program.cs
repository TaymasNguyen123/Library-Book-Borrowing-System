using Microsoft.EntityFrameworkCore;
using Library_Book_Borrowing_System.Data;
using Library_Book_Borrowing_System.GlobalException;
using Library_Book_Borrowing_System.Repositories;
using Library_Book_Borrowing_System.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<Database>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<Library_Book_Borrowing_System.Services.IBookService, Library_Book_Borrowing_System.Services.BookService>();
builder.Services.AddScoped<Library_Book_Borrowing_System.Repositories.IBookRepository, Library_Book_Borrowing_System.Repositories.BookRepository>();
builder.Services.AddScoped<Library_Book_Borrowing_System.Services.IMemberService, Library_Book_Borrowing_System.Services.MemberService>();
builder.Services.AddScoped<Library_Book_Borrowing_System.Repositories.IMemberRepository, Library_Book_Borrowing_System.Repositories.MemberRepository>();
builder.Services.AddScoped<Library_Book_Borrowing_System.Services.IBorrowRecordService, Library_Book_Borrowing_System.Services.BorrowRecordService>();
builder.Services.AddScoped<Library_Book_Borrowing_System.Repositories.IBorrowRecordRepository, Library_Book_Borrowing_System.Repositories.BorrowRecordRepository>();

builder.Services.AddMemoryCache();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{   
    var db = app.Services.CreateScope().ServiceProvider.GetRequiredService<Database>();

    BookRepository _bookRepository = new BookRepository(db);
    MemberRepository _memberRepository = new MemberRepository(db);
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();

    _bookRepository.Add(new Book
    {
        Id = new Guid(),
        Title = "Heated Rivalry",
        Author = "Rachel Reid",
        Isbn = "9786567364564",
        TotalCopies = 10000,
        AvailableCopies = 10000,
        BorrowedCount = 0,
    });

    _bookRepository.Add(new Book
    {
        Id = new Guid(),
        Title = "Project Hail Mary",
        Author = "Andy Weir",
        Isbn = "9787665774323",
        TotalCopies = 5,
        AvailableCopies = 5,
        BorrowedCount = 0,
    });

    _bookRepository.Add(new Book
    {
        Id = new Guid(),
        Title = "Fifty Shades of Grey",
        Author = "E. L. James",
        Isbn = "9782117184897",
        TotalCopies = 100,
        AvailableCopies = 100,
        BorrowedCount = 0,
    });


    _memberRepository.Add(new Member
    {
        FullName = "Taylor Alison Swift Kelce",
        Email = "taylorswift@gmail.com",
        MembershipDate = DateTime.Now.AddYears(-1).ToString("MM/dd/yyyy"),
        BorrowRecords = new List<BorrowRecord>()
    });

    _memberRepository.Add(new Member
    {
        FullName = "Dobby Bellatrix",
        Email = "meow@yahoo.com",
        MembershipDate = DateTime.Now.AddMonths(-4).ToString("MM/dd/yyyy"),
        BorrowRecords = new List<BorrowRecord>()
    });

    _memberRepository.Add(new Member
    {
        FullName = "Tuffy",
        Email = "elephant@csu.fullerton.edu",
        MembershipDate = DateTime.Now.ToString("MM/dd/yyyy"),
        BorrowRecords = new List<BorrowRecord>()
    });
    
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();