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
    public class AdoptionsController : Controller
    {
        private readonly AdopcionContext _context;

        public AdoptionsController(AdopcionContext context)
        {
            _context = context;
        }

        // GET: Adoptions - Admin ve todas, usuario ve solo las suyas
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var adopciones = _context.Adoptions
                .Include(a => a.Pet)
                .Include(a => a.User)
                .AsQueryable();

            if (!User.IsInRole("admin"))
            {
                var userId = int.Parse(User.FindFirst("UserId").Value);
                adopciones = adopciones.Where(a => a.UserId == userId);
            }

            return View(await adopciones.OrderByDescending(a => a.CreatedAt).ToListAsync());
        }

        // GET: Adoptions/Details/5 - Admin ve todas, usuario ve solo las suyas
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var adoption = await _context.Adoptions
                .Include(a => a.Pet)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.AdoptionId == id);

            if (adoption == null)
            {
                return NotFound();
            }

            // Verificar si el usuario actual puede ver esta adopción
            if (!User.IsInRole("admin") && adoption.UserId != int.Parse(User.FindFirst("UserId").Value))
            {
                return Forbid();
            }

            return View(adoption);
        }

        // POST: Adoptions/UpdateStatus/5 - Solo admin puede actualizar estado
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var adoption = await _context.Adoptions
                .Include(a => a.Pet)
                .FirstOrDefaultAsync(a => a.AdoptionId == id);

            if (adoption == null)
            {
                return NotFound();
            }

            adoption.Status = status;
            adoption.UpdatedAt = DateTime.Now;

            // Si se aprueba la adopción
            if (status == "aprobada")
            {
                adoption.AdoptionDate = DateOnly.FromDateTime(DateTime.Now); // Convertir DateTime a DateOnly
                adoption.Pet.AdoptionStatus = "adoptada";
                adoption.Pet.UserId = adoption.UserId;

                // Rechazar otras solicitudes pendientes para esta mascota
                var otherAdoptions = await _context.Adoptions
                    .Where(a => a.PetId == adoption.PetId && a.AdoptionId != adoption.AdoptionId)
                    .ToListAsync();

                foreach (var otherAdoption in otherAdoptions)
                {
                    otherAdoption.Status = "rechazada";
                    otherAdoption.UpdatedAt = DateTime.Now;
                }
            }
            // Si se rechaza la adopción y no hay otras solicitudes pendientes
            else if (status == "rechazada")
            {
                var hasPendingAdoptions = await _context.Adoptions
                    .AnyAsync(a => a.PetId == adoption.PetId &&
                                 a.AdoptionId != adoption.AdoptionId &&
                                 a.Status == "pendiente");

                if (!hasPendingAdoptions)
                {
                    adoption.Pet.AdoptionStatus = "disponible";
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = adoption.AdoptionId });
        }

        // POST: Adoptions/Cancel/5 - Usuario puede cancelar su propia solicitud pendiente
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Cancel(int id)
        {
            var adoption = await _context.Adoptions
                .Include(a => a.Pet)
                .FirstOrDefaultAsync(a => a.AdoptionId == id);

            if (adoption == null)
            {
                return NotFound();
            }

            // Verificar si el usuario actual es el dueño de la adopción
            if (adoption.UserId != int.Parse(User.FindFirst("UserId").Value))
            {
                return Forbid();
            }

            // Solo se pueden cancelar adopciones pendientes
            if (adoption.Status != "pendiente")
            {
                TempData["Error"] = "Solo se pueden cancelar solicitudes pendientes.";
                return RedirectToAction(nameof(Details), new { id });
            }

            adoption.Status = "cancelada";
            adoption.UpdatedAt = DateTime.Now;

            // Si no hay otras solicitudes pendientes, la mascota vuelve a estar disponible
            var hasPendingAdoptions = await _context.Adoptions
                .AnyAsync(a => a.PetId == adoption.PetId &&
                             a.AdoptionId != adoption.AdoptionId &&
                             a.Status == "pendiente");

            if (!hasPendingAdoptions)
            {
                adoption.Pet.AdoptionStatus = "disponible";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
