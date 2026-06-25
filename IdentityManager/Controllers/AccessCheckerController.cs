using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityManager.Controllers
{
    [Authorize]
    public class AccessCheckerController : Controller
    {
        [AllowAnonymous]
        public IActionResult AllAccess()
        {
            return View();
        }

        public IActionResult AuthorizeAccess()
        {
            return View();
        }

        [Authorize(Roles = $"{SD.Admin},{SD.User}")]
        public IActionResult UserOrAdminRoleAccess()
        {
            return View();
        }

        // Here have and so role base is not possible need policy base
        [Authorize(Policy = "AdminAndUser")]
        public IActionResult UserAndAdminRoleAccess()
        {
            return View();
        }

        //[Authorize(Roles = SD.Admin)]
        [Authorize(Policy = SD.Admin)]
        public IActionResult AdminRoleAccess()
        {
            return View();
        }

        [Authorize(Policy = "AdminRole_CreateClaim")]
        public IActionResult Admin_CreateAccess()
        {
            return View();
        }

        [Authorize(Policy = "AdminRole_CreateEditDeleteClaim")]
        public IActionResult Admin_Create_Edit_DeleteAccess()
        {
            return View();
        }

        [Authorize(Policy = "AdminRole_CreateEditDeleteClaim_ORSuperAdminRole")]
        public IActionResult Admin_Create_Edit_DeleteAccess_OR_SuperAdminRole()
        {
            return View();
        }

        [Authorize(Policy = "AdminWIthMoreTHan1000Days")]
        public IActionResult OnlyBhrugen()
        {
            return View();
        }


        [Authorize(Policy = "FirstnameAuth")]
        public IActionResult FirstnameAuth()
        {
            return View();
        }
    }
}
