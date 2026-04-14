using Library_Book_Borrowing_System.Models;

namespace Library_Book_Borrowing_System.Repositories;

public interface IMemberRepository
{
    Member CreateMember(Member member);
    IEnumerable<Member> GetAllMembers();
    Member GetMemberById(Guid id);
    Member UpdateMember(
        string? updateFullName = null,
        string? updateEmail = null
    );
    void DeleteMember(Guid id);
}