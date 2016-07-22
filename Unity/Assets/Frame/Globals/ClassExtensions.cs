using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using System;
using System.Collections;
using UnityEditor;
using VDFN;
using VectorStructExtensions;
using VTree;
using VTree_Structures;
using Object = UnityEngine.Object;
using Random = System.Random;

public static class ClassExtensions {
	// object
	public static VNullClass SetMeta(this object obj, object metaKey, VNullClass metaValue, bool useStrongStorage = true) { return VMeta.main.SetMeta(obj, metaKey, metaValue, useStrongStorage); } // for null
	public static T SetMeta<T>(this object obj, object metaKey, T metaValue, bool useStrongStorage = true) { return VMeta.main.SetMeta(obj, metaKey, metaValue, useStrongStorage); }
	public static T GetMeta<T>(this object obj, object metaKey) { return VMeta.main.GetMeta<T>(obj, metaKey); }
	public static object GetMeta(this object obj, object metaKey) { return VMeta.main.GetMeta(obj, metaKey); }
	public static T GetMeta<T>(this object obj, object metaKey, T returnValueIfMissing, bool useStrongStorage = true) { return VMeta.main.GetMeta(obj, metaKey, returnValueIfMissing, useStrongStorage); }
	public static void ClearMeta(this object obj, bool useStrongStorage = true) { VMeta.main.ClearMeta(obj, useStrongStorage); }
	//public static Type GetType_Safe<T>(this object obj) { return obj.GetType_Safe(typeof(T)); }
	//public static Type GetType_Safe(this object obj, Type fallbackType = null) { return obj != null ? obj.GetType() : fallbackType; }
	public static double ToDouble(this object s) {
		if (s is double)
			return (double)s;
		//if (s is float)
		return (float)s;
	}

	public static void ShouldBe(this object s, object value) {
		//if (s != value)
		if (!Equals(s, value))
			throw new Exception("Assert failed! A:" + s + " B:" + value);
	}

	// T
	public static T VAct<T>(this T s, Action<T> action) {
		action(s);
		return s;
	}

	// string
	public static string TrimStart(this string s, int length) { return s.Substring(length); }
	public static string TrimEnd(this string s, int length) { return s.Substring(0, s.Length - length); }
	public static string SubstringSE(this string self, int startIndex, int enderIndex) { return self.Substring(startIndex, enderIndex - startIndex); }
	public static int IndexOf_X(this string s, int x, string str) // (0-based)
	{
		var currentPos = -1;
		for (var i = 0; i <= x; i++)
		{
			var subIndex = s.IndexOf(str, currentPos + 1);
			if (subIndex == -1)
				return -1; // no such xth index
			currentPos = subIndex;
		}
		return currentPos;
	}
	public static int IndexOf_XFromLast(this string s, int x, string str) // (0-based)
	{
		var currentPos = (s.Length - str.Length) + 1; // index just after the last-index-where-match-could-occur
		for (var i = 0; i <= x; i++)
		{
			var subIndex = s.LastIndexOf(str, currentPos - 1);
			if (subIndex == -1)
				return -1; // no such xth index
			currentPos = subIndex;
		}
		return currentPos;
	}
	public static string Replace_Regex(this string s, string regexMatch, string replaceWith) { return new Regex(regexMatch).Replace(s, replaceWith); }

	// "kind of" hacky // maybe temp
	public static object GetPropValue(this object s, string propName) { //, int memberIndex = 0) {
		//return VReflection.GetPropValue(s, propName, memberIndex);
		return s.GetVTypeInfo().props[propName].GetValue(s);
	}
	public static void SetPropValue(this object s, string propName, object value) { //, int memberIndex = 0) {
		 //VReflection.SetPropValue(obj, propName, value, memberIndex);
		s.GetVTypeInfo().props[propName].SetValue(s, value);
	}

	// T
	//public static T As<T>(this T obj, int index) { return V.As(obj, index); } // for debugging
	//public static List<T> ToList<T>(this T obj) { return new List<T> {obj}; }

	// Stream
	public static void CopyTo(this Stream source, Stream destination, int bufferSize = 4096)
	{
		var buffer = new byte[bufferSize];
		int count;
		while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
			destination.Write(buffer, 0, count);
	}

	// IEnumerable
	public static bool ItemsEqual(this IEnumerable s, IEnumerable other)
	{
		var sList = new List<object>(); // could also use CopyTo, for an IList variant
		if (s != null)
			foreach (object item in s)
				sList.Add(item);
		var otherList = new List<object>();
		if (other != null)
			foreach (object item in other)
				otherList.Add(item);
		return sList.SequenceEqual(otherList);
	}
	public static List<object> ToList_Object(this IEnumerable s) { return s.OfType<object>().ToList(); }
	//public static List<T> ToList<T>(this IEnumerable s) { return s.OfType<T>().ToList(); }
	public class Pair_IndexItem<A, B> //public struct Pack<A, B>
	{
		public A index;
		public B item;
		public Pair_IndexItem(A index, B item)
		{
			this.index = index;
			this.item = item;
		}

		public override int GetHashCode() { return (index.GetHashCode() + ";" + item.GetHashCode()).GetHashCode(); }
		public override bool Equals(object obj) { return GetHashCode() == obj.GetHashCode(); }
		//public override string ToString() { return GetHashCode().ToString(); }
	}
	public static List<Pair_IndexItem<int, object>> Pairs(this IEnumerable s)
	{
		var result = new List<Pair_IndexItem<int, object>>();
		var list = s.ToList_Object();
		for (var i = 0; i < list.Count; i++)
			result.Add(new Pair_IndexItem<int, object>(i, list[i]));
		return result;
	}

	// IEnumerable<T>
	//public static string JoinUsing(this IEnumerable list, string separator) { return String.Join(separator, list.Cast<string>().ToArray()); } // moved to ClassExtensions_Plugins
	public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source) { return new HashSet<T>(source); }
	public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> s, params TSource[] items) { return Enumerable.Except(s, items); }
	public static T? FirstOrNull<T>(this IEnumerable<T> s, Func<T, bool> itemMatchFunc = null) where T : struct
	{
		if (itemMatchFunc != null)
			return s.Any(itemMatchFunc) ? (T?)s.First(itemMatchFunc) : null;
		return s.Any() ? (T?)s.First() : null;
	}
	public static List<Pair_IndexItem<int, T>> Pairs<T>(this IEnumerable<T> s)
	{
		var result = new List<Pair_IndexItem<int, T>>();
		var list = s.ToList();
		for (var i = 0; i < list.Count; i++)
			result.Add(new Pair_IndexItem<int, T>(i, list[i]));
		return result;
	}
	/*public static T2 VAverage<T, T2>(this IEnumerable<T> s, Func<T, T2> getItemToAverageFunc) where T2 : Addable
	{
		var total = new T2();
		foreach (T item in s)
			total += getItemToAverageFunc(item);
		return total;
	}*/
	public static VVector2 VAverage<T>(this IEnumerable<T> s, Func<T, VVector2> getItemToAverageFunc)
	{
		var list = s.ToList();
		var total = new VVector2();
		foreach (T item in list)
			total += getItemToAverageFunc(item);
		return total / list.Count;
	}

	// HashSet<T>
	public static void AddRange<T>(this HashSet<T> s, IEnumerable<T> source)
	{
		foreach (T item in source)
			s.Add(item);
	}

	// Array
	public static bool HasIndex(this Array s, int index) { return index >= 0 && index < s.Length; }
	public static bool HasIndex(this Array s, int index0, int index1) { return index0 >= 0 && index0 < s.GetLength(0) && index1 >= 0 && index1 < s.GetLength(1); }

	// T[]
	public static T GetValueUsingMultiDimPos<T>(this T[] s, int traveledOnFirstDim_length, int traveledOnSecondDim_index, int traveledOnFirstDim_index) { return s[(traveledOnSecondDim_index * traveledOnFirstDim_length) + traveledOnFirstDim_index]; }
	public static void SetValueUsingMultiDimPos<T>(this T[] s, int traveledOnFirstDim_length, int traveledOnSecondDim_index, int traveledOnFirstDim_index, T value) { s[(traveledOnSecondDim_index * traveledOnFirstDim_length) + traveledOnFirstDim_index] = value; }
	public static T GetValueOrX<T>(this T[] s, int index, T defaultValueX = default(T)) { return index >= 0 && index < s.Length ? s[index] : defaultValueX; }

	// T[,]
	public static T GetValueOrX<T>(this T[,] s, int index0, int index1, T defaultValueX = default(T)) { return index0 >= 0 && index0 < s.GetLength(0) && index1 >= 0 && index1 < s.GetLength(1) ? s[index0, index1] : defaultValueX; }

	// IList
	public static bool HasIndex(this IList s, int index) { return index >= 0 && index < s.Count; }

	// List<T>
	public static bool HasIndex<T>(this List<T> s, int index) { return index >= 0 && index < s.Count; }
	//public static T GetValueOrNull<T>(this List<T> list, int index) where T : class { return index >= 0 && index < list.Count ? list[index] : null; }
	//public static T GetValueOrDefault<T>(this List<T> list, int index, T defaultValue = default(T)) { return index >= 0 && index < list.Count ? list[index] : defaultValue; }
	public static T GetValue<T>(this List<T> s, int index, T defaultValue = default(T)) { return index >= 0 && index < s.Count ? s[index] : defaultValue; }
	public static T XFromLast<T>(this List<T> s, int stepsFromLastX, T defaultValue = default(T)) { return s.GetValue((s.Count - 1) - stepsFromLastX, defaultValue); }
	public static List<T> CAdd<T>(this List<T> s, T item) {
		s.Add(item);
		return s;
	}
	public static void AddIfNotNull<T>(this List<T> s, T item) { //where T : class
		if (item != null)
			s.Add(item);
	}
	public static void AddIfNotNull<T>(this List<T> list, T? item) where T : struct {
		if (item != null)
			list.Add(item.Value);
	}
	public static void Remove_Base<T>(this List<T> list, T item) {
		int index = -1;
		for (int i = 0; i < list.Count; i++)
			if ((object)list[i] == (object)item)
				index = i;
		if (index != -1)
			list.RemoveAt(index);
	}
	public static bool Contains_Base<T>(this List<T> list, T item) {
		int index = -1;
		for (int i = 0; i < list.Count; i++)
			if ((object)list[i] == (object)item)
				index = i;
		return index != -1;
	}
	public static void Shuffle<T>(this IList<T> list, Random generator = null) {
		var rng = generator ?? new Random();
		int n = list.Count;
		while (n > 1) {
			n--;
			int k = rng.Next(n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}
	public static void Move<T>(this IList<T> list, T item, int newIndex)
	{
		var oldIndex = list.IndexOf(item);
		list.RemoveAt(oldIndex);
		if (oldIndex < newIndex) // new-index is understood to be the position-in-list to move the item to, as seen before the item started being moved--so compensate for remove-from-old-position list modification
			newIndex--;
		list.Insert(newIndex, item);
	}
	public static bool Contains<T>(this List<T> s, T item, int indexHint)
	{
		//if (s.Count > indexHint && s[indexHint] == item)
		//if (s.Count > indexHint && s[indexHint].Equals(item))
		if (s.Count > indexHint && EqualityComparer<T>.Default.Equals(s[indexHint], item))
			return true;
		return s.Contains(item);
	}

	public static List<T> ToList<T>(this Array obj)
	{
		var result = new List<T>();
		foreach (T item in obj)
			result.Add(item);
		return result;
	}
	public static List<T> ToList<T>(this T[] obj)
	{
		var result = new List<T>();
		foreach (T item in obj)
			result.Add(item);
		return result;
	}
	public static List<T> ToList<T>(this T[,] obj)
	{
		var result = new List<T>();
		foreach (T item in obj)
			result.Add(item);
		return result;
	}

	// IDictionary
	public static bool ItemsEqual(this IDictionary s, IDictionary other)
	{
		if (s == null)
			return other == null;
		if (other == null)
			return false;
		return s.Keys.ItemsEqual(other.Keys) && s.Values.ItemsEqual(other.Values);
	}
	public static List<KeyValuePair<object, object>> Pairs(this IDictionary s)
	{
		var result = new List<KeyValuePair<object, object>>();
		foreach (object key in s.Keys)
			result.Add(new KeyValuePair<object, object>(key, s[key]));
		return result;
	}

	// Dictionary<TKey, TValue>
	//public static TValue GetValueOrNull<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key) where TValue : class { return GetValueOrDefault(self, key, null); } // maybe temp
	public static TValue GetValueOrX<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key, TValue defaultValueX = default(TValue)) {
		TValue val;
		if (self.TryGetValue(key, out val))
			return val;
		return defaultValueX;
	}
	public static TValue VAdd<TKey, TValue>(this Dictionary<TKey, TValue> s, TKey key, TValue value) {
		try {
			s.Add(key, value);
			return value;
		}
		catch (ArgumentException ex) {
			if (ex.Message == "An element with the same key already exists in the dictionary.")
				ex.AddToMessage(" (key: " + key + ")");
			throw;
		}
	}
	public static void AddDictionary<TKey, TValue>(this Dictionary<TKey, TValue> s, Dictionary<TKey, TValue> other) {
		foreach (TKey key in other.Keys)
			s.VAdd(key, other[key]);
	}

	// Dictionary<TKey, TValue of struct> (e.g. Dictionary<string, bool>)
	// commented out, since also exists in VDF system
	/*public static TValue? GetValueOrX<TKey, TValue>(this Dictionary<TKey, TValue> obj, TKey key, TValue? defaultValueX = default(TValue?)) where TValue : struct {
		TValue result;
		if (obj.TryGetValue(key, out result))
			return result;
		return null;
	}*/
	// old [now moved to VDF.cs]: more specific version, added so VDF system compiled (Mono compiler can't understand TValue?--it thinks the result is just TValue)
	/*public static bool? GetValueOrX(this Dictionary<MemberInfo, bool> obj, MemberInfo key, bool? defaultValueX = default(bool?)) {
		bool result;
		if (obj.TryGetValue(key, out result))
			return result;
		return null;
	}*/
	/*public static bool? GetValueOrX<TKey>(this Dictionary<TKey, bool> obj, TKey key, bool? defaultValueX = default(bool?)) {
		bool result;
		if (obj.TryGetValue(key, out result))
			return result;
		return null;
	}*/

	// Dictionary<TKey, Rect> // (custom extension required, instead of generic version above, because of compiler error)
	public static Rect? GetValueOrX<TKey>(this Dictionary<TKey, Rect> obj, TKey key, Rect? defaultValueX = default(Rect?)) {
		Rect result;
		if (obj.TryGetValue(key, out result))
			return result;
		return null;
	}

	// Dictionary<TKey, [TValue of struct]?> (e.g. Dictionary<string, bool?>)
	public static TValue? GetValueOrX<TKey, TValue>(this Dictionary<TKey, TValue?> obj, TKey key, TValue? defaultValueX = default(TValue?)) where TValue : struct {
		TValue? result;
		if (obj.TryGetValue(key, out result))
			return result;
		return null;
	}

	// Dictionary<T, List<T2>>
	public static void AddItemToListAtKey<T, T2>(this Dictionary<T, List<T2>> dictionary, T key, T2 item)
	{
		if (!dictionary.ContainsKey(key))
			dictionary[key] = new List<T2>();
		dictionary[key].Add(item);
	}
	// special versions, for structs
	/*public static float? GetValue<T>(this Dictionary<T, float?> obj, T key, float? defaultValue = null) { return obj.ContainsKey(key) ? obj[key] : defaultValue; }
	public static float GetValue<T>(this Dictionary<T, float> obj, T key, float defaultValue = 0) { return obj.ContainsKey(key) ? obj[key] : defaultValue; }*/
	// custom-generic-for-retrieved-value version
	//public static TThisValue GetValue<TKey, TValue, TThisValue>(this Dictionary<TKey, TValue> s, TKey key, TThisValue defaultValue = null) where TThisValue : class { return s.ContainsKey(key) ? (TThisValue)(object)s[key] : defaultValue; }
	//public static TThisValue GetValue<TThisValue>(this Dictionary<string, object> s, string key, TThisValue defaultValue = null) where TThisValue : class { return s.ContainsKey(key) ? (TThisValue)s[key] : defaultValue; }

	// DirectoryInfo
	public static DirectoryInfo GetSubfolder(this DirectoryInfo folder, string subpath) // use this if you want to make sure you're accessing a subfolder (e.g. so as to ensure not deleting a master folder)
	{
		var result = folder.GetFolder(subpath);
		if (folder.VFullName() != result.VFullName())
			return result;
		return null;
	}
	public static void DeleteToRecycleBin(this DirectoryInfo folder) { V.DeleteFileOrFolderToRecycleBin(folder.FullName); }
	public static DirectoryInfo[] GetDirectories_Safe(this DirectoryInfo s, string searchPattern = null, SearchOption searchOption = SearchOption.TopDirectoryOnly)
	{
		if (!s.Exists)
			return new DirectoryInfo[0];
		if (searchOption == SearchOption.AllDirectories)
			return s.GetDirectories(searchPattern ?? "*", searchOption);
		if (searchPattern != null)
			return s.GetDirectories(searchPattern);
		return s.GetDirectories();
	}
	public static string VFullName(this DirectoryInfo s, DirectoryInfo relativeTo = null)
	{
		var result = FileManager.FormatPath(s.FullName);
		if (relativeTo != null)
			result = result.Substring(relativeTo.VFullName().Length);
		return result;
	}
	
	// FileInfo
	public static string VFullName(this FileInfo s, DirectoryInfo relativeTo = null)
	{
		var result = FileManager.FormatPath(s.FullName);
		if (relativeTo != null)
			result = result.Substring(relativeTo.VFullName().Length);
		return result;
	}
	public static void DeleteToRecycleBin(this FileInfo file) { V.DeleteFileOrFolderToRecycleBin(file.FullName); }
	public static void SaveText(this FileInfo file, string text)
	{
		file.Directory.Create();
		File.WriteAllText(file.FullName, text);
	}
	public static string LoadText(this FileInfo file) { return File.ReadAllText(file.FullName); }
	public static FileInfo CreateFolders(this FileInfo s)
	{
		s.Directory.Create();
		return s;
	}
	public static string GetPathAsURL(this FileInfo s) { return "file://" + s.FullName; }

	// DateTime
	public static float GetTODAsHour(this DateTime time)
	{
		// return (float)(time - new DateTime(time.Year, time.Month, time.Day, 0, 0, 0)).TotalHours;
		return time.Hour + (time.Minute / 60f) + (time.Second / 3600f);
	}
	public static TimeSpan ToTimeSpan(this DateTime time) { return time - new DateTime(0); }
	public static bool IsContainedBy(this DateTime self, DateTime min, DateTime max) { return self.CompareTo(min) != -1 && self.CompareTo(max) != 1; }

	// TimeSpan
	public static DateTime ToDateTime(this TimeSpan time) { return new DateTime(0) + time; }

	// GameObject
	public static void SetPivotToLocalPoint(this GameObject obj, VVector3 point, bool changeRootPositionToPreserveDescendentWorldPositions = true)
	{
		var point_unity = point.ToVector3();

		if (changeRootPositionToPreserveDescendentWorldPositions)
			obj.transform.Translate(point_unity);
		foreach (GameObject child in obj.GetChildren())
			child.transform.localPosition -= point_unity; // note; may cause problems if children have non-1 local-scale
		foreach (MeshFilter meshFilter in obj.GetComponents<MeshFilter>())
		{
			var mesh = meshFilter.mesh;
			Vector3[] verts = mesh.vertices;
			for (int i = 0; i < verts.Length; i++)
				verts[i] -= point_unity;
			mesh.vertices = verts; // assign the vertex array back to the mesh
			mesh.RecalculateBounds(); // recalculate bounds of the mesh, for the renderer's sake
		}
		foreach (Collider collider in obj.GetComponents<Collider>())
			if (collider is BoxCollider)
				((BoxCollider)collider).center -= point_unity;
			else if (collider is CapsuleCollider)
				((CapsuleCollider)collider).center -= point_unity;
			else if (collider is SphereCollider)
				((SphereCollider)collider).center -= point_unity;
	}
	public static VBounds GetBounds(this GameObject self, Transform inSpaceOf = null, bool includeInactiveFromSelf = false)
	{
		var result = VBounds.Null; //new Bounds(V.Vector3Null, V.Vector3Null);

		var boundsPoints = new List<VVector3>();
		/*foreach (MeshFilter filter in self.GetComponentsInChildren<MeshFilter>(includeInactive))
			boundsPoints.AddRange(filter.transform.TransformBounds(filter.sharedMesh.bounds).GetCorners());*/
		foreach (Renderer renderer in self.GetComponentsInChildren<Renderer>(true).Where(a=>includeInactiveFromSelf || (a.enabled && a.gameObject.GetParents(true, self).All(b=>b.activeSelf))))
			boundsPoints.AddRange(renderer.bounds.ToVBounds().GetCorners());
		foreach (SkinnedMeshRenderer renderer in self.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(a=>includeInactiveFromSelf || (a.enabled && a.gameObject.GetParents(true, self).All(b=>b.activeSelf))))
			boundsPoints.AddRange(renderer.bounds.ToVBounds().GetCorners());
		/*foreach (SkinnedMeshRenderer renderer in self.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(a=>includeInactiveFromSelf || (a.enabled && a.gameObject.GetParents(true, self).All(b=>b.activeSelf))))
			boundsPoints.AddRange(renderer.transform.TransformBounds(renderer.sharedMesh.bounds).GetCorners());*/
		foreach (Collider collider in self.GetComponentsInChildren<Collider>(true).Where(a=>includeInactiveFromSelf || (a.enabled && a.gameObject.GetParents(true, self).All(b=>b.activeSelf))))
			boundsPoints.AddRange(collider.bounds.ToVBounds().GetCorners());
		
		foreach (VVector3 point in boundsPoints)
			result.Encapsulate(inSpaceOf ? inSpaceOf.InverseTransformPoint(point.ToVector3()).ToVVector3() : point);

		return result;
	}
	public static void SetIsStatic(this GameObject obj, bool isStatic, bool recursively = false) {
		obj.isStatic = isStatic;
		if (recursively)
			foreach (Transform child in obj.transform)
				child.gameObject.SetIsStatic(isStatic, true);
	}

	// Transform
	/*public static Bounds TransformBounds(this Transform self, Bounds bounds)
	{
		var center = self.TransformPoint(bounds.center);
		var points = bounds.GetCorners();

		var result = new Bounds(center, Vector3.zero);
		foreach (var point in points)
			result.Encapsulate(self.TransformPoint(point));
		return result;
	}
	public static Bounds InverseTransformBounds(this Transform self, Bounds bounds)
	{
		var center = self.InverseTransformPoint(bounds.center);
		var points = bounds.GetCorners();

		var result = new Bounds(center, Vector3.zero);
		foreach (var point in points)
			result.Encapsulate(self.InverseTransformPoint(point));
		return result;
	}*/
	public static VBounds TransformBounds(this Transform self, VBounds bounds)
	{
		var position = self.TransformPoint(bounds.position.ToVector3()).ToVVector3();
		var points = bounds.GetCorners();

		var result = new VBounds(position, VVector3.zero);
		foreach (var point in points)
			result.Encapsulate(self.TransformPoint(point.ToVector3()).ToVVector3());
		return result;
	}
	public static VBounds InverseTransformBounds(this Transform self, VBounds bounds)
	{
		var position = self.InverseTransformPoint(bounds.position.ToVector3()).ToVVector3();
		var points = bounds.GetCorners();

		var result = new VBounds(position, VVector3.zero);
		foreach (var point in points)
			result.Encapsulate(self.InverseTransformPoint(point.ToVector3()).ToVVector3());
		return result;
	}

	// Bounds
	public static VBounds ToVBounds(this Bounds self) { return new VBounds(self.min.ToVVector3(), self.size.ToVVector3()); }
	public static List<Vector3> GetCorners(this Bounds obj, bool includePosition = true)
	{
		var result = new List<Vector3>();
		for (int x = -1; x <= 1; x += 2)
			for (int y = -1; y <= 1; y += 2)
				for (int z = -1; z <= 1; z += 2)
					result.Add((includePosition ? obj.center : Vector3.zero) + (obj.size / 2).Times(new Vector3(x, y, z)));
		return result;
	}
	/*public static Bounds VEncapsulate(this Bounds self, Vector3 point)
	{
		if (self.center.Equals(V.Vector3Null))
		{
			self.center = point;
			self.size = Vector3.zero;
		}
		self.Encapsulate(point);
		return self;
	}
	public static Bounds VEncapsulate(this Bounds self, Bounds bounds)
	{
		if (self.center.Equals(V.Vector3Null))
		{
			self.center = bounds.center;
			self.size = Vector3.zero;
		}
		self.Encapsulate(bounds);
		return self;
	}
	public static bool Intersects(this Bounds self, Vector3 point) { return self.Intersects(new Bounds(point, Vector3.zero)); }
	public static VRect ToVRect_TopView(this Bounds s) { return new VRect(s.min.ToVector2_TopView(), s.size.ToVector2_TopView()); }*/

	// Color
	public static Color NewR(this Color self, float val) { return new Color(val, self.g, self.b, self.a); }
	public static Color NewG(this Color self, float val) { return new Color(self.r, val, self.b, self.a); }
	public static Color NewB(this Color self, float val) { return new Color(self.r, self.g, val, self.a); }
	public static Color NewA(this Color self, float val) { return new Color(self.r, self.g, self.b, val); }
	
	// Rect
	/*public static Rect NewX(this Rect self, float val) { return new Rect(val, self.y, self.width, self.height); }
	public static Rect NewY(this Rect self, float val) { return new Rect(self.x, val, self.width, self.height); }
	public static Rect NewWidth(this Rect self, float val) { return new Rect(self.x, self.y, val, self.height); }
	public static Rect NewHeight(this Rect self, float val) { return new Rect(self.x, self.y, self.width, val); }

	public static Rect NewLeft(this Rect self, float val) { return new Rect(val, self.y, self.width - (val - self.x), self.height); }
	public static Rect NewTop(this Rect self, float val) { return new Rect(self.x, val, self.width, self.height - (val - self.y)); }
	public static Rect NewRight(this Rect self, float val) { return new Rect(self.x, self.y, val - self.x, self.height); }
	public static Rect NewBottom(this Rect self, float val) { return new Rect(self.x, self.y, self.width, val - self.y); }*/

	public static VRect ToVRect(this Rect s) { return new VRect(s.x, s.y, s.width, s.height); }

	// VRect
	public static Rect ToRect(this VRect s) { return new Rect((float)s.x, (float)s.y, (float)s.width, (float)s.height); }

	// int
	//public static bool EqualsAbout(this int obj, float val, float maxDifForEquals = float.Epsilon) { return Math.Abs(obj - val) <= maxDifForEquals; }
	//public static bool EqualsAbout(this int obj, double val, float maxDifForEquals = float.Epsilon) { return Math.Abs(obj - val) <= maxDifForEquals; }
	public static int Distance(this int s, int other) { return Math.Abs(s - other); }
	public static int Sign(this int s) { return s >= 0 ? 1 : -1; }
	public static int Abs(this int s) { return Math.Abs(s); }
	public static int Modulus(this int s, int modulus, bool keepSignOfFirst = false, bool keepSignOfSecond = true)
	{
		int result = s % modulus;
		if (keepSignOfFirst && result.Sign() != s.Sign())
			result = -result;
		else if (keepSignOfSecond && result.Sign() != modulus.Sign())
			result = -result;
		return result;
	}
	public static int FloorToMultipleOf(this int s, int val) { return (int)(Math.Floor((double)s / val) * val); }
	public static int RoundToMultipleOf(this int s, int val) { return (int)(Math.Round((double)s / val) * val); }
	public static int CeilingToMultipleOf(this int s, int val) { return (int)(Math.Ceiling((double)s / val) * val); }
	public static int ToPower(this int s, int power) { return (int)Math.Pow(s, power); }
	public static bool IsBetween(this int s, int min, int max) { return s >= min && s <= max; }
	public static int Clamp(this int s, int min, int max) { return Math.Min(max, Math.Max(min, s)); }

	// float
	public static float Distance(this float s, float other) { return Math.Abs(s - other); }
	public static float FloorToMultipleOf(this float s, float val) { return (float)Math.Floor(s / val) * val; }
	public static float RoundToMultipleOf(this float s, float val) { return (float)Math.Round(s / val) * val; }
	public static float FloorToPowerOf(this float s, float val) { return (int)Math.Pow(val, (int)Math.Floor(Math.Log(s, val))); }
	public static float RoundToPowerOf(this float s, float val) { return (int)Math.Pow(val, (int)Math.Round(Math.Log(s, val))); }
	//public static bool EqualsAbout(this float s, float val) { return Math.Abs(s - val) / Math.Max(Math.Abs(s), Math.Abs(val)) <= float.Epsilon; }
	// the first six digits are safe-for/unchanged-during conversion to float, and then back to decimal/toward-infinity-precision
	// so if we see them differ in those first 6 digits, we know something more substantial made them differ
	public static bool EqualsAbout(this float s, float val, float maxDifForEquals = .000001f) { return Math.Abs(s - val) <= maxDifForEquals; }
	//public static bool EqualsAbout(this float s, double val, float maxDifForEquals = .000001f) { return Math.Abs(s - val) <= maxDifForEquals; }
	//public static bool EqualsAbout(this float s, float val, float epsilon = float.Epsilon) { return Float_EqualsAbout(s, val, epsilon); }
	//public static int CompareTo(this float s, float val, float maxDifForEquals) { return s.EqualsAbout(val, maxDifForEquals) ? 0 : (s < val ? -1 : 1); }
	public static float Sign(this float s) { return s >= 0 ? 1 : -1; }
	public static float Abs(this float s) { return Math.Abs(s); }
	public static float Modulus(this float s, float modulus, bool keepSignOfFirst = false, bool keepSignOfSecond = true)
	{
		float result = s % modulus;
		if (keepSignOfFirst && result >= 0 != s >= 0) //result.Sign() != s.Sign())
			result = -result;
		else if (keepSignOfSecond && result >= 0 != modulus >= 0) //result.Sign() != modulus.Sign())
			result = -result;
		return result;
	}
	public static float DivideBy(this float s, float other, bool keepSignOfFirst = true, bool keepSignOfSecond = false)
	{
		float result = s / other;
		if (keepSignOfFirst && result >= 0 != s >= 0) //result.Sign() != s.Sign())
			result = -result;
		else if (keepSignOfSecond && result >= 0 != other >= 0) //result.Sign() != other.Sign())
			result = -result;
		return result;
	}

	// double
	//public static bool EqualsAbout(this double obj, double val, double maxDifForEquals = double.Epsilon) { return Math.Abs(obj - val) <= maxDifForEquals; }
	/*public static bool EqualsAbout(this double s, double b, double epsilon = double.Epsilon)
	{
		if (s == b) // shortcut, handles infinities
			return true;
		double diff = Math.Abs(s - b);
		if (s == 0 || b == 0 || diff < Double.MinValue) // if a or b is zero or both are extremely close to it, relative error is less meaningful here
			return diff < (epsilon * Double.MinValue);
		return diff / (Math.Abs(s) + Math.Abs(b)) < epsilon; // use relative error
	}*/
	/*static bool Double_EqualsAbout(double a, double b, double epsilon = double.Epsilon)
	{
		if (a == b) // shortcut, handles infinities
			return true;
		double diff = Math.Abs(a - b);
		/*if (a == 0 || b == 0 || diff < Double.MinValue) // if a or b is zero or both are extremely close to it, relative error is less meaningful here
			return diff < (epsilon * Double.MinValue);
		return diff / (Math.Abs(a) + Math.Abs(b)) < epsilon; // use relative error*#/
		if (a * b == 0)
			return diff < (epsilon * epsilon);
		return diff / (Math.Abs(a) + Math.Abs(b)) < epsilon;
	}*/
	/*static bool Float_EqualsAbout(float a, float b, float epsilon = float.Epsilon)
	{
		if (a == b) // shortcut, handles infinities
			return true;
		float diff = Math.Abs(a - b);
		if (a * b == 0)
			return diff < (epsilon * epsilon);
		return diff / (Math.Abs(a) + Math.Abs(b)) < epsilon;
	}*/
	public static bool IsNaN(this double s) { return s != s; } // NaN isn't equal to any number--including itself
	public static double Sign(this double s) { return s >= 0 ? 1 : -1; }
	public static double Abs(this double s) { return Math.Abs(s); }
	public static double FloorToMultipleOf(this double s, double val) { return Math.Floor(s / val) * val; }
	public static double RoundToMultipleOf(this double s, double val) { return Math.Round(s / val) * val; }
	public static double CeilingToMultipleOf(this double s, double val) { return Math.Ceiling(s / val) * val; }
	public static double Modulus(this double s, double modulus, bool keepSignOfFirst = false, bool keepSignOfSecond = true) // probably todo: update all terrain-stuff to use this
	{
		double result = s % modulus;
		if (keepSignOfFirst && result >= 0 != s >= 0) //result.Sign() != s.Sign())
			result = -result;
		else if (keepSignOfSecond && result >= 0 != modulus >= 0) //result.Sign() != modulus.Sign())
			result = -result;
		return result;
	}
	public static double DivideBy(this double s, double other, bool keepSignOfFirst = true, bool keepSignOfSecond = false) // probably todo: update all terrain-stuff to use this
	{
		double result = s / other;
		if (keepSignOfFirst && result >= 0 != s >= 0) //result.Sign() != s.Sign())
			result = -result;
		else if (keepSignOfSecond && result >= 0 != other >= 0) //result.Sign() != other.Sign())
			result = -result;
		return result;
	}
	public static double ToPower(this double s, double power) { return Math.Pow(s, power); }
	//public static bool EqualsAbout(this double s, double val) { return Math.Abs(s - val) / Math.Max(Math.Abs(s), Math.Abs(val)) <= double.Epsilon; }
	public static bool EqualsAbout(this double s, double val, double maxDifForEquals = .000000000000001) { return Math.Abs(s - val) <= maxDifForEquals; }
	public static double Clamp(this double s, double min, double max) { return Math.Min(max, Math.Max(min, s)); }
	public static string ToVString(this double s)
	{
		var result = s.ToString();
		if (result.StartsWith("0."))
			result = result.Substring(1);
		else if (result.StartsWith("-0."))
			result = "-" + result.Substring(2);
		return result;
	}
	public static double Distance(this double s, double other) { return Math.Abs(s - other); }
	public static double KeepAtLeast(this double s, double min) { return Math.Max(min, s); }
	public static double KeepAtMost(this double s, double max) { return Math.Min(max, s); }
	public static double KeepBetween(this double s, double min, double max) { return Math.Min(max, Math.Max(min, s)); }

	// Vector2
	public static Vector2 NewX(this Vector2 s, float val) { return new Vector2(val, s.y); }
	public static Vector2 NewY(this Vector2 s, float val) { return new Vector2(s.x, val); }
	public static string ToVString(this Vector2 self) { return "(" + self.x + "," + self.y + ")"; }
	public static Vector2 Times(this Vector2 self, Vector2 self2) { return new Vector3(self.x * self2.x, self.y * self2.y); }
	public static Vector2 DividedBy(this Vector2 self, Vector2 self2) { return new Vector3(self.x / self2.x, self.y / self2.y); }

	public static float Distance(this Vector2 self, Vector2 other) { return Vector2.Distance(self, other); }

	//public static Vector3 ToVector3(this Vector2 s) { return new Vector3(s.x, s.y); }
	//public static Vector3 ToVector3_TopView(this Vector2 s) { return new Vector3(s.x, 0, s.y); }

	// Vector3i
	/*public static Vector3i NewX(this Vector3i self, int val) { return new Vector3i(val, self.y, self.z); }
	public static Vector3i NewY(this Vector3i self, int val) { return new Vector3i(self.x, val, self.z); }
	public static Vector3i NewZ(this Vector3i self, int val) { return new Vector3i(self.x, self.y, val); }*/
	//public static Vector3i NewXYZ(this Vector3i self, Func<int, int> func) { return new Vector3i(func(self.x), func(self.y), func(self.z)); }

	//public static Vector2i ToVector2i(this Vector3i s) { return new Vector2i(s.x, s.y); }
	//public static Vector2i ToVector2i_TopView(this Vector3i s) { return new Vector2i(s.x, s.z); }

	// Vector3
	public static bool EqualsAbout(this Vector3 s, Vector3 other, double maxDifForEquals = .000001f) { return Math.Abs(s.x - other.x) <= maxDifForEquals && Math.Abs(s.y - other.y) <= maxDifForEquals && Math.Abs(s.z - other.z) <= maxDifForEquals; }
	public static Vector3 FloorToMultipleOf(this Vector3 s, float val) { return new Vector3(s.x.FloorToMultipleOf(val), s.y.FloorToMultipleOf(val), s.z.FloorToMultipleOf(val)); }
	public static Vector3 RoundToMultipleOf(this Vector3 s, float val) { return new Vector3(s.x.RoundToMultipleOf(val), s.y.RoundToMultipleOf(val), s.z.RoundToMultipleOf(val)); }
	public static string ToVString(this Vector3 self) { return "(" + self.x + "," + self.y + "," + self.z + ")"; }
	public static Vector3 Times(this Vector3 self, Vector3 self2) { return new Vector3(self.x * self2.x, self.y * self2.y, self.z * self2.z); }
	public static Vector3 DividedBy(this Vector3 self, Vector3 self2) { return new Vector3(self.x / self2.x, self.y / self2.y, self.z / self2.z); }
	public static Vector3 NewX(this Vector3 self, float val) { return new Vector3(val, self.y, self.z); }
	public static Vector3 NewY(this Vector3 self, float val) { return new Vector3(self.x, val, self.z); }
	public static Vector3 NewZ(this Vector3 self, float val) { return new Vector3(self.x, self.y, val); }
	//public static Vector3 NewXYZ(this Vector3 self, float val) { return new Vector3(val, val, val); }
	public static Vector3 NewX(this Vector3 self, Func<float, float> func) { return new Vector3(func(self.x), self.y, self.z); }
	public static Vector3 NewY(this Vector3 self, Func<float, float> func) { return new Vector3(self.x, func(self.y), self.z); }
	public static Vector3 NewZ(this Vector3 self, Func<float, float> func) { return new Vector3(self.x, self.y, func(self.z)); }
	public static Vector3 NewXYZ(this Vector3 self, Func<float, float> func) { return new Vector3(func(self.x), func(self.y), func(self.z)); }

	/*public static Vector2 ToVector2(this Vector3 self, bool topView = true)
	{
		if (topView)
			return new Vector2(self.x, self.z);
		return self;
	}*/

	public static float Distance(this Vector3 self, Vector3 other) { return Vector3.Distance(self, other); }

	// Vector4
	public static Vector4 NewX(this Vector4 self, float val) { return new Vector4(val, self.y, self.z, self.w); }
	public static Vector4 NewY(this Vector4 self, float val) { return new Vector4(self.x, val, self.z, self.w); }
	public static Vector4 NewZ(this Vector4 self, float val) { return new Vector4(self.x, self.y, val, self.w); }
	public static Vector4 NewW(this Vector4 self, float val) { return new Vector4(self.x, self.y, self.z, val); }

	// Quaternion
	// todo: add bool argument, that, when enabled, forces the new value as the final value, rather than just one of four properties to be changed during normalization
	public static Quaternion NewX(this Quaternion self, float val) { return new Quaternion(val, self.y, self.z, self.w); }
	public static Quaternion NewY(this Quaternion self, float val) { return new Quaternion(self.x, val, self.z, self.w); }
	public static Quaternion NewZ(this Quaternion self, float val) { return new Quaternion(self.x, self.y, val, self.w); }
	public static Quaternion NewW(this Quaternion self, float val) { return new Quaternion(self.x, self.y, self.z, val); }

	// BoxCollider
	public static Bounds GetLocalBounds(this BoxCollider obj) { return new Bounds(obj.center, obj.size); }
	public static Vector3? CastRay_Point(this BoxCollider s, Ray ray, float distance = float.MaxValue)
	{
		var rayHit = s.CastRay(ray, distance);
		if (rayHit.HasValue)
			return rayHit.Value.point;
		return null;
	}
	public static RaycastHit? CastRay(this BoxCollider s, Ray ray, float distance = float.MaxValue)
	{
		RaycastHit? result = null;
		RaycastHit temp;
		if (s.Raycast(ray, out temp, distance))
			result = temp;
		return result;
	}

	// Camera
	// note; may want to remove this, since the main error-causer is understood now
	/*public static Ray VScreenPointToRay(this Camera camera, Vector3 screenPos)
	{
		//if (V.sceneViewOpen)
		//	return new Ray(new Vector3(0, 1000000, 0), Vector3.up);
		return camera.ScreenPointToRay(screenPos);
	}
	public static Ray VViewportPointToRay(this Camera camera, Vector3 viewportPos)
	{
		//if (V.sceneViewOpen)
		//	return new Ray(new Vector3(0, 1000000, 0), Vector3.up);
		return camera.ViewportPointToRay(viewportPos);
	}*/
	/*public static Vector3 VWorldToScreenPoint(this Camera s, Vector3 point)
	{
		//var result = s.WorldToScreenPoint(point);
		//return new Vector3(result.x / s.rect.width, result.y / s.rect.height, result.z);

		var oldRect = s.rect;
		s.rect = new Rect(0, 0, Screen.width, Screen.height);
		var result = s.WorldToScreenPoint(point);
		s.rect = oldRect;
		return result;
	}*/
	/*public static Vector3 VScreenPointToRay(this Camera s, Vector3 point)
	{
	}*/
	public static VVector2 WorldToScreenPoint(this Camera s, VVector3 worldPoint, bool fromYForwardToUp = true) { return s.WorldToScreenPoint(worldPoint.ToVector3(fromYForwardToUp)).ToVVector3(false).ToVVector2(); }
	public static VVector3 WorldToScreenPoint_WithDepth(this Camera s, VVector3 worldPoint, bool fromYForwardToUp = true) { return s.WorldToScreenPoint(worldPoint.ToVector3(fromYForwardToUp)).ToVVector3(false); }
	public static VRay ScreenPointToRay(this Camera s, VVector2 screenPos) { return s.ScreenPointToRay(screenPos.ToVector2(false)).ToVRay(); }
	public static VRect GetScreenRect(this Camera s) { return new VRect(s.rect.x * Screen.width, s.rect.y * Screen.height, s.rect.width * Screen.width, s.rect.height * Screen.height); }

	// Animation
	public static List<AnimationState> GetStates(this Animation self)
	{
		var result = new List<AnimationState>();
		foreach (AnimationState state in self)
			result.Add(state);
		return result;
	}

	public static Vector3 GetPosition(this Matrix4x4 self) { return self.GetColumn(3); }
	public static Quaternion GetRotation(this Matrix4x4 self) { return Quaternion.LookRotation(self.GetColumn(2), self.GetColumn(1)); }
	public static Vector3 GetScale(this Matrix4x4 self) { return new Vector3(self.GetColumn(0).magnitude, self.GetColumn(1).magnitude, self.GetColumn(2).magnitude); }

	// AnimationState
	/*public static int GetFrameCount(this AnimationState self) { return (int)(self.length * self.clip.frameRate); }
	public static int GetFrame(this AnimationState self) { return (int)(self.time * self.clip.frameRate); }
	public static void SetFrame(this AnimationState self, int frame) { self.time = frame / self.clip.frameRate; }*/
	//public static int GetFrameCount(this AnimationState self) { return (int)(self.length * self.clip.frameRate); }
	/*public static int GetFrameCount(this AnimationState self) { return (int)Math.Round(self.length * self.clip.frameRate); }
	public static int GetFrame(this AnimationState self) { return self.GetMeta<int?>("frame") ?? 0; }
	public static void SetFrame(this AnimationState self, int frame)
	{
		self.time = frame / self.clip.frameRate;
		self.SetMeta("frame", frame);
	}*/

	//public static Action<string> Start(this MethodBase s) { return VSection.Start(s); }
	public static BlockRunInfo Profile_AllFrames(this MethodBase s, string title = null) {
		if (title != null)
			return Profiler_AllFrames.CurrentBlock.StartMethod(title);
		return Profiler_AllFrames.CurrentBlock.StartMethod(s);
	}
	// use M.None instead
	//public static BlockRunInfo Profile_AllFrames_DISABLED(this MethodBase s, string title = null) { return BlockRunInfo.fakeBlockRunInfo; }
	//public static BlockRunInfo_Disabled Profile_AllFrames_DISABLED(this MethodBase s, string title = null) { return BlockRunInfo_Disabled.main; }

	/*public static BlockRunInfo Profile_LastViewFrame(this MethodBase s, string title = null) {
		if (title != null)
			return Profiler_LastViewFrame.CurrentBlock.StartMethod(title);
		return Profiler_LastViewFrame.CurrentBlock.StartMethod(s);
	}*/
	public static BlockRunInfo Profile_LastDataFrame(this MethodBase s, string title = null, bool actuallyProfile = true) {
		if (!actuallyProfile)
			return BlockRunInfo.fakeBlockRunInfo;

		if (title != null)
			return Profiler_LastDataFrame.CurrentBlock.StartMethod(title);
		return Profiler_LastDataFrame.CurrentBlock.StartMethod(s);
	}
	// use M.None instead
	//public static BlockRunInfo Profile_LastDataFrame_DISABLED(this MethodBase s, string title = null) { return BlockRunInfo.fakeBlockRunInfo; }
	//public static BlockRunInfo_Disabled Profile_LastDataFrame_DISABLED(this MethodBase s, string title = null) { return BlockRunInfo_Disabled.main; }

	// Delegate
	public static object Call(this Delegate method, params object[] args) {
		if (args.Length > method.Method.GetParameters().Length)
			args = args.Take(method.Method.GetParameters().Length).ToArray();

		//return VMethodInfo.Get(method.Method).Call(null, args);
		object result;
		try {
			result = method.DynamicInvoke(args);
		}
		catch (TargetInvocationException ex) {
			VDebug.RethrowInnerExceptionOf(ex);
			throw null; // this never actually runs, but lets method compile
		}
		return result;
	}
	public static object Call_Advanced(this Delegate method, bool profile, params object[] args) {
		if (args.Length > method.Method.GetParameters().Length)
			args = args.Take(method.Method.GetParameters().Length).ToArray();

		//return VMethodInfo.Get(method.Method).Call(null, args);
		var S = profile ? method.Method.Profile_LastDataFrame() : BlockRunInfo.fakeBlockRunInfo;
		object result;
		try {
			result = method.DynamicInvoke(args);
			S._____(null);
		}
		catch (TargetInvocationException ex) {
			S._____(null);
			VDebug.RethrowInnerExceptionOf(ex);
			throw null; // this never actually runs, but lets method compile
		}
		return result;
	}

	// Expression<T>
	public static MemberExpression GetMemberExpression<T>(this Expression<T> expression) { return expression.Body is UnaryExpression ? (expression.Body as UnaryExpression).Operand as MemberExpression : expression.Body as MemberExpression; }

	// Exception
	public static void AddToMessage(this Exception self, string messageAdd) {
		/*if (self is VDFException)
			self.SetPropValue("message", self.GetPropValue("message") + messageAdd);
		else
			//VTypeInfo.Get(typeof(Exception)).props["message"].SetValue(self, self.Message + messageAdd);*/
		self.SetPropValue("message", self.Message + messageAdd);
	}

	// StringBuilder
	public static StringBuilder Substring(this StringBuilder self, int index, int length = -1)
	{
		length = length != -1 ? length : (self.Length - index);

		var result = new StringBuilder();
		for (var i = index; i < index + length; i++)
			result.Append(self[i]);
		return result;
	}

	// Nullable<T>
	public static T ValueOrDefault<T>(this T? s, T defaultValue = default(T)) where T : struct { return s.HasValue ? s.Value : defaultValue; }

	// object [VDF stuff]
	public static VTypeInfo GetVTypeInfo(this object s) { return VTypeInfo.Get(s.GetType()); }
	public static VDFTypeInfo GetVDFTypeInfo(this object s) { return VDFTypeInfo.Get(s.GetType()); }
	public static T EndProfileBlock<T>(this T s, BlockRunInfo block) {
		//block._____(null);
		block.End();
		return s;
	}
	// VDFPropInfo
	public static VPropInfo VInfo(this VDFPropInfo self) { return VPropInfo.Get(self.memberInfo); }
	// VPropInfo
	public static VDFPropInfo VDFInfo(this VPropInfo self) { return VDFPropInfo.Get(self.memberInfo); }

	// Type
	//public static bool IsClassOrNullable(this Type s) { return s.IsClass || typeof(Nullable<>).IsAssignableFrom(s); }
	public static bool IsClassOrNullable(this Type s) { return s.IsClass || (s.IsGenericType && s.GetGenericTypeDefinition() == typeof(Nullable<>)); }
	//public static bool IsReferenceTypeOrNullable(this Type s) { return !s.IsValueType || (s.IsGenericType && s.GetGenericTypeDefinition() == typeof(Nullable<>)); }
	public static MemberInfo Prop(this Type s, string propName, int propIndex = 0) { return s.GetMember(propName)[propIndex]; }

	// VDFNodePath
	// ==========

	public static VDFNodePathNode GetNodeWithProp(this VDFNodePath s)
	{
		VDFNodePathNode result = null;
		if (s.currentNode.prop != null) // if node zero up has prop-info
			result = s.currentNode;
		else if (s.parentNode != null && s.parentNode.prop != null) // if node one up has prop
			result = s.parentNode;
		else if (s.nodes.Count >= 3 && s.nodes.XFromLast(2).prop != null) // if node two up has prop
			result = s.nodes.XFromLast(2);
		return result;
	}
	public static VDFNodePathNode GetNodeWithParent(this VDFNodePath s)
	{
		var nodeWithProp = s.GetNodeWithProp();
		return nodeWithProp != null ? s.nodes.GetValue(s.nodes.IndexOf(nodeWithProp) - 1) : null;
	}
	public static VDFNodePathNode GetNodeWithIndexOrKey(this VDFNodePath s)
	{
		var nodeWithProp = s.GetNodeWithProp();
		return nodeWithProp != null ? s.nodes.GetValue(s.nodes.IndexOf(nodeWithProp) + 1) : null;
	}

	public static VDFPropInfo GetSourceProp(this VDFNodePath s)
	{
		var result = s.currentNode.prop; //GetNodeForProp();
		if (s.parentNode != null && s.parentNode.obj is Change && (result.memberInfo.Name == "value" || result.memberInfo.Name == "item"))
			result = (s.parentNode.obj as Change).propInfo.VDFInfo();
		return result;
	}

	public static NodePath ToNodePath(this VDFNodePath s, bool addNodeForSelf = false)
	{
		var result = new NodePath(new List<NodePathNode>());
		if (addNodeForSelf)
			result.nodes.Add(new NodePathNode {vdfRoot = true});
		for (var i = 0; i < s.nodes.Count; i++)
		{
			var node = s.nodes[i];
			if (node.prop != null)
				result.nodes.Add(new NodePathNode(node.prop.VInfo()));
			else if (node.list_index != -1)
				result.nodes.Add(new NodePathNode {listIndex = node.list_index});
			else if (node.map_key != null)
				result.nodes.Add(new NodePathNode(mapKey: node.map_key));
			/*else
				//result.nodes.Add(new NodePathNode {currentParent = true});
				result.nodes.Add(new NodePathNode {vdfRoot = true});*/
		}
		return result;
	}
	
	public static object GetFinalNodeValue(this VDFNodePath s)
	{
		var parentObj = s.parentNode.obj;
		if (s.currentNode.prop != null) // if final-node is prop
			return s.currentNode.prop.GetValue(parentObj);
		//var propValue = s.parentNode.prop.GetValue(s.nodes.XFromLast(2).obj);
		if (s.currentNode.list_index != -1) // if final-node is list index
			return (parentObj as IList)[s.currentNode.list_index];
		if (s.currentNode.map_keyIndex != -1) // if final-node is map key-index
			return (parentObj as IDictionary).Keys.ToList_Object()[s.currentNode.list_index];
		// final-node must be map key
		return (parentObj as IDictionary)[s.currentNode.map_key];
	}
	public static void SetFinalNodeValue(this VDFNodePath s, object value)
	{
		var parentObj = s.parentNode.obj;
		if (s.currentNode.prop != null) // if final-node is prop
			s.currentNode.prop.SetValue(parentObj, value);
		else if (s.currentNode.list_index != -1) // if final-node is list index
			(parentObj as IList)[s.currentNode.list_index] = value;
		else if (s.currentNode.map_keyIndex != -1) // if final-node is map key-index
		{
			/*var oldPair = (propValue as IDictionary).Pairs()[s.currentNode.map_keyIndex];
			(propValue as IDictionary).Keys[s.currentNode.map_keyIndex] = value;*/

			// remove old-pair, add new pair (with old-pair's value), and have keys update their attach-point data
			/*var oldPair = (propValue as IDictionary).Pairs()[s.currentNode.list_index];
			(propValue as IDictionary).Remove(oldPair.Key);
			(propValue as IDictionary)[value] = oldPair.Value;
			// old: make-so: keys update their attach-point data*/

			// maybe temp; rely on specially-added map_key data, to find old-pair (s.currentNode.map_keyIndex might be out-of-date by now, if dictionary was modified since this path's creation)
			var map_keyIndex = (parentObj as IDictionary).Keys.ToList_Object().IndexOf(s.currentNode.map_key);
			var oldPair = (parentObj as IDictionary).Pairs()[map_keyIndex];
			(parentObj as IDictionary).Remove(s.currentNode.map_key);
			(parentObj as IDictionary).Add(value, oldPair.Value);
		}
		else // else, final-node must be map key
			(parentObj as IDictionary)[s.currentNode.map_key] = value;
	}
	// ==========

	// NodePath
	/*public static VDFNodePath ToVDFNodePath(this NodePath s)
	{
		var result = new VDFNodePath();
		foreach (NodePathNode node in s.nodes)
		{
			result.nodes.Add(new VDFNodePathNode(null, ));
		}
	}*/

	// Object
	// make-so: this works in player
	public static bool IsInAsset(this Object s) {
		return AssetDatabase.Contains(s) || AssetDatabase.IsSubAsset(s) || PrefabUtility.GetPrefabParent(s) || PrefabUtility.GetPrefabObject(s) || AssetDatabase.GetAssetPath(s) != "";
	}

	// WWW
	public static Texture2D GetTexture(this WWW self, bool generateMipMaps = true)
	{
		var source = self.texture;
		if (!generateMipMaps)
			return source;

		var temp = new Texture2D(source.width, source.height);
        //var temp = new Texture2D(source.width, source.height, source.format, true);
		//self.LoadImageIntoTexture(temp);
		temp.SetPixels32(source.GetPixels32());
		temp.Apply();
		return temp;
	}

	// Material
	public enum BlendMode
	{
		Opaque,
		Cutout,
		Fade,		// Old school alpha-blending mode, fresnel does not affect amount of transparency
		Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
	}
	public static void SetBlendMode(this Material s, BlendMode mode)
	{
		s.SetFloat("_Mode", (float)mode);
		switch (mode)
		{
			case BlendMode.Opaque:
				//s.SetOverrideTag("RenderType", "");
				s.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				s.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				s.SetInt("_ZWrite", 1);
				s.DisableKeyword("_ALPHATEST_ON");
				s.DisableKeyword("_ALPHABLEND_ON");
				s.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				s.renderQueue = -1;
				break;
			case BlendMode.Cutout:
				//s.SetOverrideTag("RenderType", "TransparentCutout");
				s.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				s.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				s.SetInt("_ZWrite", 1);
				s.EnableKeyword("_ALPHATEST_ON");
				s.DisableKeyword("_ALPHABLEND_ON");
				s.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				s.renderQueue = 2450;
				break;
			case BlendMode.Fade:
				//s.SetOverrideTag("RenderType", "Transparent");
				s.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				s.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				s.SetInt("_ZWrite", 0);
				s.DisableKeyword("_ALPHATEST_ON");
				s.EnableKeyword("_ALPHABLEND_ON");
				s.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				s.renderQueue = 3000;
				break;
			case BlendMode.Transparent:
				//s.SetOverrideTag("RenderType", "Transparent");
				s.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				s.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				s.SetInt("_ZWrite", 0);
				s.DisableKeyword("_ALPHATEST_ON");
				s.DisableKeyword("_ALPHABLEND_ON");
				s.EnableKeyword("_ALPHAPREMULTIPLY_ON");
				s.renderQueue = 3000;
				break;
		}
	}

	// runtime-assert // probably temp
	// ==========

	/*public static void RuntimeAssertIs(this object s, object val)
	{
		if (s != val)
			throw new Exception("RuntimeAssertIs assertion failed.");
	}
	public static void RuntimeAssertIs<T>(this T s, T val)
	{
		if (s != val)
			throw new Exception("RuntimeAssertIs assertion failed.");
	}*/
	public static void RuntimeAssertEquals(this object s, object val)
	{
		if (!s.Equals(val))
			throw new Exception("RuntimeAssertEquals assertion failed.");
	}
}

// note; needed to keep V methods from having priority over V class
public static class ClassExtensions_Extra
{
	// Vector3
	public static string V(this Vector3 obj) { return obj.ToVString(); }
}