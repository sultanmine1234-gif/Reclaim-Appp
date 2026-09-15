using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Reclaim.Api.Models;

public partial class FoundItem
{
    [Key]
    public int Id { get; set; }

    [StringLength(80)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string Description { get; set; } = null!;

    public int CategoryId { get; set; }

    [StringLength(80)]
    public string FoundLocation { get; set; } = null!;

    public DateOnly FoundDate { get; set; }

    [StringLength(12)]
    public string Status { get; set; } = null!;

    [ForeignKey("CategoryId")]
    [InverseProperty("FoundItems")]
    public virtual ItemCategory Category { get; set; } = null!;

    [InverseProperty("FoundItem")]
    public virtual ICollection<ItemClaim> ItemClaims { get; set; } = new List<ItemClaim>();
}
