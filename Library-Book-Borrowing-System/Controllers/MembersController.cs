using Microsoft.AspNetCore.Mvc;
using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Services;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

namespace Library_Book_Borrowing_System.Controllers;

[ApiController]
[Route("api/members")]
[Authorize]

public class MembersController(IMemberService memberService): ControllerBase
{
    [HttpPost]
    public ActionResult<GetMemberResponse> CreateMember(CreateMemberRequest member)
    {
        var newMember = memberService.CreateMember(member);
        return CreatedAtAction(nameof(CreateMember), new { id = newMember.Id }, newMember);
    }

    [HttpGet]
    public ActionResult<IEnumerable<GetMemberResponse>> GetAllMembers()
    {
        return Ok(memberService.GetAllMembers());
    } 

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetMemberResponse>> GetMemberById(Guid id)
    {
        var _member = await memberService.GetMemberById(id);
        return Ok(_member);
    }

    [HttpPut("{id:guid}")]
    public ActionResult<GetMemberResponse> UpdateMember(Guid id, [FromBody] UpdateMemberRequest newMember)
    {
        GetMemberResponse? updatedMember = memberService.UpdateMember(id, newMember);
        return Ok(updatedMember);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeleteMember(Guid id)
    {
        memberService.DeleteMember(id);
        return NoContent();
    }
}