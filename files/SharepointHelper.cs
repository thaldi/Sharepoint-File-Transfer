using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace files
{
    public class SharepointHelper
    {
        private string SiteUrl { get; set; } = StaticHelper.SiteUrl; 

        private ClientContext cContext { get; set; }

        private Web web { get; set; }

        private Site site { get; set; }

        private static SharePointOnlineCredentials Credential
        {
            get
            {
                SecureString passWord = new SecureString();
                foreach (char c in StaticHelper.Password.ToCharArray()) passWord.AppendChar(c);
                var credential = new SharePointOnlineCredentials(StaticHelper.Username, passWord);
                return credential;
            }
        }

        //constructor
        public SharepointHelper()
        {
            cContext = new ClientContext(SiteUrl);
            cContext.Credentials = Credential;
            web = cContext.Web;
            site = cContext.Site;
        }

        //döküman listesini oluşturur
        public void CreateList()
        {
            ListCreationInformation cInfo = new ListCreationInformation();
            cInfo.Title = StaticHelper.Listname;
            cInfo.TemplateType = 101;

            List list = web.Lists.Add(cInfo);
            list.Update();

            cContext.ExecuteQuery();
        }

        //dosya yolu ağacındaki bütün dosyaları döküman kitaplığına kaydeder
        public void UploadFileToDocumnetLibrary(File values)
        {
            var list = web.Lists.GetByTitle(StaticHelper.Listname);

            cContext.Load(list);
            cContext.Load(list.RootFolder);
            cContext.ExecuteQuery();

            Microsoft.SharePoint.Client.Folder folder = web.GetFolderByServerRelativeUrl(list.RootFolder.ServerRelativeUrl /*+ "/DESKTOP-KPK9VT0/Users/ysu"*/ + values.Path);

            FileCreationInformation fci = new FileCreationInformation
            {
                Url = /*"kampanya hata.txt"*/values.Name,
                Content = System.IO.File.ReadAllBytes(/*"\\\\DESKTOP-KPK9VT0\\Users\\ysu\\Desktop\\tatilsepeti\\kampanya hata.txt"*/values.AbsolutePath),
                Overwrite = true
            };

            Microsoft.SharePoint.Client.FileCollection files = folder.Files;
            Microsoft.SharePoint.Client.File file = files.Add(fci);

            cContext.Load(file);
            cContext.Load(files);
            cContext.ExecuteQuery();
        }

        #region folder ve sub folder oluşturur

        //alt metod
        public void CreateFolder(Web web, string listTitle, /*string fullFolderUrl,*/File values)
        {
            if (string.IsNullOrEmpty(/*fullFolderUrl*/values.Path))
            {
                Console.WriteLine("url de bir hata var");
                return;
            }
            var list = web.Lists.GetByTitle(listTitle);

            CreateFolderInternal(web, list.RootFolder, values.Path/*fullFolderUrl*/, values);
        }

        //sharepoint içerisinde dosya ağacını oluşturur
        private void CreateFolderInternal(Web web, Folder parentFolder, string fullFolderUrl, File values)
        {
            LogHelper.WriteLogFile(values);

            var folderUrls = fullFolderUrl.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string folderUrl = folderUrls[0];
            var curFolder = parentFolder.Folders.Add(folderUrl);
            web.Context.Load(curFolder);
            web.Context.ExecuteQuery();

            //yetkileri sadece klasör bazlı yaparak hız kazanamak için buradan kaldırıldı şimdilik
            //SetUserPermission(values, curFolder);
            if (folderUrls.Length > 1)
            {
                var subFolderUrl = string.Join("/", folderUrls, 1, folderUrls.Length - 1);

                LogHelper.WriteLogFile(values);
                CreateFolderInternal(web, curFolder, subFolderUrl, values);
            }
        }

        //dosya ağacını oluşturur
        public void CreateFolderTree(/*string urlPath*/ File values)
        {
            CreateFolder(web, StaticHelper.Listname, /*urlPath*/values);
        }

        #endregion

        //bir hata var server relative path ile alakalı
        //şimdilik get işlemi olmadığı için kullanılmıyor
        #region folderları getirir

        public Folder GetFolder(string fullFolderUrl)
        {
            if (string.IsNullOrEmpty(fullFolderUrl))
                throw new ArgumentNullException("fullFolderUrl");

            if (!web.IsPropertyAvailable("ServerRelativeUrl"))
            {
                web.Context.Load(web, w => w.ServerRelativeUrl);
                web.Context.ExecuteQuery();
            }
            var folder = web.GetFolderByServerRelativeUrl(web.ServerRelativeUrl + fullFolderUrl);
            web.Context.Load(folder);
            web.Context.ExecuteQuery();
            return folder;
        }

        public void folder()
        {
            var folder = GetFolder("/Dökümanlar/nabukatnazar");
        }

        #endregion

        //kullanıcılara yetkilerinin atamasını yapar
        public void SetPermissions(List<File> values)
        {
            var permissionNeededFolders = values.Distinct().ToList();

            foreach (File file in permissionNeededFolders)
            {
                SetUserPermission(file, GetFolderFromUrl(file.Path));
            }
        }

        //windows kullanıcılarını benzeri yetkileri sharepoint tarafında verir
        private void SetUserPermission(File values, Folder folder)
        {
            foreach (var item in values.Users)
            {
                Principal user = web.EnsureUser(item.UserName);

                cContext.Load(user);
                try
                {
                    cContext.ExecuteQuery();
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLogException(ex.Message, ex.StackTrace);
                }

                folder.ListItemAllFields.BreakRoleInheritance(true, false);

                folder.ListItemAllFields.RoleAssignments.Add(user, GetRole(item.Permission));

                try
                {
                    cContext.ExecuteQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    LogHelper.WriteLogException(ex.Message, ex.StackTrace);
                }
            }
        }

        //windows role ile sharepoint role eşleştirilmesi
        private RoleDefinitionBindingCollection GetRole(string windowsRole)
        {
            RoleDefinitionBindingCollection roles = new RoleDefinitionBindingCollection(cContext);

            switch (windowsRole)
            {
                case "FullControl":
                    roles.Add(site.RootWeb.RoleDefinitions.GetByType(RoleType.Administrator));
                    break;
                case "Reader":
                    roles.Add(site.RootWeb.RoleDefinitions.GetByType(RoleType.Reader));
                    break;
                case "Write":
                    roles.Add(site.RootWeb.RoleDefinitions.GetByType(RoleType.Contributor));
                    break;
                case "Read & execute":
                    roles.Add(site.RootWeb.RoleDefinitions.GetByType(RoleType.Reader));
                    break;
                case "Modify":
                    roles.Add(site.RootWeb.RoleDefinitions.GetByType(RoleType.Contributor));
                    break;
                default:
                    roles.Add(site.RootWeb.RoleDefinitions.GetByType(RoleType.Reader));
                    break;
            }
            return roles;
        }

        //folder a tyetki verebilmek için url ile folder nesnesi sharepoişnt içinde getirirlir.
        private Folder GetFolderFromUrl(string url)
        {
            var list = web.Lists.GetByTitle(StaticHelper.Listname);

            cContext.Load(list);
            cContext.Load(list.RootFolder);
            cContext.ExecuteQuery();

            Microsoft.SharePoint.Client.Folder folder = web.GetFolderByServerRelativeUrl(list.RootFolder.ServerRelativeUrl + url);

            return folder;
        }

    }
}
