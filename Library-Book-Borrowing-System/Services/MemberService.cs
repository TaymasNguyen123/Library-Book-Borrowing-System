using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Repositories;

namespace Library_Book_Borrowing_System.Services;

public class MemberService(IMemberRepository _memberRepository) : IMemberService
{
    public GetMemberResponse CreateMember(CreateMemberRequest member)
    {
        Member member_ = new Member
        {
            Id = new Guid(),
            FullName = member.FullName,
            Email = member.Email,
            MembershipDate = DateTime.Now,
            BorrowRecords = new List<BorrowRecord>()
        };

        _memberRepository.Add(member_);
        GetMemberResponse newMember = new GetMemberResponse
        {
            Id = member_.Id,
            FullName = member_.FullName,
            Email = member_.Email,
            MembershipDate = member_.MembershipDate
        };
        return newMember;
    }
    public IEnumerable<GetMemberResponse> GetAllMembers()
    {
        return _memberRepository.GetAll()
            .Select(member => new GetMemberResponse
            {
                Id = member.Id,
                FullName = member.FullName,
                Email = member.Email,
                BorrowRecords = member.BorrowRecords
            });
    }
    public Task<GetMemberResponse> GetMemberById(Guid id)
    {
        Member member_ = _memberRepository.GetById(id);
        GetMemberResponse newMember = new GetMemberResponse
        {
            Id = member_.Id,
            FullName = member_.FullName,
            Email = member_.Email,
            MembershipDate = member_.MembershipDate
        };
        return Task.FromResult(newMember);
    }
    public GetMemberResponse UpdateMember(Member oldMember, UpdateMemberRequest member)
    {
        Member member_ = _memberRepository.Update(
            oldMember,
            member.FullName,
            member.Email
        );

        return new GetMemberResponse
        {
            Id = member_.Id,
            FullName = member_.FullName,
            Email = member_.Email
        };
    }
    public void DeleteMember(Guid id)
    {
        _memberRepository.Delete(id);
    }
}