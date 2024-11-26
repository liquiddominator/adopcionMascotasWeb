using System;
using System.Collections.Generic;

namespace adopcionMascotasWeb.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public string Items { get; set; } = null!;

    public double Total { get; set; }

    public string Status { get; set; } = null!;

    public string? ShippingAddress { get; set; }

    public string? PaymentMethod { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
