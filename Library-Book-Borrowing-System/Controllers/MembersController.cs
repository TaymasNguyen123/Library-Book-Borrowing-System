using Microsoft.AspNetCore.Mvc;
using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Services;
using System.Text.RegularExpressions;

namespace Library_Book_Borrowing_System.Controllers;

[ApiController]
[Route("api/members")]

public class MembersController(IMemberService memberService): ControllerBase
{
    [HttpPost]
    public ActionResult<GetMemberResponse> CreateMember(CreateMemberRequest member)
    {
        Regex emailPattern = new Regex(@"\w+@\w+\.\w+");
        if (!emailPattern.IsMatch(member.Email))
        {
            return BadRequest();
        }

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
        Regex emailPattern = new Regex(@"\w+@\w+\.\w+");
        if (newMember.Email is not null && !emailPattern.IsMatch(newMember.Email))
        {
            return BadRequest();
        }

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