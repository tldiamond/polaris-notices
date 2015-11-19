using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoticeSuite.Data
{
    public partial class PolarisEntities
    {
        public PolarisEntities(string server, string database)
            : base(Methods.GetConnectionString(server, database, "Data.PolarisModel"))
        {
        }

        static PolarisEntities FromConfig()
        {
            return new PolarisEntities(AppSettings.DbServer, "Polaris");
        }

        static PolarisEntities Test()
        {
            return new PolarisEntities("traindb", "Polaris");
        }

        public static PolarisEntities Create()
        {
            return FromConfig();
            //return Test();
        }

        public static PolarisEntities Custom(string db)
        {
            return new PolarisEntities(db, "Polaris");
        }
    }
}