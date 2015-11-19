using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace NoticeSuite.Data
{
    public partial class NoticeEntities
    {
        public NoticeEntities(string server, string database)
            : base(Methods.GetConnectionString(server, database, "Data.Notices"))
        {
        }

        static NoticeEntities FromConfig()
        {
            return new NoticeEntities(AppSettings.DbServer, "CLC_Notices");
        }

        static NoticeEntities Test()
        {
            return new NoticeEntities("traindb", "CLC_Notices");
        }

        public static NoticeEntities Create()
        {
            return FromConfig();
            //return Test();
        }

        public static NoticeEntities Custom(string db)
        {
            return new NoticeEntities(db, "CLC_Notices");
        }
    }
}