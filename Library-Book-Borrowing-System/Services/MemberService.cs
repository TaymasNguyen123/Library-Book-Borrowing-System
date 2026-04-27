using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.GlobalException;
using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace Library_Book_Borrowing_System.Services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemberService> _logger;
    private readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
        SlidingExpiration = TimeSpan.FromSeconds(30)
    };

    public MemberService(
        IMemberRepository memberRepository,
        IMemoryCache cache,
        ILogger<MemberService> logger)
    {
        _memberRepository = memberRepository;
        _cache = cache;
        _logger = logger;
    }

    public GetMemberResponse CreateMember(CreateMemberRequest member)
    {
        string email = member.Email ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || !Helper.IsValidEmail(email))
        {
            throw new HttpRequestException(GlobalExceptionHandler.INVALID_EMAIL, null, System.Net.HttpStatusCode.BadRequest);
        }

        if (GetAllMembers().Any(x => x.Email == email))
        {
            throw new HttpRequestException(GlobalExceptionHandler.DUPLICATE_EMAIL, null, System.Net.HttpStatusCode.BadRequest);
        }

        Member newMember = new Member
        {
            Id = new Guid(),
            FullName = member.FullName,
            Email = email,
            MembershipDate = DateTime.Now.ToString("MM/dd/yyyy"),
            BorrowRecords = new List<BorrowRecord>()
        };

        _memberRepository.Add(newMember);
        _cache.Remove("member:list");

        GetMemberResponse response = new GetMemberResponse
        {
            Id = newMember.Id,
            FullName = newMember.FullName,
            Email = newMember.Email,
            MembershipDate = newMember.MembershipDate
        };

        _logger.LogInformation("Member created successfully. MemberId: {MemberId}", response.Id);

        return response;
    }

    public IEnumerable<GetMemberResponse> GetAllMembers()
    {
        if (_cache.TryGetValue("member:list", out IEnumerable<GetMemberResponse>? list) && list is not null)
        {
            return list;
        }

        List<GetMemberResponse> memberList = _memberRepository.GetAll()
            .Select(member => new GetMemberResponse
            {
                Id = member.Id,
                FullName = member.FullName,
                Email = member.Email,
                MembershipDate = member.MembershipDate,
                BorrowRecords = member.BorrowRecords ?? new List<BorrowRecord>()
            })
            .ToList();

        _cache.Set("member:list", memberList, _cacheOptions);

        return memberList;
    }

    public PaginatedResponse<GetMemberResponse> GetAllMembers(int pageNumber, int pageSize)
    {
        return Helper.ToPaginatedResponse(GetAllMembers(), pageNumber, pageSize);
    }

    public Task<GetMemberResponse> GetMemberById(Guid id)
    {
        if (_cache.TryGetValue($"member:{id}", out GetMemberResponse? value) && value is not null)
        {
            return Task.FromResult(value);
        }

        Member? member = _memberRepository.GetById(id);
        if (member is null)
        {
            _logger.LogWarning("Invalid member id provided: {MemberId}", id);
            throw new HttpRequestException(GlobalExceptionHandler.MISSING_MEMBER_ID, null, System.Net.HttpStatusCode.NotFound);
        }

        GetMemberResponse response = new GetMemberResponse
        {
            Id = member.Id,
            FullName = member.FullName,
            Email = member.Email,
            MembershipDate = member.MembershipDate,
            BorrowRecords = member.BorrowRecords
        };

        _cache.Set($"member:{response.Id}", response, _cacheOptions);

        return Task.FromResult(response);
    }

    public GetMemberResponse UpdateMember(Guid id, UpdateMemberRequest member)
    {
        string? updatedEmail = member.Email;
        Member? memberUpdating = _memberRepository.GetById(id);
        if (memberUpdating is null)
        {
            _logger.LogWarning("Invalid member id provided: {MemberId}", id);
            throw new HttpRequestException(GlobalExceptionHandler.MISSING_MEMBER_ID, null, System.Net.HttpStatusCode.NotFound);
        }

        if (updatedEmail is not null && !Helper.IsValidEmail(updatedEmail))
        {
            throw new HttpRequestException(GlobalExceptionHandler.INVALID_EMAIL, null, System.Net.HttpStatusCode.BadRequest);
        }

        if (updatedEmail is not null && GetAllMembers().Any(x => x.Email == updatedEmail && x.Id != memberUpdating.Id))
        {
            throw new HttpRequestException(GlobalExceptionHandler.DUPLICATE_EMAIL, null, System.Net.HttpStatusCode.BadRequest);
        }

        memberUpdating.FullName = member.FullName ?? memberUpdating.FullName;
        memberUpdating.Email = updatedEmail ?? memberUpdating.Email;
        memberUpdating.BorrowRecords = member.BorrowRecords ?? memberUpdating.BorrowRecords;

        Member? updated = _memberRepository.Update(id, memberUpdating);
        if (updated is null)
        {
            _logger.LogWarning("Invalid member id provided: {MemberId}", id);
            throw new HttpRequestException(GlobalExceptionHandler.MISSING_MEMBER_ID, null, System.Net.HttpStatusCode.NotFound);
        }

        _cache.Remove($"member:{updated.Id}");
        _cache.Remove("member:list");

        GetMemberResponse response = new GetMemberResponse
        {
            Id = id,
            FullName = updated.FullName,
            Email = updated.Email,
            MembershipDate = updated.MembershipDate,
            BorrowRecords = updated.BorrowRecords
        };

        _logger.LogInformation("Member updated successfully. MemberId: {MemberId}", response.Id);

        return response;
    }

    public void DeleteMember(Guid id)
    {
        Member? member = _memberRepository.GetById(id);
        if (member is null)
        {
            _logger.LogWarning("Invalid member id provided: {MemberId}", id);
            throw new HttpRequestException(GlobalExceptionHandler.MISSING_MEMBER_ID, null, System.Net.HttpStatusCode.NotFound);
        }

        _cache.Remove($"member:{id}");
        _cache.Remove("member:list");
        _memberRepository.Delete(id);

        _logger.LogInformation("Member deleted successfully. MemberId: {MemberId}", id);
    }
}
