using MediatR;
using ChatService.Domain.Entities;
using ChatService.Domain.Repositories;
using Shared.Domain.Common;

namespace ChatService.Application.Messages.Commands.SendMessage;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<Message>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;

    public SendMessageCommandHandler(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
    }

    public async Task<Result<Message>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        // Verify conversation exists
        var conversationResult = await _conversationRepository.GetByIdAsync(request.ConversationId);
        if (!conversationResult.IsSuccess)
        {
            return Result.Failure<Message>("Conversation not found");
        }

        var message = new Message
        {
            ConversationId = request.ConversationId,
            SenderId = request.SenderId,
            SenderUsername = request.SenderUsername,
            Content = request.Content,
            MessageType = request.MessageType,
            MediaUrl = request.MediaUrl,
            MediaThumbnailUrl = request.MediaThumbnailUrl,
            MediaDuration = request.MediaDuration,
            FileName = request.FileName,
            FileSize = request.FileSize,
            Location = request.Location,
            ReplyToMessageId = request.ReplyToMessageId,
            Mentions = request.Mentions ?? new List<Guid>()
        };

        var result = await _messageRepository.CreateAsync(message);
        if (!result.IsSuccess)
        {
            return result;
        }

        // Update conversation's last message
        var conversation = conversationResult.Value;
        conversation.UpdateLastMessage(
            message.Id,
            message.SenderId,
            message.SenderUsername,
            message.Content,
            message.MessageType);

        await _conversationRepository.UpdateAsync(conversation);

        return result;
    }
}
