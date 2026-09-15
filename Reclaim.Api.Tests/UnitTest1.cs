using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reclaim.Api.Controllers;
using Reclaim.Api.Data;
using Reclaim.Api.Dtos;
using Reclaim.Api.Models;

namespace Reclaim.Api.Tests
{
    public class ItemClaimsControllerTests
    {
        private static async Task<ReclaimDbContext> CreateContextAsync()
        {
            DbContextOptions<ReclaimDbContext> options =
                new DbContextOptionsBuilder<ReclaimDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options;

            ReclaimDbContext context = new ReclaimDbContext(options);

            ItemCategory category = new ItemCategory
            {
                Id = 1,
                Name = "Electronics"
            };

            FoundItem foundItem = new FoundItem
            {
                Id = 1,
                Name = "Blue Calculator",
                Description = "Calculator with a star sticker.",
                CategoryId = 1,
                Category = category,
                FoundLocation = "Science Lab",
                FoundDate = DateOnly.FromDateTime(DateTime.Today),
                Status = "Available"
            };

            Claimant firstClaimant = new Claimant
            {
                Id = 1,
                DisplayName = "Ali"
            };

            Claimant secondClaimant = new Claimant
            {
                Id = 2,
                DisplayName = "Mariam"
            };

            context.ItemCategories.Add(category);
            context.FoundItems.Add(foundItem);
            context.Claimants.AddRange(firstClaimant, secondClaimant);

            await context.SaveChangesAsync();

            return context;
        }

        private static ItemClaim CreateClaim(
            int id,
            int claimantId,
            string status)
        {
            return new ItemClaim
            {
                Id = id,
                FoundItemId = 1,
                ClaimantId = claimantId,
                OwnershipDescription = "Test ownership description.",
                ClaimedOn = DateOnly.FromDateTime(DateTime.Today),
                Status = status
            };
        }

        [Fact]
        public async Task AddItemClaim_ValidRequest_CreatesPendingClaim()
        {
            // Arrange
            await using ReclaimDbContext context =
                await CreateContextAsync();

            ItemClaimsController controller =
                new ItemClaimsController(context);

            CreateItemClaimDto request = new CreateItemClaimDto
            {
                FoundItemId = 1,
                ClaimantId = 1,
                OwnershipDescription = "My calculator has a star sticker."
            };

            // Act
            ActionResult<ItemClaimSummaryDto> response =
                await controller.AddItemClaim(request);

            // Assert
            CreatedAtActionResult createdResult =
                Assert.IsType<CreatedAtActionResult>(response.Result);

            ItemClaimSummaryDto result =
                Assert.IsType<ItemClaimSummaryDto>(createdResult.Value);

            Assert.Equal("Pending", result.Status);
            Assert.Equal("Ali", result.ClaimantName);
            Assert.Single(context.ItemClaims);
        }

        [Fact]
        public async Task AddItemClaim_MissingFoundItem_ReturnsBadRequest()
        {
            // Arrange
            await using ReclaimDbContext context =
                await CreateContextAsync();

            ItemClaimsController controller =
                new ItemClaimsController(context);

            CreateItemClaimDto request = new CreateItemClaimDto
            {
                FoundItemId = 999,
                ClaimantId = 1,
                OwnershipDescription = "Test description."
            };

            // Act
            ActionResult<ItemClaimSummaryDto> response =
                await controller.AddItemClaim(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
            Assert.Empty(context.ItemClaims);
        }

        [Fact]
        public async Task AddItemClaim_UnavailableItem_ReturnsConflict()
        {
            // Arrange
            await using ReclaimDbContext context =
                await CreateContextAsync();

            FoundItem foundItem =
                (await context.FoundItems.FindAsync(1))!;

            foundItem.Status = "Reserved";
            await context.SaveChangesAsync();

            ItemClaimsController controller =
                new ItemClaimsController(context);

            CreateItemClaimDto request = new CreateItemClaimDto
            {
                FoundItemId = 1,
                ClaimantId = 1,
                OwnershipDescription = "Test description."
            };

            // Act
            ActionResult<ItemClaimSummaryDto> response =
                await controller.AddItemClaim(request);

            // Assert
            Assert.IsType<ConflictObjectResult>(response.Result);
            Assert.Empty(context.ItemClaims);
        }

        [Fact]
        public async Task RejectClaim_PendingClaim_ChangesStatusToRejected()
        {
            // Arrange
            await using ReclaimDbContext context =
                await CreateContextAsync();

            context.ItemClaims.Add(
                CreateClaim(1, 1, "Pending"));

            await context.SaveChangesAsync();

            ItemClaimsController controller =
                new ItemClaimsController(context);

            // Act
            IActionResult response =
                await controller.RejectClaim(1);

            // Assert
            Assert.IsType<NoContentResult>(response);

            ItemClaim claim =
                (await context.ItemClaims.FindAsync(1))!;

            Assert.Equal("Rejected", claim.Status);
        }

        [Fact]
        public async Task ApproveClaim_PendingClaim_UpdatesWholeWorkflow()
        {
            // Arrange
            await using ReclaimDbContext context =
                await CreateContextAsync();

            context.ItemClaims.AddRange(
                CreateClaim(1, 1, "Pending"),
                CreateClaim(2, 2, "Pending")
            );

            await context.SaveChangesAsync();

            ItemClaimsController controller =
                new ItemClaimsController(context);

            // Act
            IActionResult response =
                await controller.ApproveClaim(1);

            // Assert
            Assert.IsType<NoContentResult>(response);

            ItemClaim approvedClaim =
                (await context.ItemClaims.FindAsync(1))!;

            ItemClaim otherClaim =
                (await context.ItemClaims.FindAsync(2))!;

            FoundItem foundItem =
                (await context.FoundItems.FindAsync(1))!;

            Assert.Equal("Approved", approvedClaim.Status);
            Assert.Equal("Rejected", otherClaim.Status);
            Assert.Equal("Reserved", foundItem.Status);
        }

        [Fact]
        public async Task CompleteClaim_ApprovedClaim_CompletesHandover()
        {
            // Arrange
            await using ReclaimDbContext context =
                await CreateContextAsync();

            FoundItem foundItem =
                (await context.FoundItems.FindAsync(1))!;

            foundItem.Status = "Reserved";

            context.ItemClaims.Add(
                CreateClaim(1, 1, "Approved"));

            await context.SaveChangesAsync();

            ItemClaimsController controller =
                new ItemClaimsController(context);

            // Act
            IActionResult response =
                await controller.CompleteClaim(1);

            // Assert
            Assert.IsType<NoContentResult>(response);

            ItemClaim claim =
                (await context.ItemClaims.FindAsync(1))!;

            Assert.Equal("Completed", claim.Status);
            Assert.Equal("Returned", foundItem.Status);
        }
    }
}