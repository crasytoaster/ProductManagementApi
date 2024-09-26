// Controllers/ProductController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagementApi.Data;
using ProductManagementApi.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
namespace ProductManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductDbContext _context;
        public ProductsController(ProductDbContext context)
        {
            _context = context;
        }

        // 1. List Active Products
        [HttpGet]
        public async Task<IActionResult> GetProducts(string name = null, decimal? minPrice = null, decimal? maxPrice = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Products
                .Where(p => p.Status == "active")
                .OrderByDescending(p => p.CreatedAt)
                .AsQueryable();
            if (!string.IsNullOrEmpty(name))
                query = query.Where(p => p.Name.Contains(name));
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice);
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice);
            if (startDate.HasValue)
                query = query.Where(p => p.CreatedAt >= startDate);
            if (endDate.HasValue)
                query = query.Where(p => p.CreatedAt <= endDate);
            return Ok(await query.ToListAsync());
        }

        // 2. Create Product
        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            if (product.Price > 10000)
                return BadRequest("Product price cannot exceed 10,000.");
            if (product.Price > 5000)
            {
                product.ApprovalStatus = "pending";
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                _context.ApprovalQueues.Add(new ApprovalQueue
                {
                    ProductId = product.Id,
                    RequestReason = "price over 5000",
                    ActionType = "create"
                });
            }
            else
            {
                _context.Products.Add(product);
            }
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
        }

        // 3. Update Product
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product updatedProduct)
        {
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
                return NotFound();
            decimal previousPrice = existingProduct.Price;
            existingProduct.Name = updatedProduct.Name;
            existingProduct.Price = updatedProduct.Price;
            existingProduct.Status = updatedProduct.Status;
            existingProduct.UpdatedAt = DateTime.Now;
            if (updatedProduct.Price > 5000 || updatedProduct.Price > 1.5M * previousPrice)
            {
                existingProduct.ApprovalStatus = "pending";
                _context.ApprovalQueues.Add(new ApprovalQueue
                {
                    ProductId = existingProduct.Id,
                    RequestReason = "price change",
                    ActionType = "update"
                });
            }
            await _context.SaveChangesAsync();
            return NoContent();
        }
        // 4. Delete Product (Soft delete with approval)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();
            product.ApprovalStatus = "pending";
            _context.ApprovalQueues.Add(new ApprovalQueue
            {
                ProductId = product.Id,
                RequestReason = "delete",
                ActionType = "delete"
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // 5. View Approval Queue
        [HttpGet("approval_queue")]
        public async Task<IActionResult> GetApprovalQueue()
        {
            var queue = await _context.ApprovalQueues
                .Include(q => q.Product)
                .OrderBy(q => q.RequestDate)
                .ToListAsync();
            return Ok(queue);
        }

        // 6. Approve or Reject Requests
        [HttpPost("approve/{id}")]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var request = await _context.ApprovalQueues.FindAsync(id);
            if (request == null)

                return NotFound();
            var product = await _context.Products.FindAsync(request.ProductId);
            product.ApprovalStatus = "approved";
            _context.ApprovalQueues.Remove(request);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpPost("reject/{id}")]
        public async Task<IActionResult> RejectRequest(int id)
        {
            var request = await _context.ApprovalQueues.FindAsync(id);
            if (request == null)
                return NotFound();
            _context.ApprovalQueues.Remove(request);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}