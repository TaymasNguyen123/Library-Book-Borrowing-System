namespace Library_Book_Borrowing_System.Models;

public class UpdateMemberRequest
{
    public required Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
}
