namespace Library_Book_Borrowing_System.Dtos;

public class UpdateBorrowRecordRequest
{
    public Guid BookId { get; set; }
    public Guid MemberId { get; set; }
    public DateTime ReturnDate { get; set; }
    public required string Status { get; set; }
}
