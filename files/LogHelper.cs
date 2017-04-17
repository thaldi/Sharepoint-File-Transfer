using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace files
{
    public class LogHelper
    {
        public static void WriteLogFile(File file)
        {
            using (StreamWriter writer = new StreamWriter(@"X:\SHARED FOLDERS\log.txt", true))
            {
                writer.WriteLine(file.AbsolutePath + " : " + file.Name + " : " + file.Path + " : " + UsersStr(file.Users) + Environment.NewLine + "-----------");
            }
        }

        public static void WriteLogException(string ex, string stackTrace)
        {
            using (StreamWriter writer =
            new StreamWriter(@"X:\SHARED FOLDERS\exceptions.txt", true))
            {
                writer.WriteLine(ex + " : " + stackTrace + Environment.NewLine + "---------");
            }
        }

        private static string UsersStr(List<User> u)
        {
            string values = string.Empty;
            foreach (var item in u)
                values += item.UserName + " : " + item.Permission + " ; ";
            return values;
        }
    }
}
