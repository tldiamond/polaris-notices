using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NoticeSuite.Security
{
    public class CustomAdminAuthorizeAttribute : AuthorizeAttribute
    {
        public CustomAdminAuthorizeAttribute()
        {
            this.Roles = AppSettings.AdminPermissionGroup;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return base.AuthorizeCore(httpContext);
        }
    }
}