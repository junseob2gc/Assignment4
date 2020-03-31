using FTPApp.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace FTPApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Student myrecord = new Student { StudentId = "200423859", FirstName = "Junseob", LastName = "Noh", AbsoluteUrl = Constants.FTP.BaseUrl };
            string myimagePath = $"{Constants.Locations.ImagesFolder}\\{Constants.Locations.MyImageFileName}";

            List<string> directories = FTP.GetDirectory(Constants.FTP.BaseUrl);
            List<Student> students = new List<Student>();


            foreach (var directory in directories)
            {
                Student student = new Student() { AbsoluteUrl = Constants.FTP.BaseUrl };
                student.FromDirectory(directory);

                if (FTP.FileExists(student.InfoCSVPath))
                {
                    var csvBytes = FTP.DownloadFileBytes(student.InfoCSVPath);

                    string csvFileData = Encoding.ASCII.GetString(csvBytes, 0, csvBytes.Length);

                    string[] data = csvFileData.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

                    if (data.Length != 2)
                    {
                        Console.WriteLine($"{student.FirstName} {student.LastName} has Error in CSV format");
                    }
                    else
                    {
                        student.FromCSV(data[1]);
                    }
                    //Console.WriteLine(student.Age);
                }

                if (myrecord.StudentId == student.StudentId)
                {
                    myrecord.Directory = student.Directory;
                    student.IsMe = 1;

                    if (FTP.FileExists(student.MyImagePath))
                    {
                        // download & save myimage.jpg from FTP
                        byte[] imageBytes = FTP.DownloadFileBytes(student.MyImagePath);
                        Image myimage = Imaging.ByteArrayToImage(imageBytes);

                        FileInfo imagefileinfo = new FileInfo(myimagePath);
                        myimage.Save(imagefileinfo.FullName, ImageFormat.Jpeg);
                    }
                }
                else
                {
                    student.IsMe = 0;
                }

                students.Add(student);
            }


            // insert directories of the FTP server into info.docx file with each directory on one page
            string docxName = $"{Constants.Locations.DataFolder}\\{Constants.Locations.InfoDOCXFileName}";
            CreateDocx.CreateWordprocessingDocument(docxName, students);


            // add myimage.jpg into the docx
            CreateDocx.InsertAPicture(docxName, myimagePath);

            // upload info.docx to the FTP site
            FTP.UploadFile(docxName, $"{myrecord.FullPathUrl}/{Constants.Locations.InfoDOCXFileName}");



            // create sheet 1, simple text on the A2
            string xlsxPath = $"{Constants.Locations.DataFolder}\\{Constants.Locations.InfoXLSXFileName}";
            CreateXlsx.CreateSpreadsheetWorkbook(xlsxPath);
            CreateXlsx.InsertText(xlsxPath, $"Hello, my name is {myrecord.FirstName} {myrecord.LastName}.", "A", 2);
            
            // create sheet 2, student information
            CreateXlsx.InsertObject(xlsxPath, students);

            // upload info.xlsx to the FTP site
            FTP.UploadFile(xlsxPath, $"{myrecord.FullPathUrl}/{Constants.Locations.InfoXLSXFileName}");



            // create pptx file
            string pptxPath = $"{Constants.Locations.DataFolder}\\{Constants.Locations.InfoPPTXFileName}";
            CreatePptx.CreatePresentation(pptxPath);

            // add students information into new slides
            foreach (var student in students)
            {
                CreatePptx.InsertNewSlide(pptxPath, 1, $"Hello, my name is {student.FirstName} {student.LastName}.");
            }

            // add myimage.jpg to first page           
            CreatePptx.AddImage(pptxPath, myimagePath);

            // upload info.pptx to the FTP site
            FTP.UploadFile(pptxPath, $"{myrecord.FullPathUrl}/{Constants.Locations.InfoPPTXFileName}");
        }
    }
}
