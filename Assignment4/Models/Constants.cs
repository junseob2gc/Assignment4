using System;

namespace FTPApp.Models
{
    public class Constants
    {
        public class FTP
        {
            public const string UserName = @"bdat100119f\bdat1001";
            public const string Password = "bdat1001";

            public const string BaseUrl = "ftp://waws-prod-dm1-127.ftp.azurewebsites.windows.net/bdat1001-20914";

            public const int OperationPauseTime = 10000;
        }

        public class Locations
        {
            public const string InfoCSVFileName = "info.csv";
            public const string MyImageFileName = "myimage.jpg";
            public const string InfoDOCXFileName = "info.docx";
            public const string InfoXLSXFileName = "info.xlsx";
            public const string InfoPPTXFileName = "info.pptx";

            public readonly static string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            public readonly static string ExePath = Environment.CurrentDirectory;

            public readonly static string ContentFolder = $"{ExePath}\\..\\..\\..\\Content";
            public readonly static string DataFolder = $"{ExePath}\\..\\..\\..\\Content\\Data";
            public readonly static string ImagesFolder = $"{ExePath}\\..\\..\\..\\Content\\Images";

        }
    }
}
