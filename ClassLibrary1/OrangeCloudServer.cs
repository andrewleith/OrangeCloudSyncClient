using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.IO;
using Newtonsoft.Json;

namespace OrangeCloudClient
{
    public static class OrangeCloudServer
    {

        /// <summary>
        /// helper method - needs work
        /// </summary>
        /// <param name="action"></param>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static WebRequest NewRequest(string action, string method, string args)
        {
            string actualargs = string.Empty;
            
            if (!string.IsNullOrEmpty(args))
            {
                actualargs = "?" + HttpUtility.HtmlEncode(HttpUtility.HtmlEncode(args));
            }

            WebRequest request = WebRequest.Create(OrangeCloudClient.Properties.Settings.Default["ServerUrl"].ToString() + "/" + action + actualargs);

            request.Method = method;
            request.Headers.Add("Username:userone");
            request.Headers.Add("Password:password");

            return request;

        }

        /// <summary>
        /// Add a file to the cloud
        /// </summary>
        /// <param name="f">file to add</param>
        /// <returns>File object with updated version number</returns>
        internal static File Add(File f)
        {

            string postData = "content=" + Newtonsoft.Json.JsonConvert.SerializeObject(f.content) +
                              "&fullpath=" + f.fullPath +
                              "&checksum=" + f.checksum +
                              "&base_version=" + f.version;

            WebRequest req = OrangeCloudServer.NewRequest("add", "POST", null);
            req.ContentType = "application/x-www-form-urlencoded";

            try
            {

                byte[] write = Encoding.UTF8.GetBytes(postData);
                req.ContentLength = postData.Length;
                Stream dataStream = req.GetRequestStream();
                StreamWriter sw = new StreamWriter(dataStream, Encoding.UTF8);
                sw.Write(postData);
                sw.Close();
                dataStream.Close();

                WebResponse resp = req.GetResponse();
                MemoryStream response = new MemoryStream();

                // Allocate a 1k buffer
                byte[] buffer = new byte[1024];
                int bytesRead;

                // Simple do/while loop to read from stream until
                // no bytes are returned
                do
                {
                    // Read data (up to 1k) from the stream
                    bytesRead = resp.GetResponseStream().Read(buffer, 0, buffer.Length);

                    // Write the data to the local file
                    response.Write(buffer, 0, bytesRead);

                } while (bytesRead > 0);

                response.Seek(0, SeekOrigin.Begin);

                if (response.Length > 0)
                {
                    StreamReader sr = new StreamReader(response);
                    return JsonConvert.DeserializeObject<File>(sr.ReadToEnd());
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error getting file: {0} : {1} ({2})", f.fullPath, f.version, ex.Message));

                return null;
            }


        }
       
        /// <summary>
        /// Get the current file listing from the server
        /// </summary>
        /// <returns>Hashset of File objects</returns>
        internal static HashSet<File> GetFiles()
        {
            WebRequest req = OrangeCloudServer.NewRequest("files", "GET", null);



            WebResponse resp;
            MemoryStream response = new MemoryStream();

            try
            {
                resp = req.GetResponse();

                // Allocate a 1k buffer
                byte[] buffer = new byte[1024];
                int bytesRead;

                // Simple do/while loop to read from stream until
                // no bytes are returned
                do
                {
                    // Read data (up to 1k) from the stream
                    bytesRead = resp.GetResponseStream().Read(buffer, 0, buffer.Length);

                    // Write the data to the local file
                    response.Write(buffer, 0, bytesRead);

                } while (bytesRead > 0);

                response.Seek(0, SeekOrigin.Begin);

                if (response.Length > 0)
                {
                    StreamReader sr = new StreamReader(response);

                    return Newtonsoft.Json.JsonConvert.DeserializeObject<HashSet<File>>(sr.ReadToEnd());
                }
                else
                {
                    return null;
                }

            }
            catch (Exception)
            {
                Console.WriteLine(string.Format("Error getting file listing"));
            }

            return null;
        }

        /// <summary>
        /// Download file <paramref name="f"/> from server.
        /// </summary>
        /// <param name="f">File to download</param>
        /// <returns>File object, including content</returns>
        internal static File GetFile(File f)
        {

            string postData = String.Format("fullpath={0}", f.fullPath);
            WebRequest req = OrangeCloudServer.NewRequest("get", "POST", null);

            req.ContentType = "application/x-www-form-urlencoded";
            WebResponse resp;
            MemoryStream response = new MemoryStream();

            try
            {
                req.ContentLength = postData.Length;
                Stream dataStream = req.GetRequestStream();
                StreamWriter sw = new StreamWriter(dataStream);
                sw.Write(postData);
                sw.Close();
                dataStream.Close();

                resp = req.GetResponse();

                // Allocate a 1k buffer
                byte[] buffer = new byte[1024];
                int bytesRead;

                // Simple do/while loop to read from stream until
                // no bytes are returned
                do
                {
                    // Read data (up to 1k) from the stream
                    bytesRead = resp.GetResponseStream().Read(buffer, 0, buffer.Length);

                    // Write the data to the local file
                    response.Write(buffer, 0, bytesRead);

                } while (bytesRead > 0);

                response.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception)
            {
                Console.WriteLine(string.Format("Error getting file: {0} : {1}", f.fullPath, f.version));
            }

            if (response.Length > 0)
            {
                StreamReader sr = new StreamReader(response);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<File>(sr.ReadToEnd());
            }
            else
            {
                return null;
            }


        }

        /// <summary>
        /// Not implemented yet
        /// </summary>
        /// <param name="f"></param>
        /// <param name="newFullPath"></param>
        internal static void RenameFile(File f, string newFullPath)
        {
            string postData = String.Format("fullpath={0}&newfullpath={1}", f.fullPath, newFullPath);
            WebRequest req = OrangeCloudServer.NewRequest("rename", "POST", null);


            WebResponse resp = req.GetResponse();


        }

        /// <summary>
        /// Not implemented yet
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        internal static File DeleteFile(File f)
        {

            string postData = "content=&fullpath=" + f.fullPath +
                              "&checksum=" + f.checksum +
                              "&base_version=" + f.version;

            WebRequest req = OrangeCloudServer.NewRequest("add", "POST", null);

            req.ContentLength = postData.Length;
            Stream dataStream = req.GetRequestStream();
            StreamWriter sw = new StreamWriter(dataStream);
            sw.Write(postData);
            sw.Close();
            dataStream.Close();

            WebResponse resp;
            MemoryStream response = new MemoryStream();

            try
            {
                resp = req.GetResponse();

                // Allocate a 1k buffer
                byte[] buffer = new byte[1024];
                int bytesRead;

                // Simple do/while loop to read from stream until
                // no bytes are returned
                do
                {
                    // Read data (up to 1k) from the stream
                    bytesRead = resp.GetResponseStream().Read(buffer, 0, buffer.Length);

                    // Write the data to the local file
                    response.Write(buffer, 0, bytesRead);

                } while (bytesRead > 0);

                response.Seek(0, SeekOrigin.Begin);

            }
            catch (Exception)
            {
                Console.WriteLine(string.Format("Error deleting file: {0} : {1}", f.fullPath, f.version));
            }


            if (response.Length > 0)
            {
                StreamReader sr = new StreamReader(response);
                return JsonConvert.DeserializeObject<File>(sr.ReadToEnd());
            }
            else
            {
                return null;
            }



        }
    }
}
