using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Aur.Models;
using Microsoft.AspNetCore.Identity;
using Aur.Data;
using Aur.Models;
using Aur.ViewModels;
using System.IO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Authorization;


namespace Aur.Controllers
{
    public class PartsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuthorizationService _authorizationService;

        public PartsController(ApplicationDbContext context, UserManager<AppUser> um, RoleManager<IdentityRole> rm, IAuthorizationService AuthServ)
        {
            _roleManager = rm;
            _userManager = um;
            _context = context;
            _authorizationService = AuthServ;
        }

        // GET: Parts
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Index()
        {
            /*
            IdentityRole mn = new IdentityRole();
            mn.Name = "Manager";
            await _roleManager.CreateAsync(mn);
            */
            if (_context.Roles.FirstOrDefault(r => r.Name != "Admin") == null)
            {
                IdentityRole ad = new IdentityRole();
                ad.Name = "Admin";
                await _roleManager.CreateAsync(ad);

                IdentityRole wr = new IdentityRole();
                wr.Name = "Worker";

                await _roleManager.CreateAsync(wr);

                var admin = await _userManager.FindByEmailAsync("Admin@email.com");

                var res2 = await _userManager.AddToRoleAsync(admin, "Admin");

            }
            else Console.WriteLine("ADMIN WAS CREATED");
            
            return View(await _context.Parts.Include(p =>p.Manga).ToListAsync());
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var part = await _context.Parts.FirstOrDefaultAsync(m => m.Id == id);
            _context.Entry(part).Collection(p => p.Images).Load();
            if (part == null)
            {
                return NotFound();
            }

            return View(part);
        }

        public IActionResult Create(int mangaid)
        {

            CreatePartModel CPM = new CreatePartModel();
            CPM.mangaid = mangaid;
            return View(CPM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("Create")]
        public async Task<IActionResult> CreateConfirmed(CreatePartModel createPartModel)
        {
            if (ModelState.IsValid)
            {
                Part newPart = new Part();

                newPart.Images = new List<Image>();
                newPart.Pages = HttpContext.Session.GetInt32("imgCount") ?? 0;
                newPart.Title = createPartModel.Title;

                var Manga = await _context.Mangas.FirstOrDefaultAsync(p => p.Id == createPartModel.mangaid);

                if (Manga == null) 
                    return NotFound();

                newPart.Manga = Manga;
                _context.Add(newPart);

                for (int i = 0; i < HttpContext.Session.GetInt32("imgCount"); i++)
                {

                    Image temp = new Image() { ImageData = HttpContext.Session.Get("img" + i.ToString()), Part = newPart };
                    _context.Images.Add(temp);
                    HttpContext.Session.Remove("img" + i.ToString());
                }

                HttpContext.Session.Remove("imgCount");

                await _context.SaveChangesAsync();

                return RedirectToAction("Details","Mangas",new { id = createPartModel.mangaid });
            }
            return RedirectToAction("Index", "Parts");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("AddFiles")]
        public IActionResult AddFiles(CreatePartModel part, string file_action, int partId)
        {
            if (file_action != "Edit" && file_action != "Create")
                return NotFound();

            if (part.UploadImages == null)
                return RedirectToAction(file_action, "Parts");

            List<byte[]> uploadedFiles = new List<byte[]>();

            int count = 0;
            int? ImgCount = HttpContext.Session.GetInt32("imgCount");

            if (ImgCount != null)
                for (int i = 0; i < ImgCount; i++)
                {
                    HttpContext.Session.Remove("img" + i.ToString());
                }

            foreach (var img in part.UploadImages)
            {
                byte[] imageData = null;

                using (var binaryReader = new BinaryReader(img.OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)img.Length);
                }

                HttpContext.Session.Set("img" + count.ToString(), imageData);

                count++;
                uploadedFiles.Add(imageData);
            }

            HttpContext.Session.SetInt32("imgCount", count);
            part.UploadImages = null;

            part.Images = uploadedFiles;
            part.CheckBox = new List<bool>();
            if (file_action == "Edit")
            {
                ViewBag.partId = partId;

                part.Title = _context.Parts.FirstOrDefault(p => p.Id == partId).Title;
            }
            return View(file_action, part);

        }
        // GET: Parts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var part = await _context.Parts.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);

            if (part == null)
            {
                return NotFound();
            }
            CreatePartModel CPM = new CreatePartModel();
            CPM.Images = new List<byte[]>();
            int count = 0;
            foreach (var v in part.Images)
            {
                HttpContext.Session.Set("img" + count.ToString(), v.ImageData);
                CPM.Images.Add(v.ImageData);
                count++;
            }
            HttpContext.Session.SetInt32("imgCount", count);
            CPM.Title = part.Title;
            ViewBag.partId = id;
            CPM.mangaid = part.MangaId??0;
            return View(CPM);
        }

        // POST: Parts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int partId, string[] checkboxs, CreatePartModel CPM)
        {
            if (ModelState.IsValid)
            {
                CPM.Images = new List<byte[]>();
                ViewBag.partId = partId;

                if (checkboxs != null)
                {

                    Part part = _context.Parts.Include(p => p.Images).FirstOrDefault(p => p.Id == partId);

                    part.Images.Clear();

                    int? ImgCount = HttpContext.Session.GetInt32("imgCount");

                    int count = 0;

                    for (int i = 0, j = 0; i < ImgCount && j < checkboxs.Length; i++)
                    {
                        if (i.ToString() == checkboxs[j])
                        {
                            part.Images.Add(new Image() { Part = part, ImageData = HttpContext.Session.Get("img" + i.ToString()) });
                            count++;
                            j++;
                        }
                        HttpContext.Session.Remove("Img" + i.ToString());
                    }

                    part.Pages = count;

                    HttpContext.Session.Remove("imgCount");

                    part.Title = CPM.Title;
                    {
                        try
                        {
                            _context.Update(part);
                            await _context.SaveChangesAsync();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!PartExists(part.Id))
                            {
                                return NotFound();
                            }
                            else
                            {
                                throw;
                            }
                        }

                        return RedirectToAction("Details", "Mangas", new { id = part.MangaId });
                        //return RedirectToAction(nameof(Index));
                    }
                    return View(CPM);
                }
                return View(CPM);
            }

            return RedirectToAction("Index", "Parts");
        }

        // GET: Parts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var part = await _context.Parts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (part == null)
            {
                return NotFound();
            }

            return View(part);
        }

        // POST: Parts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            var parts = _context.Parts.Include(p => p.Images);

            Part part = parts.Where(p => p.Id == id).FirstOrDefault();
            foreach (var v in part.Images)
            {
                var t = _context.Images.Remove(v);
            }

            _context.Parts.Remove(part);

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Mangas", new { id = part.MangaId });
        }

        private bool PartExists(int id)
        {
            return _context.Parts.Any(e => e.Id == id);
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
