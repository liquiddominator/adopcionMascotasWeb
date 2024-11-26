using System;
using System.Collections.Generic;

namespace adopcionMascotasWeb.Models;

public partial class Pet
{
    public int PetId { get; set; }

    public string Name { get; set; } = null!;

    public string Species { get; set; } = null!;

    public string? Breed { get; set; }

    public DateOnly? BirthDate { get; set; }

    public double? Weight { get; set; }

    public int? UserId { get; set; }

    public string? MedicalHistory { get; set; }

    public string AdoptionStatus { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Adoption> Adoptions { get; set; } = new List<Adoption>();

    public virtual User? User { get; set; }
}
