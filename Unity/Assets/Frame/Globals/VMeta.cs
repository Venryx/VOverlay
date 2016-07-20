using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VNullClass {} // for null
public class VMeta
{
	public static VMeta main = new VMeta();

	public Dictionary<int, Dictionary<object, VWeakReference>> weakMeta = new Dictionary<int, Dictionary<object, VWeakReference>>();
	public Dictionary<int, Dictionary<object, object>> strongMeta = new Dictionary<int, Dictionary<object, object>>();
	public VNullClass SetMeta(object obj, object metaKey, VNullClass metaValue, bool useStrongStorage = true) { return SetMeta<VNullClass>(obj, metaKey, metaValue, useStrongStorage); } // for null
	public T SetMeta<T>(object obj, object metaKey, T metaValue, bool useStrongStorage = true)
	{
		var objHash = obj != null ? obj.GetHashCode() : -1;
		if (useStrongStorage)
		{
			if (!strongMeta.ContainsKey(objHash))
				strongMeta[objHash] = new Dictionary<object, object>();
			strongMeta[objHash][metaKey] = metaValue;
		}
		else
		{
			var valueRef = new VWeakReference(metaValue);
			if (!weakMeta.ContainsKey(objHash))
				weakMeta[objHash] = new Dictionary<object, VWeakReference>();
			weakMeta[objHash][metaKey] = valueRef;
		}
		return metaValue;
	}
	
	// probably todo: have use the faster TryGetValue system
	public T GetMeta<T>(object obj, object metaKey) {
		var result = GetMeta(obj, metaKey);
		return (T)result;
	}
	public object GetMeta(object obj, object metaKey) { return GetMeta<object>(obj, metaKey, null); }
	public T GetMeta<T>(object obj, object metaKey, T returnValueIfMissing, bool useStrongStorage = true)
	{
		var objHash = obj != null ? obj.GetHashCode() : -1;
		if (useStrongStorage)
			return strongMeta.ContainsKey(objHash) && strongMeta[objHash].ContainsKey(metaKey) ? (T)strongMeta[objHash][metaKey] : returnValueIfMissing;
		return weakMeta.ContainsKey(objHash) && weakMeta[objHash].ContainsKey(metaKey) ? (T)weakMeta[objHash][metaKey].Target : returnValueIfMissing;
	}

	public void RemoveMeta(object obj, object metaKey, bool useStrongStorage = true)
	{
		var objHash = obj != null ? obj.GetHashCode() : -1;
		if (useStrongStorage)
			strongMeta[objHash].Remove(metaKey);
		else
			weakMeta[objHash].Remove(metaKey);
	}

	public Dictionary<object, object> GetMetaSet_Strong(object obj)
	{
		var objHash = obj != null ? obj.GetHashCode() : -1;
		return strongMeta.ContainsKey(objHash) ? strongMeta[objHash] : new Dictionary<object, object>();
	}
	public void ClearMeta(object obj, bool useStrongStorage = true)
	{
		var objHash = obj != null ? obj.GetHashCode() : -1;
		if (useStrongStorage)
			strongMeta.Remove(objHash);
		else
			weakMeta.Remove(objHash);
	}
}

public static class FrameMeta
{
	static Dictionary<int, FrameMetaPack> packs = new Dictionary<int, FrameMetaPack>();

	public static FrameMetaPack current
	{
		get { return at(Time.frameCount); }
	}
	public static FrameMetaPack last
	{
		get { return at(Time.frameCount - 1); }
	}
	public static FrameMetaPack back(int framesBack) { return at(Time.frameCount - framesBack); }
	public static FrameMetaPack at(int frameIndex)
	{
		DeleteOldPacks();
		if (!packs.ContainsKey(frameIndex))
			packs.Add(frameIndex, new FrameMetaPack());
		return packs[frameIndex];
	}

	static void DeleteOldPacks()
	{
		for (var i = Time.frameCount - 10; i >= 0; i--) // for now, delete any frame-pack at least 10 frames in the past
			if (packs.ContainsKey(i))
				packs.Remove(i);
			else
				break;
	}
}
public class FrameMetaPack
{
	public Dictionary<int, Dictionary<object, VWeakReference>> weakMeta = new Dictionary<int, Dictionary<object, VWeakReference>>();
	public Dictionary<int, Dictionary<object, object>> strongMeta = new Dictionary<int, Dictionary<object, object>>();
	public VNullClass SetMeta(object obj, object metaKey, VNullClass metaValue, bool useStrongStorage = true) { return SetMeta<VNullClass>(obj, metaKey, metaValue, useStrongStorage); } // for null
	public T SetMeta<T>(object obj, object metaKey, T metaValue, bool useStrongStorage = true)
	{
		var objHash = obj != null ? obj.GetHashCode() : -1;
		if (useStrongStorage)
		{
			if (!strongMeta.ContainsKey(objHash))
				strongMeta[objHash] = new Dictionary<object, object>();
			strongMeta[objHash][metaKey] = metaValue;
		}
		else
		{
			var valueRef = new VWeakReference(metaValue);
			if (!weakMeta.ContainsKey(objHash))
				weakMeta[objHash] = new Dictionary<object, VWeakReference>();
			weakMeta[objHash][metaKey] = valueRef;
		}
		return metaValue;
	}

	public T GetMeta<T>(object obj, object metaKey) { return (T)GetMeta(obj, metaKey); }
	public object GetMeta(object obj, object metaKey) { return GetMeta<object>(obj, metaKey, null); }
	public T GetMeta<T>(object obj, object metaKey, T returnValueIfMissing, bool useStrongStorage = true)
	{
		var objHash = obj != null ? obj.GetHashCode() : -1;
		if (useStrongStorage)
			return strongMeta.ContainsKey(objHash) && strongMeta[objHash].ContainsKey(metaKey) ? (T)strongMeta[objHash][metaKey] : returnValueIfMissing;
		return weakMeta.ContainsKey(objHash) && weakMeta[objHash].ContainsKey(metaKey) ? (T)weakMeta[objHash][metaKey].Target : returnValueIfMissing;
	}

	public void ClearMeta(object obj, bool useStrongStorage = true)
	{
		var objHash = obj != null ? obj.GetHashCode() : -1;
		if (useStrongStorage)
			strongMeta.Remove(objHash);
		else
			weakMeta.Remove(objHash);
	}
}