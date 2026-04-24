namespace Library_Book_Borrowing_System.Models;

public class Member
{
    public Guid Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public String MembershipDate { get; set; }
    public ICollection<BorrowRecord>? BorrowRecords { get; set; } = new List<BorrowRecord>();
}
