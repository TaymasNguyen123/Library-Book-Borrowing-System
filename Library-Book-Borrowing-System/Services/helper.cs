using System.Text.RegularExpressions;

namespace Library_Book_Borrowing_System.Services;

public class Helper
{
    public static bool IsValidIsbn(string isbn)
    {
        return true;
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
}

