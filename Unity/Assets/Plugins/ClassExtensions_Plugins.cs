using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public static class ClassExtensions_Plugins {
	// object
	public static VNullClass SetMeta(this object obj, object metaKey, VNullClass metaValue, bool useStrongStorage = true) { return VMeta.main.SetMeta(obj, metaKey, metaValue, useStrongStorage); } // for null
	public static T SetMeta<T>(this object obj, object metaKey, T metaValue, bool useStrongStorage = true) { return VMeta.main.SetMeta(obj, metaKey, metaValue, useStrongStorage); }
	public static T GetMeta<T>(this object obj, object metaKey) { return VMeta.main.GetMeta<T>(obj, metaKey); }
	public static object GetMeta(this object obj, object metaKey) { return VMeta.main.GetMeta(obj, metaKey); }
	public static T GetMeta<T>(this object obj, object metaKey, T returnValueIfMissing, bool useStrongStorage = true) { return VMeta.main.GetMeta(obj, metaKey, returnValueIfMissing, useStrongStorage); }
	public static void ClearMeta(this object obj, bool useStrongStorage = true) { VMeta.main.ClearMeta(obj, useStrongStorage); }

	// Stream
	public static void CopyTo(this Stream source, Stream destination, int bufferSize = 4096) {
		var buffer = new byte[bufferSize];
		int count;
		while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
			destination.Write(buffer, 0, count);
	}

	// IEnumerable<T>
	//public static string JoinUsing(this IEnumerable list, string separator) { return String.Join(separator, list.Cast<string>().ToArray()); }
	public static string JoinUsing(this IEnumerable list, string separator) { return String.Join(separator, list.OfType<object>().Select(a=>a.ToString()).ToArray()); }

	// MatchCollection
	public static List<Match> ToList(this MatchCollection obj) {
		var result = new List<Match>();
		for (int i = 0; i < obj.Count; i++)
			result.Add(obj[i]);
		return result;
	}

	// DirectoryInfo
	public static DirectoryInfo VCreate(this DirectoryInfo folder) { folder.Create(); return folder; }
	public static DirectoryInfo GetFolder(this DirectoryInfo folder, string subpath) { return new DirectoryInfo(folder.FullName + (subpath != null && subpath.StartsWith("/") ? "" : "/") + subpath); }
	public static string GetSubpathOfDescendent(this DirectoryInfo folder, DirectoryInfo descendent) { return descendent.FullName.Substring(folder.FullName.Length); }
	public static string GetSubpathOfDescendent(this DirectoryInfo folder, FileInfo descendent) { return descendent.FullName.Substring(folder.FullName.Length); }
	public static FileInfo GetFile(this DirectoryInfo folder, string subpath) { return new FileInfo(folder.FullName + (subpath != null && subpath.StartsWith("/") ? "" : "/") + subpath); }
	public static void CopyTo(this DirectoryInfo source, DirectoryInfo target) {
		if (source.FullName == target.FullName)
			throw new Exception("Source and destination cannot be the same.");
		// fix for if root-call folder has files but not folders
		if (!target.Exists)
			target.Create();
		foreach (DirectoryInfo dir in source.GetDirectories())
			dir.CopyTo(target.CreateSubdirectory(dir.Name));
		foreach (FileInfo file in source.GetFiles())
			file.CopyTo(Path.Combine(target.FullName, file.Name));
	}

	// GameObject
	public static string GetPath(this GameObject obj, GameObject relativeTo = null) {
		var result = "";
		var currentObj = obj;
		while (currentObj != null && currentObj != relativeTo) {
			result = currentObj.name + (result.Length > 0 ? "/" : "") + result;
			currentObj = currentObj.transform.parent.gameObject;
		}
		return result;
	}
	//public static GameObject GetChild(this GameObject obj, string name) { return obj.transform.FindChild(name) ? obj.transform.FindChild(name).gameObject : null; }
	public static GameObject GetChild(this GameObject obj, string name, bool createIfNotExistent = false) {
		var directChildName = name.Contains("/") ? name.Substring(0, name.IndexOf("/")) : name;
		var directChild = obj.transform.Find(directChildName) ? obj.transform.Find(directChildName).gameObject : null;
		if (directChild == null && createIfNotExistent) {
			directChild = new GameObject(directChildName);
			directChild.transform.parent = obj.transform;
		}
		if (name != directChildName && directChild != null) // if not last child in path, and child actually exists
			return directChild.GetChild(name.Substring(directChildName.Length + 1), createIfNotExistent);
		return directChild;
	}
	public static List<GameObject> GetChildren(this GameObject obj, bool includeSemiDestroyed = true) {
		var result = new List<GameObject>(obj.transform.childCount);
		for (int i = 0, count = obj.transform.childCount; i < count; i++)
#if UNITY_EDITOR
			if (includeSemiDestroyed)
#else
			if (includeSemiDestroyed || obj.transform.GetChild(i).gameObject.GetMeta("semiDestroyed") == null)
#endif
				result.Add(obj.transform.GetChild(i).gameObject);
		return result;
	}
	/*public static GameObject FindChildDeep(this GameObject self, string childName) {
		Transform[] children = self.GetComponentsInChildren<Transform>(true);
		if (childName.Contains("/")) {
			string[] segments = childName.Split('/');
			foreach (Transform child in children)
				if (child.name == segments[0]) {
					Transform currentDeepest = child;
					foreach (string segment in segments) {
						if (segment == segments[0])
							continue; // Already found self
						currentDeepest = currentDeepest.FindChild(segment);
						if (currentDeepest == null)
							break; // Child expected not found, stop search now			    		
					}
					if (currentDeepest == null)
						continue; // self was right childName, but not children
					return currentDeepest.gameObject;
				}
		}
		else
			foreach (Transform child in children)
				if (child.name == childName)
					return child.gameObject;
		return null;
	}*/
	public static void SetLayer(this GameObject obj, string layer, bool recursively = false) {
		obj.layer = LayerMask.NameToLayer(layer);
		if (recursively)
			foreach (Transform child in obj.transform)
				child.gameObject.SetLayer(layer, true);
	}
	public static List<GameObject> GetParents(this GameObject obj, bool addSelf = false, GameObject stopAtObj = null) {
		var result = new List<GameObject>();
		if (addSelf)
			result.Add(obj);
		Transform currentParent = obj.transform.parent;
		while (currentParent != null && currentParent.gameObject != stopAtObj) {
			result.Add(currentParent.gameObject);
			currentParent = currentParent.parent;
		}
		return result;
	}
	public static GameObject GetDescendent(this GameObject s, string name) {
		foreach (Transform descendent in s.GetComponentsInChildren<Transform>(true))
			if (descendent.gameObject != s && descendent.name == name)
				return descendent.gameObject;
		return null;
	}
	/*public static Object GetProperty(this GameObject obj, string key) {
		var propertyHolder = obj.GetComponent<PropertyHolder>() ?? obj.AddComponent<PropertyHolder>();
		return propertyHolder.propertyKeys.Contains(key) ? propertyHolder.propertyValues[propertyHolder.propertyKeys.IndexOf(key)] : null;
	}
	public static void SetProperty(this GameObject obj, string key, Object value) {
		var propertyHolder = obj.GetComponent<PropertyHolder>() ?? obj.AddComponent<PropertyHolder>();
		if (propertyHolder.propertyKeys.Contains(key))
			propertyHolder.propertyValues[propertyHolder.propertyKeys.IndexOf(key)] = value;
		else {
			propertyHolder.propertyKeys.Add(key); 
			propertyHolder.propertyValues.Add(value);
		}
	}*/
}