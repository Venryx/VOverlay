using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SocketIO;
using VDFN;

//[Serializable] public class VDictionary<TKey, TValue> : SerializableDictionary<TKey, TValue> {}
[Serializable] public class VDictionary_StringString : SerializableDictionary<string, string> {}
[Serializable] public class VDictionary_StringInt : SerializableDictionary<string, int> {}
[Serializable] public class VDictionary_StringDouble : SerializableDictionary<string, double> {}

public class Quick : MonoBehaviour {
	public static Quick main;
	void Start() { main = this; }

	//[Inspectionary] public VDictionary<string, string> quickStrings;
	[Inspectionary] public VDictionary_StringString quickStrings;
	[Inspectionary] public VDictionary_StringInt quickInts;
	[Inspectionary] public VDictionary_StringDouble quickDoubles;

	public static string GetString(string key, string defaultVal = null) { return main.quickStrings.GetValueOrX(key, defaultVal); }
	public static int GetInt(string key, int defaultVal = 0) { return main.quickInts.GetValueOrX(key, defaultVal); }
	public static double GetDouble(string key, double defaultVal = 0) { return main.quickDoubles.GetValueOrX(key, defaultVal); }
}