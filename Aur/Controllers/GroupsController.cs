using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Aur.Data;
using Aur.Models;
using Microsoft.AspNetCore.Identity;
using Aur.ViewModels;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Aur.Controllers
{
    [Authorize(Roles = "Worker,Admin")]
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        public GroupsController(ApplicationDbContext context,UserManager<AppUser> userManager, IAuthorizationService AuthServ)
        {
            _authorizationService = AuthServ;
            _context = context;
            _userManager = userManager;
        }

        // GET: Groups
        public async Task<IActionResult> Index()
        {
           

            return View(await _context.Groups.ToListAsync());
        }

        public async Task<IActionResult> UserSearch(string searchString,int groupid,string currentFilter,int? pageNumber)
        {

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            ViewBag.groupid = groupid;

            var @group = _context.Groups.Include(g => g.GroupMembers).FirstOrDefault(g => g.Id == groupid);

            if (!await GroupAccessAsync(@group)) return NotFound();

            if (!String.IsNullOrEmpty(searchString) && @group != null)
            {
                var users = _context.Users
                    .AsEnumerable()
                    .Where(u => u.Email!="Admin@email.com" && u.Email.Contains(searchString) && @group.GroupMembers.FirstOrDefault(gm => gm.AppUserId == u.Id) == null);

                int pageSize = 3;
                return View("Search",await PaginatedList<AppUser>.CreateAsync(users.AsQueryable<AppUser>(), pageNumber ?? 1, pageSize));

            }

            return View("Search", new List<AppUser>());
        }
        public async Task<IActionResult> Promote(string userid,int groupid, string position)
        {
            
            var @group = _context.Groups.Include(g => g.GroupMembers).FirstOrDefault(g => g.Id == groupid);

            if (!await GroupAccessAsync(@group)) return NotFound();

            var user = _context.Users.FirstOrDefault(u => u.Id == userid);
            if (@group!= null && user != null)
            {
                await _userManager.AddToRoleAsync(user, "Worker");
                @group.GroupMembers.Add(new GroupMember() { AppUserId = userid, GroupId = groupid, Position = position });
                
                try
                {
                    _context.Update(@group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(@group.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                //return RedirectToAction("UserSearch", new { searchString = "", groupid = groupid });
            }
            return RedirectToAction("Details", new { id = groupid });
            return RedirectToAction("UserSearch",new { searchString="",groupid = groupid });
        }
       
        // GET: Groups/Details/5

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var @group = await _context.Groups.Include(g =>g.Mangas).Include(g => g.GroupMembers)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (!await GroupAccessAsync(@group)) return NotFound();

            bool[] tmp = new bool[@group.GroupMembers.Count];
            int count = 0;
            List<AppUser> usersList = new List<AppUser>();

            foreach (var v in @group.GroupMembers)
            {
                if (v.Position=="Manager")
                {
                    tmp[count] = true;
                }
                else tmp[count] = false;
                usersList.Add(_context.Users.FirstOrDefault(u => u.Id == v.AppUserId));
                count++;
            }
            GroupViewModel GVM = new GroupViewModel();
            GVM.groupField = @group;
            GVM.MngWrk = tmp;
            GVM.AppUsers = usersList;
            return View(GVM);
        }
        public async Task<IActionResult> Demote(string userid,int groupid)
        {
            var @group = await _context.Groups.Include(g => g.GroupMembers)
                .FirstOrDefaultAsync(m => m.Id == groupid);

            if (!await GroupAccessAsync(@group)) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userid);

            if (user != null)
            {
                GroupMember GM = @group.GroupMembers.FirstOrDefault(gm=>gm.AppUserId == userid && gm.GroupId == groupid); 
                @group.GroupMembers.Remove(GM);
                
                if (await _userManager.IsInRoleAsync(user, "Worker"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Worker");
                }
                try
                {
                    _context.Update(@group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(@group.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return RedirectToAction("Details", new { id = groupid });
        }
        // GET: Groups/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Groups/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> Create([Bind("Id,Title")] Group @group)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@group);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(@group);
        }

        // GET: Groups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups.FindAsync(id);

            if (!await GroupAccessAsync(@group)) return NotFound();

            return View(@group);
        }

        // POST: Groups/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title")] Group @group)
        {
            if (id != @group.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(@group.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(@group);
        }

        // GET: Groups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups
                .FirstOrDefaultAsync(m => m.Id == id);

            if (!await GroupAccessAsync(@group)) return NotFound();

            return View(@group);
        }

        // POST: Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @group = await _context.Groups.Include(g => g.Mangas).FirstOrDefaultAsync(g => g.Id == id);

            if (!await GroupAccessAsync(@group)) return NotFound();

            _context.Groups.Remove(@group);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        private bool GroupExists(int id)
        {
            return _context.Groups.Any(e => e.Id == id);
        }
        [NonAction]
        private async Task<bool> GroupAccessAsync(Group @group)
        {
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
