using IdentityManager.Data;
using IdentityManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityManager.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var userList = _context.ApplicationUsers.ToList();
            var userRole = _context.UserRoles.ToList();
            var roleList = _context.Roles.ToList();

            foreach (var user in userList)
            {
                var role = userRole.FirstOrDefault(ur => ur.UserId == user.Id);
                if(role == null)
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
    }
}
