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
			var realMethodName = methodName;
			var callArgs = args;

			for (var i = 0; i < callArgs.Count; i++) {
				var argAsStr = callArgs[i] as string;
				if (argAsStr != null) {
					/*var asciiBytes = Encoding.Default.GetBytes(argAsStr);
					var utf8Bytes = Encoding.Convert(Encoding.Default, Encoding.UTF8, asciiBytes);
					var utf8Str = Encoding.UTF8.GetString(utf8Bytes);*/

					/*var origBytes = Encoding.Default.GetBytes(argAsStr);
					var utf8Str = Encoding.UTF8.GetString(origBytes);
					callArgs[i] = utf8Str;*/
					
					var inAsciiEncoding = Encoding.ASCII;
					var outUTF8Encoding = Encoding.UTF8;
					// Convert the input string into a byte[].
					byte[] inAsciiBytes = inAsciiEncoding.GetBytes(argAsStr);
					// Conversion string in ASCII encoding to UTF8 encoding byte array.
					byte[] outUTF8Bytes = Encoding.Convert(inAsciiEncoding, outUTF8Encoding, inAsciiBytes);
					// Convert the byte array into a char[] and then into a string.
					char[] inUTF8Chars = new char[outUTF8Encoding.GetCharCount(outUTF8Bytes, 0, outUTF8Bytes.Length)];
					outUTF8Encoding.GetChars(outUTF8Bytes, 0, outUTF8Bytes.Length, inUTF8Chars, 0);
					string outUTF8String = new string(inUTF8Chars);
					callArgs[i] = outUTF8String;
				}
			}

			object obj = null;
			MethodInfo method = null;
			var methods = VTypeInfo.Get(typeof(MessageHandler)).methods.Values;
			foreach (MethodInfo method2 in VTypeInfo.Get(typeof(MessageHandler)).methods.Values.Select(a=>a.memberInfo).OfType<MethodInfo>())
				//if (method2.Name == realMethodName)
				if (method2.Name == realMethodName && (method == null || method2.GetParameters().Length == callArgs.Count)) // prefer overload with matching param count
					method = method2;

			object result;
			if (method != null)
				try {
					var parameters = method.GetParameters();
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
					result = method.Invoke(obj, callArgs.ToArray());
				}
				catch (ArgumentException ex) { throw new ArgumentException("CallArg types are wrong. MethodName: " + methodName + ". SentCallArgTypes: " + String.Join(", ", callArgs.Select(arg=>arg.GetType().Name).ToArray()) + "\n" + ex); }
				catch (TargetParameterCountException ex) { throw new TargetParameterCountException("CallArg count is incorrect. MethodName: " + methodName + ". SentCallArgCount: " + callArgs.Count + "\n" + ex); }
			else // for debugging
				throw new Exception("No matching method found. MethodName: " + methodName + ". CallArgTypes: " + String.Join(", ", callArgs.Select(arg=>arg.GetType().Name).ToArray()));
		}
		/*catch (Exception ex) 		{
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
		if (message.StartsWith("!m "))
			message = message.Replace("!m ", "!move ");

		if (message == "!race")
			CallMethod_Internal("StartNewRace");
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
		}
	}
}