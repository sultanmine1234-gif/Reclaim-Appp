using System.ComponentModel.DataAnnotations;

namespace Reclaim.Api.Dtos
{
    public class UpdateFoundItemDto
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
    }
}
