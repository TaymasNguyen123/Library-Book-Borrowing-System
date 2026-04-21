using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Repositories;
using Library_Book_Borrowing_System.GlobalException;
using Microsoft.Extensions.Caching.Memory;

namespace Library_Book_Borrowing_System.Services;

public class MemberService(IMemberRepository _memberRepository, IMemoryCache _cache) : IMemberService
{
    private readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        SlidingExpiration = TimeSpan.FromMinutes(2)        
    };
    public GetMemberResponse CreateMember(CreateMemberRequest member)
    {
        if (!Helper.IsValidEmail(member.Email))
        {
            throw new HttpRequestException(GlobalExceptionHandler.INVALID_EMAIL, null, System.Net.HttpStatusCode.BadRequest);
        }

        if (GetAllMembers().Any(x => x.Email == member.Email))
        {
            throw new HttpRequestException(GlobalExceptionHandler.DUPLICATE_EMAIL, null, System.Net.HttpStatusCode.BadRequest);
        }

        Member newMember = new Member
        {
            Id = new Guid(),
            FullName = member.FullName,
            Email = member.Email,
            MembershipDate = DateTime.Now,
            BorrowRecords = new List<BorrowRecord>()
        };

        _memberRepository.Add(newMember);

        return new GetMemberResponse
        {
            Id = newMember.Id,
            FullName = newMember.FullName,
            Email = newMember.Email,
            MembershipDate = newMember.MembershipDate
        };
    }
    public IEnumerable<GetMemberResponse> GetAllMembers()
    {
        IEnumerable<GetMemberResponse> memberList = _memberRepository.GetAll()
            .Select(member => new GetMemberResponse
            {
                Id = member.Id,
                FullName = member.FullName,
                Email = member.Email,
                BorrowRecords = member.BorrowRecords
            });
        
        _cache.Set("member:list", memberList, _cacheOptions);
        
        return memberList;
    }
    public async Task<GetMemberResponse> GetMemberById(Guid id)
    {
        if (_cache.TryGetValue(id, out GetMemberResponse? value))
        {
            return value;
        }

        Member? _member = _memberRepository.GetById(id)
            ?? throw new HttpRequestException(GlobalExceptionHandler.MISSING_MEMBER_ID, null, System.Net.HttpStatusCode.NotFound);

        GetMemberResponse response = new GetMemberResponse
        {
            Id = _member.Id,
            FullName = _member.FullName,
            Email = _member.Email,
            MembershipDate = _member.MembershipDate
        };

        _cache.Set($"member:{response.Id}", response, _cacheOptions);

        return await Task.FromResult(response);
    }
    public GetMemberResponse UpdateMember(Guid id, UpdateMemberRequest member)
    {
        Member? memberUpdating = _memberRepository.GetById(id);
        if (memberUpdating is null)
        {
            throw new HttpRequestException(GlobalExceptionHandler.MISSING_MEMBER_ID, null, System.Net.HttpStatusCode.NotFound);
        }
        if (!Helper.IsValidEmail(member.Email))
        {
            throw new HttpRequestException(GlobalExceptionHandler.INVALID_EMAIL, null, System.Net.HttpStatusCode.BadRequest);
        }

        if (GetAllMembers().Any(x => x.Email == member.Email & x.Id != memberUpdating.Id))
        {
            throw new HttpRequestException(GlobalExceptionHandler.DUPLICATE_EMAIL, null, System.Net.HttpStatusCode.BadRequest);
        }

        memberUpdating.FullName = member.FullName ?? memberUpdating.FullName;
        memberUpdating.Email = member.Email ?? memberUpdating.Email;
        memberUpdating.BorrowRecords = member.BorrowRecords ?? memberUpdating.BorrowRecords;

        Member? updated = _memberRepository.Update(id, memberUpdating);

        _cache.Remove($"member:{updated.Id}");
        _cache.Remove("member:list");

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
        _cache.Remove($"member:{id}");
        _cache.Remove("member:list");

        _memberRepository.Delete(id);
    }
}