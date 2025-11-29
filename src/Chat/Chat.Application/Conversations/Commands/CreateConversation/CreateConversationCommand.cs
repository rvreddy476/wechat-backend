using Chat.Domain.Entities;
using MediatR;
using Shared.Domain.Common;

namespace Chat.Application.Conversations.Commands.CreateConversation;

public record CreateConversationCommand(
    ConversationType Type,
    Guid CreatorId,
    List<Guid> ParticipantIds,
    Dictionary<Guid, string> ParticipantUsernames,
    string? GroupName = null,
    string? GroupAvatarUrl = null
) : IRequest<Result<Conversation>>;
