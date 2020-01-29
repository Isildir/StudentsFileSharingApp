using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace StudentsFileSharingApp.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected int GetUserId()
        {
            var principal = Request.HttpContext.Request.HttpContext.User;

            if (!(principal?.Identity is ClaimsIdentity identity))
                return 0;

            var claim = identity.Claims.FirstOrDefault(a => a.Type.Equals(ClaimTypes.Name)).Value;

            return int.Parse(claim);
        }
    }
}