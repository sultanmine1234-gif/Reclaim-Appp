namespace Reclaim.Api.Dtos
{
    public class ItemClaimSummaryDto
    {
        public int Id { get; set; }
        public string ClaimantName { get; set; } = null!;
        public string OwnershipDescription { get; set; } = null!;
        public DateOnly ClaimedOn { get; set; }
        public string Status { get; set; } = null!;
    }
}
