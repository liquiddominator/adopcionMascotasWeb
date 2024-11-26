using System;
using System.Collections.Generic;

namespace adopcionMascotasWeb.Models;

public partial class Adoption
{
    public int AdoptionId { get; set; }

    public int UserId { get; set; }

    public int PetId { get; set; }

    public string Status { get; set; } = null!;

    public DateOnly? AdoptionDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Pet Pet { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
