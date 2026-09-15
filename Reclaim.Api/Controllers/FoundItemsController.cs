using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reclaim.Api.Data;
using Reclaim.Api.Dtos;
using Reclaim.Api.Models;

namespace Reclaim.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoundItemsController : ControllerBase
    {
        private readonly ReclaimDbContext _context;

        public FoundItemsController(ReclaimDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<FoundItemSummaryDto>>> GetAllFoundItems(
            string? search,
            string? status,
            int? categoryId)
        {
            IQueryable<FoundItem> query = _context.FoundItems;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(item =>
                    item.Name.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(item =>
                    item.Status == status);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(item =>
                    item.CategoryId == categoryId.Value);
            }

            query = query.OrderByDescending(item => item.FoundDate);

            List<FoundItemSummaryDto> summaries = await query
                .Select(item => new FoundItemSummaryDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    CategoryId = item.CategoryId,
                    CategoryName = item.Category.Name,
                    FoundLocation = item.FoundLocation,
                    FoundDate = item.FoundDate,
                    Status = item.Status,
                    ClaimCount = item.ItemClaims.Count
                })
                .ToListAsync();

            return Ok(summaries);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FoundItemDetailsDto>> GetFoundItem(int id)
        {
            FoundItemDetailsDto? foundItem = await _context.FoundItems
                .Where(item => item.Id == id)
                .Select(item => new FoundItemDetailsDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    CategoryId = item.CategoryId,
                    CategoryName = item.Category.Name,
                    FoundLocation = item.FoundLocation,
                    FoundDate = item.FoundDate,
                    Status = item.Status,

                    Claims = item.ItemClaims
                        .Select(claim => new ItemClaimSummaryDto
                        {
                            Id = claim.Id,
                            ClaimantName = claim.Claimant.DisplayName,
                            OwnershipDescription = claim.OwnershipDescription,
                            ClaimedOn = claim.ClaimedOn,
                            Status = claim.Status
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (foundItem == null)
            {
                return NotFound();
            }

            return Ok(foundItem);
        }

        [HttpPost]
        public async Task<ActionResult<FoundItemDetailsDto>> CreateFoundItem(
            CreateFoundItemDto request)
        {
            ItemCategory? category = await _context.ItemCategories
                .Where(category => category.Id == request.CategoryId)
                .FirstOrDefaultAsync();

            if (category == null)
            {
                return BadRequest(
                    "The selected category does not exist.");
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (request.FoundDate > today)
            {
                return BadRequest(
                    "Found date cannot be in the future.");
            }

            FoundItem foundItem = new FoundItem
            {
                Name = request.Name,
                Description = request.Description,
                CategoryId = request.CategoryId,
                FoundLocation = request.FoundLocation,
                FoundDate = request.FoundDate,
                Status = "Available"
            };

            await _context.FoundItems.AddAsync(foundItem);
            await _context.SaveChangesAsync();

            FoundItemDetailsDto result = new FoundItemDetailsDto
            {
                Id = foundItem.Id,
                Name = foundItem.Name,
                Description = foundItem.Description,
                CategoryId = foundItem.CategoryId,
                CategoryName = category.Name,
                FoundLocation = foundItem.FoundLocation,
                FoundDate = foundItem.FoundDate,
                Status = foundItem.Status,
                Claims = new List<ItemClaimSummaryDto>()
            };

            return CreatedAtAction(
                nameof(GetFoundItem),
                new { id = foundItem.Id },
                result
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFoundItem(
            int id,
            UpdateFoundItemDto request)
        {
            FoundItem? foundItem = await _context.FoundItems
                .Where(item => item.Id == id)
                .FirstOrDefaultAsync();

            if (foundItem == null)
            {
                return NotFound();
            }

            ItemCategory? category = await _context.ItemCategories
                .Where(category => category.Id == request.CategoryId)
                .FirstOrDefaultAsync();

            if (category == null)
            {
                return BadRequest(
                    "The selected category does not exist.");
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (request.FoundDate > today)
            {
                return BadRequest(
                    "Found date cannot be in the future.");
            }

            foundItem.Name = request.Name;
            foundItem.Description = request.Description;
            foundItem.CategoryId = request.CategoryId;
            foundItem.FoundLocation = request.FoundLocation;
            foundItem.FoundDate = request.FoundDate;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            FoundItem? foundItem = await _context.FoundItems
                .Where(item => item.Id == id)
                .FirstOrDefaultAsync();

            if (foundItem == null)
            {
                return NotFound();
            }

            bool hasClaims = await _context.ItemClaims
                .AnyAsync(claims => claims.FoundItemId == id);
            
            if (hasClaims)
            {
                return Conflict("An item with claims cannot be deleted.");
            }

            _context.FoundItems.Remove(foundItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}