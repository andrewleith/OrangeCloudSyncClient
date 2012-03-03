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

    /// <summary>
    /// Represents a file in the cloud.  
    /// </summary>
    [Serializable]
    public class File
    {
        [JsonConstructor]
        public File()
        {
        }

        public File(string fullfilepath, byte[] fileContents, long version)
            : this(fullfilepath,fileContents, new System.IO.FileInfo(fullfilepath).LastAccessTime, version)
        {
        }

        public File(string fullFilePath, byte[] fileContents, DateTime lastModified, long version)
        {
            // init properties
            this.fullPath = fullFilePath;
            this.timestamp = lastModified;
            this.version = version;

                      

            // compress the file using GZip
            if (fileContents.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (GZipStream compressed = new GZipStream(ms, CompressionMode.Compress))
                    {
                        compressed.Write(fileContents, 0, fileContents.Length);
                    }

                    this.content = ms.ToArray();
                }
            }

            // calculate its checksun
            this.ComputeMD5Hash();

        }

        #region "Equality"
        public static bool operator ==(File a, File b)
        {

            if (ReferenceEquals(b, null) && ReferenceEquals(a, null))
            {
                return true;
            }

            if (ReferenceEquals(a, null) && !ReferenceEquals(b, null))
            {
                return false;
            }

            if (ReferenceEquals(b, null) && !ReferenceEquals(a, null))
            {
                return false;
            }



            // if the files are the same path 
            if (a.fullPath == b.fullPath)
            {
                // .. and the same version, they are the same
                if (a.version == b.version)
                {
                    return true;
                }
                else
                {
                    return false;
                }
                //else // if the version has changed, lets look to checksums
                //{
                //    // if we dont have a checksum for this object, calculate one
                //    if (String.IsNullOrEmpty(a.checksum))
                //    {
                //        a.ComputeMD5Hash();
                //    }
                //    // if we dont have acheckusm for the object we are comparing, calculate one
                //    if (String.IsNullOrEmpty(b.checksum))
                //    {
                //        b.ComputeMD5Hash();
                //    }

                //    // if all the conditions that led here + this one are true, they are the same file
                //    if (a.checksum == b.checksum)
                //    {
                //        return true;
                //    }
                //    else
                //    {
                //        return false;
                //    }
                //}
            }
            else // not the same name and size
            {
                return false;
            }
        }
        public static bool operator !=(File a, File b)
        {
            return !(a == b);
        }
        public override bool Equals(Object obj)
        {
            if (!(obj is File))
            {
                return false;
            }
            else
            {
                return Equals((File)obj);
            }
        }
        public bool Equals(File cf)
        {


            if (ReferenceEquals(this, null) && !ReferenceEquals(cf, null))
            {
                return false;
            }

            if (ReferenceEquals(cf, null) && !ReferenceEquals(this, null))
            {
                return false;
            }


            // if the files are the same path..
            if (this.fullPath == cf.fullPath)
            {
                // .. and the same version, they are the same
                if (this.version == cf.version)
                {
                    return true;
                }
                else
                {
                    return false;
                }
                //else // if the timestamp has changed, lets look to checksums
                //{
                //    // if we dont have a checksum for this object, calculate one
                //    if (String.IsNullOrEmpty(this.checksum))
                //    {
                //        this.ComputeMD5Hash();
                //    }
                //    // if we dont have a checkusm for the object we are comparing, calculate one
                //    if (String.IsNullOrEmpty(cf.checksum))
                //    {
                //        cf.ComputeMD5Hash();
                //    }

                //    // if all the conditions that led here + they have the same checksum, they are the same file
                //    if (this.checksum == cf.checksum)
                //    {
                //        return true;
                //    }
                //    else
                //    {
                //        return false;
                //    }
                //}
            }
            else // not the same name and size
            {
                return false;
            }
        }


        public override int GetHashCode()
        {
            return this.fullPath.GetHashCode();
        }
        #endregion


        #region "Properties"

        /// <summary>
        /// Full directory and filename of the file
        /// </summary>
        public string fullPath { get; set; }

        /// <summary>
        /// timestamp of the file
        /// </summary>
        public DateTime timestamp { get; set; }

        /// <summary>
        /// GZipped File contents
        /// </summary>
        public byte[] content;

        /// <summary>
        /// MD5 Hash of file
        /// </summary>
        public string checksum { get; set; }

        /// <summary>
        /// Version of the file, set by the server
        /// </summary>
        public long version { get; set; }
        #endregion

        #region "Methods"


        /// <summary>
        /// Use JSON.Net to convert this object into JSON notation.
        /// If the file is binary it will automatically encode it as Base64.
        /// </summary>
        /// <returns>JSONified string representation of this object.</returns>
        public string saveAsJSON()
        {
            return JsonConvert.SerializeObject(this).ToString();
        }

        public static File loadFromJSON(string contents)
        {
            return JsonConvert.DeserializeObject<File>(contents);
        }

        public void ComputeMD5Hash()
        {
            if (this.content != null)
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(this.content);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }

                this.checksum = sb.ToString();
            }
        }


        #endregion
    }

    /// <summary>
    /// For a deletion, if the full file name is the same, they are equal.
    /// </summary>
    public class DeleteComparer : IEqualityComparer<File>
    {
        #region IEqualityComparer<File> Members

        public bool Equals(File x, File y)
        {
            if (x.fullPath == y.fullPath)
            {
                return true;
            }
            else
            {
                return false;
            }


        }

        public int GetHashCode(File obj)
        {
            return base.GetHashCode();
        }

        #endregion
    }


    public class DefaultComparer : IEqualityComparer<File>
    {

        public bool Equals(File x, File y)
        {
            if (ReferenceEquals(y, null) && ReferenceEquals(x, null))
            {
                return true;
            }

            if (ReferenceEquals(x, null) && !ReferenceEquals(y, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null) && !ReferenceEquals(x, null))
            {
                return false;
            }



            // if the files are the same path 
            if (x.fullPath == y.fullPath)
            {
                // .. and the same version, they are the same
                if (x.version == y.version)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else // not the same name and size
            {
                return false;
            }
        }
        public int GetHashCode(File obj)
        {
            return base.GetHashCode();
        }

    }


    /// <summary>
    /// When comparing the local share vs the saved metadata, versions wont ever match, so we need to go down to the checksum to see if they are equal.
    /// </summary>
    public class StartupComparer : IEqualityComparer<File>
    {
        #region IEqualityComparer<File> Members

        public bool Equals(File x, File y)
        {
            // if the files are the same path..
            if (x.fullPath == y.fullPath)
            {
                // if we dont have a checksum for this object, calculate one
                if (String.IsNullOrEmpty(x.checksum))
                {
                    x.ComputeMD5Hash();
                }
                // if we dont have a checkusm for the object we are comparing, calculate one
                if (String.IsNullOrEmpty(y.checksum))
                {
                    x.ComputeMD5Hash();
                }

                // if all the conditions that led here + they have the same checksum, they are the same file
                if (x.checksum == y.checksum)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else // not the same name and contents
            {
                return false;
            }


        }

        public int GetHashCode(File obj)
        {
            return base.GetHashCode();
        }

        #endregion
    }

}
