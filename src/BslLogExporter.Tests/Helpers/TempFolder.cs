using System;
using System.IO;

namespace BslLogExporter.Tests.Helpers;

public sealed class TempFolder : IDisposable
{
    public string FullName { get; }

    private TempFolder(string tempFolder)
    {
        FullName = tempFolder;
    }

    public string FullPathTo(string relative)
    {
        return Path.Combine(FullName, relative);
    }

    public void CopyChild(string relativeSourcePath, string relativeTargetPath)
    {
        var source = FullPathTo(relativeSourcePath);
        var target = FullPathTo(relativeTargetPath);
        
        CopyDirectory(source, target);
    }
    
    public static TempFolder FromSourceFolder(string sourceFolder)
    {
        var targetFolder = Directory.CreateTempSubdirectory();

        CopyDirectory(sourceFolder, targetFolder.FullName);

        return new TempFolder(targetFolder.FullName);
    }
    
    public void Dispose()
    {
        Directory.Delete(FullName, true);
    }

    private static void CopyDirectory(string sourceFolder, string targetFolder)
    {
        try
        {
            foreach (var dirPath in Directory.GetDirectories(sourceFolder, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(NewPath(dirPath));
            }
        
            foreach (var file in Directory.GetFiles(sourceFolder, "*.*",SearchOption.AllDirectories))
            {
                File.Copy(file, NewPath(file), true);
            }
        }
        catch
        {
            Directory.Delete(targetFolder, true);
            throw;
        }
        
        string NewPath(string sourcePath)
        {
            return sourcePath.Replace(sourceFolder, targetFolder);
        }
    }
}