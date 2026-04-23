namespace Library_Book_Borrowing_System.Models;

public class UpdateMemberRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public ICollection<BorrowRecord>? BorrowRecords { get; set; } = new List<BorrowRecord>();
}
