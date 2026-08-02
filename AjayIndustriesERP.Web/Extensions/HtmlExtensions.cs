using Microsoft.AspNetCore.Mvc.Rendering;

namespace AjayIndustriesERP.Web.Extensions
{
    public static class HtmlExtensions
    {
        public static string IsActive(this IHtmlHelper htmlHelper, string controller, string action = "")
        {
            var routeData = htmlHelper.ViewContext.RouteData;

            var currentController = routeData.Values["controller"]?.ToString();
            var currentAction = routeData.Values["action"]?.ToString();

            if (!string.IsNullOrEmpty(action))
            {
                return currentController == controller &&
                       currentAction == action
                    ? "active"
                    : "";
            }

            return currentController == controller
                ? "active"
                : "";
        }
    }
}