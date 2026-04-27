using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;

namespace Library_Book_Borrowing_System.Services;
public interface IMemberService
{
    GetMemberResponse CreateMember(CreateMemberRequest member);
    IEnumerable<GetMemberResponse> GetAllMembers();
    PaginatedResponse<GetMemberResponse> GetAllMembers(int pageNumber, int pageSize);
    Task<GetMemberResponse> GetMemberById(Guid id);
    GetMemberResponse UpdateMember(Guid id, UpdateMemberRequest member);
    void DeleteMember(Guid id);
}
