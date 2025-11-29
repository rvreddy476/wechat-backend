using Chat.Domain.Entities;
using Chat.Domain.Repositories;
using MediatR;
using Shared.Domain.Common;

namespace Chat.Application.Conversations.Commands.CreateConversation;

public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, Result<Conversation>>
{
    private readonly IConversationRepository _conversationRepository;

    public CreateConversationCommandHandler(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<Result<Conversation>> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        if (request.Type == ConversationType.OneToOne && request.ParticipantIds.Count == 1)
        {
            var existingResult = await _conversationRepository.GetOneToOneConversationAsync(
                request.CreatorId, request.ParticipantIds[0]);
            
            if (existingResult.IsSuccess)
                return existingResult;
        }

        var conversation = new Conversation
        {
            Type = request.Type,
            CreatedBy = request.CreatorId,
            Admins = new List<Guid> { request.CreatorId },
            GroupName = request.GroupName,
            GroupAvatarUrl = request.GroupAvatarUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var participantId in request.ParticipantIds)
        {
            var username = request.ParticipantUsernames.GetValueOrDefault(participantId, "User");
            conversation.AddParticipant(participantId, username);
        }

        var creatorUsername = request.ParticipantUsernames.GetValueOrDefault(request.CreatorId, "User");
        conversation.AddParticipant(request.CreatorId, creatorUsername);

        return await _conversationRepository.CreateAsync(conversation);
    }
}
