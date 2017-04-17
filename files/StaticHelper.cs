using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace files
{
    public class StaticHelper
    {
        public static string SiteUrl
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["SiteUrl"];
            }
        }

        public static string Username
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["UserName"];
            }
        }


        public static string Password
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["Password"];
            }
        }
        public static string Listname
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["ListName"];
            }
        }

        public static string Source
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["FilePath"];
            }
        }
    }
}
