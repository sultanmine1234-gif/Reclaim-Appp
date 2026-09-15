using System.ComponentModel.DataAnnotations;

namespace Reclaim.Api.Dtos
{
    public class CreateItemClaimDto
    {
        public int FoundItemId { get; set; }

        public int ClaimantId { get; set; }

        [Required]
        [StringLength(500)]
        public string OwnershipDescription { get; set; } = null!;
    }
}