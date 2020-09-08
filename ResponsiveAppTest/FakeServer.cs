using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResponsiveAppTest
{
    class FakeServer
    {
        private int bytesTotal;
        private int byteToGet = 0;
        private Random random;

        public FakeServer()
        {
            random = new Random();
            bytesTotal = 10000; ;
        }

        public int BytesRemaining()
        {
            return bytesTotal - byteToGet;
        }

        public byte[] GetBytes(int count)
        {
            byte[] bytes = new byte[count];
            random.NextBytes(bytes);
            byteToGet += count;
            return bytes;
        }
    }
}
