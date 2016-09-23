using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CopyDir
{
  class Program
  {
    private static bool IsDryRun = false;
    private static bool IsRegex = false;
    private static int copyCount = 0;
    static void Main(string[] args)
    {
      var usage = @"usage: CopyDir.exe destinationDir [files ...]
The source directory is current dir. You can use stdin for files. Specified files by relative path from current dir.
Options:
  -d dry run.
  -r use regex for files.
";

      var argList = new List<string>();
      foreach (var arg in args)
      {
        if (arg == "-d")
        {
          IsDryRun = true;
        }
        else if (arg == "-r")
        {
          IsRegex = true;
        }
        else
        {
          argList.Add(arg);
        }
      }

      if (argList.Count < 1)
      {
        Console.WriteLine(usage);
        return;
      }


      var sourceDir = Directory.GetCurrentDirectory();
      var destDir = argList[0];
      if(destDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
      {
        destDir = destDir.Substring(0, destDir.Length - 1);
      }

      var patterns = new List<Regex>();
      var files = new List<string>();
      if(argList.Count > 1)
      {
        var targets = argList.Skip(1);
        if(IsRegex)
        {
          var pathSep = Path.DirectorySeparatorChar.ToString();
          if (pathSep == "\\")
          {
            pathSep = "\\\\";
          }
          patterns.AddRange(targets.Select(file => new Regex(file.Replace("/", pathSep))));
        }
        else
        {
          files.AddRange(targets.Select(file => file.Replace("/", Path.DirectorySeparatorChar.ToString())));
        }
        
      }

      //read stdin
      if(Console.IsInputRedirected)
      {
        var input = Console.In;
        string line;
        while ((line = input.ReadLine()) != null)
        {
          files.Add(line.Replace("/", Path.DirectorySeparatorChar.ToString()));
        }
      }

      if(IsDryRun)
      {
        Console.WriteLine("Dry run...");
      }

      CopyFiles(sourceDir, destDir, sourceDir, files, patterns);
      Console.WriteLine(copyCount.ToString("N0") + " files.");
    }

    private static void CopyFiles(string from, string to, string targetDir, List<string> files, List<Regex>patterns)
    {
      foreach (var sourceFile in Directory.GetFiles(targetDir))
      {
        var isCopied = false;
        var destFile = sourceFile.Replace(from, to);
        var relativeSourceFile = sourceFile.Replace(from + Path.DirectorySeparatorChar, "");

        foreach (var file in files)
        {
          if (relativeSourceFile == file)
          {
            CopyFiles(sourceFile, destFile);
            isCopied = true;
          }
        }

        foreach (var pttern in patterns)
        {
          var match = pttern.Match(relativeSourceFile);
          if (match.Success)
          {
            CopyFiles(sourceFile, destFile);
            isCopied = true;
          }
        }

        //何も指定されていなかったらすべてコピー
        if(files.Count == 0 && patterns.Count == 0)
        {
          CopyFiles(sourceFile, destFile);
          isCopied = true;
        }

        if(isCopied)
        {
          ++copyCount;
        }
      }

      foreach (var dir in Directory.GetDirectories(targetDir))
      {
        CopyFiles(from, to, dir, files, patterns);
      }
    }

    private static void CopyFiles(string sourceFile, string destFile)
    {
      if (File.Exists(destFile))
      {
        Console.WriteLine("Already exists " + destFile);
        return;
      }

      if(!IsDryRun)
      {
        var dir = Path.GetDirectoryName(destFile);
        if (!Directory.Exists(dir))
        {
          Directory.CreateDirectory(dir);
        }

        File.Copy(sourceFile, destFile);
      }
      
      Console.WriteLine(sourceFile + " > " + destFile);
    }


  }
}
