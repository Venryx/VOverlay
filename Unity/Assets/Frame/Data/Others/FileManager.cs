using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;

public enum BuildType
{
	Development,
	ReleaseTester,
	Release
}
public static class FileManager
{
	//public static char PathSep = Path.DirectorySeparatorChar;
	public static char PathSep = '/';
	public static DirectoryInfo root = Application.isEditor ? new DirectoryInfo(Application.dataPath).Parent.GetFolder("Root_Development") : new DirectoryInfo(Application.dataPath).Parent;
	public static DirectoryInfo unityRoot = Application.isEditor ? new DirectoryInfo(Application.dataPath).Parent : null;
	
	public static DirectoryInfo GetFolder(string subpath = null) { return root.GetFolder(subpath); }
	public static FileInfo GetFile(string subpath = null) { return root.GetFile(subpath); }
	public static BuildType GetBuildType()
	{
		//var rootPath = FormatPath(root.FullName);
		if (Application.isEditor)
			return BuildType.Development;
		if (root.Name.Contains("Tester")) //rootPath.StartsWith("C:/Projects/Unity/World/Releases/Creative Defense/")) //|| rootPath.StartsWith("C:/Others/Drive/Public/Games/Creative Defense/Releases/"))
			return BuildType.ReleaseTester;
		return BuildType.Release;
	}

	//public static string SimplifyPath(string path)
	public static string FormatPath(string path)
	{
		//var result = path.Replace('\\', '/');
		var result = path.Replace('/', PathSep).Replace('\\', PathSep);
		/*if (result.EndsWith("/")) // if final path-sep, remove it
			result = result.Substring(0, result.Length - 1);*/
		if (!result.EndsWith(PathSep.ToString())) // if final path-sep not-existing, add it
			result += PathSep;
		return result;
	}
	public static void CreateFoldersInPathIfMissing(string path)
	{
		path = FormatPath(path);
		
		string folder = path.Substring(0, path.LastIndexOf(PathSep) + 1);
		if (!Directory.Exists(folder))
			Directory.CreateDirectory(folder);
	}

	public static string GetFullPath(string pathFromRoot) { return FormatPath(root.VFullName() + pathFromRoot); }
	public static string GetRelativePath(string fullPath) { return FormatPath(fullPath).Replace(root.VFullName(), ""); }
}