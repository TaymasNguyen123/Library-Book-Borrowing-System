using Library_Book_Borrowing_System.Models;

namespace Library_Book_Borrowing_System.Repositories;

public interface IMemberRepository
{
    Member Add(Member member);
    IEnumerable<Member> GetAll();
    Member? GetById(Guid id);
    Task<Member?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Member? Update(Guid id, Member member);
    void Delete(Guid id);

    Task<bool> ExistsAsync(Guid memberId);
}