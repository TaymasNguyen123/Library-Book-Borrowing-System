using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Repositories;
using Library_Book_Borrowing_System.GlobalException;
using Microsoft.Extensions.Caching.Memory;

namespace Library_Book_Borrowing_System.Services;

public class MemberService(IMemberRepository _memberRepository, IMemoryCache _cacheGuid, IMemoryCache _cacheEmail) : IMemberService
{
    public GetMemberResponse CreateMember(CreateMemberRequest member)
    {
        if (!Helper.IsValidEmail(member.Email))
        {
            throw new HttpRequestException(GlobalExceptionHandler.INVALID_EMAIL, null, System.Net.HttpStatusCode.BadRequest);
        }


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
        _cacheGuid.Set(newMember.Id, newMember);
        _cacheEmail.Set(newMember.Email, newMember);
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
        Member? member_;
        if (_cacheGuid.TryGetValue(id, out GetMemberResponse? value))
        {
            member_ = new Member
            {
                Id = value.Id,
                FullName = value.FullName,
                Email = value.Email,
                MembershipDate = value.MembershipDate
            };
        }
        else
        {
            member_ = _memberRepository.GetById(id);
        }

        if (member_ is null)
        {
            throw new HttpRequestException(GlobalExceptionHandler.MISSING_MEMBER_ID, null, System.Net.HttpStatusCode.NotFound);
        }

        GetMemberResponse newMember = new GetMemberResponse
        {
            Id = member_.Id,
            FullName = member_.FullName,
            Email = member_.Email,
            MembershipDate = member_.MembershipDate
        };
        return Task.FromResult(newMember);
    }
    public GetMemberResponse UpdateMember(Guid id, UpdateMemberRequest member)
    {
        Member? memberUpdating = _memberRepository.GetById(id);
        if (memberUpdating is null)
        {
            throw new HttpRequestException("Member with that id does not exist", null, System.Net.HttpStatusCode.NotFound);
        }

        memberUpdating.FullName = member.FullName ?? memberUpdating.FullName;
        memberUpdating.Email = member.Email ?? memberUpdating.Email;
        memberUpdating.BorrowRecords = member.BorrowRecords ?? memberUpdating.BorrowRecords;

        Member? updated = _memberRepository.Update(id, memberUpdating);

        _cacheGuid.Set(updated.Id, updated);
        _cacheEmail.Set(updated.Email, updated);

        return new GetMemberResponse
        {
            Id = id,
            FullName = updated.FullName,
            Email = updated.Email,
            BorrowRecords = updated.BorrowRecords
        };
    }
    public void DeleteMember(Guid id)
    {
        _cacheGuid.Remove(id);
        _cacheEmail.Remove(_memberRepository.GetById(id).Email);

        _memberRepository.Delete(id);
    }
}