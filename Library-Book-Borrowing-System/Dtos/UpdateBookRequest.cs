namespace Library_Book_Borrowing_System.Dtos;

public class UpdateBookRequest
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public required string Isbn { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public int BorrowedCount { get; set; }
}
