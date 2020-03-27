using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Threading;

namespace FTPApp.Models
{
    public class FTP
    {
        public static List<string> GetDirectory(string url, string username = Constants.FTP.UserName, string password = Constants.FTP.Password)
        {
            List<string> output = new List<string>();

            //Build the WebRequest
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);

            request.Credentials = new NetworkCredential(username, password);

            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.EnableSsl = false;

            //Connect to the FTP server and prepare a Response
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                //Get a reference to the Response stream
                using (Stream responseStream = response.GetResponseStream())
                {
                    //Read the Response stream
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        //Retrieve the entire contents of the Response
                        string responseString = reader.ReadToEnd();

                        //Split the response by Carriage Return and Line Feed character to return a list of directories
                        output = responseString.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                }

                Console.WriteLine($"Directory List Complete with status code: {response.StatusDescription}");
            }

            return (output);
        }
        
        /// <summary>
        /// Tests to determine whether a file exists on an FTP site
        /// </summary>
        /// <param name="remoteFileUrl"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool FileExists(string remoteFileUrl, string username = Constants.FTP.UserName, string password = Constants.FTP.Password)
        {
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remoteFileUrl);

            //Specify the method of transaction
            request.Method = WebRequestMethods.Ftp.GetFileSize;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential(username, password);

            try
            {
                //Create an instance of a Response object
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode ==
                    FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    //Does not exist
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Retreives the contents of a file from an FTP site into an in-memory byte array
        /// </summary>
        /// <param name="sourceFileUrl"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static byte[] DownloadFileBytes(string sourceFileUrl, string username = Constants.FTP.UserName, string password = Constants.FTP.Password)
        {
            byte[] output;

            try
            {
                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(sourceFileUrl);

                //Specify the method of transaction
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential(username, password);

                //Create an instance of a Response object
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    //Request a Response from the server
                    output = ToByteArray(response.GetResponseStream());

                    //Thread.Sleep(Constants.FTP.OperationPauseTime);

                    //Return the output of the Response
                    return output;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return new byte[0];
        }

        /// <summary>
        /// Convert a Stream to a byte array
        /// </summary>
        /// <param name="stream">A Stream Object</param>
        /// <returns>Array of bytes from the Stream</returns>
        public static byte[] ToByteArray(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] chunk = new byte[1024];
                int bytesRead;
                while ((bytesRead = stream.Read(chunk, 0, chunk.Length)) > 0)
                {
                    ms.Write(chunk, 0, bytesRead);
                }

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Uploads a file to an FTP site
        /// </summary>
        /// <param name="sourceFilePath">Local file</param>
        /// <param name="destinationFileUrl">Destination Url</param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string UploadFile(string sourceFilePath, string destinationFileUrl, string username = Constants.FTP.UserName, string password = Constants.FTP.Password)
        {
            string output;

            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(destinationFileUrl);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // Credential
            request.Credentials = new NetworkCredential(username, password);

            // Copy the contents of the file to the request stream.
            byte[] fileContents;
            using (StreamReader sourceStream = new StreamReader(sourceFilePath))
            {
                fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            }

            //Get the length or size of the file
            request.ContentLength = fileContents.Length;

            //Write the file to the stream on the server
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(fileContents, 0, fileContents.Length);
            }

            //Send the request
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                output = $"Upload File Complete, status {response.StatusDescription}";
            }
            Thread.Sleep(Constants.FTP.OperationPauseTime);

            return (output);
        }
    }
}
