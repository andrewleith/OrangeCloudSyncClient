using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace OrangeCloudClient
{
    public class FileWatcher : IDisposable
    {
        FileSystemWatcher fWatcher;

        public FileWatcher(string path, string filter)
        {
            Initialize(path, filter);
        }

        void Initialize(string path, string filter)
        {
            fWatcher = new FileSystemWatcher(path, filter);
            fWatcher.IncludeSubdirectories = true;
            fWatcher.Created += OnFileCreated;
            fWatcher.Changed += OnChanged;
            fWatcher.Deleted += OnChanged;
            fWatcher.Renamed += OnChanged;
            fWatcher.Error += OnError;
        }

        void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            FileInfo lFile = new FileInfo(e.FullPath);
            if (lFile.Exists)
            {
                // schedule the processing on a different thread
                ThreadPool.QueueUserWorkItem(delegate
                {
                    while (true)
                    {
                        // wait 100 milliseconds between attempts to read
                        Thread.Sleep(TimeSpan.FromMilliseconds(100));
                        try
                        {
                            // try to open the file
                            lFile.OpenRead().Close();
                            break;
                        }
                        catch (IOException)
                        {
                            // if the file is still locked, keep trying
                            continue;
                        }
                    }

                    // the file can be opened successfully: raise the event
                    if (Created != null)
                        Created(this, e);
                });
            }
        }

        void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (Created != null)
                Created(this, e);
        }

        void OnError(object sender, ErrorEventArgs e)
        {
            // when an error occurs, the current FileSystemWatcher is disposed,
            // and a new one is created
            string lPath = fWatcher.Path;
            string lFilter = fWatcher.Filter;
            fWatcher.Dispose();
            Initialize(lPath, lFilter);
        }

        public void Start() { fWatcher.EnableRaisingEvents = true; }

        public void Stop() { fWatcher.EnableRaisingEvents = false; }

        public event FileSystemEventHandler Created;

        #region IDisposable Members

        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }

        ~FileWatcher() { Dispose(false); }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                fWatcher.Dispose();
        }

        #endregion
    }
}
