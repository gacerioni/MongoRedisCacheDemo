using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoRedisCacheDemo.Models;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace MongoRedisCacheDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMongoCollection<Product> _productsCollection;
    private readonly IDistributedCache _cache;

    public ProductsController(IMongoCollection<Product> productsCollection, IDistributedCache cache)
    {
        _productsCollection = productsCollection;
        _cache = cache;
    }

    // Get Product by ID with Cache-Aside Pattern
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProductById(string id)
    {
        var cacheKey = $"product:{id}";

        // Check if product is in cache
        var cachedProduct = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedProduct))
        {
            return Ok(JsonConvert.DeserializeObject<Product>(cachedProduct));
        }

        // If not cached, retrieve from MongoDB
        var product = await _productsCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (product == null) return NotFound();

        // Cache the product with a 5-minute expiration
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
        await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(product), cacheOptions);

        return Ok(product);
    }

    // Update Product (with Cache Invalidation)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] Product updatedProduct)
    {
        var result = await _productsCollection.ReplaceOneAsync(p => p.Id == id, updatedProduct);
        if (result.MatchedCount == 0) return NotFound();

        // Invalidate cache after update
        var cacheKey = $"product:{id}";
        await _cache.RemoveAsync(cacheKey);

        return NoContent();
    }

    // Delete Product from Cache Manually
    [HttpDelete("cache/{id}")]
    public async Task<IActionResult> ClearProductCache(string id)
    {
        var cacheKey = $"product:{id}";
        await _cache.RemoveAsync(cacheKey);
        return NoContent();
    }
}