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
        return database.Members.Include(p => p.BorrowRecords).AsNoTracking().FirstOrDefault(member => member.Id == id);
    }

    public Member? Update(Guid id, Member member)
    {
        var existingMember = database.Members
            .Include(p => p.BorrowRecords)
            .FirstOrDefault(m => m.Id == id);

        if (existingMember != null)
        {
            existingMember.FullName = member.FullName;
            existingMember.Email = member.Email;

            if (member.MembershipDate != null)
            {
                existingMember.MembershipDate = member.MembershipDate;
            }
            
            if (member.BorrowRecords != null && member.BorrowRecords.Any())
            {
                existingMember.BorrowRecords = member.BorrowRecords;
            }

            database.SaveChanges();
            return existingMember;
        }

        return null;
    }

    public void Delete(Guid id)
    {
        var member = database.Members.FirstOrDefault(m => m.Id == id);
                if (member != null)
                {
                    database.Members.Remove(member);
                    database.SaveChanges();
                }
    }

    public Task<bool> ExistsAsync(Guid memberId)
    {
        return database.Members.AnyAsync(member => member.Id == memberId);
    }
}