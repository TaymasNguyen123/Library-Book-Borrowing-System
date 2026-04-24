
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Library_Book_Borrowing_System.Services;

public class AuthHelper
{
    private readonly ClaimsPrincipal _user;

    public AuthHelper(ClaimsPrincipal user)
    {
        _user = user;
    }
    public bool isSelf(Guid id)
    {
        string? memberId = _user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (memberId is null || Guid.Parse(memberId) != id) return false;
        return true;
    }

    public bool isAdmin()
    {
        string? role = _user.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "Admin") return false;
        return true;        
    }

    public bool isSelfOrAdmin(Guid id)
    {
        return isSelf(id) || isAdmin();
    }
}