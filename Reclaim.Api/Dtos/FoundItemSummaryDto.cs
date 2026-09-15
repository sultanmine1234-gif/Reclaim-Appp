namespace Reclaim.Api.Dtos
{
    public class FoundItemSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public int CategoryId { get; set; }
        public string FoundLocation { get; set; } = null!;
        public DateOnly FoundDate { get; set; }
        public string Status { get; set; } = null!;
        public int ClaimCount { get; set; }
    }
}
