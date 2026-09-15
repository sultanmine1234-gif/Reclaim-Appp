using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reclaim.Api.Data;
using Reclaim.Api.Dtos;
using Reclaim.Api.Models;

namespace Reclaim.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemClaimsController : ControllerBase
    {
        private readonly ReclaimDbContext _context;

        public ItemClaimsController(ReclaimDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemClaimSummaryDto>> GetItemClaim(int id)
        {
            ItemClaimSummaryDto? claim = await _context.ItemClaims
                .Where(claim => claim.Id == id)
                .Select(claim => new ItemClaimSummaryDto
                {
                    Id = claim.Id,
                    ClaimantName = claim.Claimant.DisplayName,
                    OwnershipDescription = claim.OwnershipDescription,
                    ClaimedOn = claim.ClaimedOn,
                    Status = claim.Status
                })
                .FirstOrDefaultAsync();

            if (claim == null)
            {
                return NotFound();
            }

            return Ok(claim);

        }

        [HttpPost]
        public async Task<ActionResult<ItemClaimSummaryDto>> AddItemClaim(
            CreateItemClaimDto request)
        {
            FoundItem? foundItem = await _context.FoundItems
                .FirstOrDefaultAsync(item => item.Id == request.FoundItemId);

            if (foundItem == null)
            {
                return BadRequest("The found item does not exist.");
            }

            Claimant? claimant = await _context.Claimants
                .FirstOrDefaultAsync(claimant => claimant.Id == request.ClaimantId);

            if (claimant == null)
            {
                return BadRequest("The claimant does not exist.");
            }

            if (foundItem.Status != "Available")
            {
                return Conflict("The item is not available.");
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            ItemClaim itemClaim = new ItemClaim
            {
                FoundItemId = request.FoundItemId,
                ClaimantId = request.ClaimantId,
                OwnershipDescription = request.OwnershipDescription,
                ClaimedOn = today,
                Status = "Pending"
            };

            await _context.ItemClaims.AddAsync(itemClaim);
            await _context.SaveChangesAsync();

            ItemClaimSummaryDto result = new ItemClaimSummaryDto
            {
                Id = itemClaim.Id,
                ClaimantName = claimant.DisplayName,
                OwnershipDescription = itemClaim.OwnershipDescription,
                ClaimedOn = itemClaim.ClaimedOn,
                Status = itemClaim.Status
            };

            return CreatedAtAction(
                nameof(GetItemClaim),
                new { id = itemClaim.Id },
                result
            );
        }

        [HttpPut("{id}/reject")]
        public async Task<ActionResult> RejectClaim(int id)
        {
            ItemClaim? claim = await _context.ItemClaims
                .FirstOrDefaultAsync(claim => claim.Id == id);

            if (claim == null)
            {
                return NotFound();
            }

            if (claim.Status != "Pending")
            {
                return Conflict("Only pending claims can be rejected.");

            }

            claim.Status = "Rejected";

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}/approve")]
        public async Task<ActionResult> ApproveClaim(int id)
        {
            ItemClaim? claim = await _context.ItemClaims
                .FirstOrDefaultAsync(claim => claim.Id == id);

            if (claim == null)
            {
                return NotFound();
            }

            if (claim.Status != "Pending")
            {
                return Conflict("Only pending claims can be approved.");
            }

            FoundItem? foundItem = await _context.FoundItems
                .FirstOrDefaultAsync(item => item.Id == claim.FoundItemId);

            if (foundItem == null)
            {
                return NotFound("The related found item does not exist.");
            }

            if (foundItem.Status != "Available")
            {
                return Conflict("The item is not available.");
            }

            claim.Status = "Approved";
            foundItem.Status = "Reserved";

            List<ItemClaim> otherPendingClaims = await _context.ItemClaims
                .Where(otherClaim =>
                    otherClaim.FoundItemId == claim.FoundItemId &&
                    otherClaim.Id != claim.Id &&
                    otherClaim.Status == "Pending")
                .ToListAsync();

            foreach (ItemClaim otherPendingClaim in otherPendingClaims)
            {
                otherPendingClaim.Status = "Rejected";
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteClaim(int id)
        {
            ItemClaim? claim = await _context.ItemClaims
                .FirstOrDefaultAsync(claim => claim.Id == id);

            if (claim == null)
            {
                return NotFound();
            }

            if (claim.Status != "Approved")
            {
                return Conflict("Only approved claims can be completed.");
            }

            FoundItem? foundItem = await _context.FoundItems
                .FirstOrDefaultAsync(item => item.Id == claim.FoundItemId);

            if (foundItem == null)
            {
                return NotFound("The related found item does not exist.");
            }

            if (foundItem.Status != "Reserved")
            {
                return Conflict("The related item is not reserved.");
            }

            claim.Status = "Completed";
            foundItem.Status = "Returned";

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
