using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;

namespace Library_Book_Borrowing_System.Services;
public interface IMemberService
{
    GetMemberResponse CreateMember(CreateMemberRequest member);
    IEnumerable<GetMemberResponse> GetAllMembers();
    Task<GetMemberResponse> GetMemberById(Guid id);
    GetMemberResponse UpdateMember(Member oldMember, UpdateMemberRequest member);
    void DeleteMember(Guid id);
}