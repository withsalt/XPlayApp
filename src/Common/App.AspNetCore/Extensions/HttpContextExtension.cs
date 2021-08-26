using App.Data.Model.Response.User;
using Microsoft.AspNetCore.Http;

namespace App.AspNetCore.Extensions
{
    public static class HttpContextExtension
    {
        public static UserInfoModel CurrentUser(this HttpContext context)
        {
            try
            {
                if (context == null)
                    return null;
                var userInfo = context.Session.Get<UserInfoModel>("USER");
                if (userInfo != null)
                    return userInfo;
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        public static string HostName(this HttpRequest context)
        {
            try
            {
                if (context == null)
                    return null;
                return $"{context.Scheme}://{context.Host}";
            }
            catch
            {
                return null;
            }
        }

        public static bool IsAjax(this HttpRequest req)
        {
            bool result = false;

            var xreq = req.Headers.ContainsKey("x-requested-with");
            if (xreq)
            {
                result = req.Headers["x-requested-with"] == "XMLHttpRequest";
            }

            return result;
        }
    }
}
