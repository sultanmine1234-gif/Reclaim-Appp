using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace Reclaim.Api.Dtos
{
    public class CreateFoundItemDto
    {
        [Required]
        [StringLength(80)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(80)]
        public string Description { get; set; } = null!;

        [Required]
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(80)]
        public string FoundLocation { get; set; } = null!;

        public DateOnly FoundDate { get; set; } 

        public int FoundItemId { get; set; }
        public int ClaimantId { get; set; }

        [Required]
        [StringLength(500)]
        public string OwnerShipDescription { get; set; } = null!;
    }
}
