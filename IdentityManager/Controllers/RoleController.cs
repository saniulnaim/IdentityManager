using IdentityManager.Data;
using IdentityManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityManager.Controllers
{
    public class RoleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var roleList = _context.Roles.ToList();

            return View(roleList);
        }

        [HttpGet]
        public IActionResult Upsert(string roleId)
        {
            if(string.IsNullOrEmpty(roleId))
            {
                return View(new IdentityRole());
            }
            else
            {
                var role = _context.Roles.FirstOrDefault(r => r.Id == roleId);
                if (role == null)
                {
                    return NotFound();
                }
                return View(role);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>  Upsert(IdentityRole role)
        {
            if (ModelState.IsValid)
            {
                if(await _roleManager.RoleExistsAsync(role.Name))
                {
                    ModelState.AddModelError("Name", "Role already exists.");
                    return View(role);
                }

                if(string.IsNullOrEmpty(role.NormalizedName))
                {
                    await _roleManager.CreateAsync(role);
                }
                else
                {
                    var existingRole = await _roleManager.FindByIdAsync(role.Id);
                    if (existingRole == null)
                    {
                        return NotFound();
                    }
                    existingRole.Name = role.Name;
                    existingRole.NormalizedName = role.Name.ToUpper();

                    await _roleManager.UpdateAsync(existingRole);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound();
            }

            var userRolseForThisRole = await _userManager.GetUsersInRoleAsync(role.Name);
            if(userRolseForThisRole == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Error deleting role.");
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
