var classNames = require('classnames');
function Classes(arg1, arg2) {
    var classesObj = typeof arg1 == "object" ? arg1 : arg2;
    if (typeof arg1 == "string") {
        var classStr_classes = arg1.split(" ");
        for (var className of classStr_classes)
            classesObj[className] = true;
    }
    return classNames(classesObj);
}

// variables
// ==================

var livePage;
var liveSubpage;
var liveSubpage2;

//var lastLoadedUrl = null;

// angular
// ==========

/*function AngularCompile(root)
{
    var injector = angular.element($('[ng-app]')[0]).injector();
    var $compile = injector.get('$compile');
    var $rootScope = injector.get('$rootScope');
    var result = $compile(root)($rootScope);
    $rootScope.$digest();
    return result;
}*/

// polyfills for constants
// ==========

if (Number.MIN_SAFE_INTEGER == null)
	Number.MIN_SAFE_INTEGER = -9007199254740991;
if (Number.MAX_SAFE_INTEGER == null)
	Number.MAX_SAFE_INTEGER = 9007199254740991;

//function Break() { debugger; };
function Debugger() { debugger; }
function Debugger_True() { debugger; return true; }

// methods: url writing/parsing
// ==================

function CurrentURL() { return window.location.href.replace(/%22/, "\""); } // note; look into the escaping issue more
function GetURLVars(url = CurrentURL()) {
	if (!url.contains('?'))
		return { length: 0 };

	var vars = {};

	var urlVarStr = url.contains("?") ? (url.contains("runJS=") ? url.slice(url.indexOf("?") + 1) : url.slice(url.indexOf("?") + 1).split("#")[0]) : "";
	var parts = urlVarStr.split("&");
	for (var i = 0; i < parts.length; i++)
		vars[parts[i].substring(0, parts[i].indexOf("="))] = parts[i].substring(parts[i].indexOf("=") + 1);

	return vars;
}

// Unity-linked
// ==================

var inUnity = GetURLVars(CurrentURL()).inUnity == "true"; // this var is set within the initial url of the Unity CoherentUIView
var inTestMode = true; //GetURLVars(CurrentURL()).inTestMode == "true";
function InUnity() { return inUnity; } //return window.Unity;
function InTestMode() { return inTestMode; }

//window.evalOld = window.eval;
//window.eval = function() { try { evalOld.apply(this, arguments); } catch(error) { Log("JS error: " + error); }};

/*window.evalOld = eval;
window.eval = function(code)
{
    if (true) //new Error().stack.contains("Packages/VDF")) //!code.contains(";") && code != "CallCS_Callback")
    {
        window.lastSpecialEvalExpression = code;
        window.lastSpecialEvalStack = new Error().stack;
        //window.evalStacks = window.evalStacks || [];
        //window.evalStacks.push(new Error().stack);
        window.evalExpressionsStr += code + "\n";
        window.evalStacksStr += new Error().stack + "\n";
    }
    return evalOld.apply(this, arguments);
};*/

/*var Debug = true;

var Log = function(msg, type = 'default') { if(!Debug) return;
 var colorString = '';
 switch(type) { case 'error': colorString = '\x1b[91m';
 break;
 case 'warning': colorString = '\x1b[93m';
 break;
 case 'default': default: colorString = '\x1b[0m';
 break;
 } var spaceString = Array(7 - process.pid.toString().length).join(' ');
 console.log(colorString, process.pid + '' + spaceString + msg + '\x1b[0m');
};*/

console.error_orig = console.error;
console.error = function(exception) {
    var str = exception + '';
    if (str.Contains('Warning: A component is `contentEditable`')) return;
    //if (str.Contains("Warning: Unknown prop `")) return;
    console.error_orig.apply(this, arguments);
};

//window.alertOld = window.alert;
window.alertNew = function(message) { CSBridge.CallCS("V.Alert", "JavaScript Alert", message, "OK"); };
window.Log = InUnity() ? function(message, /*o:*/ appendStackTrace, appendCurrentCallInfo, logLater)
{
	//appendStackTrace = appendStackTrace != null ? appendStackTrace : true; // maybe todo: make add-stack-trace-and-current-call-info-to-logs setting
	//appendCurrentCallInfo = appendCurrentCallInfo != null ? appendCurrentCallInfo : true;

	var finalMessage = message;
	if (appendStackTrace)
		finalMessage += "\n\nStackTrace) " + new Error().stack;
	if (appendCurrentCallInfo)
		finalMessage += "\n\n" + CSBridge.currentCallInfo;

	CSBridge.CallCS(logLater ? "VDebug.LogLater" : "VDebug.Log", finalMessage);
	return message;
} : function(message, /*optional:*/ appendStackTrace, logLater) { console.log(message); return message; };
window.LogLater = function(message, /*o:*/ appendStackTrace, appendCurrentCallInfo) { Log(message, appendStackTrace, appendCurrentCallInfo, true); }
window.LogError = InUnity() ? function(message)
{
	CSBridge.CallCS("VDebug.LogError", message);
	return message;
} : function(message, /*optional:*/ appendStackTrace, logLater) { console.log("LogError) " + message); return message; };

/*window.Time = {frameCount: 0};
window.updateCalls = [];
window.AddUpdateCall = function(func) { updateCalls.push(func); };
window.RemoveUpdateCall = function (func) { updateCalls.splice(updateCalls.indexOf(func), 1); };
if (InUnity())
    window.Update = function(frameCount)
	{
		Time.frameCount = frameCount;
		for (var i in updateCalls)
			updateCalls[i]();
    }
/*else
    window.Update = function(frameCount)
	{
		Time.frameCount = frameCount;
		for (var i in updateCalls)
			updateCalls[i]();
		setTimeout(function() { Update(++Time.frameCount); }, 33); // simulate Update call at 30 fps (i.e. 33 ms = 1000/33 fps = 60 fps)
	};
	Update(++Time.frameCount);*/

//window.onerror = function(message, url, line) { Log("JS) " + message + " (" + line + "; " + url + ")\n"); };
function HandleError(error) { LogError("JS) " + error.stack); }

//PreJSCallSend("TestMethodName", -1, "TestStackTrace", "List(object)>[\"test string argument\"]"); // for testing
/*function PreJSCallSend(methodName, callbackID, csStack, callArgsVDF)
{
	//if (["VO.Update", "VO.SetFPSText"].Contains(methodName))
	//	return;
	if (methodName == "VO.main.Node_SendMessage" || methodName == "VO.main.Node_BroadcastMessage")
	{
		var finalMethodName = callArgs[3];
		if (["Update", "SetFPSText"].Contains(finalMethodName))
			return;
	}

    var node = VDFLoader.ToVDFNode(callArgsVDF);
	//PostProcessJSCallArgsVDFNode(node);

    var callArgStrings = [];
	for (var i in node.listChildren)
		callArgStrings.push(node.listChildren[i].ToVDF());

    if (window["BD"] && VO.console && VO.console.callLoggerEnabled)
        VO.console.AddCallLogEntry({type: "CallJS", methodName: methodName, callArgStrings: callArgStrings, callbackID: callbackID, callerStack: csStack});
}*/
/*function PostProcessJSCallArgsVDFNode(node)
{
	for (var key in node.mapChildren.Keys)
		PostProcessJSCallArgsVDFNode(node[key]);
	for (var i in node.listChildren)
		PostProcessJSCallArgsVDFNode(node[i]);

	if (node.metadata && node.metadata.Contains("Texture2D"))
		node.primitiveValue = "[...]";
}*/
/*function SimplifyCallArgsNode(node)
{
	for (var key in node.mapChildren.Keys)
		SimplifyCallArgsNode(node[key]);
	for (var i in node.listChildren)
		SimplifyCallArgsNode(node[i]);

	if (node.metadata && node.metadata.Contains("Texture2D"))
		node.primitiveValue = "[...]";
}*/

/*function PreJSCallSend(methodName, callbackID, csStack, callArgs)
{
	if (methodName == "VO.Node_SendMessage" || methodName == "VO.Node_BroadcastMessage")
	{
		var finalMethodName = callArgs[2];
		if (["Update", "SetFPSText"].Contains(finalMethodName))
			return;
	}

	var callArgStrings = new List("string");
	for (var i in callArgs)
		callArgStrings.push(ToVDF(callArgs[i]));

    if (window["BD"] && VO.console && VO.console.callLoggerEnabled)
        VO.console.AddCallLogEntry({type: "CallJS", methodName: methodName, callArgStrings: callArgStrings, callbackID: callbackID, callerStack: csStack});
}

function PreCSCallSend(methodName, callbackID, jsStack, callArgs)
{
	if (["VInput.SetWebUIHasMouseFocus", "VInput.SetWebUIHasKeyboardFocus", "V.SetFreezeScreenOpacity"].Contains(methodName))
		return;
	if (methodName == "VO.main.Node_SendMessage" || methodName == "VO.main.Node_BroadcastMessage")
	{
		var finalMethodName = callArgs[2];
		if (["SetMainCameraRect", "SetMinimapCameraRect"].Contains(finalMethodName))
			return;
	}

	var callArgStrings = new List("string");
    for (var i in callArgs)
        callArgStrings.push(ToVDF(callArgs[i]));

    if (window["BD"] && VO.console && VO.console.callLoggerEnabled)
	    VO.console.AddCallLogEntry({type: "CallCS", methodName: methodName, callArgStrings: callArgStrings, callbackID: callbackID, callerStack: csStack});
}*/

function PreJSCallSend(methodName, callArgsNode, callbackID, csStack) {
	if (methodName == "OnLogMessageAdded")
		return;
	if (methodName == "VO.Node_SendMessage" || methodName == "VO.Node_BroadcastMessage") {
		var finalMethodName = callArgsNode[2].primitiveValue;
		if (["Update", "SetFPSText", "SetTerrainPositionsText"].Contains(finalMethodName))
			return;
	}

    if (window["BD"] && VO.console && VO.console.callLoggerEnabled)
        VO.console.AddCallLogEntry({type: "CallJS", methodName: methodName, callbackID: callbackID, callerStack: csStack, callArgsNode: callArgsNode});
}

function PreCSCallSend(methodName, callArgsNode, callbackID, jsStack) {
	if (["VInput.SetWebUIHasMouseFocus", "VInput.SetWebUIHasKeyboardFocus", "V.SetFreezeScreenOpacity"].Contains(methodName))
		return;
	if (methodName == "VO.main.Node_SendMessage" || methodName == "VO.main.Node_BroadcastMessage") {
		var finalMethodName = callArgsNode[2].primitiveValue;
		if (["SetMainCameraRect", "SetMinimapCameraRect"].Contains(finalMethodName))
			return;
	}

	if (window["BD"] && VO.console && VO.console.callLoggerEnabled)
	    VO.console.AddCallLogEntry({type: "CallCS", methodName: methodName, callbackID: callbackID, callerStack: jsStack, callArgsNode: callArgsNode});
}

// base function for storing log messages (visible in the Console menu)
function OnLogMessageAdded(type, message, /*o:*/ stackTrace)
{
    if (!type || !message) //|| !stackTrace)
		return;

	/*if(window["Frame"] && Frame.Console && Frame.Console.data && Frame.Console.data.loggerEnabled)
	{
		message = message.replace(/#nl#/g, "\n");
		stackTrace = stackTrace.replace(/#nl#/g, "\n").replace(/(^\n+|\n+$)/g, "");

		Frame.Console.log += (Frame.Console.log.length > 0 ? "\n" : "");
		Frame.Console.log += (message || "") + "\n";
		if (Frame.Console.data.logStackTrace)
		    Frame.Console.log += (stackTrace ? "\n" + stackTrace + "\n" : "");

	    //if (Frame.Console.data.logStackTrace && (type == "Error" || type == "Exception"))
	    //    Frame.Console.log += "LastCallCS_Stack) " + window.lastCallCS_stack + "\n";
	    /*if (message == "Uncaught SyntaxError: Unexpected token ILLEGAL")
        {
            Frame.Console.log += "LastSpecialEvalExpression) " + lastSpecialEvalExpression + "\n";
            Frame.Console.log += "LastSpecialEvalStack) " + lastSpecialEvalStack + "\n";
            Frame.Console.log += "EvalExpressionsStr) " + evalExpressionsStr;
            //Frame.Console.log += "EvalStacksStr) " + evalStacksStr;
        }*#/

		Frame.Console.log += "==================";
        if (Frame.Console.root.css("display") != "none")
		    Frame.Console.Refresh();
	}*/

	//if (window["BD"] && VO.console && VO.console.loggerEnabled)
    if (window["BD"] && VO.console && VO.console.capture_general)
        VO.console.AddLogEntry({type: type, message: message, stackTrace: stackTrace});
}

//var prefCache = {}; // note; this assumes that prefs retreived by JS are only ever set by JS
function GetPref(pName, callback)
{
	//if (!InUnity())
	//{ callback(null); return; }
	/*if (prefCache[pName])
		callback(FromVDF(prefCache[pName]));
	else
		VO.SendMessage("GetPref", pName, function(pValueVDF) { prefCache[pName] = pValueVDF; callback(pValueVDF ? FromVDF(pValueVDF) : null); });*/
	VO.SendMessage(ContextGroup.Local_CS, "GetPref", pName, function(pValueVDF) { callback(pValueVDF ? FromVDF(pValueVDF) : null); });
}
function SetPref(pName, pValue)
{
	var pValueVDF = ToVDF(pValue) || "";
	//prefCache[pName] = pValueVDF;
	VO.SendMessage(ContextGroup.Local_CS, "SetPref", pName, pValueVDF);
}

var hasFocus = false;
function UpdateUIHasKeyboardFocus()
{
	var focused = $(document.activeElement);

	var newHasFocus = false;
	if ($(".ui-widget-overlay.ui-front").length > 0) // if modal dialog is open
		newHasFocus = true;
	else if ($("#context-menu-layer").length > 0) // if context menu is open
		newHasFocus = true;
	else if ($(".ui-menu:visible").length > 0) // if menu bar is open
		newHasFocus = true;
	else if (focused.length > 0) // if an element is in focus
	{
		if (focused.is("input[type=text]") || focused.is("textarea") || focused.attr("captureFocus"))
			newHasFocus = true;
	}

	if (newHasFocus != hasFocus)
		CSBridge.CallCS("VInput.SetWebUIHasKeyboardFocus", newHasFocus);
	hasFocus = newHasFocus;
}
var lastHasMouseFocus = false;
function UpdateWebUIHasMouseFocus(hasMouseFocus)
{
    if (lastHasMouseFocus == hasMouseFocus)
        return;
    CSBridge.CallCS("VInput.SetWebUIHasMouseFocus", hasMouseFocus);
    lastHasMouseFocus = hasMouseFocus;
}

// methods: serialization
// ==========

// object-Json
function FromJSON(json) { return JSON.parse(json); }
function ToJSON(obj, excludePropNames___) {
	try {
		if (arguments.length > 1) {
			var excludePropNames = V.Slice(arguments, 1);
			return JSON.stringify(obj, function(key, value) {
				if (excludePropNames.Contains(key))
					return;
				return value;
			});
		}
		return JSON.stringify(obj);
	}
	catch (ex) {
		if (ex.toString() == "TypeError: Converting circular structure to JSON")
			return ToJSON_Safe.apply(this, arguments);
		throw ex;
	}
}
function ToJSON_Safe(obj, excludePropNames___) {
	var excludePropNames = V.Slice(arguments, 1);

	var cache = [];
	var foundDuplicates = false;
	var result = JSON.stringify(obj, function(key, value) {
		if (excludePropNames.Contains(key))
			return;
		if (typeof value == 'object' && value !== null) {
			// if circular reference found, discard key
			if (cache.indexOf(value) !== -1) {
				foundDuplicates = true;
				return;
			}
			cache.push(value); // store value in our cache
		}
		return value;
	});
	//cache = null; // enable garbage collection
	if (foundDuplicates)
		result = "[was circular]" + result;
	return result;
}

// object-VDF
// ----------

//Function.prototype._AddGetter_Inline = function VDFSerialize() { return function() { return VDF.CancelSerialize; }; };
Function.prototype.Serialize = function() { return VDF.CancelSerialize; }.AddTags(new VDFSerialize());

function FinalizeFromVDFOptions(options)
{
	options.loadUnknownTypesAsBasicTypes = true;
	return options;
}
function FromVDF(vdf, /*o:*/ declaredTypeName_orOptions, options)
{
	if (declaredTypeName_orOptions instanceof VDFLoadOptions)
		return FromVDF(vdf, null, declaredTypeName_orOptions);

	try { return VDF.Deserialize(vdf, declaredTypeName_orOptions, FinalizeFromVDFOptions(options || new VDFLoadOptions())); }
	/*catch(error) { if (!InUnity()) throw error; else LogError("Error) " + error + "Stack)" + error.stack + "\nNewStack) " + new Error().stack + "\nVDF) " + vdf); }/**/finally{}
}
function FromVDFInto(vdf, obj, /*o:*/ options)
{
	try { return VDF.DeserializeInto(vdf, obj, FinalizeFromVDFOptions(options || new VDFLoadOptions())); }
	/*catch(error) { if (!InUnity()) throw error; else LogError("Error) " + error + "Stack)" + error.stack + "\nNewStack) " + new Error().stack + "\nVDF) " + vdf); }/**/finally{}
}
function FromVDFToNode(vdf, /*o:*/ declaredTypeName_orOptions, options) {
	if (declaredTypeName_orOptions instanceof VDFLoadOptions)
		return FromVDF(vdf, null, declaredTypeName_orOptions);

	try { return VDFLoader.ToVDFNode(vdf, declaredTypeName_orOptions, FinalizeFromVDFOptions(options || new VDFLoadOptions())); }
	/*catch(error) { if (!InUnity()) throw error; else LogError("Error) " + error + "Stack)" + error.stack + "\nNewStack) " + new Error().stack + "\nVDF) " + vdf); }/**/finally { }
}

function Node_ToObject(node) { // alternative to .ToObject(), which applies default (program) settings
	return node.ToObject(FinalizeFromVDFOptions(new VDFLoadOptions()));
}

function FinalizeToVDFOptions(options) {
	return options;
}
/*function ToVDF(obj, /*o:*#/ declaredTypeName_orOptions, options_orNothing)
{
	try { return VDF.Serialize(obj, declaredTypeName_orOptions, options_orNothing); }
	/*catch(error) { if (!InUnity()) throw error; else LogError("Error) " + error + "Stack)" + error.stack + "\nNewStack) " + new Error().stack + "\nObj) " + obj); }
	//catch(error) { if (!InUnity()) { debugger; throw error; } else LogError("Error) " + error + "Stack)" + error.stack + "\nNewStack) " + new Error().stack + "\nObj) " + obj); }
}*/
function ToVDF(obj, /*o:*/ markRootType, typeMarking, options) {
	markRootType = markRootType != null ? markRootType : true; // maybe temp; have JS side assume the root-type should be marked
	typeMarking = typeMarking || VDFTypeMarking.Internal;

	try {
		options = FinalizeToVDFOptions(options || new VDFSaveOptions());
		options.typeMarking = typeMarking;
		return VDF.Serialize(obj, !markRootType && obj != null ? obj.GetTypeName() : null, options);
	}
	/*catch(error) { if (!InUnity()) throw error; else LogError("Error) " + error + "Stack)" + error.stack + "\nNewStack) " + new Error().stack + "\nObj) " + obj); }/**/finally { }
	//catch(error) { if (!InUnity()) { debugger; throw error; } else LogError("Error) " + error + "Stack)" + error.stack + "\nNewStack) " + new Error().stack + "\nObj) " + obj); }
}
function ToVDFNode(obj, /*o:*/ declaredTypeName_orOptions, options_orNothing) {
	try { return VDFSaver.ToVDFNode(obj, declaredTypeName_orOptions, options_orNothing); }
	catch (error) { if (!InUnity()) throw error; else LogError("Error) " + error + "Stack)" + error.stack + "\nNewStack) " + new Error().stack + "\nObj) " + obj); }
}

// standard types
// ----------

function IsPrimitive(obj) { return IsBool(obj) || IsNumber(obj) || IsString(obj); }
function IsBool(obj) { return typeof obj == "boolean"; } //|| obj instanceof Boolean
function ToBool(boolStr) { return boolStr == "true" ? true : false; }
function IsNumber(obj) { return typeof obj == "number"; } //|| obj instanceof Number
function ToInt(stringOrFloatVal) { return parseInt(stringOrFloatVal); }
function ToDouble(stringOrIntVal) { return parseFloat(stringOrIntVal); }
function IsString(obj) { return typeof obj == "string"; } //|| obj instanceof String
function ToString(val) { return "" + val; }

function IsInt(obj) { return typeof obj == "number" && parseFloat(obj) == parseInt(obj); }
function IsDouble(obj) { return typeof obj == "number" && parseFloat(obj) != parseInt(obj); }

var types = {};
function GetType(name) {
	if (name == null) // maybe temp
		return null;

	if (types[name] == null)
		types[name] = new Type(name);
	return types[name];
}

//window.T = window.Type;

// maybe make-so: calling Type with name of type that already exists, returns that Type object, rather than creating a new Type object
function Type(name) {
	var s = this;
	s.name = name;
	s.nameRoot = name.contains("(") ? name.substr(0, name.indexOf("(")) : name;
	s.genericArguments = VDF.GetGenericArgumentsOfType(name);

	s.toString = function() { return name; };
	//s.Serialize = function() { return new VDFNode(s.toString()); }.AddTags(new VDFSerialize());
	s.Serialize = function() { return new VDFNode(s.toString()).Init({metadata_override: "Type"}); }.AddTags(new VDFSerialize()); // maybe temp; fix for that C#-side crashes without metadata set to "Type"

	s.IsDerivedFrom = function(other, /*o:*/ allowSameType) {
		allowSameType = allowSameType != null ? allowSameType : true;

		if (other.name == "Property" && s.name != null) // todo: add more sophisticated/correct-in-all-cases version of this
			return true;
		if (other.name == "object" && s.name != null)
			return true;
		if (other.name == "IList" && s.name && s.name.startsWith("List("))
			return true;
		if (other.name == "IDictionary" && s.name && s.name.startsWith("Dictionary("))
			return true;
		if (s.name == other.name)
			return allowSameType;
		if (window[s.name] && window[other.name] && window[s.name].prototype instanceof window[other.name])
			return true;
		return false;
	};
}
Type.Deserialize = function(node) { return node.primitiveValue ? GetType(node.primitiveValue) : null; }.AddTags(new VDFDeserialize(true));

// methods
// ==========

function Range(start, stop) {
	var result = [];
	for (var i = start; i < stop; i++)
		result.push(i);
	return result;
}

function TryCall(func, /*optional:*/ args_) { if (func instanceof Function) func.apply(this, V.CloneArray(arguments).splice(0, 1)); }
function TryCall_OnX(obj, func, /*optional:*/ args_) { if (func instanceof Function) func.apply(obj, V.CloneArray(arguments).splice(0, 1)); }

function WaitXThenRun(waitTime, func) { setTimeout(func, waitTime); }
function Sleep(ms) {
	var startTime = new Date().getTime();
	while (new Date().getTime() - startTime < ms)
	{}
}
function WaitXThenRun_Multiple(waitTime, func, /*o:*/ count) {
	count = count != null ? count : -1;

	var countDone = 0;
	var timerID = setInterval(function() {
		func();
		countDone++;
		if (count != -1 && countDone >= count)
			clearInterval(timerID);
	}, waitTime);

	var controller = {};
	controller.Stop = function() { clearInterval(timerID); }
	return controller;
}

// interval is in seconds (can be decimal)
function Timer(interval, func, /*o:*/ maxCallCount) {
	maxCallCount = maxCallCount != null ? maxCallCount : -1;

	var s = this;
	s.timerID = -1;
	s.callCount = 0;
	s.Start = function() {
		s.timerID = setInterval(function() {
			func();
			s.callCount++;
			if (maxCallCount != -1 && s.callCount >= maxCallCount)
				s.Stop();
		}, interval * 1000);
	};
	s.Stop = function() {
		clearInterval(s.timerID);
		s.timerID = -1;
	};
}
Timer.SetAsBaseClassFor = function TimerMS(interval_decimal, func, /*o:*/ maxCallCount) {
	var s = this.CallBaseConstructor(interval_decimal / 1000, func, maxCallCount);
};

var keyTicks = {};
function Tick_AlertIfPastLimit(key, limit) {
	if (keyTicks[key] == null)
		keyTicks[key] = 0;
	keyTicks[key]++;
	if (keyTicks[key] > limit)
		debugger;
}

function SetCookie(name, value, /*optional:*/ daysToExpire)
{
	daysToExpire = daysToExpire != null ? daysToExpire : 100000;
	var expireDate = new Date();
	expireDate.setDate(expireDate.getDate() + daysToExpire);
	document.cookie = name + "=" + value + "; expires=" + expireDate.toUTCString() + "; path=/;";
}
function GetCookie(name)
{
	var result = null;

	var container = document.cookie;

	var cookieStart = container.indexOf(" " + name + "=");
	if (cookieStart == -1)
		cookieStart = container.indexOf(name + "=");
	if (cookieStart != -1)
	{
		var cookieValueStart = container.indexOf("=", cookieStart) + 1;
		var cookieValueEnd = container.indexOf(";", cookieValueStart);
		if (cookieValueEnd == -1)
			cookieValueEnd = container.length;
		result = decodeURIComponent(container.substring(cookieValueStart, cookieValueEnd));
	}

	return result;
}

function IsQuickMenuOpen(id) { return $("#" + id).css("display") != "none"; }
function ToggleQuickMenu(id)
{
	var open = IsQuickMenuOpen(id);
	SetQuickMenuOpen(id, !open);
	return !open;
}
function SetQuickMenuOpen(id, open) { $("#" + id).css("display", open ? "" : "none"); }
function CloseQuickMenus() { $(".quickMenu.autoClose").css("display", "none"); }

function GetSelection() { return {startControl: GetSelectionStartControl(), endControl: GetSelectionEndControl(), startOffset: GetSelectionStartOffset(), endOffset: GetSelectionEndOffset()}; }
function GetSelectionStartControl() { return getSelection().anchorNode; } //getSelection().getRangeAt(0).startContainer; }
function GetSelectionEndControl() { return getSelection().anchorNode; } //return getSelection().getRangeAt(getSelection().rangeCount - 1).endContainer; }
function GetSelectionStartOffset() { return getSelection().rangeCount ? getSelection().getRangeAt(0).startOffset : 0; }
function GetSelectionEndOffset() { return getSelection().rangeCount ? getSelection().getRangeAt(getSelection().rangeCount - 1).endOffset : 0; }

function SetSelection(control, startOffset, endOffset)
{
    var selection = getSelection();
    var range = document.createRange();
    range.setStart(control, startOffset);
    range.setEnd(control, endOffset);
    selection.removeAllRanges();
    selection.addRange(range);
}

function IsNaN(number) { return typeof number == "number" && number != number; }

//VDebug.AddErrorHandlingToMethodsIn(window);

/*function GetBD()
{
	try { return BD; }
	catch(error) { HandleError(error); }
}*/

// common/helper types for UI<>CS communication
// ==========

function Vector2i(/*o:*/ x, y) {
	var s = this;
	s.x = x != null ? x : 0;
	s.y = y != null ? y : 0;

	s.Deserialize = function(node) {
		var strParts = node.primitiveValue.split(" ");
		s.x = parseInt(strParts[0]);
		s.y = parseInt(strParts[1]);
	}.AddTags(new VDFDeserialize());
	//s.VDFSerialize = function() { return s.toString(); }; //Swapped().toString(); };
	s.Serialize = function() { return new VDFNode(s.toString()); }.AddTags(new VDFSerialize());
	s.toString = function() { return s.x + " " + s.y; };

	s.NewX = function(xOrFunc) { return new Vector2i(xOrFunc instanceof Function ? xOrFunc(s.x) : xOrFunc, s.y); };
	s.NewY = function(yOrFunc) { return new Vector2i(s.x, yOrFunc instanceof Function ? yOrFunc(s.y) : yOrFunc); };

	s.Minus = function(arg1, arg2) {
		if (arg1 instanceof Vector2i)
			return new Vector2i(s.x - arg1.x, s.y - arg1.y);
		return new Vector2i(s.x - arg1, s.y - arg2);
	};
	s.Plus = function(arg1, arg2) {
		if (arg1 instanceof Vector2i)
			return new Vector2i(s.x + arg1.x, s.y + arg1.y);
		return new Vector2i(s.x + arg1, s.y + arg2);
	};
}
Vector2i._AddGetter_Inline = function zero() { return new Vector2i(0, 0); }
Vector2i._AddGetter_Inline = function one() { return new Vector2i(1, 1); }

function VVector2(/*o:*/ x, y) {
	var s = this;
	s.x = x != null ? x : 0;
	s.y = y != null ? y : 0;

	s.Deserialize = function(node) {
		var strParts = node.primitiveValue.split(" ");
		s.x = parseInt(strParts[0]);
		s.y = parseInt(strParts[1]);
	}.AddTags(new VDFDeserialize());
	s.Serialize = function() { return new VDFNode(s.toString()); }.AddTags(new VDFSerialize());
	s.toString = function() { return s.x + " " + s.y; };

	s.NewX = function(xOrFunc) { return new VVector2(xOrFunc instanceof Function ? xOrFunc(s.x) : xOrFunc, s.y); };
	s.NewY = function(yOrFunc) { return new VVector2(s.x, yOrFunc instanceof Function ? yOrFunc(s.y) : yOrFunc); };
}
VVector2._AddGetter_Inline = function zero() { return new VVector2(0, 0); };
VVector2._AddGetter_Inline = function one() { return new VVector2(1, 1); };

function Vector3i(/*o:*/ x, y, z) {
	var s = this;
	s.x = x != null ? x : 0;
	s.y = y != null ? y : 0;
	s.z = z != null ? z : 0;

	s.Deserialize = function(node) {
		var strParts = node.primitiveValue.split(" ");
		s.x = parseInt(strParts[0]);
		s.y = parseInt(strParts[1]);
		s.z = parseInt(strParts[2]);
	}.AddTags(new VDFDeserialize());
	//s.VDFSerialize = function() { return s.toString(); }; //Swapped().toString(); };
	s.Serialize = function() { return new VDFNode(s.toString()); }.AddTags(new VDFSerialize());
	s.toString = function() { return s.x + " " + s.y + " " + s.z; };

	s.NewX = function(xOrFunc) { return new Vector3i(xOrFunc instanceof Function ? xOrFunc(s.x) : xOrFunc, s.y, s.z); };
	s.NewY = function(yOrFunc) { return new Vector3i(s.x, yOrFunc instanceof Function ? yOrFunc(s.y) : yOrFunc, s.z); };
	s.NewZ = function(zOrFunc) { return new Vector3i(s.x, s.y, zOrFunc instanceof Function ? zOrFunc(s.z) : zOrFunc); };
}
Vector3i._AddGetter_Inline = function zero() { return new Vector3i(0, 0, 0); }
Vector3i._AddGetter_Inline = function one() { return new Vector3i(1, 1, 1); }

function VVector3(/*o:*/ x, y, z) {
	var s = this;
	s.x = x != null ? x : 0;
	s.y = y != null ? y : 0;
	s.z = z != null ? z : 0;

	s.Deserialize = function(node) {
		var strParts = node.primitiveValue.split(" ");
		s.x = parseInt(strParts[0]);
		s.y = parseInt(strParts[1]);
		s.z = parseInt(strParts[2]);
		//s.Swap();
	}.AddTags(new VDFDeserialize());
	//s.VDFSerialize = function() { return s.toString(); }; //Swapped().toString(); };
	s.Serialize = function() { return new VDFNode(s.toString()); }.AddTags(new VDFSerialize());
	s.toString = function() { return s.x + " " + s.y + " " + s.z; };

	s.NewX = function(xOrFunc) { return new VVector3(xOrFunc instanceof Function ? xOrFunc(s.x) : xOrFunc, s.y, s.z); };
	s.NewY = function(yOrFunc) { return new VVector3(s.x, yOrFunc instanceof Function ? yOrFunc(s.y) : yOrFunc, s.z); };
	s.NewZ = function(zOrFunc) { return new VVector3(s.x, s.y, zOrFunc instanceof Function ? zOrFunc(s.z) : zOrFunc); };

	s.Minus = function(arg1, arg2, arg3) {
		if (arg1 instanceof VVector3)
			return new VVector3(s.x - arg1.x, s.y - arg1.y, s.z - arg1.z);
		return new VVector3(s.x - arg1, s.y - arg2, s.z - arg3);
	};
	s.Plus = function(arg1, arg2, arg3) {
		if (arg1 instanceof VVector3)
			return new VVector3(s.x + arg1.x, s.y + arg1.y, s.z + arg1.z);
		return new VVector3(s.x + arg1, s.y + arg2, s.z + arg3);
	};
}
VVector3._AddGetter_Inline = function zero() { return new VVector3(0, 0, 0); };
VVector3._AddGetter_Inline = function one() { return new VVector3(1, 1, 1); };

function VRect(x, y, width, height) {
	var s = this;
	s.x = x != null ? x : 0;
	s.y = y != null ? y : 0;
	s.width = width != null ? width : 0;
	s.height = height != null ? height : 0;

	s._AddGetter_Inline = function left() { return s.x; };
	s._AddGetter_Inline = function right() { return s.x + s.width; };
	s._AddGetter_Inline = function top() { return s.y + s.height; };
	s._AddGetter_Inline = function bottom() { return s.y; };

	s.Deserialize = function(node) {
		var strParts = node.primitiveValue.split(" ");
		s.x = parseInt(strParts[0]);
		s.y = parseInt(strParts[1]);
		s.width = parseInt(strParts[2]);
		s.height = parseInt(strParts[3]);
	}.AddTags(new VDFDeserialize());
	s.Serialize = function() { return new VDFNode(s.toString()); }.AddTags(new VDFSerialize());
	s.toString = function() { return s.x + " " + s.y + " " + s.width + " " + s.height; };

	s.Equals = function(other) {
		if (!(other instanceof VRect))
			return false;
		return s.toString() == other.toString();
	};

	s.NewX = function(xOrFunc) { return new VVector3(xOrFunc instanceof Function ? xOrFunc(s.x) : xOrFunc, s.y, s.z); };
	s.NewY = function(yOrFunc) { return new VVector3(s.x, yOrFunc instanceof Function ? yOrFunc(s.y) : yOrFunc, s.z); };
	s.NewWidth = function(widthOrFunc) { return new VVector3(s.x, s.y, widthOrFunc instanceof Function ? widthOrFunc(s.width) : widthOrFunc, s.height); };
	s.NewHeight = function(heightOrFunc) { return new VVector3(s.x, s.y, s.width, heightOrFunc instanceof Function ? heightOrFunc(s.height) : heightOrFunc); };
	s.Grow = function(amountOnEachSide) { return new VRect(x - amountOnEachSide, y - amountOnEachSide, width + (amountOnEachSide * 2), height + (amountOnEachSide * 2)); };
	s.Encapsulating = function(rect) {
		var posX = Math.min(s.x, rect.x);
		var posY = Math.min(s.y, rect.y);
		return new VRect(posX, posY, Math.max(s.x + s.width, rect.x + rect.width) - posX, Math.max(s.y + s.height, rect.y + rect.height) - posY);
	};
}

function VBounds(position, size) {
	var s = this;
	s.position = position;
	s.size = size;

	s.Deserialize = function(node) {
		var parts = node.primitiveValue.split("|");
		var posParts = parts[0].split(' ');
		s.position = new VVector3(parseFloat(posParts[0]), parseFloat(posParts[1]), parseFloat(posParts[2]));
		var sizeParts = parts[1].split(' ');
		s.size = new VVector3(parseFloat(sizeParts[0]), parseFloat(sizeParts[1]), parseFloat(sizeParts[2]));
	}.AddTags(new VDFDeserialize());
	s.Serialize = function() { return new VDFNode(s.toString()); }.AddTags(new VDFSerialize());
	s.toString = function() { return s.position.x + " " + s.position.y + " " + s.position.z + "|" + s.size.x + " " + s.size.y + " " + s.size.z; };
}

function VColor(red, green, blue, /*o:*/ alpha) {
	var s = this;
	s.red = red;
	s.green = green;
	s.blue = blue;
	s.alpha = alpha != null ? alpha : 1;

	s.Deserialize = function(node) {
		var strParts = node.primitiveValue.split(" ");
		s.red = parseFloat(strParts[0]);
		s.green = parseFloat(strParts[1]);
		s.blue = parseFloat(strParts[2]);
		s.alpha = strParts.length >= 4 ? parseFloat(strParts[3]) : 1;
	}.AddTags(new VDFDeserialize());
	s.Serialize = function() { return new VDFNode(s.toString()); }.AddTags(new VDFSerialize());
	s.toString = function() { return s.red + " " + s.green + " " + s.blue + (s.alpha != 1 ? " " + s.alpha : ""); };

	VColor.FromTinyColor = function(tinyColor) {
		/*var rgb = tinyColor.toRgb();
		return new VColor(rgb.r / 255, rgb.g / 255, rgb.b / 255, rgb.a);*/
		return new VColor(tinyColor._r / 255, tinyColor._g / 255, tinyColor._b / 255, tinyColor._a);
	};
	VColor.FromHex = function(hex, /*o:*/ alpha) {
		var strParts = hex.replace("#", "").match(/(.{2})/g);
		return new VColor(parseInt(strParts[0], 16) / 255, parseInt(strParts[1], 16) / 255, parseInt(strParts[2], 16) / 255, alpha != null ? alpha : 1);
	};
	s.ToHexStr = function() { return "#" + ("0" + (s.red * 255).toString(16)).slice(-2) + ("0" + (s.green * 255).toString(16)).slice(-2) + ("0" + (s.blue * 255).toString(16)).slice(-2); }
	//s.ToRGBString = function() { return s.alpha != 1 ? ("rgba(" + (s.red * 255) + "," + (s.green * 255) + "," + (s.blue * 255) + "," + s.alpha + ")") : ("rgb(" + (s.red * 255) + "," + (s.green * 255) + "," + (s.blue * 255) + ")"); }
	s.ToRGBAString = function() { return "rgba(" + (s.red * 255) + "," + (s.green * 255) + "," + (s.blue * 255) + "," + s.alpha + ")"; }

	s.Equals = function(other) {
		if (!(other instanceof VColor))
			return false;
		return s.toString() == other.toString();
	};
}

/*function HexToRGB(hex, /*o:*#/ alpha)
{
    var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    var toString = function()
    {
        if (this.alpha == undefined)
            return "rgb(" + this.r + ", " + this.g + ", " + this.b + ")";
        if (this.alpha > 1)
            this.alpha = 1;
        else if (this.alpha < 0)
            this.alpha = 0;
        return "rgba(" + this.r + ", " + this.g + ", " + this.b + ", " + this.alpha + ")";
    } 
    if (alpha == undefined)
        return result ? {r: parseInt(result[1], 16), g: parseInt(result[2], 16), b: parseInt(result[3], 16), toString: toString} : null;
    if (alpha > 1)
        alpha = 1;
    else if (alpha < 0)
        alpha = 0;
    return result ? {r: parseInt(result[1], 16), g: parseInt(result[2], 16), b: parseInt(result[3], 16), alpha: alpha, toString: toString} : null;
}
// expects a string like "rgba(255, 255, 0, 0.25)" (though will ignore the alpha layer)
function RGBToHex(r_orRGBStr_orRGBObj, g, b)
{
	if (g == undefined || b == undefined)
	{
		var rgbStr_orRGBObj = r_orRGBStr;
		if (typeof rgbStr_orRGBObj == "string")
		{
			var result = /^rgb[a]?\(([\d]+)[ \n]*,[ \n]*([\d]+)[ \n]*,[ \n]*([\d]+)[ \n]*,?[ \n]*([.\d]+)?[ \n]*\)$/i.exec(rgbStr_orRGBObj);
            return RGBToHex(parseInt(result[1]), parseInt(result[2]), parseInt(result[3]));
		}
		var rgbObj = rgbStr_orRGBObj;
		if (rgbObj.r == undefined || rgbObj.g == undefined || rgbObj.b == undefined)
            return null;
		return RGBToHex(rgbObj.r, rgbObj.g, rgbObj.b);
    }
    var r = rgb;
    function componentToHex(c)
    {
        var hex = c.toString(16);
        return hex.length == 1 ? "0" + hex : hex;
    }
    return "#" + componentToHex(r) + componentToHex(g) + componentToHex(b);
}*/

function Random(seed) {
	seed = seed != null ? seed : new Date().getTime();

	if (seed == 0)
		throw new Error("Cannot use 0 as seed. (prng algorithm isn't very good, and doesn't work with seeds that are multiples of PI)");

	var s = this;
	s.seed = seed;
	s.NextDouble = function() {
		s.seed = Math.sin(s.seed) * 10000; return s.seed - Math.floor(s.seed);
	};
	s.NextColor = function() { return new VColor(s.NextDouble(), s.NextDouble(), s.NextDouble()); };
	s.NextColor_ImageStr = function() {
		var color = s.NextColor();
		Random.canvasContext.fillStyle = color.ToHexStr();
		Random.canvasContext.fillRect(0, 0, Random.canvas[0].width, Random.canvas[0].height);
		var imageStr = Random.canvas[0].toDataURL();
		return imageStr.substr(imageStr.indexOf(",") + 1);
	};
}
//VO.extraMethod = function NotifyInitializedInJS() {
Random.PostUIInit = function() {
	Random.canvas = $("<canvas width='1' height='1'>").appendTo("#hiddenPersistentHolder");
	Random.canvasContext = Random.canvas[0].getContext("2d");
};

// React components
// ==========

var React = require("react");
var ReactDOM = require("react-dom");
BaseComponent = class BaseComponent extends React.Component {
	/*constructor(args) {
	    super(args);
	}*/
	_bind(...methods) {
		methods.forEach(method=>this[method] = this[method].bind(this));
	}
	rootElement() { return $(ReactDOM.findDOMNode(this)); }
}
//var BaseComponent = BaseComponent;