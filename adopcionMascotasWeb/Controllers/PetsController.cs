using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using adopcionMascotasWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace adopcionMascotasWeb.Controllers
{
    public class PetsController : Controller
    {
        private readonly AdopcionContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PetsController(AdopcionContext context, IWebHostEnvironment webHostEnvironment)
        {//
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        // GET: Pets - Todos pueden ver las mascotas
        public async Task<IActionResult> Index()
        {
            var pets = await _context.Pets
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(pets);
        }

        // GET: Pets/Details/5 - Todos pueden ver detalles
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _context.Pets
                .Include(p => p.User)
                .Include(p => p.Adoptions)
                .FirstOrDefaultAsync(m => m.PetId == id);

            if (pet == null)
            {
                return NotFound();
            }

            return View(pet);
        }

        // GET: Pets/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            // Se muestra el texto descriptivo en lugar del valor
            ViewData["Species"] = new List<SelectListItem>
            {
                new SelectListItem { Value = "perro", Text = "Perro" },
                new SelectListItem { Value = "gato", Text = "Gato" },
                new SelectListItem { Value = "ave", Text = "Ave" },
                new SelectListItem { Value = "otro", Text = "Otro" }
            };

            return View();
        }

        // POST: Pets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([Bind("Name,Species,Breed,BirthDate,Weight,MedicalHistory")] Pet pet, IFormFile image)
        {
            if (true)
            {
                pet.CreatedAt = DateTime.Now;
                pet.AdoptionStatus = "disponible";
                pet.UserId = null;

                if (pet.BirthDate != null)
                {
                    pet.BirthDate = DateOnly.FromDateTime(DateTime.Parse(pet.BirthDate.ToString()));
                }

                // Procesar imagen si se proporcionó una
                if (image != null && image.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "pets");
                    // Crear el directorio si no existe
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Crear nombre único para la imagen
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Guardar la imagen
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    pet.ImageUrl = "/images/pets/" + uniqueFileName;
                }

                _context.Add(pet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Species"] = new List<SelectListItem>
            {
                new SelectListItem { Value = "perro", Text = "Perro" },
                new SelectListItem { Value = "gato", Text = "Gato" },
                new SelectListItem { Value = "ave", Text = "Ave" },
                new SelectListItem { Value = "otro", Text = "Otro" }
            };

            return View(pet);
        }

        // GET: Pets/Edit/5 - Solo admin puede editar
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _context.Pets.FindAsync(id);
            if (pet == null)
            {
                return NotFound();
            }

            ViewData["Species"] = new SelectList(new[] { "perro", "gato", "ave", "otro" }, pet.Species);
            ViewData["AdoptionStatus"] = new SelectList(new[] { "disponible", "adoptada", "en proceso" }, pet.AdoptionStatus);
            return View(pet);
        }

        // POST: Pets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id, [Bind("PetId,Name,Species,Breed,BirthDate,Weight,MedicalHistory,AdoptionStatus,ImageUrl")] Pet pet, IFormFile image)
        {
            if (id != pet.PetId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Procesar nueva imagen si se proporcionó una
                    if (image != null && image.Length > 0)
                    {
                        // Eliminar imagen anterior si existe
                        if (!string.IsNullOrEmpty(pet.ImageUrl))
                        {
                            string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, pet.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "pets");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }

                        pet.ImageUrl = "/images/pets/" + uniqueFileName;
                    }

                    pet.UpdatedAt = DateTime.Now;
                    _context.Update(pet);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PetExists(pet.PetId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["Species"] = new SelectList(new[] { "perro", "gato", "ave", "otro" }, pet.Species);
            ViewData["AdoptionStatus"] = new SelectList(new[] { "disponible", "adoptada", "en proceso" }, pet.AdoptionStatus);
            return View(pet);
        }

        // GET: Pets/Delete/5 - Solo admin puede eliminar
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _context.Pets
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PetId == id);
            if (pet == null)
            {
                return NotFound();
            }

            return View(pet);
        }

        // POST: Pets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet != null)
            {
                if (pet.AdoptionStatus == "disponible")
                {
                    // Eliminar la imagen si existe
                    if (!string.IsNullOrEmpty(pet.ImageUrl))
                    {
                        string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, pet.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    _context.Pets.Remove(pet);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "No se puede eliminar una mascota que está adoptada o en proceso de adopción.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Pets/RequestAdoption/5 - Usuarios autenticados pueden solicitar adopción
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> RequestAdoption(int id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null || pet.AdoptionStatus != "disponible")
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst("UserId").Value);

            // Verificar si el usuario ya tiene una solicitud pendiente para esta mascota
            var existingAdoption = await _context.Adoptions
                .AnyAsync(a => a.PetId == id && a.UserId == userId && a.Status != "rechazada");

            if (existingAdoption)
            {
                TempData["Error"] = "Ya tienes una solicitud de adopción para esta mascota.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var adoption = new Adoption
            {
                UserId = userId,
                PetId = id,
                Status = "pendiente",
                CreatedAt = DateTime.Now
            };

            pet.AdoptionStatus = "en proceso";

            _context.Adoptions.Add(adoption);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Tu solicitud de adopción ha sido enviada.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private bool PetExists(int id)
        {
            return _context.Pets.Any(e => e.PetId == id);
        }
    }
}
