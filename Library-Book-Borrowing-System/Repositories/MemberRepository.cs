using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Data;
using Microsoft.EntityFrameworkCore;

namespace Library_Book_Borrowing_System.Repositories;

public class MemberRepository(Database database) : IMemberRepository
{
    public Member Add(Member member)
    {
        database.Members.Add(member);
        database.SaveChanges();
        return member;
    }

    public IEnumerable<Member> GetAll()
    {
        return database.Members.AsNoTracking().ToList();
    }

    public Member? GetById(Guid id)
    {
        return database.Members.FirstOrDefault(member => member.Id == id);
    }

    public Task<Member?> GetByEmailAsync(string email)
    {
        return database.Members.FirstOrDefaultAsync(u => u.Email == email);
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        return database.Members.AnyAsync(u => u.Email == email);
    }

    public Member? Update(Guid id, Member member)
    {
        Member? found = GetById(id);
        if (found is null)
            return null;

        database.Entry(found).CurrentValues.SetValues(member);        
        database.SaveChanges();

        return found;
    }

    public void Delete(Guid id)
    {
        database.Members.Where(member => member.Id == id).ExecuteDeleteAsync();
        database.SaveChanges();
    }

    public Task<bool> ExistsAsync(Guid memberId)
    {
        return database.Members.AnyAsync(member => member.Id == memberId);
    }
}