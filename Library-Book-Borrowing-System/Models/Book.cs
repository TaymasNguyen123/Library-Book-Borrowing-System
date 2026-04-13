namespace Library_Book_Borrowing_System.Models;

public class Book
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public required string Isbn { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
}
