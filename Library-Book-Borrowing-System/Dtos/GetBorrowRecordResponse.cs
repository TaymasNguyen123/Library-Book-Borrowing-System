namespace Library_Book_Borrowing_System.Dtos;

public class GetBorrowRecordResponse
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Guid MemberId { get; set; }
    public string BorrowDate { get; set; }
    public string? ReturnDate { get; set; }
    public required string Status { get; set; }
}
