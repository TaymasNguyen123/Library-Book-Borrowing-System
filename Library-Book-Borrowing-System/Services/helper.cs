using System.Text.RegularExpressions;
using Library_Book_Borrowing_System.Dtos;

namespace Library_Book_Borrowing_System.Services;

public class Helper
{
    public static bool IsValidIsbn(string isbn)
    {
        isbn = isbn.Replace("-", "");
        if (!isbn.All(char.IsDigit) || !(isbn.Substring(0, 3) == "978" || isbn.Substring(0, 3) == "979") || isbn.Length != 13)
        {
            return false;
        }

        int sum = 0;
        for (int i = 0; i <= 12; i++)
        {
            int digit = isbn[i] - '0';
            sum += (i % 2 == 0) ? digit : digit * 3;
        }

        return sum % 10 == 0;
    }

    public static bool IsValidEmail(string email)
    {
        Regex emailPattern = new Regex(@"\w+@\w+\.\w+");
        if (email is not null && !emailPattern.IsMatch(email))
        {
            return false;
        }

        return true;
    }

    public static (int PageNumber, int PageSize) NormalizePagination(int pageNumber, int pageSize)
    {
        int normalizedPageNumber = pageNumber < 1 ? 1 : pageNumber;
        int normalizedPageSize = pageSize < 1 ? 10 : pageSize;

        if (normalizedPageSize > 50)
        {
            normalizedPageSize = 50;
        }

        return (normalizedPageNumber, normalizedPageSize);
    }

    public static PaginatedResponse<T> ToPaginatedResponse<T>(
        IEnumerable<T> source,
        int pageNumber,
        int pageSize)
    {
        var (normalizedPageNumber, normalizedPageSize) = NormalizePagination(pageNumber, pageSize);
        List<T> items = source.ToList();
        int totalCount = items.Count;
        int totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)normalizedPageSize);

        List<T> pagedItems = items
            .Skip((normalizedPageNumber - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToList();

        return new PaginatedResponse<T>
        {
            Items = pagedItems,
            PageNumber = normalizedPageNumber,
            PageSize = normalizedPageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}
