using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoRedisCacheDemo.Models;

public class Product
{
    [BsonId] // Maps the MongoDB _id field to this property
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; }

    [BsonElement("name")] // Explicitly maps the MongoDB field 'name' to this property
    public string Name { get; set; }

    [BsonElement("category")] // Maps 'category' to the 'Category' property
    public string Category { get; set; }

    [BsonElement("price")] // Maps 'price' to the 'Price' property
    public double Price { get; set; }
}