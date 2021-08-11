using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Aur.Data;
using Aur.Models;
using System.Web;

using Microsoft.AspNetCore.Authorization;

namespace Aur.Controllers
{
    [Authorize(Roles = "Worker,Admin")]
    public class MangasController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IAuthorizationService _authorizationService;
        public MangasController(ApplicationDbContext context, IAuthorizationService AuthServ)
        {
            _context = context;
            _authorizationService =  AuthServ;
        }

        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Mangas.ToListAsync());
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manga = await _context.Mangas.Include(p=>p.Parts)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (!await GroupAccessAsync(manga)) return NotFound();
            
            return View(manga);
        }
        public async Task<IActionResult> Create(int? groupid)
        {
            if(!(await _authorizationService.AuthorizeAsync(User, _context.Groups.FirstOrDefault(g => g.Id == groupid), "GroupMember")).Succeeded)
                return NotFound();

            ViewBag.groupid = groupid;
            return View();
        }

        // POST: Mangas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Desc,Price,Genre,Count,PriceStart,GroupId")] Manga manga)
        {
            if (ModelState.IsValid)
            {
                if (!await GroupAccessAsync(manga)) return NotFound();

                _context.Add(manga);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details","Groups",new { id = manga.GroupId});
            }
            return View(manga);
        }

        // GET: Mangas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manga = await _context.Mangas.FindAsync(id);

            if (!await GroupAccessAsync(manga)) return NotFound();

            return View(manga);
        }

        // POST: Mangas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Desc,Price,Genre,Count,PriceStart,GroupId")] Manga manga)
        {
            if (id != manga.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                if (!await GroupAccessAsync(manga)) return NotFound();
                try
                {
                    _context.Update(manga);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MangaExists(manga.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details","Groups", new { id = manga.GroupId});
            }
            return View("Index", "Groups");
        }

        // GET: Mangas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manga = await _context.Mangas
                .FirstOrDefaultAsync(m => m.Id == id);

            if (!await GroupAccessAsync(manga)) return NotFound();

            return View(manga);
        }

        // POST: Mangas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var manga = await _context.Mangas.FindAsync(id);

            if (!await GroupAccessAsync(manga)) return NotFound();

            _context.Mangas.Remove(manga);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Groups", new { id = manga.GroupId });
        }

        private bool MangaExists(int id)
        {
            return _context.Mangas.Any(e => e.Id == id);
        }

        [NonAction]
        private async Task<bool> GroupAccessAsync(Manga manga)
        {
            if (manga == null) 
                return false;
            _context.Entry(manga).Reference(m => m.Group).Load();
            var @group = manga.Group;
            if (@group == null)
            {
                return false;
            }
            var authorizationResult = await _authorizationService
                    .AuthorizeAsync(User, @group, "GroupMember");

            return authorizationResult.Succeeded;
        }
    }
}
