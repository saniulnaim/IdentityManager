using IdentityManager.Data;
using IdentityManager.Models;
using IdentityManager.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdentityManager.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var userList = _context.ApplicationUsers.ToList();
            var userRole = _context.UserRoles.ToList();
            var roleList = _context.Roles.ToList();

            foreach (var user in userList)
            {
                var role = userRole.FirstOrDefault(ur => ur.UserId == user.Id);
                if (role == null)
                {
                    user.Role = "none";
                }
                else
                {
                    user.Role = roleList.FirstOrDefault(r => r.Id == role.RoleId)?.Name ?? "unknown";
                }
            }

            return View(userList);
        }

        public async Task<IActionResult> ManageRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user) as List<string>;
            var model = new RolesViewModel()
            {
                User = user
            };

            foreach (var role in _roleManager.Roles)
            {
                RoleSelection roleSelection = new RoleSelection()
                {
                    RoleName = role.Name
                };

                if (userRoles.Any(r => r == role.Name))
                {
                    roleSelection.IsSelected = true;
                }
                model.Roles.Add(roleSelection);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRole(RolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.User.Id);
            if (user == null)
            {
                return NotFound();
            }

            var oldUserRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, oldUserRoles);

            if (!result.Succeeded)
            {
                return View(result);
            }

            result = await _userManager.AddToRolesAsync(user, model.Roles.Where(a => a.IsSelected).Select(_ => _.RoleName));

            if (!result.Succeeded)
            {
                return View(result);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUnlock(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if(user.LockoutEnd != null && user.LockoutEnd > DateTime.Now) 
            {
                // User is locked and will remain locked untill lockout end time clicking on this action will unlock them
                user.LockoutEnd = DateTime.Now;
            }
            else
            {
                user.LockoutEnd = DateTime.Now.AddYears(1000);
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId) 
        {
            var user = _context.ApplicationUsers.FirstOrDefault(u=> u.Id == userId);
            if(user == null)
            {
                return NotFound();
            }

            _context.ApplicationUsers.Remove(user);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> ManageUserClaim(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var userClaims = await _userManager.GetClaimsAsync(user);
            var model = new ClaimsViewModel()
            {
                User = user
            };

            foreach (var claim in ClaimStore.claims)
            {
                ClaimsSelection userClaim = new ClaimsSelection()
                {
                    ClaimType = claim.Type
                };

                if (userClaims.Any(r => r.Type == claim.Type))
                {
                    userClaim.IsSelected = true;
                }
                model.Claims.Add(userClaim);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserClaim(ClaimsViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.User.Id);
            if (user == null)
            {
                return NotFound();
            }

            var oldClaims = await _userManager.GetClaimsAsync(user);
            var result = await _userManager.RemoveClaimsAsync(user, oldClaims);

            if (!result.Succeeded)
            {
                return View(model);
            }

            result = await _userManager.AddClaimsAsync(user, model.Claims.Where(a => a.IsSelected).Select(_ => new Claim(_.ClaimType, _.IsSelected.ToString())));

            if (!result.Succeeded)
            {
                return View(result);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

