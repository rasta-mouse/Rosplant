using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using PrettyPrompt;
using PrettyPrompt.Consoles;

namespace RoslynServer
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var console = new SystemConsole();
            var prompt = new Prompt(console: console);
            
            var listener = new TcpListener(IPAddress.Any, 4444);
            listener.Start();
            
            console.Write("Waiting for client... ");
            var client = await listener.AcceptTcpClientAsync();
            console.Write("connected!\n");
            
            var stream = client.GetStream();

            do
            {
                var input = await prompt.ReadLineAsync("> ");
                if (!input.IsSuccess) continue;
                if (string.IsNullOrEmpty(input.Text)) continue;
                if (input.Text.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

                var code = input.Text.TrimEnd();
                await WriteToStream(stream, code);

                var result = await ReadFromStream(stream);
                console.WriteLine(result);
            }
            while (true);
            
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