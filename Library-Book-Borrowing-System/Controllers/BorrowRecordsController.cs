
using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library_Book_Borrowing_System.Controllers;

[ApiController]
[Route("api/borrowrecord")]

public class BorrowRecordsController(IBorrowRecordService borrowRecordService): ControllerBase
{
    [HttpPost("borrow")]
    public ActionResult<GetBorrowRecordResponse> BorrowBook(CreateBorrowRecordRequest borrowRecord)
    {
        GetBorrowRecordResponse? newBorrowRecord = borrowRecordService.BorrowBook(borrowRecord);
        return CreatedAtAction(nameof(BorrowBook), new { id = newBorrowRecord.Id }, newBorrowRecord);
    }

    [HttpPut("return")]
    public ActionResult<GetBorrowRecordResponse> ReturnBook(UpdateBorrowRecordRequest borrowRecord)
    {
        GetBorrowRecordResponse? updatedBorrowRecord = borrowRecordService.ReturnBook(borrowRecord);
        return Ok(updatedBorrowRecord);
    }

    [HttpGet]
    public ActionResult<GetBorrowRecordResponse> GetAllRecords()
    {
        return Ok(borrowRecordService.GetAllRecords());
    }

    [HttpGet("{memberId:guid}")]
    public ActionResult<IEnumerable<GetBorrowRecordResponse>> GetAllRecordsByMember(Guid memberId)
    {
        IEnumerable<GetBorrowRecordResponse>? memberRecords = borrowRecordService.GetAllRecordsByMember(memberId);
        return Ok(memberRecords);
    }
}