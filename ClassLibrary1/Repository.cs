
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using System.Security.Permissions;

namespace OrangeCloudClient
{
    public enum RepositoryCreationType
    {
        Live, Local
    }

    public enum ModificationType
    {
        Addition, Download, Deletion
    }

    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class Repository
    {

        public Repository(string _repoPath)
        {
            // Repository property initialization
            LocalShare = new HashSet<File>();
            repoPath = _repoPath;
            LiveChanges = new HashSet<File>();

        }

        private HashSet<File> loadMetaData()
        {
            HashSet<File> metaData = new HashSet<File>();

            if (System.IO.File.Exists(Environment.CurrentDirectory + "\\metadata.json"))
            {
                string hashset = System.IO.File.ReadAllText(Environment.CurrentDirectory + "\\metadata.json");

                metaData = JsonConvert.DeserializeObject<HashSet<File>>(hashset);
            }
            else
            {
                Console.WriteLine("No meta data found, starting fresh.");
            }
            return metaData;

        }

        #region "Properties"


        /// <summary>
        /// Full path to where the repo is stored
        /// </summary>
        public string repoPath { get; set; }

        /// <summary>
        /// State of local repository 
        /// </summary>
        public HashSet<File> LocalShare;

        public HashSet<File> ShareMetaData;

        /// <summary>
        /// Set of changes to be sent to server 
        /// </summary>
        public HashSet<File> LiveChanges;

        #endregion

        #region "Methods"


        /// <summary>
        /// Updating the repo:
        /// 1. load saved share meta data (meta data will be as current as last time online user was online)
        /// 2. load current state of share folder
        /// 3. compare the two sets for adds/mods/deletes
        /// 4. push changes to the server, updating meta data on success
        ///    a. if there is a conflicted file, copy it locally using name server gives
        /// 5. ask server for current file list
        /// 6. compare this list against local share
        /// 7. download out-of-date and new files, updating meta data as we go
        /// 8. write out meta data
        /// </summary>
        public void SyncWithTheCloud()
        {
            // 1. load saved share meta data (meta data will be as current as last time online user was online)
            ShareMetaData = this.loadMetaData();

            // 2. load current state of share folder
            LocalShare = this.GetFiles();

            // 3. compare the two sets for adds/mods/deletes
            HashSet<File> changeSet = this.GetChangeSet(this.LocalShare, this.ShareMetaData, new StartupComparer());

            // 4. push changes to the server, updating meta data on success
            foreach (File f in changeSet)
            {
                // differentiate deletes from adds
                if (f.content == null) // delete
                {
                    File deleteResult = OrangeCloudServer.DeleteFile(f);

                    // if the file is null, the server didnt respond
                    // we'll do nothing now and try to push again later
                    if (deleteResult == null)
                    {
                        continue;
                    }
                    // if server returns -1, we have a conflict
                    else if (deleteResult.version == -1)
                    {
                        // do something
                    }
                    // operation was successful with server
                    else
                    {
                        // update file meta data
                        File metaFile = ShareMetaData.Where(file => file.fullPath == f.fullPath).First();

                        if (metaFile != null)
                        {
                            ShareMetaData.Remove(metaFile);
                        }
                    }
                }
                else // add
                {
                    File addResult = OrangeCloudServer.Add(f);

                    // if the file is null, the server didnt respond
                    // we'll do nothing now and try to push again later
                    if (addResult == null)
                    {
                        continue;
                    }
                    // if server returns -1, we have a conflict
                    else if (addResult.version == -1)
                    {
                        // do something
                    }
                    // operation was successful with server
                    else
                    {
                        // update file meta data

                        // if the file is in the meta data already
                        if (ShareMetaData.Contains(f))
                        {
                            // update the version
                            ShareMetaData.SingleOrDefault(file => file.fullPath == f.fullPath).version = addResult.version;
                        }
                        // add it to the meta data
                        else
                        {
                            f.version = addResult.version;
                            ShareMetaData.Add(f);
                        }
                        
                    }
                }

            }

            changeSet.RemoveWhere(files => files.content == null);

            // 5. ask server for current file list
            HashSet<File> ServerList = OrangeCloudServer.GetFiles();

            // 6. compare this list against local share
            HashSet<File> newFiles = this.GetChangeSet(ServerList, LocalShare, new DefaultComparer());

            // 7. download out-of-date and new files, updating meta data as we go
            this.DownloadNewFiles(newFiles);

            // 8. write out meta data
            System.IO.File.WriteAllText(Environment.CurrentDirectory + "\\metadata.json", JsonConvert.SerializeObject(ShareMetaData));

        }

        private void DownloadNewFiles(HashSet<File> newFiles)
        {
            // un gzip

            foreach (File f in newFiles)
            {
                File download = OrangeCloudServer.GetFile(f);

                if (download != null)
                {
                    //if ((download.content != null) && (download.content.Length > 0))
                    //{
                    //    System.IO.MemoryStream mOut = new System.IO.MemoryStream();

                    //    using (System.IO.MemoryStream ms = new System.IO.MemoryStream(download.content))
                    //    {
                    //        using (System.IO.Compression.GZipStream compressed = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress))
                    //        {
                    //            compressed.CopyTo(mOut);
                    //        }

                    //        download.content = mOut.ToArray();
                    //    }
                    //}
                    


                    if (download.content == null)
                    {
                        System.IO.File.Delete(repoPath + download.fullPath);
                        // update meta data
                        ShareMetaData.Remove(download);
                    }
                    else
                    {
                        System.IO.File.WriteAllBytes(repoPath + download.fullPath, download.content);
                        // update meta data
                        ShareMetaData.Add(download);
                    }


                }
            }
        }


        /// <summary>
        /// Get a list of files currently stored on the local machine
        /// </summary>
        /// <param name="repoType">The type of repository to create.</param>
        /// <returns>new RepositoryInformation object</returns>
        private HashSet<File> GetFiles()
        {
            DateTime start = DateTime.Now;
            HashSet<File> files = new HashSet<File>();


            // initalize repo files
            DirectoryInfo repoDir = new DirectoryInfo(repoPath);

            // if the repo doesnt exist, create it
            if (!repoDir.Exists)
            {
                Directory.CreateDirectory(repoPath);
            }

            // get the list of all files in the repo and add them to the set (removing the repo DIR)
            foreach (FileInfo fileInfo in new DirectoryInfo(repoPath).GetFiles("*", SearchOption.AllDirectories))
            {
                files.Add(new File(fileInfo.FullName.Replace(repoPath, ""), System.IO.File.ReadAllBytes(fileInfo.FullName), fileInfo.LastWriteTimeUtc, -1));
            }

            Console.WriteLine("Repo creation: " + DateTime.Now.Subtract(start));

            return files;
        }

        /// <summary>
        /// Get a changeset by comparing two sets of files
        /// </summary>
        /// <param name="Set1"></param>
        /// <param name="Set2"></param>
        /// <param name="ComparerType"></param>
        /// <returns></returns>
        public HashSet<File> GetChangeSet(HashSet<File> Set1, HashSet<File> Set2, IEqualityComparer<File> comparer)
        {
            if (Set1 == null || Set2 == null)
            {
                return new HashSet<File>();
            }

            // set to store additions and modifications (at first
            HashSet<File> ChangeSet = new HashSet<File>(Set1, comparer);

            // set to store deletions
            HashSet<File> ChangeSetDelete = new HashSet<File>(Set2, new DeleteComparer());

            // Remove any identical files from the two sets - this will leave us with only mods and adds
            ChangeSet.ExceptWith(Set2);

            // remove any files with the same names set1, whatever is left is deletes
            ChangeSetDelete.ExceptWith(ChangeSet);
            ChangeSetDelete.ExceptWith(Set1);

            // remove the contents portion of the delete files to mark them as a delete
            foreach (File f in ChangeSetDelete)
            {
                f.content = null;
            }

            // Merge the two sets
            ChangeSet.UnionWith(ChangeSetDelete);

            return ChangeSet;


        }

      

        #endregion




    }
}


