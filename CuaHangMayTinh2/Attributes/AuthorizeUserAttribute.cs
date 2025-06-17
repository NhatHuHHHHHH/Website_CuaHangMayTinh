using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CuaHangMayTinh2.Controllers
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class AuthorizeUserAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext.Session["KhachHangID"] == null)
            {
                return false; // Chưa đăng nhập
            }
            return true; // Đã đăng nhập
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
            string returnUrl = filterContext.HttpContext.Request.Url != null
                ? filterContext.HttpContext.Request.Url.PathAndQuery
                : "/";
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary 
                {
                    { "controller", "MayTinh" },
                    { "action", "Login" },
                    { "returnUrl", returnUrl }
                });
        }
    }
}