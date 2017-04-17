using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace files
{
    class Program
    {
        static void Main(string[] args)
        {
            FileHelper f = new FileHelper();
            SharepointHelper h = new SharepointHelper();

            Console.WriteLine("Dosyalar load ediliyor..");
            var datas = f.AllList;
            Console.WriteLine("Dosyalar load edildi.\n");


            //Console.WriteLine("Liste Oluşturuluyor..");
            //h.CreateList();
            //Console.WriteLine("Liste Oluşturuldu.\n\n");

            Console.WriteLine("Dosya ağacı oluşturuluyor..");
            foreach (var item in datas)
            {
                h.CreateFolderTree(item);
            }
            Console.WriteLine("Dosya ağacı oluşturuldu.\n");


            //Console.WriteLine("Yetkiler Veriliyor..");
            //h.SetPermissions(f.AllList);
            //Console.WriteLine("Yetkiler Verildi.\n");


            Console.WriteLine("Dosya içerikleri ekleniyor..");
            foreach (var item in datas)
            {
                h.UploadFileToDocumnetLibrary(item);
            }
            Console.WriteLine("Dosya içerikleri eklendi.\n\n");


            Console.WriteLine("İşlemler Bitti.");
            Console.ReadKey();
        }

    }
}
