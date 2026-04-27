
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

    [HttpPut]
    public ActionResult<GetBorrowRecordResponse> ReturnBook(UpdateBorrowRecordRequest borrowRecord)
    {
        GetBorrowRecordResponse? updatedBorrowRecord = borrowRecordService.ReturnBook(borrowRecord);
        return Ok(updatedBorrowRecord);
    }

    [HttpGet]
    public ActionResult<PaginatedResponse<GetBorrowRecordResponse>> GetAllRecords(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        return Ok(borrowRecordService.GetAllRecords(pageNumber, pageSize));
    }

    [HttpGet("{memberId:guid}")]
    public ActionResult<PaginatedResponse<GetBorrowRecordResponse>> GetAllRecordsByMember(
        Guid memberId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        PaginatedResponse<GetBorrowRecordResponse> memberRecords = borrowRecordService.GetAllRecordsByMember(memberId, pageNumber, pageSize);
        return Ok(memberRecords);
    }
}
