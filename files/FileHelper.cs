using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace files
{
    public class FileHelper
    {
        //shared folder path
        public static string RootPath { get; set; } = /*StaticHelper.Source; */@"X:\SHARED FOLDERS";/*@"C:\Users\ysu\Desktop\tatilsepeti"*/

        public List<File> AllList { get; set; }

        //constructor
        public FileHelper()
        {
            AllList = new List<File>();

            GetRootFiles(RootPath);

            GetPathAndUsersPermission(RootPath);
        }

        //verilen path içindeki herşeyin dosya yolu ve user yetkilerini getiriyor
        private void GetPathAndUsersPermission(string path)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(path))
                {
                    foreach (string f in Directory.GetFiles(d))
                    {
                        var values = GetFileNameAndPath(f);
                        AllList.Add(new File
                        {
                            Path = ConvertPathToTreeUrl(values.Item1),
                            AbsolutePath = f,
                            Name = values.Item2,
                            Users = GetUserFilePermissionList(f)
                        });
                    }
                    GetPathAndUsersPermission(d);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                LogHelper.WriteLogException(ex.Message, ex.StackTrace);
            }
        }

        // bütün ağacı çıkaran kod verilen path içindeki root hariç herşeyi getiriyor bu metod ise root içinde kalanları listeye ekliyor
        private void GetRootFiles(string path)
        {
            try
            {
                foreach (var p in Directory.GetFiles(path))
                {
                    var values = GetFileNameAndPath(p);
                    AllList.Add(new File
                    {
                        Path = ConvertPathToTreeUrl(values.Item1),
                        AbsolutePath = p,
                        Name = values.Item2,
                        Users = GetUserFilePermissionList(p)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                LogHelper.WriteLogException(ex.Message, ex.StackTrace);
            }
        }

        //dosyanın adı ve hangi dosya yoluna sahip olduğu bilgisini verir
        //1- dosya yolu, 2- dosaynın adı
        public Tuple<string, string> GetFileNameAndPath(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            return Tuple.Create(dir.FullName.Replace(dir.Name, string.Empty), dir.Name);
        }

        //path içindeki dosya ve herşeyin kişilerini ve yetkilerini getiriyor
        private List<User> GetUserFilePermissionList(string path)
        {
            try
            {
                var userList = new List<User>();

                DirectorySecurity dirSecurity = Directory.GetAccessControl(path);

                foreach (FileSystemAccessRule fileSystemAccessRule in dirSecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
                {
                    userList.Add(new User
                    {
                        UserName = fileSystemAccessRule.IdentityReference.Value,
                        Permission = fileSystemAccessRule.FileSystemRights.ToString(),
                        Type = fileSystemAccessRule.AccessControlType.ToString()
                    });
                }

                if (userList.Count != 0)
                    return userList;

                return null;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                LogHelper.WriteLogException(ex.Message, ex.StackTrace);
                return null;
            }
        }

        //saharepoint tree oluşturabilmek için dosya yolunun istenilen formata çevirilmesini sağlar
        //revize edilebilir ana root oluşturabilmek için
        private string ConvertPathToTreeUrl(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                string[] tree = path.Split('\\');

                path = string.Empty;

                foreach (string i in tree)
                    path += i + "/";

                path = path.Remove(0, 1);

                if (path.Contains(":"))
                {
                    path = path.Replace(":", string.Empty).TrimEnd().TrimStart();
                }

                return path.Remove(path.Length - 1, 1);
            }

            return string.Empty;
        }
    }


    public class User
    {
        public string UserName { get; set; } //Accountname
        public string Permission { get; set; }
        public string Type { get; set; }
    }

    public partial class File
    {
        public string Path { get; set; } //sharepoint dosya hiyerarşiyi oluşturabilmek için gerekli
        public string AbsolutePath { get; set; } //sharepoint herarşisindeki yerin kaydedebilmek için dosyanı alınıcağı yer
        public string Name { get; set; } //sharepoint listesine kayıt esnasında verilecek dosya ismi
        public List<User> Users { get; set; } //dosyaya ait kişiler ve bu kişilere ait dosya izinleri
    }

    //pathlere yetki verebilmek için hepsiyle uğraşmak yerine sadece klasörlere gidip yetki verebilsin 
    //diye path ile benzersiz olanları alabilmek için eklenen partial class
    public partial class File
    {
        public override bool Equals(object obj)
        {
            var item = obj as File;

            if (item == null)
            {
                return false;
            }

            return this.Path.Equals(item.Path);
        }

        public override int GetHashCode()
        {
            return this.Path.GetHashCode();
        }
    }
}
