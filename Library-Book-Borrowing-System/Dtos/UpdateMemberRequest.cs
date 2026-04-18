namespace Library_Book_Borrowing_System.Models;

public class UpdateMemberRequest
{
    public Guid Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
}
