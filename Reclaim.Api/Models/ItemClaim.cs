using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Reclaim.Api.Models;

public partial class ItemClaim
{
    [Key]
    public int Id { get; set; }

    public int FoundItemId { get; set; }

    public int ClaimantId { get; set; }

    [StringLength(500)]
    public string OwnershipDescription { get; set; } = null!;

    public DateOnly ClaimedOn { get; set; }

    [StringLength(12)]
    public string Status { get; set; } = null!;

    [ForeignKey("ClaimantId")]
    [InverseProperty("ItemClaims")]
    public virtual Claimant Claimant { get; set; } = null!;

    [ForeignKey("FoundItemId")]
    [InverseProperty("ItemClaims")]
    public virtual FoundItem FoundItem { get; set; } = null!;
}
