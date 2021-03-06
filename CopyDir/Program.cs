﻿using System;
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
    private static bool IsForceOverWrite = false;
    private static bool HasSourceDir = false;
    private static int copyCount = 0;
    static void Main(string[] args)
    {
      var usage = @"usage: CopyDir.exe destinationDir [files ...]
       CopyDir.exe sourceDir destinationDir [files ...]
The source directory is current dir. You can use stdin for files. Specified files by relative path from current dir.
Options:
  -d dry run.
  -r use regex for files.
  -f force over write.
  -s specify source dir. if not use this option, the source dir is current dir.
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
        else if (arg == "-f")
        {
          IsForceOverWrite = true;
        }
        else if (arg == "-s")
        {
          HasSourceDir = true;
        }
        else
        {
          argList.Add(arg);
        }
      }

      var skipCount = 0;
      var sourceDir = "";
      var destDir = "";
      if(HasSourceDir)
      {
        skipCount = 2;
        if (argList.Count < skipCount)
        {
          Console.WriteLine(usage);
          return;
        }

        sourceDir = argList[0];
        destDir = argList[1];
      }
      else
      {
        skipCount = 1;
        if (argList.Count < skipCount)
        {
          Console.WriteLine(usage);
          return;
        }

        sourceDir = Directory.GetCurrentDirectory();
        destDir = argList[0];
      }

      if (sourceDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
      {
        sourceDir = sourceDir.Substring(0, sourceDir.Length - 1);
      }

      if(destDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
      {
        destDir = destDir.Substring(0, destDir.Length - 1);
      }

      var patterns = new List<Regex>();
      var files = new List<string>();
      if (argList.Count > skipCount)
      {
        var targets = argList.Skip(skipCount);
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
      if (!IsForceOverWrite && File.Exists(destFile))
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

        File.Copy(sourceFile, destFile, IsForceOverWrite);
      }
      
      Console.WriteLine(sourceFile + " > " + destFile);
    }


  }
}
