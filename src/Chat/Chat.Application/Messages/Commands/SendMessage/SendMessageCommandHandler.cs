using Chat.Domain.Entities;
using Chat.Domain.Repositories;
using MediatR;
using Shared.Domain.Common;

namespace Chat.Application.Messages.Commands.SendMessage;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<Message>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;

    public SendMessageCommandHandler(IMessageRepository messageRepository, IConversationRepository conversationRepository)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
    }

    public async Task<Result<Message>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var conversationResult = await _conversationRepository.GetByIdAsync(request.ConversationId);
        if (!conversationResult.IsSuccess)
            return Result.Failure<Message>("Conversation not found");

        var message = new Message
        {
            ConversationId = request.ConversationId,
            SenderId = request.SenderId,
            SenderUsername = request.SenderUsername,
            Content = request.Content,
            MessageType = request.MessageType,
            MediaUrl = request.MediaUrl,
            ReplyToMessageId = request.ReplyToMessageId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _messageRepository.CreateAsync(message);
        if (!result.IsSuccess)
            return result;

        var conversation = conversationResult.Value;
        conversation.UpdateLastMessage(message.Id, message.SenderId, message.SenderUsername, message.Content);
        await _conversationRepository.UpdateAsync(conversation);

        return result;
    }
}
