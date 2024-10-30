using MongoDB.Driver;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using MongoRedisCacheDemo.Models;

var builder = WebApplication.CreateBuilder(args);

// Load MongoDB Configuration
var mongoSettings = builder.Configuration.GetSection("MongoDB");
var mongoConnectionString = mongoSettings["ConnectionString"];
var databaseName = mongoSettings["DatabaseName"];

// MongoDB Client and Database
var mongoClient = new MongoClient(mongoConnectionString);
var mongoDatabase = mongoClient.GetDatabase(databaseName);
builder.Services.AddSingleton(mongoDatabase.GetCollection<Product>("products"));

// Redis Cache Configuration
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379"; // Update if Redis is hosted elsewhere
});

// Add Controllers and Swagger services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Add Swagger support

var app = builder.Build();

// Enable Swagger UI in Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();