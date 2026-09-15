using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Reclaim.Api.Models;

[Index("Name", Name = "UQ_ItemCategories_Name", IsUnique = true)]
public partial class ItemCategory
{
    [Key]
    public int Id { get; set; }

    [StringLength(40)]
    public string Name { get; set; } = null!;

    [InverseProperty("Category")]
    public virtual ICollection<FoundItem> FoundItems { get; set; } = new List<FoundItem>();
}
