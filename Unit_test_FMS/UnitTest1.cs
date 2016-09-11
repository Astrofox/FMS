using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using File_Manager_System;
using File_Manager_System.IO;
using System.IO;

namespace Unit_test_FMS
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            My_File file = new My_File("D:\\124.txt");

            Assert.AreEqual(false, file.Exists());
        }

        [TestMethod]
        public void TestMethod2()
        {
            My_File file = new My_File("D:\\124.txt");

            file.Create();
            Assert.AreEqual(true, file.Exists());
        }

        [TestMethod]
        public void TestMethod3()
        {
            string srch_result = Model_Presenter.Search("D:\\New\\3.txt");

            Assert.AreEqual("Telephone number: +79315363226\r\n", srch_result);
        }

        [TestMethod]
        public void TestMethod4()
        {
            My_File file = new My_File("D:\\124.txt");
            file.Delete();
            Assert.AreEqual(false, file.Exists());
        }

        [TestMethod]
        public void TestMethod5()
        {
            My_ZipArchive file = new My_ZipArchive("D:\\New\\1.zip");
            file.Dearchivefile("New\\3.txt");
            Assert.AreEqual(true, new My_File("D:\\Restart\\AF\\VS_Projects\\tmp\\New\\3.txt").Exists());
        }

        [TestMethod]
        public void TestMethod6()
        {
            My_File file = new My_File("D:\\124.txt");

            Assert.AreEqual(false, file.Exists());
        }
    }
}
