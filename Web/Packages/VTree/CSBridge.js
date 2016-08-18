var inUnity = window.location.href.contains("inUnity=true");
function InUnity() { return inUnity; }

CSBridge = new function()
{
	var s = this;

	// JS<>Unity bridge: called from JS
	// ==========

	s.lastCallCSCallbackID = -1;
	s.callCSCallbacks = {};
	s.CallCS = function(methodName, callArgs___) { // if last arg is a function, it will be used as the callback
    	//if ((s.CallCS_count = s.CallCS_count != null ? s.CallCS_count + 1 : 0) >= 100) // limits to first 100 calls
    	//	return;

		var callArgsArray = V.CloneArray(arguments).slice(1);
		var callArgs = new List("object");
		for (var i in callArgsArray)
			callArgs.push(callArgsArray[i]);

		var callback = callArgs.slice(-1)[0] instanceof Function ? callArgs.splice(-1)[0] : null; // if last argument is callback function, grab it, and remove it from argument list
		var callbackID = -1;
		if (callback) {
			callbackID = s.lastCallCSCallbackID + 1;
			s.callCSCallbacks[callbackID] = callback; // add callback to map
			s.lastCallCSCallbackID = callbackID;
		}

		var callArgsVDF = null;
		try {
			var callArgsNode = ToVDFNode(callArgs);
			PreCSCallSend(methodName, callArgsNode, callbackID, V.GetStackTraceStr());
			//callArgsVDF = ToVDF(callArgs);
			callArgsVDF = callArgsNode.ToVDF();
		}
		catch(error) { Log(error + "\nJSStack) " + error.stack + "\nNewStack) " + V.GetStackTraceStr()); }

		if (InUnity())
			engine.call("CallCS", methodName, callArgsVDF, callbackID, V.GetStackTraceStr());
		else if (callback)
			callback();
	};
	s.CallCS_Callback = function(callbackID, result) { s.callCSCallbacks[callbackID](result); };

	// JS<>Unity bridge: called from CS
	// ==========

	s.RunJS = function(code, catchErrors) {
		if (catchErrors) {
			try { return eval(code); }
			catch (error) { Log(error + "\nJSCode) " + code + "\nJSStack) " + error.stack + "\nNewStack) " + V.GetStackTraceStr()); }
		}
		else
			return eval(code);
	};
	if (InUnity())
		engine.on("RunJS", s.RunJS); // make RunCS method callable as an event // this must be called after the "CoherentUI.js" script has run

	s.CallJS = function(methodName, callArgsVDF, callbackID, csStack) {
		var callArgsVDF_simple = SimplifyCallArgsVDF(callArgsVDF);
		try {
			s.currentCallInfo = "JSMethod) " + methodName + "\nJSCallArgsVDF) " + callArgsVDF_simple + "\nJSStack) " + V.GetStackTraceStr() + "\nCSStack) " + csStack; // for Log method, when append-current-call-info is enabled

			//PreJSCallSend(methodName, callbackID, csStack, callArgsVDF_simple);

			//var callArgs = FromVDF(callArgsVDF); //, new VDFLoadOptions(null, true)
			//PreJSCallSend(methodName, callbackID, csStack, callArgs);

			var callArgsNode = FromVDFToNode(callArgsVDF);
			PreJSCallSend(methodName, callArgsNode, callbackID, csStack);
			var callArgs = Node_ToObject(callArgsNode); //callArgsNode.ToObject();

			var result = null;
			if (s.RunJS(methodName) instanceof Function) // if (s[methodName] instanceof Function)
				try { result = s.RunJS(methodName).apply(this, callArgs); }
				catch(error) { LogError(error + "\nJSMethod) " + methodName + "\nJSCallArgsVDF) " + callArgsVDF_simple + "\nJSStack) " + error.stack + "\nCSStack) " + csStack); } // errors that sometimes should happen

			if (callbackID != -1)
				s.CallJS_SendCallback(callbackID, result);
		}
		catch(error) { // errors that shouldn't happen
			//PreJSCallSend(methodName, callbackID, csStack, ToVDFNode([callArgsVDF_simple]));
			LogError(error + "\nJSMethod) " + methodName + "\nJSCallArgsVDF) " + callArgsVDF_simple + "\nJSStack) " + error.stack + "\nCSStack) " + csStack);
		}
		s.currentCallInfo = null;
	};
	s.CallJS_SendCallback = function(callbackID, result) { s.CallCS("JSBridge.CallJS_Callback", callbackID, result); }

	function SimplifyCallArgsVDF(vdf) { return vdf.replace(/Texture2D>".+?"/g, 'Texture2D>"[...]"'); }

	// probably todo; clean up the cross-context actions/callback system
	/*s.CallJSVisibleAction = function(actionID, callCSArgs___) {
		var callCSArgs = V.CloneArray(arguments);
		callCSArgs.splice(0, 0, "V.CallJSVisibleAction"); // add method-name as first callCSArg
		s.CallCS.apply(this, callCSArgs);
	};*/
};