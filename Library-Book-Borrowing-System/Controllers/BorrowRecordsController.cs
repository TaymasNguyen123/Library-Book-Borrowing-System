
using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library_Book_Borrowing_System.Controllers;

[ApiController]
[Route("api/borrowrecord")]

public class BorrowRecordsController(IBorrowRecordService borrowRecordService): ControllerBase
{
    [HttpPost]
    public ActionResult<GetBorrowRecordResponse> BorrowBook(CreateBorrowRecordRequest borrowRecord)
    {
        GetBorrowRecordResponse? newBorrowRecord = borrowRecordService.BorrowBook(borrowRecord);
        return CreatedAtAction(nameof(BorrowBook), new { id = newBorrowRecord.Id }, newBorrowRecord);
    }
}