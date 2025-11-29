using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Domain.Common;

namespace UserProfile.Domain.Entities;

public class FriendRequest : AggregateRoot
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("senderId")]
    [BsonRepresentation(BsonType.String)]
    public Guid SenderId { get; set; }

    [BsonElement("receiverId")]
    [BsonRepresentation(BsonType.String)]
    public Guid ReceiverId { get; set; }

    [BsonElement("status")]
    public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;

    [BsonElement("message")]
    public string? Message { get; set; }

    [BsonElement("respondedAt")]
    public DateTime? RespondedAt { get; set; }
}

public enum FriendRequestStatus
{
    Pending,
    Accepted,
    Rejected
}
