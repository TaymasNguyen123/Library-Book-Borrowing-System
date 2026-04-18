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
        return database.Members.AsNoTracking().FirstOrDefault(member => member.Id == id);
    }

    public Member Update(
        Member oldMember,
        string? updateFullName = null,
        string? updateEmail = null,
        ICollection<BorrowRecord>? updateBorrowRecords = null
    )
    {
        Member? findMember = database.Members.AsNoTracking().FirstOrDefault(member => member.Id == oldMember.Id);

        findMember.FullName = updateFullName == null ? findMember.FullName : updateFullName;
        findMember.Email = updateEmail == null ? findMember.Email : updateEmail;
        findMember.BorrowRecords = updateBorrowRecords == null ? findMember.BorrowRecords : updateBorrowRecords;

        database.SaveChanges();

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