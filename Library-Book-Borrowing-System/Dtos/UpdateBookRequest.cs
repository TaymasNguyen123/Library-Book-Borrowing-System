namespace Library_Book_Borrowing_System.Dtos;

public class UpdateBookRequest
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Isbn { get; set; }
    public int? TotalCopies { get; set; }
    public int? AvailableCopies { get; set; }
    public int? BorrowedCount { get; set; }
}
