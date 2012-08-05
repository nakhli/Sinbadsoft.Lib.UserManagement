using System;
using System.Net.Mail;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;

using MonkeyOrm;

using SampleWebApplication.Models;

using Sinbadsoft.Lib.UserManagement;

namespace SampleWebApplication.Controllers
{
    public class AccountController : BaseController
    {
        public AccountController()
        {
            var connectionString = WebConfigurationManager.ConnectionStrings["default"].ConnectionString;
            var connectionFactory = new DbProviderBasedConnectionFactory("default", connectionString);
            this.UsersManager = new UserManager(connectionFactory);
        }

        private IUserManager UsersManager { get; set; }

        public ActionResult LogOn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                int id;
                var loginResult = this.UsersManager.Login(model.Email, model.Password, out id);

                if (LoginResult.Success == loginResult)
                {
                    this.AuthenticationManager.Set(model.Email, id, true);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }

                    return this.RedirectToAction("Index", "Home");
                }

                var errorMessage = loginResult == LoginResult.EmailNotVerified
                    ? "Your email is not verified yet."
                    : "The user name or password provided is incorrect.";
                this.ModelState.AddModelError(string.Empty, errorMessage);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult LogOff()
        {
            this.AuthenticationManager.Remove();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Register()
        {
            ViewBag.MinPasswordLength = this.UsersManager.MinPasswordLength;
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            // TODO(cnakhli) need captcha?

            if (ModelState.IsValid)
            {
                VerificationToken token;
                int id;
                var registerResult = this.UsersManager.Register(model.Email, model.Password, out token, out id);

                if (registerResult == RegisterResult.Success)
                {
                    this.SendVerificationEmail(id, model.Email, token.ToString());
                    return View("VerificationSent");
                }

                this.ModelState.AddModelError(string.Empty, ErrorCodeToString(registerResult));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>
        /// Validate the token sent to the user by email.
        /// </summary>
        public ActionResult Verify(int id, string token)
        {
            var verificationToken = VerificationToken.Parse(token);

            // NOTE you can check for token freshness using verificationToken.IsFresh()
            string email;
            if (VerifyResult.Success != this.UsersManager.CheckAndClearVerificationToken(id, verificationToken, out email))
            {
                // Invalid token, redirect to logon
                // We can display a nice message to inform user that his token is invalid and invite him to register
                return RedirectToAction("LogOn");
            }

            this.AuthenticationManager.Set(email, id, false);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (this.UsersManager.ChangePassword(this.AuthenticatedUser.Id, model.OldPassword, model.NewPassword))
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }

                ModelState.AddModelError(string.Empty, "The current password is incorrect or the new password is invalid.");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        private static string ErrorCodeToString(RegisterResult createStatus)
        {
            switch (createStatus)
            {
                case RegisterResult.DuplicateEmail:
                    return "User name already exists. Please enter a different user name.";

                case RegisterResult.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case RegisterResult.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        private void SendVerificationEmail(int id, string email, string token)
        {
            var url = Request.Url;
            var port = url.IsDefaultPort ? string.Empty : (":" + url.Port);
            var domain = url.Scheme + Uri.SchemeDelimiter + url.Host + port;
            var action = Url.Action("Verify", "Account", new { id, token });
            var body = string.Format(
@"Hi,
An account has been created on Funky App with this email {0}.
In order to validate you email please click here:
{1}
", email, domain + action);

            using (var smtp = new SmtpClient())
            {
                smtp.Send("funkyapp@example.com", email, "Verify your email!", body);
            }
        }
    }
}
