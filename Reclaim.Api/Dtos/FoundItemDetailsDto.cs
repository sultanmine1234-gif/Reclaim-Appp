namespace Reclaim.Api.Dtos
{
    public class FoundItemDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string FoundLocation { get; set; } = null!;
        public DateOnly FoundDate { get; set; }
        public string Status { get; set; } = null!;
        public List<ItemClaimSummaryDto> Claims { get; set; } = new List<ItemClaimSummaryDto>();

    }
}
