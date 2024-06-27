using Index.Shared.DTOs;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Index.Source.Models;

public class Ad
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Title { get; set; }
    public string[] Categories { get; set; }

    public AdDto ToDto()
    {
        return new AdDto()
        {
            Id = Id,
            Title = Title,
            Categories = Categories
        };
    }
}
