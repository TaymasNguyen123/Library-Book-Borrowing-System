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
        return database.Members.Include(p => p.BorrowRecords).AsNoTracking().ToList();
    }

    public Member? GetById(Guid id)
    {
        return database.Members.AsNoTracking().FirstOrDefault(member => member.Id == id);
    }

    public Member? Update(Guid id, Member member)
    {
        Member? findMember = GetById(id);
        if (findMember is not null)
        {
            findMember.FullName = member.FullName;
            findMember.Email = member.Email;
            findMember.BorrowRecords = member.BorrowRecords;
            
            database.Entry(findMember).State = EntityState.Modified;
            database.SaveChanges();
        }
        return findMember;
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