using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace OrangeCloudClient
{
    [Serializable]
    public class Files : IEnumerable<CloudFile>
    {
        public Dictionary<string, CloudFile> Items = new Dictionary<string, CloudFile>();

        #region IEnumerable<CloudFile> Members

        public IEnumerator<CloudFile> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class FilesEnum : IEnumerator<CloudFile>
    {
        #region IEnumerator<CloudFile> Members

        public CloudFile Current
        {
            get 
            {
                try
                {
                    return CloudFile[position]
                }
                catch (Exception)
                {
                    
                    throw;
                }
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current
        {
            get { throw new NotImplementedException(); }
        }

        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
