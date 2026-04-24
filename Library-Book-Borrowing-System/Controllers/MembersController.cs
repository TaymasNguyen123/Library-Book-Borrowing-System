using Microsoft.AspNetCore.Mvc;
using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Services;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Library_Book_Borrowing_System.Controllers;

[ApiController]
[Route("api/members")]
[Authorize]

public class MembersController(IMemberService memberService, AuthHelper _authHelper): ControllerBase
{
 
    [HttpPost]
    public ActionResult<GetMemberResponse> CreateMember(CreateMemberRequest member)
    {
        if (!_authHelper.isAdmin()) return Forbid();

        var newMember = memberService.CreateMember(member);
        return CreatedAtAction(nameof(CreateMember), new { id = newMember.Id }, newMember);
    }

    [HttpGet]
    public ActionResult<IEnumerable<GetMemberResponse>> GetAllMembers()
    {
        if (!_authHelper.isAdmin()) return Forbid();

        return Ok(memberService.GetAllMembers());
    } 

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetMemberResponse>> GetMemberById(Guid id)
    {
        if (!_authHelper.isSelfOrAdmin(id)) return Forbid();    

        var _member = await memberService.GetMemberById(id);
        return Ok(_member);
    }

    [HttpPut("{id:guid}")]
    public ActionResult<GetMemberResponse> UpdateMember(Guid id, [FromBody] UpdateMemberRequest newMember)
    {
        if (!_authHelper.isSelfOrAdmin(id)) return Forbid();    

        GetMemberResponse? updatedMember = memberService.UpdateMember(id, newMember);
        return Ok(updatedMember);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeleteMember(Guid id)
    {
        if (!_authHelper.isSelfOrAdmin(id)) return Forbid();    

        memberService.DeleteMember(id);
        return NoContent();
    }
}