using Microsoft.EntityFrameworkCore;
using Library_Book_Borrowing_System.Data;
using Library_Book_Borrowing_System.GlobalException;
using Library_Book_Borrowing_System.Repositories;
using Library_Book_Borrowing_System.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Library_Book_Borrowing_System.Services;
using Library_Book_Borrowing_System.Settings;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.OpenApi.Writers;
using Microsoft.Extensions.Options;
using Library_Book_Borrowing_System.Dtos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
     options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT like: Bearer {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

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
builder.Services.AddScoped<Library_Book_Borrowing_System.Services.IAuthService, Library_Book_Borrowing_System.Services.AuthService>();
builder.Services.AddScoped<Library_Book_Borrowing_System.Services.AuthHelper>();

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IPasswordHasher<Member>, PasswordHasher<Member>>();
builder.Services.Configure<Library_Book_Borrowing_System.Settings.JwtSettings>(builder.Configuration.GetSection("Jwt"));

// JWT Authentication setup
var jwtKey = builder.Configuration["Jwt:Key"]
?? throw new InvalidOperationException("JWT Key is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => 
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
            };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{   
    var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<Database>();
    var services = scope.ServiceProvider;
    
    var hasher = services.GetRequiredService<IPasswordHasher<Member>>();
    var jwtOptions = services.GetRequiredService<IOptions<JwtSettings>>();

    BookRepository _bookRepository = new BookRepository(db);
    MemberRepository _memberRepository = new MemberRepository(db);
    AuthService _authService = new AuthService(_memberRepository, hasher, jwtOptions);

    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();

    _bookRepository.Add(new Book
    {
        Id = new Guid("593d8150-b074-401e-b606-0ad6f9755510"),
        Title = "Heated Rivalry",
        Author = "Rachel Reid",
        Isbn = "9786567364564",
        TotalCopies = 10000,
        AvailableCopies = 10000,
        BorrowedCount = 0,
    });

    _bookRepository.Add(new Book
    {
        Id = new Guid("593d8150-b074-401e-b606-0ad6f9755520"),
        Title = "Project Hail Mary",
        Author = "Andy Weir",
        Isbn = "9787665774323",
        TotalCopies = 5,
        AvailableCopies = 5,
        BorrowedCount = 0,
    });

    _bookRepository.Add(new Book
    {
        Id = new Guid("593d8150-b074-401e-b606-0ad6f9755530"),
        Title = "Fifty Shades of Grey",
        Author = "E. L. James",
        Isbn = "9782117184897",
        TotalCopies = 100,
        AvailableCopies = 100,
        BorrowedCount = 0,
    });

    IEnumerable<RegisterRequest> registerRequests = [
        new RegisterRequest {
            FullName = "Test User",
            Email = "user@email.com",
            Password = "user"
        },
        new RegisterRequest {
            FullName = "Test Admin",
            Email = "admin@email.com",
            Password = "admin"
        },
        new RegisterRequest {
            FullName = "Taylor Alison Swift Kelce",
            Email = "taylorswift@gmail.com",
            Password = "iamtaylorswift"
        },
        new RegisterRequest {
            FullName = "Dobby Bellatrix",
            Email = "meow@yahoo.com",
            Password = "meowmeowmeow"
        },
        new RegisterRequest {
            FullName = "Tuffy",
            Email = "elephant@csu.fullerton.edu",
            Password = "tuffythetitan123"
        }
    ];

    foreach (var req in registerRequests)
    {
        AuthResponse? res = await _authService.RegisterAsync(req);
        if (res is not null && res.Email == "admin@email.com")
        {
            res = _authService.UpdateMemberRole(res.MemberId, "Admin");
        }
    }

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();