using Hangfire;
using Microsoft.Owin;
using Owin;
using Hangfire.SqlServer;
using System;
using Hangfire.Dashboard;
using System.Collections.Generic;
using NoticeSuite.Services.Dialout;
using NoticeSuite.Services.SMS;

[assembly: OwinStartup(typeof(NoticeSuite.Startup))]

namespace NoticeSuite
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            if (AppSettings.EnableHangfireMsmq)
            {
                GlobalConfiguration.Configuration
                .UseSqlServerStorage("Data Source=" + AppSettings.DbServer + ";Initial Catalog=CLC_Notices;Integrated Security=True")
                .UseMsmqQueues(@".\hangfire-notices");
            }
            else
            {
                GlobalConfiguration.Configuration
                .UseSqlServerStorage("Data Source=" + AppSettings.DbServer + ";Initial Catalog=CLC_Notices;Integrated Security=True");
            }
            

            if (AppSettings.EnableDialout)
            {
                RecurringJob.AddOrUpdate("Delete old recordings", () => Janitor.CleanupRecordings(), Cron.Daily(2), TimeZoneInfo.Local);
                RecurringJob.AddOrUpdate("Run dialout", () => Dialout.Run(true, false), Cron.Hourly(), TimeZoneInfo.Local);
            }
            if (AppSettings.EnableSmsProcessing)
            {
                RecurringJob.AddOrUpdate("Send SMS Queue", () => SMS.SendQueue(), Cron.Daily(8, 5), TimeZoneInfo.Local);
            }

            var options = new DashboardOptions
            {
                AuthorizationFilters = new IAuthorizationFilter[] 
                {
                    new AuthorizationFilter{Roles = AppSettings.AdminPermissionGroup}
                }
            };

            app.UseHangfireDashboard("/hangfire", options);
			app.UseHangfireServer();
        }
    }
}