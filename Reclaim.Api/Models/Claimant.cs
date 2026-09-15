using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Reclaim.Api.Models;

public partial class Claimant
{
    [Key]
    public int Id { get; set; }

    [StringLength(40)]
    public string DisplayName { get; set; } = null!;

    [InverseProperty("Claimant")]
    public virtual ICollection<ItemClaim> ItemClaims { get; set; } = new List<ItemClaim>();
}
