namespace Library_Book_Borrowing_System.Dtos;

public class CreateBorrowRecordRequest
{
    public Guid BookId { get; set; }
    public Guid MemberId { get; set; }
}
