using System;
using System.Collections.Generic;

namespace adopcionMascotasWeb.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public double Price { get; set; }

    public string Category { get; set; } = null!;

    public int Stock { get; set; }

    public string? Images { get; set; }

    public string? Specifications { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
