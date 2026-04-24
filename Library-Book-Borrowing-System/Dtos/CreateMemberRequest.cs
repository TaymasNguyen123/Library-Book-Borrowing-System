namespace Library_Book_Borrowing_System.Dtos;

public class CreateMemberRequest
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public string Password { get; set; } = string.Empty;
}
