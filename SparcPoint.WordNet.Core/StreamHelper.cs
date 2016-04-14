using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparcPoint.WordNet
{
    static class StreamHelper
    {
        public static async Task<string> ReadLineAsync(this Stream stream, byte newLineChar)
        {
            if (stream.Position == stream.Length) return null;

            byte findByte = Convert.ToByte(newLineChar);
            bool eofFound = false;

            List<byte> byteList = new List<byte>(128);
            byte[] buffer = new byte[128];
            
            while(true) {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    eofFound = true;
                    break;
                }

                // Check for Less Bytes than Desired
                int foundIndex = -1;
                for(int i = 0; i < bytesRead; i++)
                {
                    if (buffer[i] == findByte)
                    {
                        foundIndex = i;
                        break;
                    }
                    byteList.Add(buffer[i]);
                }

                // Seek Back the necessary number of bytes
                if (foundIndex > -1)
                {
                    int seekBack = foundIndex - (bytesRead - 1);
                    stream.Seek(seekBack, SeekOrigin.Current);
                    break;
                }
            }

            if (eofFound && byteList.Count() == 0) return null;

            return ASCIIEncoding.ASCII.GetString(byteList.ToArray());
        }
    }
}
