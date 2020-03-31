using System;
using System.Collections.Generic;
using System.Text;

namespace FTPApp.Models
{
    public class Student
    {
        public static string HeaderRow = $"{nameof(Student.StudentId)},{nameof(Student.FirstName)},{nameof(Student.LastName)},{nameof(Student.DateOfBirth)},{nameof(Student.ImageData)}";
        public string UID { get { return Guid.NewGuid().ToString(); } }
        public string StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        private string _DateOfBirth;
        public string DateOfBirth
        {
            get { return _DateOfBirth; }
            set
            {
                _DateOfBirth = value;

                //Convert DateOfBirth to DateTime
                DateTime dtOut;
                DateTime.TryParse(_DateOfBirth, out dtOut);
                DateOfBirthDT = dtOut;
            }
        }

        public DateTime DateOfBirthDT { get; internal set; }
        public int IsMe { get; set; }
        public string ImageData { get; set; }

        public string AbsoluteUrl { get; set; }
        public string Directory { get; set; }

        public string InfoCSVPath { get { return (Constants.FTP.BaseUrl + "/" + Directory + "/" + Constants.Locations.InfoCSVFileName); } }
        public string MyImagePath { get { return (Constants.FTP.BaseUrl + "/" + Directory + "/" + Constants.Locations.MyImageFileName); } }
        public string FullPathUrl
        {
            get
            {
                return AbsoluteUrl + "/" + Directory;
            }
        }

        public List<string> Exceptions { get; set; } = new List<string>();

        public virtual int Age
        {
            get
            {
                if(DateOfBirthDT == DateTime.MinValue)
                {
                    return 0;
                }

                DateTime Now = DateTime.Now;
                int Years = new DateTime(DateTime.Now.Subtract(DateOfBirthDT).Ticks).Year - 1;
                DateTime PastYearDate = DateOfBirthDT.AddYears(Years);
                int Months = 0;
                for (int i = 1; i <= 12; i++)
                {
                    if (PastYearDate.AddMonths(i) == Now)
                    {
                        Months = i;
                        break;
                    }
                    else if (PastYearDate.AddMonths(i) >= Now)
                    {
                        Months = i - 1;
                        break;
                    }
                }
                int Days = Now.Subtract(PastYearDate.AddMonths(Months)).Days;
                int Hours = Now.Subtract(PastYearDate).Hours;
                int Minutes = Now.Subtract(PastYearDate).Minutes;
                int Seconds = Now.Subtract(PastYearDate).Seconds;
                return Years;
            }
        }

        public void FromCSV(string csvdata)
        {
            string[] data = csvdata.Split(",", StringSplitOptions.None);

            try
            {
                StudentId = data[0];
                FirstName = data[1];
                LastName = data[2];
                DateOfBirth = data[3];
                ImageData = data[4];
            }
            catch(Exception e)
            {
                Exceptions.Add(e.Message);
            }
        }

        public void FromDirectory(string directory)
        {
            Directory = directory;

            if(String.IsNullOrEmpty(directory.Trim()))
            {
                return;
            }

            string[] data = directory.Trim().Split(" ", StringSplitOptions.None);

            StudentId = data[0];
            FirstName = data[1];
            LastName = data[2];
        }

        public string ToCSV()
        {
            string result = $"{StudentId},{FirstName},{LastName},{DateOfBirthDT.ToShortDateString()},{ImageData}";
            return result;
        }

        public override string ToString()
        {
            string result = $"{StudentId} {FirstName} {LastName} { (IsMe == 1 ? "***It is me.***" : "")}";
            return result;
        }
    }
}
