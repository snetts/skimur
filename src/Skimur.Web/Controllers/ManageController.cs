﻿using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Infrastructure.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Skimur.Web.Identity;
using Skimur.Web.Models;

namespace Skimur.Web.Controllers
{
    [Authorize]
    public class ManageController : BaseController
    {
        private ApplicationSignInManager _signInManager;
        private readonly IAuthenticationManager _authenticationManager;
        private readonly IUserContext _userContext;
        private ApplicationUserManager _userManager;

        public ManageController(ApplicationUserManager userManager,
            ApplicationSignInManager signInManager,
            IAuthenticationManager authenticationManager,
            IUserContext userContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authenticationManager = authenticationManager;
            _userContext = userContext;
        }

        public ActionResult Index()
        {
            ViewBag.ManageNavigationKey = "Account";

            return View(new IndexViewModel());
        }

        #region Password

        [HttpGet]
        public async Task<ActionResult> Password()
        {
            return await _userManager.HasPasswordAsync(User.Identity.GetUserId().ParseGuid())
                ? Redirect(Url.ChangePassword())
                : Redirect(Url.SetPassword());
        }

        public ActionResult ChangePassword()
        {
            ViewBag.ManageNavigationKey = "Password";

            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            ViewBag.ManageNavigationKey = "Password";

            if (!ModelState.IsValid)
                return View(model);

            var result = await _userManager.ChangePasswordAsync(_userContext.CurrentUser.Id, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByIdAsync(User.Identity.GetUserId().ParseGuid());
                if (user != null)
                    await _signInManager.SignInAsync(user, false, false);

                AddSuccessMessage("Your password was successfully changed.");
            }
            else
                foreach (var error in result.Errors)
                    AddErrorMessage(error);

            return View(model);
        }

        public ActionResult SetPassword()
        {
            ViewBag.ManageNavigationKey = "Password";

            return View(new SetPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            ViewBag.ManageNavigationKey = "Password";

            if (ModelState.IsValid)
            {
                var result = await _userManager.AddPasswordAsync(User.Identity.GetUserId().ParseGuid(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByIdAsync(User.Identity.GetUserId().ParseGuid());
                    if (user != null)
                        await _signInManager.SignInAsync(user, false, false);

                    AddSuccessMessage("Your password has succesfully been set.");

                    return Redirect(Url.ChangePassword());
                }

                foreach (var error in result.Errors)
                    AddErrorMessage(error);
            }

            return View(model);
        }

        #endregion

        #region Logins

        public async Task<ActionResult> ManageLogins()
        {
            ViewBag.ManageNavigationKey = "Logins";

            var user = await _userManager.FindByIdAsync(User.Identity.GetUserId().ParseGuid());
            var userLogins = await _userManager.GetLoginsAsync(User.Identity.GetUserId().ParseGuid());
            var otherLogins = _authenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();

            return View(new ManageLoginsViewModel
            {
                IsPasswordSet = !string.IsNullOrEmpty(user.PasswordHash),
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            ViewBag.ManageNavigationKey = "Logins";

            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), _authenticationManager, User.Identity.GetUserId());
        }

        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await _authenticationManager.GetExternalLoginInfoAsync("XsrfId", User.Identity.GetUserId());

            if (loginInfo == null)
            {
                AddErrorMessage("An error has occurred.");
                return Redirect(Url.ManageLogins());
            }

            var result = await _userManager.AddLoginAsync(User.Identity.GetUserId().ParseGuid(), loginInfo.Login);

            if (!result.Succeeded)
                foreach (var error in result.Errors)
                    AddErrorMessage(error);

            return Redirect(Url.ManageLogins());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            var result = await _userManager.RemoveLoginAsync(User.Identity.GetUserId().ParseGuid(), new UserLoginInfo(loginProvider, providerKey));

            if (result.Succeeded)
            {
                var user = await _userManager.FindByIdAsync(User.Identity.GetUserId().ParseGuid());
                if (user != null)
                    await _signInManager.SignInAsync(user, false, false);

                AddSuccessMessage("The external login was successfully removed.");
            }
            else
                AddErrorMessage("An error has occurred.");

            return Redirect(Url.ManageLogins());
        }

        #endregion

        //        //
        //        // POST: /Manage/RemoveLogin
        //        [HttpPost]
        //        [ValidateAntiForgeryToken]
        //        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        //        {
        //            ManageMessageId? message;
        //            var result = await _userManager.RemoveLoginAsync(User.Identity.GetUserId().ParseGuid(), new UserLoginInfo(loginProvider, providerKey));
        //            if (result.Succeeded)
        //            {
        //                var user = await _userManager.FindByIdAsync(User.Identity.GetUserId().ParseGuid());
        //                if (user != null)
        //                {
        //                    await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
        //                }
        //                message = ManageMessageId.RemoveLoginSuccess;
        //            }
        //            else
        //            {
        //                message = ManageMessageId.Error;
        //            }
        //            return RedirectToAction("ManageLogins", new { Message = message });
        //        }

        //        //
        //        // GET: /Manage/AddPhoneNumber
        //        public ActionResult AddPhoneNumber()
        //        {
        //            return View();
        //        }

        //        //
        //        // POST: /Manage/AddPhoneNumber
        //        [HttpPost]
        //        [ValidateAntiForgeryToken]
        //        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        //        {
        //            if (!ModelState.IsValid)
        //            {
        //                return View(model);
        //            }
        //            // Generate the token and send it
        //            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId().ParseGuid(), model.Number);
        //            if (_userManager.SmsService != null)
        //            {
        //                var message = new IdentityMessage
        //                {
        //                    Destination = model.Number,
        //                    Body = "Your security code is: " + code
        //                };
        //                await _userManager.SmsService.SendAsync(message);
        //            }
        //            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        //        }

        //        //
        //        // POST: /Manage/EnableTwoFactorAuthentication
        //        [HttpPost]
        //        [ValidateAntiForgeryToken]
        //        public async Task<ActionResult> EnableTwoFactorAuthentication()
        //        {
        //            await _userManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId().ParseGuid(), true);
        //            var user = await _userManager.FindByIdAsync(User.Identity.GetUserId().ParseGuid());
        //            if (user != null)
        //            {
        //                await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
        //            }
        //            return RedirectToAction("Index", "Manage");
        //        }

        //        //
        //        // POST: /Manage/DisableTwoFactorAuthentication
        //        [HttpPost]
        //        [ValidateAntiForgeryToken]
        //        public async Task<ActionResult> DisableTwoFactorAuthentication()
        //        {
        //            await _userManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId().ParseGuid(), false);
        //            var user = await _userManager.FindByIdAsync(User.Identity.GetUserId().ParseGuid());
        //            if (user != null)
        //            {
        //                await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
        //            }
        //            return RedirectToAction("Index", "Manage");
        //        }

        //        //
        //        // GET: /Manage/VerifyPhoneNumber
        //        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        //        {
        //            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId().ParseGuid(), phoneNumber);
        //            // Send an SMS through the SMS provider to verify the phone number
        //            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        //        }

        //        //
        //        // POST: /Manage/VerifyPhoneNumber
        //        [HttpPost]
        //        [ValidateAntiForgeryToken]
        //        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        //        {
        //            if (!ModelState.IsValid)
        //            {
        //                return View(model);
        //            }
        //            var result = await _userManager.ChangePhoneNumberAsync(User.Identity.GetUserId().ParseGuid(), model.PhoneNumber, model.Code);
        //            if (result.Succeeded)
        //            {
        //                var user = await _userManager.FindByIdAsync(User.Identity.GetUserId().ParseGuid());
        //                if (user != null)
        //                {
        //                    await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
        //                }
        //                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
        //            }
        //            // If we got this far, something failed, redisplay form
        //            ModelState.AddModelError("", "Failed to verify phone");
        //            return View(model);
        //        }

        //        //
        //        // GET: /Manage/RemovePhoneNumber
        //        public async Task<ActionResult> RemovePhoneNumber()
        //        {
        //            var result = await _userManager.SetPhoneNumberAsync(User.Identity.GetUserId().ParseGuid(), null);
        //            if (!result.Succeeded)
        //            {
        //                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
        //            }
        //            var user = await _userManager.FindByIdAsync(User.Identity.GetUserId().ParseGuid());
        //            if (user != null)
        //            {
        //                await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
        //            }
        //            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        //        }

        //        //
        //        // GET: /Manage/ChangePassword
        //        public ActionResult ChangePassword()
        //        {
        //            return View();
        //        }

        //        //
        //        // POST: /Manage/ChangePassword
        //        [HttpPost]
        //        [ValidateAntiForgeryToken]
        //        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        //        {
        //            if (!ModelState.IsValid)
        //            {
        //                return View(model);
        //            }
        //            var result = await _userManager.ChangePasswordAsync(User.Identity.GetUserId().ParseGuid(), model.OldPassword, model.NewPassword);
        //            if (result.Succeeded)
        //            {
        //                var user = await _userManager.FindByIdAsync(User.Identity.GetUserId().ParseGuid());
        //                if (user != null)
        //                {
        //                    await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
        //                }
        //                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
        //            }
        //            AddErrors(result);
        //            return View(model);
        //        }

        //        //
        //        // GET: /Manage/SetPassword
        //        public ActionResult SetPassword()
        //        {
        //            return View();
        //        }

        //        //
        //        // POST: /Manage/SetPassword
        //        [HttpPost]
        //        [ValidateAntiForgeryToken]
        //        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        //        {
        //            if (ModelState.IsValid)
        //            {
        //                var result = await _userManager.AddPasswordAsync(User.Identity.GetUserId().ParseGuid(), model.NewPassword);
        //                if (result.Succeeded)
        //                {
        //                    var user = await _userManager.FindByIdAsync(User.Identity.GetUserId().ParseGuid());
        //                    if (user != null)
        //                    {
        //                        await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
        //                    }
        //                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
        //                }
        //                AddErrors(result);
        //            }

        //            // If we got this far, something failed, redisplay form
        //            return View(model);
        //        }

        //        //
        //        // GET: /Manage/ManageLogins
        //        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        //        {
        //            ViewBag.StatusMessage =
        //                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
        //                : message == ManageMessageId.Error ? "An error has occurred."
        //                : "";
        //            var user = await _userManager.FindByIdAsync(User.Identity.GetUserId().ParseGuid());
        //            if (user == null)
        //            {
        //                return View("Error");
        //            }
        //            var userLogins = await _userManager.GetLoginsAsync(User.Identity.GetUserId().ParseGuid());
        //            var otherLogins = _authenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
        //            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
        //            return View(new ManageLoginsViewModel
        //            {
        //                CurrentLogins = userLogins,
        //                OtherLogins = otherLogins
        //            });
        //        }

        //        //
        //        // POST: /Manage/LinkLogin
        //        [HttpPost]
        //        [ValidateAntiForgeryToken]
        //        public ActionResult LinkLogin(string provider)
        //        {
        //            // Request a redirect to the external login provider to link a login for the current user
        //            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), _authenticationManager, User.Identity.GetUserId());
        //        }

        //        //
        //        // GET: /Manage/LinkLoginCallback
        //        public async Task<ActionResult> LinkLoginCallback()
        //        {
        //            var loginInfo = await _authenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
        //            if (loginInfo == null)
        //            {
        //                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        //            }
        //            var result = await _userManager.AddLoginAsync(User.Identity.GetUserId().ParseGuid(), loginInfo.Login);
        //            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        //        }

        //#region Helpers
        //        // Used for XSRF protection when adding external logins
        //        private const string XsrfKey = "XsrfId";

        //        private void AddErrors(IdentityResult result)
        //        {
        //            foreach (var error in result.Errors)
        //            {
        //                ModelState.AddModelError("", error);
        //            }
        //        }

        //        private bool HasPassword()
        //        {
        //            var user = _userManager.FindById(User.Identity.GetUserId().ParseGuid());
        //            if(user == null) return false;
        //            return !string.IsNullOrEmpty(user.PasswordHash);
        //        }

        //        private bool HasPhoneNumber()
        //        {
        //            var user = _userManager.FindById(User.Identity.GetUserId().ParseGuid());
        //            if(user == null) return false;
        //            return !string.IsNullOrEmpty(user.PhoneNumber);
        //        }

        //        public enum ManageMessageId
        //        {
        //            AddPhoneSuccess,
        //            ChangePasswordSuccess,
        //            SetTwoFactorSuccess,
        //            SetPasswordSuccess,
        //            RemoveLoginSuccess,
        //            RemovePhoneSuccess,
        //            Error
        //        }

        //#endregion
    }
}