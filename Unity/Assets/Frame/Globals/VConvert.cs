using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using ProtoBuf;
using VDFN;
using VTree_Structures;

public class VConvert : MonoBehaviour
{
	// vdf
	// ==========

	static Dictionary<string, string> extraNamespaceAliases;
	static Dictionary<string, string> GetExtraNamespacesAliases() {
		if (extraNamespaceAliases == null) {
			extraNamespaceAliases = new Dictionary<string, string>();
			extraNamespaceAliases.Add("VTree_Structures", null);
			extraNamespaceAliases.Add("PathFinderSystem", null);
		}
		return extraNamespaceAliases;
	}
	static Dictionary<Type, string> extraTypeAliases;
	static Dictionary<Type, string> GetExtraTypeAliases() {
		if (extraTypeAliases == null) {
			extraTypeAliases = new Dictionary<Type, string>();
			foreach (Type type in Assembly.GetExecutingAssembly().GetExportedTypes())
				if (type.Namespace != null && (type.Namespace.StartsWith("VTree.") || type.Namespace.StartsWith("VScriptN")))
					extraTypeAliases.Add(type, type.Name);
			if (!V.inUnitTests)
				extraTypeAliases.Add(Type.GetType("System.MonoType"), "Type");
			//options.typeAliasesByType.Add(typeof(VDictionary<,>), "Dictionary");
			extraTypeAliases.Add(typeof(Vector3), "Vector3");
			//options.typeAliasesByType.Add(typeof(Change_Set<object>), "Change_Set");
			extraTypeAliases.Add(typeof(IList), "IList");
			extraTypeAliases.Add(typeof(IDictionary), "IDictionary");
			//extraTypeAliases.Add(typeof(VObject_Type), "VOT");
		}
		return extraTypeAliases;
	}

	public static string GetNameOfType(Type type, VDFSaveOptions options = null)
	{
		var result = VDF.GetNameOfType(type, FinalizeToVDFOptions(options ?? new VDFSaveOptions()));
		if (result == "System.Type") // (fix must occur here, since we can't reference typeof(object) directly, since odd crash occurs when that's attempted)
			result = "Type";
		return result;
	}
	public static Type GetTypeByName(string name, VDFLoadOptions options = null) { return VDF.GetTypeByName(name, FinalizeFromVDFOptions(options ?? new VDFLoadOptions())); }
	public static VDFSaveOptions FinalizeToVDFOptions(VDFSaveOptions options)
	{
		// for now, assume that we aren't going to modify the static dictionaries
		/*options.namespaceAliasesByName.AddDictionary(GetExtraNamespacesAliases());
		options.typeAliasesByType.AddDictionary(GetExtraTypeAliases());*/
		options.namespaceAliasesByName = GetExtraNamespacesAliases();
		options.typeAliasesByType = GetExtraTypeAliases();
		return options;
	}
	/*public static VDFNode ToVDFNode(object obj, bool markRootType = false, VDFTypeMarking typeMarking = VDFTypeMarking.Internal, VDFSaveOptions options = null)
	{
		options = FinalizeToVDFOptions(options ?? new VDFSaveOptions());
		options.typeMarking = typeMarking;
		return VDFSaver.ToVDFNode(obj, !markRootType && obj != null ? obj.GetType() : null, options);
	}*/
	public static string ToVDF(object obj, bool markRootType = false, VDFTypeMarking typeMarking = VDFTypeMarking.Internal, VDFSaveOptions options = null) {
		var S = M.GetCurrentMethod().Profile_LastDataFrame(actuallyProfile: options is VDFSaveOptionsV && (options as VDFSaveOptionsV).profile);
		
		S._____("part 1");
		//return ToVDFNode(obj, markRootType, typeMarking, options).ToVDF(options);
		options = FinalizeToVDFOptions(options ?? new VDFSaveOptions());
		options.typeMarking = typeMarking;

		S._____("to VDFNode");
		//return VDF.Serialize(obj, !markRootType && obj != null ? obj.GetType() : null, options);
		var node = VDFSaver.ToVDFNode(obj, !markRootType && obj != null ? obj.GetType() : null, options);
		S._____("to VDF");
		var result = node.ToVDF(options);
		S._____(null);
		return result;
	}
	public static VDFNode ToVDFNode(object obj, bool markRootType = false, VDFTypeMarking typeMarking = VDFTypeMarking.Internal, VDFSaveOptions options = null) {
		options = FinalizeToVDFOptions(options ?? new VDFSaveOptions());
		options.typeMarking = typeMarking;
		var result = VDFSaver.ToVDFNode(obj, !markRootType && obj != null ? obj.GetType() : null, options);
		return result;
	}

	static Dictionary<Type, string> typeAliasesByType_standard; 
	public static VDFLoadOptions FinalizeFromVDFOptions(VDFLoadOptions options)
	{
		// for now, assume that we aren't going to modify the static dictionaries
		/*options.namespaceAliasesByName.AddDictionary(GetExtraNamespacesAliases());
		options.typeAliasesByType.AddDictionary(GetExtraTypeAliases());
		options.typeAliasesByType.Add(typeof(List<object>), "Array");*/
		
		options.namespaceAliasesByName = GetExtraNamespacesAliases();
		if (typeAliasesByType_standard == null) {
			typeAliasesByType_standard = GetExtraTypeAliases().ToDictionary(a=>a.Key, a=>a.Value);
			typeAliasesByType_standard.Add(typeof(List<object>), "Array");
		}
		options.typeAliasesByType = typeAliasesByType_standard;
		return options;
	}
	public static T FromVDF<T>(string vdf, VDFLoadOptions options = null) {
		//return VDFLoader.ToVDFNode(vdf, declaredType, options).ToObject(declaredType, options)
		return (T)FromVDF(vdf, typeof(T), options);
	}
	public static object FromVDF(string vdf, Type declaredType, VDFLoadOptions options = null) {
		// split the code into parts, so profiling code can be added
		//return VDF.Deserialize(vdf, declaredType, FinalizeFromVDFOptions(options ?? new VDFLoadOptions()));

		options = FinalizeFromVDFOptions(options ?? new VDFLoadOptions());
		var S = options.profile ? M.GetCurrentMethod().Profile_AllFrames() : M.None_SameType;
		S._____("to vdf node");
		var vdfNode = VDFLoader.ToVDFNode(vdf, declaredType, options);
		S._____("to object");
		var result = vdfNode.ToObject(declaredType, options);
		S._____(null);
		return result;
	}
	public static T FromVDFNode<T>(VDFNode vdfNode, VDFLoadOptions options = null) {
		options = FinalizeFromVDFOptions(options ?? new VDFLoadOptions());
		var result = (T)vdfNode.ToObject(typeof(T), options);
		return result;
	}
	public static void FromVDFInto(string vdf, object obj, VDFLoadOptions options = null) {
		VDF.DeserializeInto(vdf, obj, FinalizeFromVDFOptions(options ?? new VDFLoadOptions()));
	}

	//public static T Clone<T>(T obj, VDFSaveOptions saveOptions = null, VDFLoadOptions loadOptions = null) { return FromVDF<T>(ToVDF(obj, options: saveOptions), loadOptions); }

	// others
	// ==========

	public static string TextureToPNGString(Texture2D obj) { return Convert.ToBase64String(obj.EncodeToPNG()); }
	public static Texture2D PNGStringToTexture(string str)
	{
		var result = new Texture2D(0, 0);
		result.LoadImage(Convert.FromBase64String(str));
		result.Apply();
		return result;
	}
	//public static string TextureToJPGString(Texture2D obj) { return Convert.ToBase64String(obj.EncodeToJPG()); }
	public static Texture2D TextureToEditableTexture(Texture2D image) // may be broken; this didn't seem to blit the texture properly last time I tried it
	{
		//var renderTexture = new RenderTexture(image.width, image.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
		var renderTexture = new RenderTexture(image.width, image.height, 0);
		renderTexture.Create();
		Graphics.Blit(image, renderTexture);

		var oldActiveRenderTexture = RenderTexture.active;
		RenderTexture.active = renderTexture;
		var result = new Texture2D(image.width, image.height);
		result.ReadPixels(new Rect(0, 0, image.width, image.height), 0, 0);
		RenderTexture.active = oldActiveRenderTexture;

		return result;
	}

	public static string Vector3ToString(Vector3 obj) { return obj.x + "&" + obj.y + "&" + obj.z; }
	public static Vector3 StringToVector3(string obj)
	{
		string[] a = obj.Split("&"[0]);
		return new Vector3(float.Parse(a[0]), float.Parse(a[1]), float.Parse(a[2]));
	}

	public static string QuaternionToString(Quaternion obj) { return obj.x + "&" + obj.y + "&" + obj.z + "&" + obj.w; }
	public static Quaternion StringToQuaternion(string obj)
	{
		string[] a = obj.Split("&"[0]);
		return new Quaternion(float.Parse(a[0]), float.Parse(a[1]), float.Parse(a[2]), float.Parse(a[3]));
	}

	public static string StringArrayToString(string[] obj) { return String.Join("#"[0].ToString(), obj); }
	public static String[] StringToStringArray(string obj) { return obj.Split("#"[0]); }

	public static string ToFormalName(string name)
	{
		string result = name;
		char[] oldChars = name.ToCharArray();
		foreach (char ch in oldChars)
			if (ch >= 65 && ch < 65 + 26) // if capital letter
				result = result.Replace(ch.ToString(), " " + ch); // add space before it
		return result.TrimStart();
	}

	public static string FloatToTime(float time)
	{
		string after;
		if (time >= 12.00f)
			after = " PM";
		else
			after = " AM";

		if (time < 1.0f) // The hour before 1 AM is 12 AM.
			time += 12.0f;
		if (time >= 13.00f) // The hours after 12 AM are X - 12 PM
			time -= 12.0f;

		string timeString = time.ToString();
		if (!timeString.Contains("."))
			timeString = timeString + ".00";

		int hours = int.Parse(timeString.Substring(0, timeString.IndexOf(".")));
		string minutesString = (float.Parse(timeString.Substring(timeString.IndexOf("."))) * .6f).ToString() + "000";
		if (minutesString.Contains("E"))
			minutesString = "0000"; // So close to zero, it's using scientific notation; simplify.
		int minutes = int.Parse(minutesString.Substring(2, 2));

		return hours + ":" + (minutes < 10.0f ? "0" : "") + minutes + after;
	}
	public static string FloatToTimePeriod(float time)
	{
		string after = " Hours";

		string timeString = time.ToString();
		if (!timeString.Contains("."))
			timeString = timeString + ".00";

		int hours = int.Parse(timeString.Substring(0, timeString.IndexOf(".")));
		string minutesString = (float.Parse(timeString.Substring(timeString.IndexOf("."))) * .6f).ToString() + "000";
		if (minutesString.Contains("E"))
			minutesString = "0000"; // so close to zero, it's using scientific notation; simplify.
		int minutes = int.Parse(minutesString.Substring(2, 2));

		return hours + ":" + (minutes < 10.0f ? "0" : "") + minutes + after;
	}

	/*[ProtoContract] public class ArrayWrap<T>
	{
		public ArrayWrap(int length) { val = new T[length]; }
		[ProtoMember(1)] public T[] val;
	}
	public static ArrayWrap<T>[] MultiArrayToJagged_2_Wrapped<T>(T[,] array)
	{
		var result = new ArrayWrap<T>[array.GetLength(0)];
		for (int i1 = 0; i1 < array.GetLength(0); i1++)
		{
			result[i1] = new ArrayWrap<T>(array.GetLength(1));
			for (int i2 = 0; i2 < array.GetLength(1); i2++)
				result[i1].val[i2] = array[i1, i2];
		}
		return result;
	}
	public static T[,] JaggedArrayToMulti_2_Wrapped<T>(ArrayWrap<T>[] array)
	{
		var result = new T[array.Length, array[0].val.Length];
		for (int i1 = 0; i1 < array.Length; i1++)
			for (int i2 = 0; i2 < array[0].val.Length; i2++)
				result[i1, i2] = array[i1].val[i2];
		return result;
	}*/

	/*public static T[][] MultiArrayToJagged_2<T>(T[,] array)
	{
		var result = new T[array.GetLength(0)][];
		for (int i1 = 0; i1 < array.GetLength(0); i1++)
		{
			result[i1] = new T[array.GetLength(1)];
			for (int i2 = 0; i2 < array.GetLength(1); i2++)
				result[i1][i2] = array[i1, i2];
		}
		return result;
	}
	public static T[,] JaggedArrayToMulti_2<T>(T[][] array)
	{
		var result = new T[array.Length, array[0].Length];
		for (int i1 = 0; i1 < array.Length; i1++)
			for (int i2 = 0; i2 < array[0].Length; i2++)
				result[i1, i2] = array[i1][i2];
		return result;
	}

	public static T[][][] MultiArrayToJagged_3<T>(T[,,] array)
	{
		var result = new T[array.GetLength(0)][][];
		for (int i1 = 0; i1 < array.GetLength(0); i1++)
		{
			result[i1] = new T[array.GetLength(1)][];
			for (int i2 = 0; i2 < array.GetLength(1); i2++)
			{
				result[i1][i2] = new T[array.GetLength(2)];
				for (int i3 = 0; i3 < array.GetLength(2); i3++)
					result[i1][i2][i3] = array[i1, i2, i3];
			}
		}
		return result;
	}
	public static T[,,] JaggedArrayToMulti_3<T>(T[][][] array)
	{
		var result = new T[array.Length, array[0].Length, array[0][0].Length];
		for (int i1 = 0; i1 < array.Length; i1++)
			for (int i2 = 0; i2 < array[0].Length; i2++)
				for (int i3 = 0; i3 < array[0][0].Length; i3++)
					result[i1, i2, i3] = array[i1][i2][i3];
		return result;
	}

	public static T[] MultiArrayToFlat_3<T>(T[,,] array, out int[] array_lengths)
	{
		array_lengths = new int[3] {array.GetLength(0), array.GetLength(1), array.GetLength(2)};
		var result = new T[array_lengths[0] * array_lengths[1] * array_lengths[2]];
		for (int i1 = 0; i1 < array_lengths[0]; i1++)
			for (int i2 = 0; i2 < array_lengths[1]; i2++)
				for (int i3 = 0; i3 < array_lengths[2]; i3++)
					result[(i1 * array_lengths[1] * array_lengths[2]) + (i2 * array_lengths[2]) + i3] = array[i1, i2, i3];
		return result;
	}
	public static T[,,] FlatArrayToMulti_3<T>(T[] array, int[] array_lengths)
	{
		var result = new T[array_lengths[0], array_lengths[1], array_lengths[2]];
		for (int i1 = 0; i1 < array_lengths[0]; i1++)
			for (int i2 = 0; i2 < array_lengths[1]; i2++)
				for (int i3 = 0; i3 < array_lengths[2]; i3++)
					result[i1, i2, i3] = array[(i1 * array_lengths[1] * array_lengths[2]) + (i2 * array_lengths[2]) + i3];
		return result;
	}*/
}