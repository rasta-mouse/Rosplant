using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace RoslynApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var client = new TcpClient();

            do
            {
                await client.ConnectAsync(IPAddress.Loopback, 4444);
            }
            while (!client.Connected);

            var stream = client.GetStream();

            while (true)
            {
                var input = await ReadFromStream(stream);
                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

                var code = input.TrimEnd();
                
                try
                {
                    var scriptState = await CSharpScript.RunAsync(code);
                    await WriteToStream(stream, $"{scriptState?.ReturnValue}");
                }
                catch (Exception e)
                {
                    await WriteToStream(stream, e.Message);
                }
            }
            
            client.Dispose();
        }

        private static async Task<string> ReadFromStream(Stream stream)
        {
            await using var ms = new MemoryStream();
            var read = 0;
            
            do
            {
                var buf = new byte[1024];
                read = await stream.ReadAsync(buf, 0, buf.Length);
                await ms.WriteAsync(buf, 0, read);
            }
            while (read >= 1024);

            var result = ms.ToArray();
            return Encoding.UTF8.GetString(result);
        }

        private static async Task WriteToStream(Stream stream, string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}