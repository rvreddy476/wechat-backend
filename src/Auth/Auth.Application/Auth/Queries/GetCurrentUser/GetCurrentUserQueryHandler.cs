using Auth.Domain.Repositories;
using MediatR;
using Shared.Contracts.Auth;
using Shared.Domain.Common;

namespace Auth.Application.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(IUserRepository userRepository) => _userRepository = userRepository;

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var result = await _userRepository.GetByIdAsync(request.UserId);
        
        if (!result.IsSuccess)
            return Result.Failure<UserDto>("User not found");

        var user = result.Value;
        
        return Result.Success(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Roles = user.Roles
        });
    }
}
