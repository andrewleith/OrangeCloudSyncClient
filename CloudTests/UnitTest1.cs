using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrangeCloudClient;

namespace CloudTests
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        public UnitTest1()
        {
            //
            // TODO: Add constructor logic here
            //
        }

  

        /// <summary>
        /// Test to see if a file add can be seen correctly by other clients
        /// </summary>
        [TestMethod]
        public void TestFileAdd()
        {
            string client1repo = @"C:\TEMP\dsencloudclient1";
            string client2repo = @"C:\TEMP\dsencloudclient2";
            string fileNameToCreate = @"\test.txt";

            Repository client1 = new Repository(client1repo);
            Repository client2 = new Repository(client2repo);

            // write a file to client 1's local share
            System.IO.Directory.CreateDirectory(client1repo);
            System.IO.Directory.CreateDirectory(client2repo);

            System.IO.File.WriteAllText(client1repo + fileNameToCreate, "New file creation");

            // sync client 1 with the cloud
            client1.SyncWithTheCloud();

            // sync client 2 with the cloud
            client2.SyncWithTheCloud();

            // ensure that both clients see this file in their meta data
            Assert.IsTrue(client1.ShareMetaData.Where(file => file.fullPath == fileNameToCreate).Count() == 1, 
                "Client1 has no record in metadata of: " + fileNameToCreate);
            Assert.IsTrue(client2.ShareMetaData.Where(file => file.fullPath == fileNameToCreate).Count() == 1, 
                "Client2 has no record in metadata of: " + fileNameToCreate);

            // ensure that the file exists on disk for both clients
            Assert.IsTrue(System.IO.File.Exists(client1repo + fileNameToCreate),
                "Client1 has no file in repository for: " + fileNameToCreate);
            Assert.IsTrue(System.IO.File.Exists(client2repo + fileNameToCreate),
                "Client1 has no file in repository for: " + fileNameToCreate);

            // Ensure the file is versioned as 1 on both clients
            Assert.AreEqual(client1.ShareMetaData.First(file => file.fullPath == fileNameToCreate).version, 1);
            Assert.AreEqual(client2.ShareMetaData.First(file => file.fullPath == fileNameToCreate).version, 1);
        }

        [TestMethod]
        public void TestFileDelete()
        {
            Console.WriteLine("yAEHAEH");
        }
    }
}
