using Microsoft.EntityFrameworkCore;
using Library_Book_Borrowing_System.Data;

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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();