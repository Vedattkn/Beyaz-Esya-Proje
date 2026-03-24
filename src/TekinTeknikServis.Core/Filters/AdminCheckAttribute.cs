using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using TekinTeknikServis.Core.Infrastructure;
using TekinTeknikServis.Core.Models;

namespace TekinTeknikServis.Core.Filters
{
    public class AdminCheckAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.Session.GetJson<UserSession>("CurrentUser");
            if (user == null || !user.IsAdmin)
            {
                // Kullanıcı yetkisiz veya admin değilse anasayfaya veya logine gönder
                if (user == null)
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Account", action = "Login" }));
                else
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Home", action = "Index" }));
            }
            base.OnActionExecuting(context);
        }
    }
}
