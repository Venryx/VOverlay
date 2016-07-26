using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using SocketIO;
using VTree;

public class VBotBridge {
	public static void CallMethod(string methodName, List<object> args) {
		try {
			var callArgs = args;
			for (var i = 0; i < callArgs.Count; i++) {
				var argAsStr = callArgs[i] as string;
				if (argAsStr != null) {
					/*var asciiBytes = Encoding.Default.GetBytes(argAsStr);
					var utf8Bytes = Encoding.Convert(Encoding.Default, Encoding.UTF8, asciiBytes);
					var utf8Str = Encoding.UTF8.GetString(utf8Bytes);*/

					var origBytes = Encoding.Default.GetBytes(argAsStr);
					var utf8Str = Encoding.UTF8.GetString(origBytes);
					callArgs[i] = utf8Str;
					
					/*var inAsciiBytes = Encoding.ASCII.GetBytes(argAsStr); // convert string into a byte[]
					var outUTF8Bytes = Encoding.Convert(Encoding.ASCII, Encoding.UTF8, inAsciiBytes); // convert ascii-string bytes to utf8-string bytes
					// convert utf8-string bytes into char-array, then into string
					var inUTF8Chars = new char[Encoding.UTF8.GetCharCount(outUTF8Bytes, 0, outUTF8Bytes.Length)];
					Encoding.UTF8.GetChars(outUTF8Bytes, 0, outUTF8Bytes.Length, inUTF8Chars, 0);
					var outUTF8String = new string(inUTF8Chars);
					callArgs[i] = outUTF8String;*/
				}
			}

			object obj = null;
			VMethodInfo method = null;
			if (methodName.StartsWith("VO.main.")) {
				obj = VO.main;
				var pathRemaining = methodName.Substring("VO.main.".Length);
				while (pathRemaining.Contains(".")) {
					var propName = pathRemaining.Substring(0, pathRemaining.IndexOf("."));
					obj = obj.GetVTypeInfo().props[propName].GetValue(obj);
					pathRemaining = pathRemaining.Substring(propName.Length + 1);
				}

				string realMethodName = methodName.Substring(methodName.LastIndexOf(".") + 1);
				foreach (var method2 in obj.GetVTypeInfo().methods.Values)
					if (method2.memberInfo.Name == realMethodName && (method == null || method2.memberInfo.GetParameters().Length == callArgs.Count)) // prefer overload with matching param count
						method = method2;
			}
			else if (methodName.Contains(".")) { // if "methodName" contains a "." (currently only way), then attempt to call static method indicated
				string typeName = methodName.Substring(0, methodName.LastIndexOf("."));
				var type = Type.GetType(typeName);
				if (type == null)
					throw new Exception("Type does not exist. TypeName: " + typeName);
				string realMethodName = methodName.Substring(methodName.LastIndexOf(".") + 1);
				foreach (var method2 in VTypeInfo.Get(type).methods.Values)
					if (method2.memberInfo.Name == realMethodName && (method == null || method2.memberInfo.GetParameters().Length == callArgs.Count)) // prefer overload with matching param count
						method = method2;
			}

			object result;
			if (method != null)
				try {
					var parameters = method.memberInfo.GetParameters();
					var lastArgIsParams = parameters.Length > 0 && parameters[parameters.Length - 1].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
					if (lastArgIsParams) {
						var paramsArray = Array.CreateInstance(parameters.Last().ParameterType.GetElementType(), callArgs.Count - (parameters.Length - 1));
						for (var i = parameters.Length - 1; i < callArgs.Count; i++)
							paramsArray.SetValue(callArgs[i], i - (parameters.Length - 1)); //paramsArray[i - (parameters.Length - 1)] = callArgs[i];
						callArgs.RemoveRange(parameters.Length - 1, callArgs.Count - (parameters.Length - 1));
						callArgs.Add(paramsArray);
						//result = method.Invoke(obj, callArgs.Concat(new[] {Array.CreateInstance(parameters.Last().ParameterType.GetElementType(), 0)}).ToArray());
					}
					//else
					result = method.Call(obj, callArgs.ToArray());
				}
				catch (ArgumentException ex) { throw new ArgumentException("CallArg types are wrong. MethodName: " + methodName + ". SentCallArgTypes: " + String.Join(", ", callArgs.Select(arg=>arg.GetType().Name).ToArray()) + "\n" + ex); }
				catch (TargetParameterCountException ex) { throw new TargetParameterCountException("CallArg count is incorrect. MethodName: " + methodName + ". SentCallArgCount: " + callArgs.Count + "\n" + ex); }
			else // for debugging
				throw new Exception("No matching method found. MethodName: " + methodName + ". CallArgTypes: " + String.Join(", ", callArgs.Select(arg=>arg.GetType().Name).ToArray()));
		}
		/*catch (Exception ex) {
			Debug.LogWarning(ex.Message + "\nCSMethod) " + methodName + "\nArgsLength) " + args.Count + "\n\n\n");
			throw;
		}*/
		finally {}
	}
	static void CallMethod_Internal(string methodName, params object[] args) { CallMethod(methodName, args.ToList()); }

	// called from local chat-input box, simulating chat-message-forwarding by v-bot
	public static void OnLocalChatMessageAdded(string message) {
		Debug.Log("Local chat message:" + message);

		// message shortcuts - dev
		if (message.StartsWith("rm")) {
			OnLocalChatMessageAdded("!race");
			OnLocalChatMessageAdded("!play 😒");
			return;
		}

		// message aliases
		/*if (message.StartsWith("!m "))
			message = message.Replace("!m ", "!move ");

		if (message == "!race") {
			VO.main.race.MakeVisible();
			VO.main.race.StartNewMatch();
		}
		else if (message == "!tower") {
			VO.main.tower.MakeVisible();
			VO.main.tower.StartNewMatch();
		}
		else if (message == "!play") {
			var parts = message.Split(' ').Skip(1).ToList();
			var emoji = parts.GetValue(0, "😒");
			var emoji_encodedStr = "\\" + Uri.EscapeDataString(emoji) + "\\";
			CallMethod_Internal("AddPlayer", "local", emoji_encodedStr);
		}
		else if (message.StartsWith("!move ")) {
			var parts = message.Split(' ').Skip(1).ToList();
			var x = double.Parse(parts[0]);
			var z = double.Parse(parts[1]);
			var strength = parts.Count >= 3 ? double.Parse(parts[2]) : 1000; // maybe temp
			CallMethod_Internal("PlayerJump", "local", x, z, strength);
		}*/

		VBotBridgeScript.main.socket.Emit("HandleMessage", JSONObject.CreateStringObject(message));
	}
}