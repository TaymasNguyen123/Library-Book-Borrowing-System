using Library_Book_Borrowing_System.Models;

namespace Library_Book_Borrowing_System.Repositories;

public interface IMemberRepository
{
    Member Add(Member member);
    IEnumerable<Member> GetAll();
    Member? GetById(Guid id);
    Member Update(
        string? updateFullName = null,
        string? updateEmail = null
    );
    void Delete(Guid id);

    Task<bool> ExistsAsync(Guid memberId);
}