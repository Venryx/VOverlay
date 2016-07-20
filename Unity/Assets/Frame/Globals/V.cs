using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using UnityEngine;
using VDFN;
using VectorStructExtensions;
using VTree;
using VTree_Structures;
using Debug = UnityEngine.Debug;
using Expression = System.Linq.Expressions.Expression;
using Object = UnityEngine.Object;

// fix for obscure compiler error (note; a better solution, which may work, is to add the NET_35, or such, defines)
namespace System.Runtime.CompilerServices { public class ExtensionAttribute : Attribute {} }

public static class DebugFlags
{
	// general
	public static bool catchErrors;

	// temp
	public static bool flag1;
}

/*public static class VAssetProcessor_RuntimeData
{
	public static bool makeTexturesReadable;
}*/

/*public class MessageBoxException : Exception
{
	public MessageBoxException(string message) : base(message) {}
}*/
public class ScriptException : Exception {
	static List<ScriptException> sharedExceptionPool = new List<ScriptException>(); // create pool, so that same-frame exception-throws don't overwrite the same exception's message
	static ScriptException() {
		for (var i = 0; i < 100; i++)
			sharedExceptionPool.Add((ScriptException)FormatterServices.GetUninitializedObject(typeof(ScriptException)));
	}

	static int lastSharedExceptionIndex = -1;
	public static ScriptException New(string message) { // faster (when full-debugging is disabled), since stack-trace creation is bypassed
		//if (VO.main.settings.fullScriptDebugging) // maybe todo: add this back
		//	throw new ScriptException(message);
		var sharedExceptionIndex = lastSharedExceptionIndex < sharedExceptionPool.Count - 1 ? lastSharedExceptionIndex + 1 : 0;
		var ex = sharedExceptionPool[sharedExceptionIndex]; //(ScriptException)FormatterServices.GetUninitializedObject(typeof(ScriptException));
		ex.message = message; //FrameMeta.current.SetMeta(ex, "message", message);
		lastSharedExceptionIndex = sharedExceptionIndex;
		//throw ex;
		return ex;
	}

	public string message;
	public ScriptException(string message) : base(message) {} // does not set ScriptException.message prop, but rather, the Exception.Message prop
	/*public static implicit operator Exception(ScriptException obj)
	{
		var ex = (Exception)FormatterServices.GetUninitializedObject(typeof(Exception));
		FrameMeta.current.SetMeta(ex, "message", obj.message);
		throw ex;
	}*/

	public override string ToString() { return message ?? base.ToString(); }
}

public class M : MethodBase {
	//public static BlockRunInfo_Disabled DISABLED() { return BlockRunInfo_Disabled.main; }
	public static BlockRunInfo_Disabled None = BlockRunInfo_Disabled.main;
	public static BlockRunInfo None_SameType = BlockRunInfo.fakeBlockRunInfo;

	public override object[] GetCustomAttributes(Type attributeType, bool inherit) { throw new NotImplementedException(); }
	public override MethodImplAttributes GetMethodImplementationFlags() { throw new NotImplementedException(); }
	public override ParameterInfo[] GetParameters() { throw new NotImplementedException(); }
	public override Type DeclaringType { get { throw new NotImplementedException(); } }
	public override MemberTypes MemberType { get { throw new NotImplementedException(); } }
	public override RuntimeMethodHandle MethodHandle { get { throw new NotImplementedException(); } }
	public override MethodAttributes Attributes { get { throw new NotImplementedException(); } }
	public override string Name { get { throw new NotImplementedException(); } }
	public override Type ReflectedType { get { throw new NotImplementedException(); } }
	public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture) { throw new NotImplementedException(); }
	public override bool IsDefined(Type attributeType, bool inherit) { throw new NotImplementedException(); }
	public override object[] GetCustomAttributes(bool inherit) { throw new NotImplementedException(); }
}

// custom exception class, set to not trigger debugger on being thrown
public class Exception_Silent : Exception {
	public Exception_Silent(string message) : base(message) { }
	public Exception_Silent(string message, Exception innerException) : base(message, innerException) {}
}

public class Pack<A, B> { //public struct Pack<A, B>
	public A a;
	public B b;
	public Pack(A a, B b) {
		this.a = a;
		this.b = b;
	}

	public override int GetHashCode() { return (a.GetHashCode() + ";" + b.GetHashCode()).GetHashCode(); }
	public override bool Equals(object obj) { return GetHashCode() == obj.GetHashCode(); }
	//public override string ToString() { return GetHashCode().ToString(); }
}
public class Pack<A, B, C> { //public struct Pack<A, B, C>
	public A a;
	public B b;
	public C c;
	public Pack(A a, B b, C c) {
		this.a = a;
		this.b = b;
		this.c = c;
	}

	public override int GetHashCode() { return (a.GetHashCode() + ";" + b.GetHashCode() + ";" + c.GetHashCode()).GetHashCode(); }
	public override bool Equals(object obj) { return GetHashCode() == obj.GetHashCode(); }
	//public override string ToString() { return GetHashCode().ToString(); }
}

/*public enum VColorEnum {
	White,
	Red,
	Green,
	Blue
}*/
public struct VColor {
	public static VColor Black { get { return new VColor(0, 0, 0); } }
	public static VColor Gray { get { return new VColor(.5, .5, .5); } }
	public static VColor White { get { return new VColor(1, 1, 1); } }
	public static VColor Red { get { return new VColor(1, 0, 0); } }
	public static VColor Green { get { return new VColor(0, 1, 0); } }
	public static VColor Blue { get { return new VColor(0, 0, 1); } }

	public static List<VColor> GetBasicColors() {
		var colors = new List<VColor>();
		for (var r = 0d; r <= 1; r += .5)
			for (var g = 0d; g <= 1; g += .5)
				for (var b = 0d; b <= 1; b += .5) {
					var comps = new[] {r, g, b};
					if (comps.Count(a=>a == 1) >= 1 && comps.Count(a=>a == 1) <= 2) // if has at least one, and at most two, 1-comp
						if (comps.Count(a=>a == .5) <= 1) // and has at most one .5-comp
							colors.Add(new VColor(r, g, b)); // then it's valid; add it to the color-list
				}
		//colors.Shuffle(new System.Random(0)); // 'randomize' color order, but in the same way every time
		//colors.Insert(0, Black);
		return colors;
	}

	[VDFDeserialize] void Deserialize(VDFNode node) {
		var strParts = node.primitiveValue.ToString().Split(' ').ToList();
		red = float.Parse(strParts[0]);
		green = float.Parse(strParts[1]);
		blue = float.Parse(strParts[2]);
		alpha = strParts.Count >= 4 ? float.Parse(strParts[3]) : 1;
	}
	[VDFSerialize] VDFNode Serialize() { return new VDFNode(ToString()); }
	public override string ToString() { return red + " " + green + " " + blue + (alpha != 1 ? " " + alpha : ""); }

	public VColor(double red, double green, double blue, double alpha = 1) {
		this.red = red;
		this.green = green;
		this.blue = blue;
		this.alpha = alpha;
	}

	public double red;
	public double green;
	public double blue;
	public double alpha;

	public static implicit operator Color(VColor s) { return new Color((float)s.red, (float)s.green, (float)s.blue, (float)s.alpha); }
	public static implicit operator VColor(Color s) { return new VColor(s.r, s.g, s.b, s.a); }
	public static implicit operator Color32(VColor s) { return new Color32((byte)(s.red * 255), (byte)(s.green * 255), (byte)(s.blue * 255), (byte)(s.alpha * 255)); }
	public static implicit operator VColor(Color32 s) { return new VColor(s.r / 255d, s.g / 255d, s.b / 255d, s.a / 255d); }

	/*public static Color ToColor() {
		if (color == VColor.White)
			return Color.white;
		if (color == VColor.Red)
			return Color.red;
		if (color == VColor.Green)
			return Color.green;
		return Color.blue;
	}*/

	// VColor
	public VColor NewR(double val) { return new VColor(val, green, blue, alpha); }
	public VColor NewG(double val) { return new VColor(red, val, blue, alpha); }
	public VColor NewB(double val) { return new VColor(red, green, val, alpha); }
	public VColor NewA(double val) { return new VColor(red, green, blue, val); }
}

public static class V {
	//static int mainThreadID = Thread.CurrentThread.ManagedThreadId;
	//static Thread mainThread = Thread.CurrentThread;
	public static bool OnMainThread() {
		//return Thread.CurrentThread.ManagedThreadId == mainThreadID;
		//return Thread.CurrentThread == mainThread;
		return Thread.CurrentThread.GetApartmentState() != ApartmentState.MTA && !Thread.CurrentThread.IsBackground && !Thread.CurrentThread.IsThreadPoolThread && Thread.CurrentThread.IsAlive;
	}

	/*public static int lastAutoID_general = -1;
	public static int GetAutoID_General() { return ++lastAutoID_general; }*/

	public static void Alert(string title, object message, string ok = "OK") {
#if UNITY_EDITOR
		UnityEditor.EditorUtility.DisplayDialog(title, "" + message, ok);
#else
		Debug.Log("[\"" + title + "\"]" + message);
#endif
	}

	public static void Nothing(params object[] args) {}
	//public static void DoNothing(params object[] args) {}

	/// <summary>Assert a condition and logs a formatted error message to the Unity console on failure.</summary>
	/// <param name="condition">Condition you expect to be true.</param>
	/// <param name="context">Object to which the message applies.</param>
	/// <param name="message">String or object to be converted to string representation for display.</param>
	//public static void Assert(bool condition, object message = null, Object context = null) {
	public static void Confirm(bool condition, object message = null, Object context = null) {
		if (!condition)
			Debug.logger.Log(LogType.Assert, message, context);
	}

	public static void Try(Action call, Action<Exception> failHandler) {
		try { call(); } catch (Exception ex) { failHandler(ex); }
	}

	public static void Try(Action action, bool break_ = true, bool logError = false, bool rethrowError = true) {
		try { action(); }
		catch (Exception ex) {
			var exCopy = ex;
			if (break_)
				BreakIfTrue(true); //V.Nothing();
			if (logError)
				Debug.LogException(ex);
			if (rethrowError)
				throw;
		}
	}
	public static T TryF<T>(Func<T> func, bool logError = false, bool rethrowError = true) {
		try { return func(); }
		catch (Exception ex) {
			var exCopy = ex;
			BreakIfTrue(true); //V.Nothing();
			if (logError)
				Debug.LogException(ex);
			if (rethrowError)
				throw;
			//return null;
			return default(T);
		}
	}
	public static bool Break()
	{
		Nothing();
		return true;
	}
	public static bool BreakIfTrue(bool value)
	{
		if (value)
			Nothing();
		return true;
	}
	//public static bool BreakIfTrue(bool value) { return (bool)BreakIfTrue(value, true); }
	public static object BreakIfTrue(bool value, object returnValue)
	{
		if (value)
			Nothing();
		return returnValue;
	}

	//public static object CreateEmptyInstance(Type type) { return FormatterServices.GetUninitializedObject(type); }
	public static T CreateInstance<T>() { return (T)FormatterServices.GetUninitializedObject(typeof(T)); }
	/*public static object CreateInstance(Type type, bool callDefaultConstructor = false) {
		if (callDefaultConstructor)
			return Activator.CreateInstance(type, true);
		return FormatterServices.GetUninitializedObject(type);
	}*/

	public static T ParseEnum<T>(string enumName, bool firstLetterCaseMatters = true) {
		if (!firstLetterCaseMatters)
			enumName = enumName.Substring(0, 1).ToUpper() + enumName.Substring(1);
		return (T)System.Enum.Parse(typeof(T), enumName);
	}

	public static void BroadcastGlobalMessage(string method)
	{
		var objects = (GameObject[])Object.FindObjectsOfType(typeof(GameObject));
		foreach (GameObject obj in objects)
			obj.SendMessage(method, SendMessageOptions.DontRequireReceiver);
	}
	public static void BroadcastGlobalMessage(string method, object argument)
	{
		var objects = (GameObject[])Object.FindObjectsOfType(typeof(GameObject));
		foreach (GameObject obj in objects)
			obj.SendMessage(method, argument, SendMessageOptions.DontRequireReceiver);
	}
	public static List<T> FindComponentsOfType<T>(bool includeInactive = true) where T : Component { return VO.main.rootObjects.SelectMany(a=>a.GetComponentsInChildren<T>(includeInactive)).ToList(); }

	/// <summary>Return checks are evaluated in the order their enablement-args are listed.</summary>
	public static bool Equals(object a, object b, bool trueIfSameRef = true, bool falseIfOneNull = true, bool trueIfSameItems = false) //bool valOfObjEqualsMethod = true)
	{
		if (trueIfSameRef && a == b)
			return true;
		if (falseIfOneNull && (a == null || b == null))
			return false;
		if (trueIfSameItems)
			if (a is IList && b is IList && (a as IList).ItemsEqual(b as IList))
				return true;
			else if (a is IDictionary && b is IDictionary && (a as IDictionary).ItemsEqual(b as IDictionary))
				return true;
		//if (valOfObjEqualsMethod)
		return a.Equals(b);
	}

	public static Color ConvertChannelIndexToColor(int channelIndex, float alphaOverride = -1)
	{
		Color result = new List<Color> {new Color(1, 0, 0, 0), new Color(0, 1, 0, 0), new Color(0, 0, 1, 0), new Color(0, 0, 0, 1)}[channelIndex];
		if (alphaOverride != -1)
			result.a = alphaOverride;
		return result;
	}

	public static int Min(params int[] numbers) { return numbers.Min(a=>a); }
	public static float Average(params int[] numbers) { return (float)numbers.Average(a => a); }
	public static int Max(params int[] numbers) { return numbers.Max(a=>a); }

	public static float Min(params float[] numbers) { return numbers.Min(a=>a); }
	public static float Average(params float[] numbers) { return numbers.Average(a=>a); }
	public static float Max(params float[] numbers) { return numbers.Max(a=>a); }

	public static double Min(params double[] numbers) { return numbers.Min(a=>a); }
	public static double Average(params double[] numbers) { return numbers.Average(a=>a); }
	public static double Max(params double[] numbers) { return numbers.Max(a=>a); }

	// just use the word 'percent', even though value is represented as fraction (e.g. 0.5, rather than 50[%])
	public static double Lerp(double from, double to, double percentFromXToY) { return from + ((to - from) * percentFromXToY); }
	public static double GetPercentFromXToY(double xMin, double yMax, double val, bool clampResultTo0Through1 = true) {
		if (clampResultTo0Through1)
			val = Math.Min(yMax, Math.Max(xMin, val));
		return (val - xMin) / (yMax - xMin); // distance from x / distance from x required for result '1'
	}

	public static Vector3 Average_V3(List<Vector3> vectors) { return Average_V3(vectors.ToArray()); }
	public static Vector3 Average_V3(params Vector3[] vectors) {
		Vector3 total = Vector3.zero;
		foreach (Vector3 vector in vectors)
			total += vector;
		return total / vectors.Length;
	}
	public static Vector4 Average_V4(List<Vector4> vectors) { return Average_V4(vectors.ToArray()); }
	public static Vector4 Average_V4(params Vector4[] vectors) {
		Vector4 total = Vector4.zero;
		foreach (Vector4 vector in vectors)
			total += vector;
		return total / vectors.Length;
	}

	static float Square(float v) { return v * v; }
	//bool doesCubeIntersectSphere(vec3 C1, vec3 C2, vec3 S, float R)
	public static bool DoesCubeIntersectSphere(Vector3 cubeOrigin, Vector3 cubeSize, Vector3 sphereOrigin, float sphereRadius)
	{
		Vector3 cubeLeftBottomBack = cubeOrigin;
		Vector3 cubeRightTopFront = cubeOrigin + cubeSize;
		float dist_squared = Square(sphereRadius);

		// assume C1 and C2 are element-wise sorted, if not, do that now
		if (sphereOrigin.x < cubeLeftBottomBack.x)
			dist_squared -= Square(sphereOrigin.x - cubeLeftBottomBack.x);
		else if (sphereOrigin.x > cubeRightTopFront.x)
			dist_squared -= Square(sphereOrigin.x - cubeRightTopFront.x);
		if (sphereOrigin.y < cubeLeftBottomBack.y)
			dist_squared -= Square(sphereOrigin.y - cubeLeftBottomBack.y);
		else if (sphereOrigin.y > cubeRightTopFront.y)
			dist_squared -= Square(sphereOrigin.y - cubeRightTopFront.y);
		if (sphereOrigin.z < cubeLeftBottomBack.z)
			dist_squared -= Square(sphereOrigin.z - cubeLeftBottomBack.z);
		else if (sphereOrigin.z > cubeRightTopFront.z)
			dist_squared -= Square(sphereOrigin.z - cubeRightTopFront.z);
		return dist_squared > 0;
	}
	public static bool DoesRectangleIntersectCircle(Rect rect, VVector2 circleCenter, double circleRadius)
	{
		rect = new Rect(rect.x + (rect.width / 2), rect.y + (rect.height / 2), rect.width, rect.height); // algorithm wants the x and y pos values to represent the center
		var circleDistance = new VVector2(Math.Abs(circleCenter.x - rect.x), Math.Abs(circleCenter.y - rect.y));
		if ((circleDistance.x > ((rect.width / 2) + circleRadius)) || (circleDistance.y > ((rect.height / 2) + circleRadius)))
			return false;

		if ((circleDistance.x <= (rect.width / 2)) || (circleDistance.y <= (rect.height / 2)))
			return true;

		var cornerDistance_sq = Math.Pow(circleDistance.x - (rect.width / 2), 2) + Math.Pow(circleDistance.y - (rect.height / 2), 2);
		return cornerDistance_sq <= (circleRadius * circleRadius);
	}

	public static int GetSharedAxesCount(Vector3 obj, Vector3 other)
	{
		int result = 0;
		result += obj.x == other.x ? 1 : 0;
		result += obj.y == other.y ? 1 : 0;
		result += obj.z == other.z ? 1 : 0;
		return result;
	}

	public static void RunAfter(int milliseconds, Action action)
	{
		var thread = new Thread(()=>
		{
			Thread.Sleep(milliseconds);
			action();
		});
		thread.Start();
	}
	public static void WaitXSecondsThenCall(float seconds, Action call) { VO.main.WaitXSecondsThenCall(seconds, call); }
	public static void WaitXFramesThenCall(int frames, Action call) { VO.main.WaitXFramesThenCall(frames, call); }
	public static void WaitForEndOfFrameThenCall(Action call) { VCoroutine.Start(VCoroutine.WaitForEndOfFrame(), VCoroutine.Do(call)); }
	//public static void RunXOnMainThread(Action action) { VO.main._actionsToCallOnMainThread.Add(action); }
	//public static void RunXInOnGUI(Action action) { Frame.live.actionsToCallInOnGUI.Add(action); }
	public static void RunXInBackground(Action backgroundAction, Action actionOnceDone = null) { StartCoroutine(RunXInBackground_Internal(backgroundAction, actionOnceDone)); }
	static IEnumerator RunXInBackground_Internal(Action backgroundAction, Action actionOnceDone = null)
	{
		bool done = false;
		var thread = new Thread(() =>
		{
			try
			{
				backgroundAction();
				done = true;
			}
			catch (Exception ex) // make sure we see errors that occur!
			{
				Debug.LogException(ex);
				throw;
			}
		});
		thread.Start();
		while (!done)
			yield return null;
		if (actionOnceDone != null)
			actionOnceDone();
	}

	//public static Coroutine StartCoroutine(IEnumerator coroutine) { return (VO.main._script ?? Object.FindObjectOfType<MonoBehaviour>()).StartCoroutine(coroutine); }
	public static Coroutine StartCoroutine(IEnumerator coroutine) { return VCoroutine.Start(coroutine); }
	/*public static void StartCoroutine_AnyThread(IEnumerator coroutine) {
		if (OnMainThread())
			VCoroutine.Start(coroutine);
		else
			RunXOnMainThread(()=>VCoroutine.Start(coroutine));
	}*/
	//public static void ClearOverlays() { Frame.live.ui.UnfreezeScreen(); CallJS("ClearOverlays"); }

	public static void FreezeScreen() { VO.main.gameObject.GetChild("#General/ScreenFreezer").GetComponent<ScreenFreezer>().FreezeScreen(); }
	public static void UnfreezeScreen() { VO.main.gameObject.GetChild("#General/ScreenFreezer").GetComponent<ScreenFreezer>().UnfreezeScreen(); }
	public static void SetFreezeScreenOpacity(float opacity) { VO.main.gameObject.GetChild("#General/ScreenFreezer").GetComponent<ScreenFreezer>().screenFreezeTextureOpacity = opacity; }
	public static void ShowLoadingScreen(string message, bool freezeScreen = false, bool captureMouseFocus = true) {
		if (freezeScreen)
			FreezeScreen(); // (old:) JS calls this already, but call it here too, since we might be able to get it in one frame earlier
		VO.main.SendMessage(ContextGroup.Local_UI, "ShowLoadingScreen", message, captureMouseFocus);
	}
	public static void HideLoadingScreen() {
		VO.main.SendMessage(ContextGroup.Local_UI, "HideLoadingScreen");
		UnfreezeScreen();
	}

	public static Texture2D GetScreenshot() {
		var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false); // create a texture to pass to encoding
		texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0); // put buffer into texture
		texture.Apply();
		return texture;
	}

	public static void GetScreenshot_Async(Action<Texture2D> callback) { StartCoroutine(GetScreenshot_Async_Internal(callback)); }
	static IEnumerator GetScreenshot_Async_Internal(Action<Texture2D> callback)
	{
		yield return new WaitForEndOfFrame(); // wait for graphics to fully render
		var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false); // create a texture to pass to encoding
		texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0); // put buffer into texture
		texture.Apply();
		callback(texture);
	}

	/*static T Cast<T>(object o) { return (T)o; }
	public static object CastObjectToType(object obj, Type type)
	{
		MethodInfo castMethod = typeof(V).GetMethod("Cast").MakeGenericMethod(type);
		return castMethod.Invoke(null, new[] {obj});
	}*/
	public class ExtraCall<T>
	{
		public ExtraCall(Action call) { call(); }
		public T val;
	}

	public static Vector3 RotateVector3AroundPoint(Vector3 point, Vector3 pivot, Quaternion angle) { return angle * (point - pivot) + pivot; }

	//public static Color ColorLerp(Color a, Color b, float percentFromAToB) { return new Color(Mathf.Lerp(a.r, b.r, percentFromAToB), Mathf.Lerp(a.g, b.g, percentFromAToB), Mathf.Lerp(a.b, b.b, percentFromAToB), Mathf.Lerp(a.a, b.a, percentFromAToB)); }
	//public static Color ColorLerp(Color a, Color b, float percentFromAToB) { return new Color(a.r + ((b.r - a.r) * percentFromAToB), a.g + ((b.g - a.g) * percentFromAToB), a.b + ((b.b - a.b) * percentFromAToB), a.a + ((b.a - a.a) * percentFromAToB)); }

	// todo; make sure this works, and understand how it works
	/*public static float GetSignedAngleBetween(Vector3 a, Vector3 b, Vector3 n)
	{
		// angle in [0,180]
		float angle = Vector3.Angle(a, b);
		float sign = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(a, b)));

		// angle in [-179,180]
		float signed_angle = angle * sign;

		// angle in [0,360] (not used but included here for completeness)
		//float angle360 =  (signed_angle + 180) % 360;

		return signed_angle;
	}*/

	//public static GameObject FindGameObject(string name) { return Frame.live.l_rootObjects.SelectMany(a=>a.GetComponentsInChildren<Transform>()).First(a=>a.name == name).gameObject; }
	public static GameObject FindGameObject(string name) {
		var nextSet = VO.main.rootObjects.ToList();
		while (nextSet.Count > 0) {
			List<GameObject> currentSet = nextSet;
			nextSet = new List<GameObject>();
			foreach (GameObject obj in currentSet) {
				if (obj.name == name)
					return obj;
				for (int i = 0; i < obj.transform.childCount; i++)
					nextSet.Add(obj.transform.GetChild(i).gameObject);
			}
		}
		return null;
	}

	// maybe temp
	//public static T Clone<T>(T obj, bool changeName = false, bool allowSpecialTextureClone = true) where T : Object
	//public static T Clone<T>(T obj, bool changeName = false, bool makeActive = false) where T : Object
	public static T Clone<T>(T obj, bool changeName = false) where T : Object { return Clone<T>(obj, Vector3.zero, Quaternion.identity, changeName); }
	public static T Clone<T>(T obj, Vector3 position, Quaternion rotation, bool changeName = false) where T : Object {
		T result = (T)Object.Instantiate(obj, position, rotation);
		if (!changeName)
			result.name = obj.name;
		//if (!makeActive && obj is GameObject)
		//	(result as GameObject).SetActive((obj as GameObject).activeSelf);
		return result;
	}
	public static void Destroy(Object obj, bool markAsSemiDestroyed = false) {
		Object.Destroy(obj);
		/*if (markAsSemiDestroyed)
			obj.SetMeta("semiDestroyed", true);*/
	}
	public static void DestroyImmediate(Object obj, bool allowDestroyingAssets = false) { Object.DestroyImmediate(obj, allowDestroyingAssets); }

	// gets the angle between dirA and dirB around axis
	public static double Angle_AroundAxis(VVector3 dirA, VVector3 dirB, VVector3 axis) {
		// project A and B onto the plane orthogonal target axis
		dirA = dirA - VVector3.Project(dirA, axis);
		dirB = dirB - VVector3.Project(dirB, axis);

		double angle = VVector3.Angle(dirA, dirB); // find (positive) angle between A and B
		return angle * (VVector3.Dot(axis, VVector3.Cross(dirA, dirB)) < 0 ? -1 : 1); // return angle multiplied by 1 or -1
	}

	static bool mouseLocked;
	public static bool GetIsMouseLocked() { return mouseLocked; }
	public static void LockMouse(Vector2? point = null) {
		point = point ?? GetCursorPosition();

		mouseLocked = true;

		var rect = new RECT();
		rect.left = (int)point.Value.x;
		rect.right = (int)point.Value.x + 1;
		rect.top = (int)point.Value.y;
		rect.bottom = (int)point.Value.y + 1;
		ClipCursor(ref rect);
	}
	public static void UnlockMouse() {
		mouseLocked = false;

		var rect = new RECT();
		rect.left = -5000000;
		rect.right = 5000000;
		rect.top = -5000000;
		rect.bottom = 5000000;
		ClipCursor(ref rect);
	}

	/*public static bool GetIsMouseLocked() { return Cursor.lockState == CursorLockMode.Locked; }
	public static void LockMouse() { Cursor.lockState = CursorLockMode.Locked; }
	public static void UnlockMouse() { Cursor.lockState = CursorLockMode.None; }*/

	[StructLayout(LayoutKind.Sequential)] public struct POINT {
		public int X;
		public int Y;
		public static implicit operator Vector2(POINT point) { return new Vector2(point.X, point.Y); }
	}
	[DllImport("user32.dll")] public static extern bool GetCursorPos(out POINT lpPoint);
	public static Vector2 GetCursorPosition() {
		POINT lpPoint;
		GetCursorPos(out lpPoint);
		return lpPoint;
	}
	[DllImport("user32", EntryPoint = "ClipCursor")] public static extern int ClipCursor(ref RECT lpRect);
	public struct RECT {
		public int left;
		public int top;
		public int right;
		public int bottom;
	}

	/*public static Dictionary<int, object> asValues = new Dictionary<int, object>();
	public static T As<T>(T obj, int index) { // for debugging
		asValues[index] = (object)obj ?? "[null]";
		return obj;
	}
	static bool readyForAnotherLiveDebug = true;
	public static void LiveDebug(params object[] asValueArgs) { // for debugging
		if (!readyForAnotherLiveDebug)
			return;
		readyForAnotherLiveDebug = false;
		WaitXSecondsThenCall(1, ()=>readyForAnotherLiveDebug = true);
		JSBridge.CallJS("BD.console.GetCSCode", (Action<string>)(code=> {
			for (var i = 0; i < asValueArgs.Length; i++)
				As(asValueArgs[i], i);
			RunCS(code);
		}));
	}*/

	//const float RareFloat = -9876.54321f;
	public static Vector2 Vector2Null { get { return new Vector2(float.NaN, float.NaN); } } //new Vector2(RareFloat, RareFloat);
	public static Vector3 Vector3Null { get { return new Vector3(float.NaN, float.NaN, float.NaN); } } //new Vector3(RareFloat, RareFloat, RareFloat);

	public const int Int_FakeNaN = -987604321;

	public static bool inUnitTests;

	const int FO_DELETE = 0x0003;
	const int FOF_ALLOWUNDO = 0x0040; // preserve undo information, if possible
	const int FOF_NOCONFIRMATION = 0x0010; // show no confirmation dialog box to the user

	// contains information that the SHFileOperation function uses to perform file operations
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)] public struct SHFILEOPSTRUCT {
		public IntPtr hwnd;
		[MarshalAs(UnmanagedType.U4)] public int wFunc;
		public string pFrom;
		public string pTo;
		public short fFlags;
		[MarshalAs(UnmanagedType.Bool)] public bool fAnyOperationsAborted;
		public IntPtr hNameMappings;
		public string lpszProgressTitle;
	}
	[DllImport("shell32.dll", CharSet = CharSet.Auto)] static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);
	public static void DeleteFileOrFolderToRecycleBin(string path) {
		var deleteCommand = new SHFILEOPSTRUCT();
		deleteCommand.wFunc = FO_DELETE;
		deleteCommand.pFrom = path + '\0' + '\0';
		deleteCommand.fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION;
		SHFileOperation(ref deleteCommand);
	}

	public static bool sceneViewOpen {
#if UNITY_EDITOR
		get { return UnityEditor.SceneView.focusedWindow && UnityEditor.SceneView.focusedWindow.title == "UnityEditor.SceneView"; }
#else
		get { return false; }
#endif
	}

	/*static int lastJSVisibleActionID = -1;
	static Dictionary<int, Delegate> jsVisibleActions = new Dictionary<int, Delegate>();
	public static int AddJSVisibleAction(Action action) {
		int actionID = lastJSVisibleActionID + 1;
		jsVisibleActions[actionID] = action;
		lastJSVisibleActionID = actionID;
		return actionID;
	}
	public static void CallJSVisibleAction(int callbackID, params object[] args) {
		if (jsVisibleActions[callbackID].Method.GetParameters().Length > 0)
			jsVisibleActions[callbackID].DynamicInvoke(args);
		else
			jsVisibleActions[callbackID].DynamicInvoke();
	}*/

	public static Texture2D CloneTexture(Texture2D texture, bool useMethodThatWorksOnNonReadableTexture = true) {
		if (useMethodThatWorksOnNonReadableTexture)
			return VConvert.TextureToEditableTexture(texture);

		var result = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount > 0);
		/*for (var x = 0; x < texture.width; x++)
			for (var y = 0; y < texture.height; y++)
				result.SetPixel(x, y, texture.GetPixel(x, y));*/
		result.SetPixels32(texture.GetPixels32()); //result.SetPixels(texture.GetPixels());
		result.Apply();
		return result;
	}

	public static void ShowMessageBox(string title, string message) { JSBridge.CallJS("V.ShowMessageBox_Simple", title, message); }

	public static double GetGraphYValueAtX(Dictionary<double, double> graphPoints_xy, double x) { return GetGraphYValueAtX(graphPoints_xy, x, graphPoints_xy.Keys.OrderBy(a => a).ToList()); }
	public static double GetGraphYValueAtX(Dictionary<double, double> graphPoints_xy, double x, List<double> graphPoints_xy_keys_sorted) {
		double result = 0;
		for (var i = 0; i < graphPoints_xy_keys_sorted.Count; i++)
			if (i == graphPoints_xy_keys_sorted.Count - 1 || graphPoints_xy_keys_sorted[i + 1] > x) { // if we're at last point, or next point's x is above given x
				var costA = graphPoints_xy[graphPoints_xy_keys_sorted[i]];
				var costB = i + 1 < graphPoints_xy_keys_sorted.Count ? graphPoints_xy[graphPoints_xy_keys_sorted[i + 1]] : costA;
				var percentFromAToB = Mathf.Clamp((float)(costA == costB ? 0 : (x - costA) / (costB - costA)), 0, 1);
				result = costA + ((costB - costA) * percentFromAToB);
				break;
			}
		return result;
	}

	// note: this may well be broken; oh well, leave it for testing phase
	public static Expression<Func<TObj, TResult_New>> ChangeReturnTypeTo<TObj, TResult_Old, TResult_New>(
		this Expression<Func<TObj, TResult_Old>> expression, TResult_New newReturnType_stub
	) {
		var memberExpression = expression.Body is UnaryExpression ? (expression.Body as UnaryExpression).Operand as MemberExpression : expression.Body as MemberExpression;
		var memberName = memberExpression.Member.Name;

		var param = Expression.Parameter(typeof(TObj), memberName);
		var field = Expression.Property(param, memberName);
		return Expression.Lambda<Func<TObj, TResult_New>>(field, param);
	}

	// note that Color32 and Color implictly convert to each other; you may pass a Color object to this method without first casting it
	public static string ColorToHexString(Color32 color) { return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2"); }
	public static Color HexStringToColor(string hex) {
		hex = hex.Replace("0x", ""); // in case the string is formatted 0xFFFFFF
		hex = hex.Replace("#", ""); // in case the string is formatted #FFFFFF
		
		byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
		byte a = 255; //assume fully visible unless specified in hex
		if (hex.Length == 8) // only use alpha if the string has enough characters
			a = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
		return new Color32(r, g, b, a);
	}

	public static double GetDistanceRequiredToSeeObject(double cameraFieldOfView, double objDiameter) { return objDiameter / (Mathf.Sin(((float)cameraFieldOfView * Mathf.Deg2Rad) / 2f)); }

	public static int GetLayerMask(params string[] layers) { return VPhysics.GetLayerMask(layers.ToList(), null, null); }

	public static double GetRotationForPositionOffset(Vector2i posOffset) {
		double result; // z-rotation needed to change from forward (+y) to direction
		if (posOffset.x == -1 && posOffset.y == -1)
			result = 135;
		else if (posOffset.x == -1 && posOffset.y == 0)
			result = 90;
		else if (posOffset.x == -1 && posOffset.y == 1)
			result = 45;
		else if (posOffset.x == 0 && posOffset.y == 1)
			result = 0;
		else if (posOffset.x == 1 && posOffset.y == 1)
			result = -45;
		else if (posOffset.x == 1 && posOffset.y == 0)
			result = -90;
		else if (posOffset.x == 1 && posOffset.y == -1)
			result = -135;
		else if (posOffset.x == 0 && posOffset.y == -1)
			result = -180;
		else
			//throw new Exception("Position offset is not simple. (-1 or +1, on each axis)");
			return 0;
		return result;
	}

	public static string GetStackTraceStr() {
		var result = Environment.StackTrace;
		return result.Substring(result.IndexOf_X(1, "\n")); // remove first stack-frame (that of the Enrivonment.StackTrace call) and second stack-frame (that of this method)
	}

	public static void GL_AddVertex(VVector3 pos) { GL.Vertex3((float)pos.x, (float)pos.z, (float)pos.y); }
	public static void GL_AddLine(VVector3 a, VVector3 b) {
		GL.Begin(GL.LINES);
		GL_AddVertex(a);
		GL_AddVertex(b);
	}
	public static void GL_AddLine(VVector3 a, VVector3 b, double lineThickness) {
		var center = (a + b) / 2;

		var thicknessPerSide = lineThickness / 2;
		var lineFaceDirection_a = Camera.main.transform.position.ToVVector3() - a;
		var lineFaceDirection_b = Camera.main.transform.position.ToVVector3() - center;

		var a1 = a.RotateAround(center, -thicknessPerSide / a.Distance(center), lineFaceDirection_a);
		var a2 = a.RotateAround(center, thicknessPerSide / a.Distance(center), lineFaceDirection_a);
		var b1 = b.RotateAround(center, -thicknessPerSide / b.Distance(center), lineFaceDirection_b);
		var b2 = b.RotateAround(center, thicknessPerSide / b.Distance(center), lineFaceDirection_b);
		GL.Begin(GL.QUADS);
		GL_AddVertex(a1);
		GL_AddVertex(a2);
		GL_AddVertex(b1);
		GL_AddVertex(b2);
	}

	// maybe temp; fix for common issue of 'floats' and 'doubles' being sent/received as ints, because JS language doesn't distinguish and therefore JS serializer can't mark
	public static object FixAmbiguousFromJSValue(object value, Type expectedType) {
		if (value is int)
			if (expectedType == typeof(float))
				return (float)(int)value;
			else if (expectedType == typeof(double))
				return (double)(int)value;
		return value;
	}

	public static object CallMethod_AndExtraMethods(object obj, string methodName, params object[] args) {
		object result = null;
		var typeInfo = VTypeInfo.Get(obj.GetType());
		var method = typeInfo.GetMethod(methodName);
		if (method != null)
			result = method.Call(obj, args); // for now, only main method can send back result
		var extraMethodNumber = 1;
		while ((method = typeInfo.GetMethod(methodName + "_EM" + extraMethodNumber)) != null) {
			if (method != null)
				method.Call(obj, args);

			extraMethodNumber++;
		}
		return result;
	}
}

/*public static class Hash {
    public const int Base = 17;
    public static int HashObject(this int hash, object obj) { unchecked { return hash * 23 + (obj == null ? 0 : obj.GetHashCode()); } }
    public static int HashValue<T>(this int hash, T value) where T : struct { unchecked { return hash * 23 + value.GetHashCode(); } }
}*/

public static class Props {
	public static PropInclusions Include(params MemberInfo[] props) { return new PropInclusions().Include(props); }
	public static PropInclusions Exclude(params MemberInfo[] props) { return new PropInclusions().Exclude(props); }
}
public class PropInclusions : Dictionary<MemberInfo, bool> {
	/*public static PropInclusions True(params MemberInfo[] props) { return new PropInclusions().True(props); }
	public static PropInclusions False(params MemberInfo[] props) { return new PropInclusions().False(props); }*/
	//public static PropInclusions New() { return new PropInclusions(); }

	public PropInclusions Include(params MemberInfo[] props) {
		foreach (var prop in props)
			Add(prop, true);
		return this;
	}
	public PropInclusions Exclude(params MemberInfo[] props) {
		foreach (var prop in props)
			Add(prop, false);
		return this;
	}
}
