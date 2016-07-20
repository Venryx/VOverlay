using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using VDFN;
using VTree;
using VTree_Structures;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public class RethrowException : Exception {
	string message;
	Exception innerException;
	public RethrowException(string message, Exception innerException) {
		this.message = message;
		this.innerException = innerException;
	}

	public override string ToString() { return innerException + (innerException.ToString().EndsWith("\n==================") ? "" : "\n==================") + "\nRethrownAs) " + message + "\n" + base.StackTrace + "\n=================="; }
	public override string Message { get { return ToString(); } }
	public override string StackTrace { get { return ""; } }
}

public enum VisualizerShape
{
	Sphere,
	Cross3D_Thickness10,
	Cross3D_Thickness1
}
public class VDebug : MonoBehaviour
{
	public static VDebug live;
	VDebug() { live = this; }

	// general
	// ==========

	// maybe make-so: a stack-trace is recorded with each in-waiting log-message, and that stack-trace is written to the Log_Calls.txt file, rather than the (short) BiomeDefense.Update() one
	public static Queue frameLogMessages = new Queue();
	public static void Log(object message)
	{
		// old; rather than checking here, just use find-and-replace to special-comment-out the VDebug.XXX calls: (then undo once unit-testing is done)
		// replace: \tVDebug.
		// with:    \t///VDebug.
		/*if (V.inUnitTests) 
			return;*/
		/*if (!VO.main.console.loggerEnabled) // old; removed these checks from non-message-func versions, since the C#-side logging has no overhead
			return;*/
		//Debug.Log(message);
		frameLogMessages.Enqueue(message);
	}
	public static void Log(string logCategory, object message)
	{
		if (V.inUnitTests) // (unit-tests should use their own logging system, e.g. Console.WriteLine)
			return;

		if (logCategory == "general")
			//Debug.Log(message);
			frameLogMessages.Enqueue(message);
		else if (logCategory == "path finding")
			//Debug.Log("[C:path finding]" + message);
			frameLogMessages.Enqueue("[C:path finding]" + message);
	}
	public static void Log(string logCategory, Func<string> messageFunc) // todo: make sure message-func closures are cheaper than the to-string-funcs that would otherwise be called
	{
		if (V.inUnitTests) // (unit-tests should use their own logging system, e.g. Console.WriteLine)
			return;

		if (logCategory == "general")
			//Debug.Log(messageFunc());
			frameLogMessages.Enqueue(messageFunc());
		else if (logCategory == "path finding")
			//Debug.Log("[C:path finding]" + messageFunc());
			frameLogMessages.Enqueue("[C:path finding]" + messageFunc());
	}
	/*public static void LogEach(params object[] messages)
	{
		foreach (object message in messages)
			//Debug.Log(message);
			frameLogMessages.Enqueue(message);
	}*/
	public static void LogLine(params object[] objects) { LogLine_Internal(objects); }
	public static void LogLine_Full(bool writeFullVector3 = false, params object[] objects) { LogLine_Internal(objects, writeFullVector3); }
	//public static int logsLeft = 1000000;
	static void LogLine_Internal(object[] objects, bool writeFullVector3 = false)
	{
		/*if (logsLeft <= 0)
		{
			if (logsLeft == 0)
				//Debug.Log("Log limit reached.");
				frameLogMessages.Enqueue("Log limit reached.");
			return;
		}
		logsLeft--;*/

		var message = "";
		foreach (var obj in objects)
		{
			string str;
			if (obj is Vector3 && writeFullVector3)
				str = ((Vector3)obj).V();
			else
				str = obj != null ? obj.ToString() : "[null]";
			//message += (message.Length > 0 ? ";" : "") + str;
			message += str;
		}
		//Debug.Log(message);
		frameLogMessages.Enqueue(message);
	}
	// maybe make-so: these use frame-linked queues as well
	public static void LogError(object message) { Debug.LogError(FinalizeMessage(message.ToString())); }
	public static void LogException(Exception ex) { Debug.LogException(ex); }

	static string FinalizeMessage(string message)
	{
		var result = message;

		result = result.Replace("/Users/builduser/buildslave/mono-runtime-and-classlibs/build", "Mono");
		result = result.Replace("C:\\Projects\\Unity\\Biome Defense\\Unity\\", "");

		if (result.Contains("SCallArgsVDF) ") && result.Length > 12000)
		{
			var callArgsVDF_pos = result.IndexOf("SCallArgsVDF) ");
			var stack_pos = result.IndexOf("SStack) ") - 2;
			if (stack_pos - callArgsVDF_pos > 12000) // if call-args-vdf part is longer than 12000, trim to 12000 (max for message overall is 16857)
				result = result.Substring(0, callArgsVDF_pos) + result.Substring(callArgsVDF_pos, Math.Min(12000, stack_pos - callArgsVDF_pos)) + "<...>" + result.Substring(stack_pos);
		}

		return result;
	}

	// log-later
	// ==========

	static bool trimNewestVSOldest = true;
	static int messageLimit = 10000; //int.MaxValue
	static List<string> logMessages = new List<string>();
	public static void LogLater(object obj) { logMessages.Add(obj.ToString()); }
	void OnApplicationQuit()
	{
		var finalMessage = new StringBuilder();
		var startIndex = trimNewestVSOldest ? 0 : Math.Max(logMessages.Count - messageLimit, 0);
		for (int i = startIndex; i < logMessages.Count && i - startIndex < messageLimit; i++)
			finalMessage.Append("<#").Append(i).Append(">").AppendLine(logMessages[i]);
		if (finalMessage.Length > 0)
			Debug.Log(finalMessage);

		foreach (string key in logWriters.Keys)
			//logWriters[key].Flush();
			//logWriters[key].Dispose();
			logWriters[key].Close();
	}

	// log to file
	// ==========

	static Dictionary<string, StreamWriter> logWriters = new Dictionary<string, StreamWriter>();
	public static void LogToFile(string message, string filePath = "Log_Main.txt")
	{
		if (!logWriters.ContainsKey(filePath))
		{
			FileManager.GetFile(filePath).Delete();
			logWriters.Add(filePath, new StreamWriter(FileManager.GetFile(filePath).FullName));
		}
		if (logWriters[filePath].BaseStream != null && logWriters[filePath].BaseStream.CanWrite) // if not already closed (i.e. from program closing)
			logWriters[filePath].WriteLine(message);
	}

	// profiling - new
	// ==========

	/*public static string log { set { Log(value); } }

	static Dictionary<string, Stopwatch> layerStopwatches_time = new Dictionary<string, Stopwatch>();
	public static double S(string layer = "main") { return T(layer); } // start
	public static double M(string layer = "main") { return T(layer, false); } // mid
	public static double T(string layer = "main", bool resetStopwatch = true) // everything method
	{
		if (!layerStopwatches_time.ContainsKey(layer))
			layerStopwatches_time[layer] = new Stopwatch();

		// get time since last call
		var result = layerStopwatches_time[layer].ElapsedTicks / 10000d;
		// start the stopwatch for the next call
		if (resetStopwatch)
		{
			layerStopwatches_time[layer].Reset();
			layerStopwatches_time[layer].Start();
		}

		return result;
	}*/

	// log-based profiling
	// ==========

	static HashSet<string> layersWhoseLoggingIsDisabled = new HashSet<string>();
	public static void SetLayerLoggingEnabled(string layer, bool enabled)
	{
		if (enabled)
			layersWhoseLoggingIsDisabled.Remove(layer);
		else
			layersWhoseLoggingIsDisabled.Add(layer);
	}

	// this approach excludes the Debug.Log run-time
	static Dictionary<string, Stopwatch> layerStopwatches_fromStart = new Dictionary<string, Stopwatch>();
	static Dictionary<string, Stopwatch> layerStopwatches_fromLast = new Dictionary<string, Stopwatch>();
	public static double T(string layer, string logKey = null) // T = Time
	{
		if (V.inUnitTests)
			return -1;

		if (!layerStopwatches_fromStart.ContainsKey(layer))
		{
			layerStopwatches_fromStart[layer] = new Stopwatch();
			layerStopwatches_fromLast[layer] = new Stopwatch();
		}

		// get time since last call
		var result = layerStopwatches_fromLast[layer].ElapsedTicks / 10000d;

		// perform action specified by log-key argument
		if (logKey != null)
			if (logKey == "#") // if 'start' marker, restart from-start-call stopwatch
			{
				layerStopwatches_fromStart[layer].Reset();
				layerStopwatches_fromStart[layer].Start();
			}
			else // else, must just be log name, so log time since last call, as well as since start call
				if (!layersWhoseLoggingIsDisabled.Contains(layer))
					Log(layer + "_" + logKey + ") " + result + " [total: " + (layerStopwatches_fromStart[layer].ElapsedTicks / 10000d) + "]\n");

		layerStopwatches_fromLast[layer].Reset();
		layerStopwatches_fromLast[layer].Start();

		return result;
	}

	// this approach includes the Debug.Log run-time
	/*static Dictionary<string, Stopwatch> layerStopwatch = new Dictionary<string, Stopwatch>();
	static Dictionary<string, double> layerStopwatchTimeAtLastCall = new Dictionary<string, double>();
	public static double T(string layer, string logKey = null)
	{
		if (!layerStopwatch.ContainsKey(layer))
		{
			layerStopwatch[layer] = new Stopwatch();
			layerStopwatchTimeAtLastCall[layer] = 0;
		}

		// get time since last call
		var stopwatchTime = layerStopwatch[layer].ElapsedTicks / 10000d;
		var result = stopwatchTime - layerStopwatchTimeAtLastCall[layer];

		// perform action specified by log-key argument
		if (logKey != null)
			if (logKey == "#") // if 'start' marker, restart stopwatch
			{
				layerStopwatch[layer].Reset();
				layerStopwatch[layer].Start();
				layerStopwatchTimeAtLastCall[layer] = 0;
			}
			else // else, must just be log name, so log time since last call, as well as since start call
				if (!layersWhoseLoggingIsDisabled.Contains(layer))
					Debug.Log(layer + "_" + logKey + ") " + result + " [total: " + stopwatchTime + "]");

		layerStopwatchTimeAtLastCall[layer] = stopwatchTime;

		return result;
	}*/

	// visualizers
	// ==========

	class Visualizer
	{
		public GameObject obj;
		public bool persistWhenPaused;
		public Visualizer(GameObject obj, bool persistWhenPaused)
		{
			this.obj = obj;
			this.persistWhenPaused = persistWhenPaused;
		}
	}
	Dictionary<int, List<Visualizer>> visualizers = new Dictionary<int, List<Visualizer>>();
	public Material textMaterial;
	public Font textFont;
	public static void MarkShape(VisualizerShape shape, VVector3 position, float size, bool persistWhenPaused = false, bool autoDestroy = true)
	{
		var obj = (GameObject)Instantiate(Resources.Load<GameObject>(shape.ToString()));
		obj.transform.parent = GameObject.Find("Visualizers").transform;
		obj.transform.position = position.ToVector3();
		obj.transform.localScale = Vector3.one * size;
		if (autoDestroy)
		{
			if (!live.visualizers.ContainsKey(Time.frameCount))
				live.visualizers.Add(Time.frameCount, new List<Visualizer>());
			live.visualizers[Time.frameCount].Add(new Visualizer(obj, persistWhenPaused));
		}
	}
	public static GameObject MarkText(VVector3 position, string text, float size, int fontSize = 14, Color? textColor = null, bool autoLookAwayFromCamera = false, bool persistWhenPaused = false, bool autoDestroy = true)
	{
		textColor = textColor ?? Color.white;

		/*var obj = new GameObject("Text");
		obj.transform.parent = GameObject.Find("Visualizers").transform;
		obj.transform.position = position;
		obj.transform.localScale = Vector3.one * size;

		//var textComp = obj.AddComponent<GUIText>();
		//textComp.text = text;

		//var renderer = obj.AddComponent<MeshRenderer>();
		//renderer.material = live.textMaterial;
		var textComp = obj.AddComponent<TextMesh>();
		textComp.characterSize = .05f;
		textComp.fontSize = 50;
		textComp.font = live.textFont;
		textComp.text = text;

		obj.transform.rotation = Camera.main.transform.rotation;
		if (!live.visualizers.ContainsKey(Time.frameCount))
			live.visualizers.Add(Time.frameCount, new List<GameObject>());
		live.visualizers[Time.frameCount].Add(obj);*/

		/*V.RunXInOnGUI(()=>
		{
			var uiPosition = Camera.main.WorldToScreenPoint(position);
			GUI.Label(new Rect(uiPosition.x - (size / 2), uiPosition.y - (size / 2), size, size), text);
		});*/

		var obj = new GameObject("Text");
		obj.transform.parent = GameObject.Find("Visualizers").transform;
		obj.transform.position = position.ToVector3();
		obj.transform.localScale = Vector3.one * .01f * size;
		obj.transform.rotation = Camera.main.transform.rotation;

		var canvas = obj.AddComponent<Canvas>();
		var textComp = obj.AddComponent<Text>();
		textComp.material = live.textMaterial;
		textComp.font = live.textFont;
		textComp.fontSize = fontSize;
		textComp.color = textColor.Value;
		textComp.text = text;
		textComp.alignment = TextAnchor.MiddleCenter;

		/*if (autoLookAwayFromCamera) {
			var lookAt = obj.AddComponent<LookAt>();
			lookAt.target = Camera.main.transform;
			lookAt.reverse = true;
		}*/

		if (autoDestroy)
		{
			if (!live.visualizers.ContainsKey(Time.frameCount))
				live.visualizers.Add(Time.frameCount, new List<Visualizer>());
			live.visualizers[Time.frameCount].Add(new Visualizer(obj, persistWhenPaused));
		}

		return obj;
	}
	void Update() {
		for (int i = visualizers.Keys.Count - 1; i >= 0; i--) {
			var key = visualizers.Keys.ToList()[i];
			if (key < Time.frameCount) {
				bool anySkipped = false;
				foreach (var visualizer in visualizers[key])
					if (!visualizer.persistWhenPaused)
						Destroy(visualizer.obj);
					else
						anySkipped = true;
				if (!anySkipped)
					visualizers.Remove(key);
			}
		}
	}

	// live-label system
	// make-so: a dictionary is used, and ui rows are added dynamically in custom Drawer/Inspector
	// ==========

	public string liveLabel1;
	public string liveLabel2;
	public string liveLabel3;

	// exception rethrowing
	// ==========

	// maybe make-so: these process exception messages/stack-traces to remove not-useful stack-trace-entries
	public static void RethrowException(Exception ex, string rethrowExceptionMessage = null) {
		//var message = rethrowExceptionMessage ?? ex.Message;
		var message = rethrowExceptionMessage;

		Exception exception = null;
		try {
			// assume typed Exception has "new (String message, Exception innerException)" signature
			exception = (Exception)Activator.CreateInstance(ex.GetType(), message, ex);
		}
		catch {
			// constructor doesn't have the right constructor; eat the error and throw the original exception, as below
		}
		if (exception == null) // if creating rethrow-exception failed, fall back to just throwing exception
			exception = ex;

		throw exception;
	}
	//[DebuggerHidden]
	public static void RethrowInnerExceptionOf(TargetInvocationException ex, string rethrowExceptionMessage = null) {
		//var message = rethrowExceptionMessage ?? ex.InnerException.Message;
		var message = rethrowExceptionMessage;

		Exception exception = null;
		try {
			// assume typed Exception has "new (String message, Exception innerException)" signature
			exception = (Exception)Activator.CreateInstance(ex.InnerException.GetType(), message, ex.InnerException);
		}
		catch {
			// constructor doesn't have the right constructor; eat the error and throw the original inner-exception, as below
		}
		//if (exception == null) { //|| exception.InnerException == null || exception.Message != message) {
		if (exception == null) // if creating rethrow-exception failed, fall back to just throwing inner-exception
			exception = ex.InnerException;

		throw exception;
	}
}

// maybe make-so: a unified interface exists that adds entries to all profilers at once

// (data is never cleared from this class, since it's meant to track total run-time of a 'section' of code)
public static class Profiler_AllFrames {
	public static BlockRunInfo rootBlockRunInfo;
	static Profiler_AllFrames() { ResetRootBlockRunInfo(); }
	public static BlockRunInfo CurrentBlock { get { return rootBlockRunInfo.GetCurrentDescendant(); } }

	// to be called by JS side's Console page code
	public static object GetRootBlockRunInfo() { return rootBlockRunInfo; }
	public static void ResetRootBlockRunInfo() { rootBlockRunInfo = new BlockRunInfo(null, "Root", true, 0); }
}
/*public static class Profiler_LastViewFrame {
	public static bool enabled;
	public static BlockRunInfo rootBlockRunInfo; // reset each view-frame, by PostViewFrameTick() below
	static Profiler_LastViewFrame() { rootBlockRunInfo = new BlockRunInfo(null, "Root", true, 0); } // for first view-frame (rest are set by PostViewFrameTick() below)
	public static BlockRunInfo CurrentBlock {
		get {
			if (!enabled)
				return BlockRunInfo.fakeBlockRunInfo;
			return rootBlockRunInfo.GetCurrentDescendant();
		}
	}

	public static void PreViewFrameTick() {
		rootBlockRunInfo = new BlockRunInfo(null, "Root", true, 0);
		enabled = true;
	}
	public static void PostViewFrameTick() { enabled = false; }

	// to be called by JS side's Console page code
	public static object GetRootBlockRunInfo() { return rootBlockRunInfo; }
}*/
public static class Profiler_LastDataFrame {
	public static bool enabled;
	public static BlockRunInfo rootBlockRunInfo = BlockRunInfo.fakeBlockRunInfo; // reset each frame, by PostViewFrameTick() below
	//static Profiler_LastDataFrame() { rootBlockRunInfo = new BlockRunInfo(null, "Root", true, 0).Start(); } // for first frame (rest are set by PostViewFrameTick() below)
	public static BlockRunInfo CurrentBlock {
		get {
			if (!enabled)
				return BlockRunInfo.fakeBlockRunInfo;
			return rootBlockRunInfo.GetCurrentDescendant();
		}
	}

	public static void PreViewFrameTick_Outer() {
		if (enabled) {
			rootBlockRunInfo = new BlockRunInfo(null, "Root", true, 0).Start();
			rootBlockRunInfo._____("outside [before] data-frame code [in view-frame]");
		}
	}
	public static void PreDataFrameTick() {
		if (enabled) { // if section's profiling enabled, mark its start
			// if outside profiling was not enabled [i.e. root-block-run-info for this frame was not created], create now
			/*if (!VO.main.console.lastDataFrame_outsideDataFrame)
				rootBlockRunInfo = new BlockRunInfo(null, "Root", true, 0).Start();*/
			rootBlockRunInfo._____("inside data-frame code [in view-frame]");
		}
	}
	public static void PostDataFrameTick() {
		if (!enabled) // if after-data-frame section's profiling not enabled, end profiling for view-frame
			rootBlockRunInfo.End();
		else // else, mark after-data-frame section's start
			rootBlockRunInfo._____("outside [after] data-frame code [in view-frame]");
	}
	public static void PostViewFrameTick_Outer() {
		if (rootBlockRunInfo != null)
			rootBlockRunInfo.End();
		enabled = false;
	}

	// to be called by JS side's Console page code
	public static object GetRootBlockRunInfo() { return rootBlockRunInfo; }
}

public class BlockRunInfo {
	public static BlockRunInfo fakeBlockRunInfo = new BlockRunInfo(null, "fakeBlockRunInfo", true, -1);

	public BlockRunInfo(BlockRunInfo parent, string name, bool method, int depth) {
		if (depth > 100)
			throw new Exception("Cannot profile call-path with a depth greater than 100.");

		root = parent != null ? parent.root : this;
		this.parent = parent;
		this.name = name;
		this.method = method;
		this.depth = depth;
		timer = new Stopwatch();
	}

	BlockRunInfo root; // for keeping track of what profiler self is part of
	BlockRunInfo parent;
	[P, D] public bool method; // (as opposed to a section)
	public int depth;
	[P] public string name;
	public Stopwatch timer;
	[P] public double runTime;
	[P] public Dictionary<string, BlockRunInfo> children = new Dictionary<string, BlockRunInfo>();
	BlockRunInfo currentChild;

	public BlockRunInfo GetCurrentDescendant() {
		if (currentChild != null)
			return currentChild.GetCurrentDescendant();
		return this;
	}

	//bool started; // use bool rather than timer.ElapsedTicks, since timer.ElapsedTicks can be 0, when BlockRunInfo is very-quickly ended
	[P] int runCount;
	public BlockRunInfo Start() {
		//Profiler_AllRuns.currentCallPath.Add(this);
		//Profiler_AllRuns.currentCallPath.Count.ShouldBe(depth + 1);
		if (parent != null)
			parent.currentChild = this;
		timer.Start();
		//started = true;
		runCount++;
		return this;
	}

	// for child methods
	public BlockRunInfo StartMethod(MethodBase method) { return StartMethod(method.DeclaringType.Name + "-" + method.Name); }
	public BlockRunInfo StartMethod(string name) {
		// make-so: this handles calls from other threads (e.g. by appending thread-id to name)
		if (!V.OnMainThread() || this == fakeBlockRunInfo) // if on non-main-thread (or if fake-block-run-info), return a fake block-run-info, since otherwise stack-trace could get messed up
			return fakeBlockRunInfo;

		//while (root == Profiler_LastViewFrame.rootBlockRunInfo || root == Profiler_LastDataFrame.rootBlockRunInfo && children.ContainsKey(name))
		while (root == Profiler_LastDataFrame.rootBlockRunInfo && children.ContainsKey(name))
			name += "_";

		if (name == "Match-dataFrame_PostSet")
			V.Nothing();

		if (!children.ContainsKey(name))
			children.Add(name, new BlockRunInfo(this, name, true, depth + 1));
		children[name].Start();
		return children[name];
	}

	// for child sections
	//public string _____ { set { _____(value); } }
	[Conditional("DEBUG")] // makes-so calls to this method are only compiled-in/run when the define symbol "DEBUG" is present
	public void _____(string name) { StartSection(name); }
	public BlockRunInfo _____2(string name) { return StartSection(name); }
	public BlockRunInfo StartSection(string name) {
		if (this == fakeBlockRunInfo)
			return fakeBlockRunInfo;

		if (name == null) {
			End();
			return fakeBlockRunInfo;
		}

		EndLastSection();
		if (!children.ContainsKey(name))
			children.Add(name, new BlockRunInfo(this, name, false, depth + 1));
		children[name].Start();
		return children[name];
	}
	public void EndLastSection() {
		/*var lastRunningSection = children.LastOrDefault(a=>!a.Value.method && a.Value.timer.IsRunning);
		if (lastRunningSection.Value != null)
			lastRunningSection.Value.End();*/
		if (currentChild != null)
			currentChild.End();
	}

	public void End() {
		if (this == fakeBlockRunInfo)
			return;
		V.Confirm(runCount > 0); // confirm that this block we're ending was at some point started
		if (!timer.IsRunning) // if already ended, return
			return;

		EndLastSection();

		//timer.Stop(); // Stop() is called automatically by Reset() below
		//runTime = timer.ElapsedTicks;
		runTime += timer.ElapsedTicks / 10000d; // show in ms
		timer.Reset();
		
		//Profiler_AllRuns.currentCallPath.RemoveAt(Profiler_AllRuns.currentCallPath.Count - 1);
		//Profiler_AllRuns.currentCallPath.Count.ShouldBe(depth);
		if (parent != null)
			parent.currentChild = null;
	}
}
public class BlockRunInfo_Disabled {
	//public static BlockRunInfo_Disabled fakeBlockRunInfo = new BlockRunInfo_Disabled();
	public static BlockRunInfo_Disabled main = new BlockRunInfo_Disabled();

	[Conditional("NEVER")] // makes-so calls to this method are never compiled
	public void _____(string name) {}
	public BlockRunInfo_Disabled _____2(string name) { return main; }
}