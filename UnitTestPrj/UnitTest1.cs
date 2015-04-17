using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestPrj
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            string strName = " 1．对高热患者的观察，不正确的是（ ）".Trim();
            Match firstMatch = Regex.Match(strName, @"^(\d)[．]", RegexOptions.Singleline);
            if (firstMatch.Success)
            {

            }
        }
    }
}
