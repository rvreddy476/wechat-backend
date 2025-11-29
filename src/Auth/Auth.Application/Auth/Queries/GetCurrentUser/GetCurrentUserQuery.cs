using MediatR;
using Shared.Contracts.Auth;
using Shared.Domain.Common;

namespace Auth.Application.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery(Guid UserId) : IRequest<Result<UserDto>>;
