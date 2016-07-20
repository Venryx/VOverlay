using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using VTree;
using System.Linq;
using VDFN;
using VTree_Structures;

// bridge to the chatbot
public static class JSBridge
{
	// JS<>Unity bridge: called from CS (custom)
	// ==========

	static int lastRunJSFrame;
	static int lastRunJSFrame_runJSCalls;

	static int lastCallJSCallbackID = -1;
	static Dictionary<int, Delegate> callJSCallbacks = new Dictionary<int, Delegate>();
	static VDFSaveOptionsV CallJS_options = new VDFSaveOptionsV(toJS: true);
	/// <summary>If the last argument is a delegate (e.g. Action{string}), it will be taken as the 'callback', and will be called after the JS code's completion.</summary>
	public static void CallJS(string methodName, params object[] args) {
		var S = M.GetCurrentMethod().Profile_AllFrames();

		S._____("part 1");
		List<object> callArgs = args != null ? args.ToList() : new List<object> {null};

		Delegate callback = null;
		if (callArgs.Count >= 1 && callArgs[callArgs.Count - 1] is Delegate)
			callback = (Delegate)callArgs[callArgs.Count - 1];
		int callbackID = -1;
		if (callback != null) {
			callArgs.RemoveAt(callArgs.Count - 1);
			callbackID = lastCallJSCallbackID + 1;
			callJSCallbacks[callbackID] = callback; // add callback to map
			lastCallJSCallbackID = callbackID;
		}

		S._____("to vdf node");
		
		var callArgsVDFNode = VConvert.ToVDFNode(callArgs, true, VDFTypeMarking.External, CallJS_options);
		S._____("to vdf");
		var callArgsVDF = callArgsVDFNode.ToVDF(CallJS_options);
		S._____("part 2");
		//var callArgsVDF = VConvert.ToVDF(callArgs, true, VDFTypeMarking.External, new VDFSaveOptions(new List<object> {"to ui"}) {profile = true});
		//var csStack = V.GetStackTraceStr();
		var csStack = "[log-stack-trace disabled]";
		var js = "CSBridge.CallJS(\"" + methodName + "\", \"" + EscapeForJSRunString(callArgsVDF) + "\", " + callbackID + ", \"" + EscapeForJSRunString(csStack) + "\");";

		var ignoreCall = false;
		/*if (methodName == "OnLogMessageAdded")
			ignoreCall = true;
		else*/ if ((methodName == "BD.Node_SendMessage" || methodName == "BD.Node_BroadcastMessage") && callArgs[0] is NodePath && (callArgs[0] as NodePath).nodes.Count == 1) {
			var finalMethodName = callArgs[2] as string;
			if (finalMethodName == "Update" || finalMethodName == "SetFPSText")
				ignoreCall = true;
		}
		/*if (VO.main.settings.logCallsToFile && !ignoreCall)
			VDebug.LogToFile("==========\nCallJS>"
				+ (callbackID != -1 ? "MethodName+CallbackID) " + methodName + " (" + callbackID + ")" : "MethodName) " + methodName)
				+ "\n----------\nCallArgsVDF) " + callArgsVDF + "\n----------\nCSStack) " + csStack, "Log_Calls.txt");*/

		S._____("part 3");
		//RunJS(js);
		// make-so: this sends call to v-bot
		S._____(null);
	}
	static string EscapeForJSRunString(string str) { return str.Replace("\\", "\\\\").Replace("\r", "").Replace("\n", "\\n").Replace("\n", "\\n").Replace("\"", "\\\""); } // escape live back-slashes, carriage-returns (they break stuff), line-breaks, and quotes

	// JS<>Unity bridge: called from JS (custom)
	// ==========

	static void CallJS_Callback(int callbackID, object result = null)
	{
		if (callJSCallbacks[callbackID].Method.GetParameters().Length == 1)
			callJSCallbacks[callbackID].DynamicInvoke(result);
		else
			callJSCallbacks[callbackID].DynamicInvoke();
	}

	/*public void CallMethod(string methodPath, params object[] args)
	{
		if (methodPath.Contains("."))
		{
			var propName = methodPath.Substring(0, methodPath.IndexOf("."));
			(VTypeInfo.Get(GetType()).propInfo[propName].GetValue(this) as Node).CallMethod(methodPath.Substring(methodPath.IndexOf(".") + 1));
		}

		VTypeInfo.Get(GetType()).methodInfo.First(a=>a.memberInfo.Name == methodPath).Call(this, args);
	}*/
	/*public static void CallCS(string methodName, string callArgsVDF, int callbackID, string jsStack)
	{
		try
		{
			var callArgs = VConvert.FromVDF<List<object>>(callArgsVDF ?? "");

			var ignoreCall = false;
			if (methodName == "VInput.SetWebUIHasMouseFocus" || methodName == "VInput.SetWebUIHasKeyboardFocus" || methodName == "V.SetFreezeScreenOpacity")
				ignoreCall = true;
			else if ((methodName == "BD.Node_SendMessage" || methodName == "BD.Node_BroadcastMessage") && callArgs[0] is NodePath && (callArgs[0] as NodePath).nodes.Count == 1)
			{
				var finalMethodName = callArgs[2] as string;
				if (finalMethodName == "SetMainCameraRect" || finalMethodName == "SetMinimapCameraRect")
					ignoreCall = true;
			}
            /*if (VO.main != null && VO.main.settings != null && VO.main.settings.logCallsToFile && !ignoreCall)
				VDebug.LogToFile("==========\nCallCS>" + (callbackID != -1 ? "MethodName+CallbackID) "
					+ methodName + " (" + callbackID + ")" : "MethodName) " + methodName) + "\n----------\nCallArgsVDF) "
					+ callArgsVDF + "\n----------\nJSStack) " + jsStack, "Log_Calls.txt");*#/

			object obj = null;
			MethodInfo method = null;
			if (methodName.StartsWith("VO.main.")) // probably todo: make more generic approach, that also works for deep methods (e.g. VO.main.live.liveMatch.Start)
			{
				obj = VO.main;
				string realMethodName = methodName.Substring(methodName.LastIndexOf(".") + 1);
				foreach (MethodInfo method2 in VTypeInfo.Get(typeof(VOverlay)).methods.Values.Select(a=>a.memberInfo).OfType<MethodInfo>())
					//if (method2.Name == realMethodName)
					if (method2.Name == realMethodName && (method == null || method2.GetParameters().Length == callArgs.Count)) // prefer overload with matching param count
						method = method2;
			}
			else if (methodName.Contains(".")) // if "methodName" contains a "." (currently only way), then attempt to call static method indicated
			{
				string typeName = methodName.Substring(0, methodName.LastIndexOf("."));
				var type = Type.GetType(typeName);
				if (type == null)
					throw new Exception("Type does not exist. TypeName: " + typeName);
				string realMethodName = methodName.Substring(methodName.LastIndexOf(".") + 1);
				foreach (MethodInfo method2 in VTypeInfo.Get(typeof(VOverlay)).methods.Values.Select(a => a.memberInfo).OfType<MethodInfo>())
					//if (method2.Name == realMethodName)
					if (method2.Name == realMethodName && (method == null || method2.GetParameters().Length == callArgs.Count)) // prefer overload with matching param count
						method = method2;
			}

			object result;
			if (method != null)
				try
				{
					var parameters = method.GetParameters();
					var lastArgIsParams = parameters.Length > 0 && parameters[parameters.Length - 1].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
					if (lastArgIsParams)
					{
						var paramsArray = Array.CreateInstance(parameters.Last().ParameterType.GetElementType(), callArgs.Count - (parameters.Length - 1));
						for (var i = parameters.Length - 1; i < callArgs.Count; i++)
							paramsArray.SetValue(callArgs[i], i - (parameters.Length - 1)); //paramsArray[i - (parameters.Length - 1)] = callArgs[i];
						callArgs.RemoveRange(parameters.Length - 1, callArgs.Count - (parameters.Length - 1));
						callArgs.Add(paramsArray);
						//result = method.Invoke(obj, callArgs.Concat(new[] {Array.CreateInstance(parameters.Last().ParameterType.GetElementType(), 0)}).ToArray());
					}
					//else
					result = method.Invoke(obj, callArgs.ToArray());
				}
				catch (ArgumentException ex) { throw new ArgumentException("CallArg types are wrong. MethodName: " + methodName + ". SentCallArgTypes: " + String.Join(", ", callArgs.Select(arg=>arg.GetType().Name).ToArray()) + (". VDF: " + callArgsVDF) + "\n" + ex); }
				catch (TargetParameterCountException ex) { throw new TargetParameterCountException("CallArg count is incorrect. MethodName: " + methodName + ". SentCallArgCount: " + callArgs.Count + "\n" + ex); }
			else // for debugging
				throw new Exception("No matching method found. MethodName: " + methodName + ". CallArgTypes: " + String.Join(", ", callArgs.Select(arg=>arg.GetType().Name).ToArray()));

			if (callbackID != -1)
				CallCS_SendCallback(callbackID, result);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.Message + "\nCSMethod) " + methodName + "\nCSCallArgsVDF) " + callArgsVDF + "\nCSStack) " + ex.StackTrace + "\nJSStack) " + jsStack + "\n\n\n");
			throw;
		}
	}
	static void CallCS_SendCallback(int callbackID, object result = null)
	{
		if (result != null)
			CallJS("CSBridge.CallCS_Callback", callbackID, result);
		else
			CallJS("CSBridge.CallCS_Callback", callbackID);
	}*/

	// called from v-bot (and from local chat-input box, simulating chat-message-forwarding by v-bot)
	public static void OnMessageAdded(string message) { MessageHandler.OnMessageAdded(message); }
}