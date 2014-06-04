using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSocket.Portable.Compression;

namespace WebSocket.Portable.Tests.Compression
{
    [TestClass]
    public class DeflateStreamTests
    {
        [TestMethod]
        public void CompressAndDecompressTest()
        {
            CompressAndDecompressTest(true, true);
        }

        [TestMethod]
        public void BuildinCompressAndDecompressTest()
        {
            CompressAndDecompressTest(false, false);
        }

        [TestMethod]
        public void CompatibleCompressAndBuildinDecompressTest()
        {
            CompressAndDecompressTest(true, false);
        }

        [TestMethod]
        public void CompatibleBuildinCompressAndDecompressTest()
        {
            CompressAndDecompressTest(false, true);
        }

        private static void CompressAndDecompressTest(bool ourCompress, bool ourDecompress)
        {
            using (var compressed = new MemoryStream())
            {
                CompressData(compressed, ourCompress);
                compressed.Position = 0;

                using (var decompressed = new MemoryStream())
                {
                    Decompress(compressed, decompressed, ourDecompress);
                    decompressed.Position = 0;

                    var bytes = decompressed.ToArray();
                    var data = Encoding.UTF8.GetString(bytes);

                    Assert.AreEqual(Data, data);
                }
            }
        }

        private static void CompressData(Stream output, bool our)
        {
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes(Data)))
            {
                Compress(input, output, our);
            }
        }

        private static void Compress(Stream input, Stream output, bool our)
        {
            using (var deflateStream = CreateDeflateStream(output, true, our))
            {
                input.CopyTo(deflateStream);
            }
        }

        private static void Decompress(Stream input, Stream output, bool our)
        {
            using (var deflateStream = CreateDeflateStream(input, false, our))
            {
                deflateStream.CopyTo(output);
            }
        }

        private static Stream CreateDeflateStream(Stream input, bool compress, bool our)
        {
            if (our)
            {
                return compress ? new DeflateStream(input, CompressionMode.Compress, true) : new DeflateStream(input, CompressionMode.Decompress, true);
            }
            return compress ? new System.IO.Compression.DeflateStream(input, System.IO.Compression.CompressionMode.Compress, true) : new System.IO.Compression.DeflateStream(input, System.IO.Compression.CompressionMode.Decompress, true);
        }


        private const string Data =
            "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.";
    }
}
