using Library_Book_Borrowing_System.Models;

namespace Library_Book_Borrowing_System.Dtos;

public class GetMemberResponse
{
    public Guid Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public String MembershipDate { get; set; }
    public ICollection<BorrowRecord>? BorrowRecords { get; set; } = new List<BorrowRecord>();
}
