using IdentityManager.Models;
using IdentityManager.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace IdentityManager.Controllers
{
    public class AccountController : Controller
    {
        //private readonly UserManager<IdentityUser> _userManager;
        //private readonly SignInManager<IdentityUser> _signInManager;

        //public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        //{
        //    _userManager = userManager;
        //    _signInManager = signInManager;
        //}

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UrlEncoder _urlEncoder;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, UrlEncoder urlEncoder)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _urlEncoder = urlEncoder;
        }

        public async Task<IActionResult> Register(string returnUrl = null)
        {
            //if (_roleManager.RoleExistsAsync(SD.Admin).GetAwaiter().GetResult())
            //{
            //    await _roleManager.CreateAsync(new IdentityRole(SD.Admin));
            //    await _roleManager.CreateAsync(new IdentityRole(SD.User));
            //}

            //List<SelectListItem> roleList = new List<SelectListItem>();
            //roleList.Add(new SelectListItem()
            //{
            //    Value = SD.User,
            //    Text = SD.User
            //});
            //roleList.Add(new SelectListItem()
            //{
            //    Value = SD.Admin,
            //    Text = SD.Admin
            //});
            var roleList = _roleManager.Roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            }).ToList();

            ViewData["ReturnUrl"] = returnUrl;
            RegisterViewModel model = new RegisterViewModel();
            model.RoleList = roleList;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Name = model.Name,
                    DateCreated = DateTime.Now,
                };
                //var user = new IdentityUser
                //{
                //    UserName = model.Email,
                //    Email = model.Email
                //};


                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if(model.RoleSelected != null && model.RoleSelected.Length > 0 && model.RoleSelected == SD.Admin)
                    {
                        await _userManager.AddToRoleAsync(user, SD.Admin);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, SD.User);
                    }

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userid = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

                    // Send email

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    //return RedirectToAction("Index", "Home");
                    return LocalRedirect(returnUrl);
                }

                AddErrors(result);
            }

            var roleList = _roleManager.Roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            }).ToList();
            model.RoleList = roleList;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> RemoveAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _userManager.ResetAuthenticatorKeyAsync(user);
            await _userManager.SetTwoFactorEnabledAsync(user, false);
            
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string code, string userId)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return View("Error");
                }

                var result = await _userManager.ConfirmEmailAsync(user, code);
                if (result.Succeeded)
                {
                    return View();
                }
            }
            return View("Error");
        }
         

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    var user = await _userManager.GetUserAsync(User);
                    var claim = await _userManager.GetClaimsAsync(user);

                    if(claim.Count > 0)
                    {
                        await _userManager.RemoveClaimAsync(user, claim.FirstOrDefault(u => u.Type == "FirstName"));
                    }

                    await _userManager.AddClaimAsync(user, new Claim("FirstName", user.Name));

                    // return RedirectToAction("Index", "Home");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(VerifyAuthenticatorCode), new { rememberMe = model.RememberMe, returnUrl });
                }
            if (result.IsLockedOut)
                {
                    return RedirectToAction("Lockout");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> VerifyAuthenticatorCode(bool rememberMe, string returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View(new VerifyAuthenticatorViewModel
            {
                RememberMe = rememberMe, ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyAuthenticatorCode(VerifyAuthenticatorViewModel model)
        {
            model.ReturnUrl ??= Url.Content("~/");
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, model.RememberMe, rememberClient: false); // rememberClient everytime will ask if it's false
            if(result.Succeeded)
            {
                return LocalRedirect(model.ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                return RedirectToAction("Lockout");
            }

            ModelState.AddModelError(string.Empty, "Invalid code.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        public IActionResult NoAccess()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return RedirectToAction("ForgotPasswordConfirmation");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);

                var callbackUrl = Url.Action("ResetPassword", "Account", new { userid = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

                // Send email with this link


                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ForgotPasswordConfirmation()
        {
            // SendEmail
          
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            AddErrors(result);
            return View();
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AuthenticatorConfirmation()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EnableAuthenticator()
        {
            string authenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _userManager.ResetAuthenticatorKeyAsync(user); // Reset old key to generate a new one
            var token = await _userManager.GetAuthenticatorKeyAsync(user);

            string authenticatorUri = string.Format(authenticatorUriFormat, _urlEncoder.Encode("IdentityManager"), _urlEncoder.Encode(user.Email), token);

            var model = new TwoFactorAuthenticationViewModel
            {
               Token = token,
               QRCodeUrl = authenticatorUri
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableAuthenticator(TwoFactorAuthenticationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Error");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var result = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, model.Code);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Invalid code.");
                return View(model);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);

            return RedirectToAction("AuthenticatorConfirmation");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach(var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
