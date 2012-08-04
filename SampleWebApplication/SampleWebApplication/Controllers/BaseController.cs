using System.Web.Mvc;

using Sinbadsoft.Lib.UserManagement.Authentication;

namespace SampleWebApplication.Controllers
{
    public class BaseController : Controller
    {
        public BaseController() : this(new AuthenticationTokenManager())
        {
        }

        public BaseController(IAuthenticationTokenManager authenticationManager)
        {
            this.AuthenticationManager = authenticationManager;
        }

        public AuthenticatedUserInfo AuthenticatedUser { get; private set; }

        // could be injected using your DI framework
        public IAuthenticationTokenManager AuthenticationManager { get; set; }

        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            this.ViewBag.AuthenticatedUser = this.AuthenticatedUser = this.AuthenticationManager.Verify();
        }
    }
}