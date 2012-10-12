using System.Web.Mvc;

using Sinbadsoft.Lib.UserManagement.Authentication;

namespace SampleWebApplication.Controllers
{
    public class BaseController : Controller
    {
        public BaseController() : this(new AuthenticationTokenManager()) { }

        public BaseController(IAuthenticationTokenManager tokenManager)
        {
            this.TokenManager = tokenManager;
        }

        public AuthenticatedUserInfo AuthenticatedUser { get; private set; }

        // could be injected using a DI framework
        public IAuthenticationTokenManager TokenManager { get; set; }

        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            // This is an example of how you can keep track of the user id and email in all controllers.
            this.ViewBag.AuthenticatedUser = this.AuthenticatedUser = this.TokenManager.Verify();
        }
    }
}