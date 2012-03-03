using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrangeCloudClient
{
    public static class MockServer
    {
        public static Dictionary<int, Repository> ServerRepo = new Dictionary<int, Repository>();

        public static void Start()
        {
            //ServerRepo.Add(1, new Repository(@"C:\dsencloudserver"));
        }

        //public static HashSet<File> Update(HashSet<File> changeset)
        //{

        //    ServerRepo.Last().Value.MergeToRepo(changeset);

        //   // HashSet<File> serverChanges = ServerRepo.Last().Value.GetUploadChangeSet();

        //    //ServerRepo.Last().Value.FilesSnapshot = ServerRepo.Last().Value.GetFiles();
        //    return serverChanges;
        //}

        public static HashSet<File> GetCurrentFileList()
        {
            HashSet<File> files = new HashSet<File>();

            // get the list of all files in the repo and add them to the set (removing the repo DIR)
            foreach (System.IO.FileInfo fileInfo in new System.IO.DirectoryInfo(@"C:\dsencloudserver").GetFiles("*", System.IO.SearchOption.AllDirectories))
            {
                files.Add(new File(fileInfo.FullName, System.IO.File.ReadAllBytes(fileInfo.FullName), fileInfo.LastWriteTimeUtc, 0));
            }

            return files;
        }

        internal static decimal PushFile(File f)
        {
            HashSet<File> current = GetCurrentFileList();

            File serverFile = current.FirstOrDefault(find => find.fullPath == f.fullPath);

            // un gzip
            if ((f.content != null) && (f.content.Length > 0))
            {
                System.IO.MemoryStream mOut = new System.IO.MemoryStream();

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(f.content))
                {
                    using (System.IO.Compression.GZipStream compressed = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress))
                    {
                        compressed.CopyTo(mOut);
                    }

                    f.content = mOut.ToArray();
                }
            }



            if (f.content == null)
            {
                System.IO.File.Delete(@"C:\dsencloudserver" + f.fullPath);
            }
            else if (serverFile == null)
            {
                System.IO.File.WriteAllBytes(@"C:\dsencloudserver" + f.fullPath, f.content);
                return f.version + 1;
            }
            else if (serverFile.version == f.version)
            {
                 System.IO.File.WriteAllBytes(@"C:\dsencloudserver" + f.fullPath, f.content);
                return f.version + 1;
            }

          
            return -1;           
        }
    }
}
