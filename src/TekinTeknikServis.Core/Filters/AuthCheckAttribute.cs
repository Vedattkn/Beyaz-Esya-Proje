using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TekinTeknikServis.Core.Infrastructure;
using TekinTeknikServis.Core.Models;

namespace TekinTeknikServis.Core.Filters
{
    public class AuthCheckAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.Session.GetJson<UserSession>("CurrentUser");
            if (user == null)
            {
                // Kullanıcı giriş yapmamışsa, login sayfasına yönlendir
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Account", action = "Login" }));
            }
            base.OnActionExecuting(context);
        }
    }
}
