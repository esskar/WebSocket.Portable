using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSocket.Portable.Security;

namespace WebSocket.Portable.Tests.Security
{
    [TestClass]
    public class Sha1DigestTests
    {
        [TestMethod]
        public void TestVectors()
        {
            AssertSha1("The quick brown fox jumps over the lazy dog", "L9ThxnotKPzthJ7hu3bnORuT6xI=");
            AssertSha1("The quick brown fox jumps over the lazy cog", "3p8sf9JeGzr60+haC9F9mxANtLM=");
            AssertSha1("", "2jmj7l5rSw0yVb/vlWAYkK/YBwk=");
        }

        private static void AssertSha1(string input, string expected)
        {
            var hash = Sha1Digest.ComputeHash(Encoding.ASCII.GetBytes(input));
            var actual = Convert.ToBase64String(hash);

            Assert.AreEqual(expected, actual, "Invalid hash for input '{0}'", input);
        }
    }    
}
