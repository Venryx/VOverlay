var VDebug = new function()
{
    var s = this;

	/*this.LinesToString = function(lines, maxLines, linePrepend)
	{
		maxLines = (maxLines && maxLines < lines.length ? maxLines : lines.length);
		var result = "";
		for (var i = 0; i < maxLines; i++)
			result += (result.length > 0 ? "\n" : "") + linePrepend + lines[i];
		if (lines.length > maxLines)
			result += " [...]"; //\n" + linePrepend + "[...]";
		return result;
	}
	// used to log the 'current-function' and 'value' for a variable assignment (just add "LogMe().log = " before each assignment expression, like so: "var a = LogMe().log = 25;")
	Object.defineProperty(Array.prototype, "log", { set: this.valueSetFunc });
	this.valueSetFunc = function(val)
	{
		if (val === undefined)
			val = "[undefined]";
		else if (val === null)
			val = "[null]";
		if (logSetVal)
		{
			var message = messagePrepend + "SETVAL) " + LinesToString(val.toString().split(/[\r\n]+/gm), callInfoMaxLines, linePrepend).replace(new RegExp("^" + linePrepend), ""); //me.toString();
			if (message.split("\n").length < 2) // if less than 2 lines
				message = message + "\n";
			message = message.replace(/\r/g, ""); // in Unity, [\r] chars will break parser/logger
			Log(message);
		}
	}

	var callStackDepth = 5;
	var callInfoMaxLines = 5;
	var messagePrepend = "#";
	var linePrepend = "               ";
	var logInFunc = true;
	var logCaller = true; // tries to calculate the caller-line; can be mistaken
	var logSetVal = true;

	var lastLogMeCallerFunc;
	var logsWithSameLogMeCallerFunc = 1;
	window.LogMe = function()
	{
		var callerFunc = arguments.callee.caller;
		if (callerFunc)
		{
			if (callerFunc != lastLogMeCallerFunc) // if callerFunc is different from last one
			{
				lastLogMeCallerFunc = callerFunc;
				logsWithSameLogMeCallerFunc = 0;
			}
			logsWithSameLogMeCallerFunc++;

			if (logInFunc && logsWithSameLogMeCallerFunc == 1)
			{
				var stack = "";
				var current = callerFunc;
				for (var i = 0; i < callStackDepth && current != null; i++)
				{
					stack += (stack.length ? "\n" + linePrepend + "------------------\n" : "") + LinesToString(current.toString().split(/[\r\n]+/gm), callInfoMaxLines, linePrepend);
					if (i == 0) // if we're the first block in stack, remove line-prepend of our first-line
						stack = stack.replace(new RegExp("^" + linePrepend), "");
					if (current.toString().indexOf("Unity.invoke") != -1)
						break; // don't go any further back, as this will crash Unity
					current = current.caller;
				}
				//if (i == callStackDepth)
				//	stack += "\n" + linePrepend + "[...]";

				var message = messagePrepend + "INFUNC) " + stack;
				if (message.split("\n").length < 2) // if less than 2 lines
					message = message + "\n";
				message = message.replace(/\r/g, ""); // in Unity, [\r] chars will break parser/logger
				Log(message);
			}
			if (logCaller)
			{
				var callPos = -1;
				for (var i = 0; i < logsWithSameLogMeCallerFunc; i++)
					callPos += (callerFunc.toString().substring(callPos + 1).match(/LogMe\(\)/) || {}).index + 1;

				var lengthOfPassedLines = 0;
				var funcLines = callerFunc.toString().split(/[\r\n]+/gm);
				for (var i = 0; i < funcLines.length && lengthOfPassedLines + funcLines[i].length < callPos; i++) // while we haven't passed up callPos
					lengthOfPassedLines += funcLines[i].length;

				var callerLine = funcLines[i];
				if (!callerLine.contains("LogMe()"))
					callerLine = "[invalid]"; // probably caused by successive calls to same function, from outside log-me-marked code

				var message = messagePrepend + "CALLER) " + callerLine;
				if (message.split("\n").length < 2) // if less than 2 lines
					message = message + "\n";
				message = message.replace(/\r/g, ""); // in Unity, [\r] chars will break parser/logger
				Log(message);
			}
		}
		//else // must have been called from a global/window context
		//	Log("[no caller func]");

		return [];
	}*/

	var timerStartTime = 0;
	s.StartTime = function()
	{
		timerStartTime = new Date().getTime();
		Log("Starting timer.");
	};
	s.MarkTime = function( /*;optional*/ postpend)
	{
		postpend = postpend || "";
		Log("Mark: " + (new Date().getTime() - timerStartTime) + postpend);
	};

	s.LogStackTrace = function() { Log("LogStackTrace) " + new Error().stack); };

	s.AddErrorHandlingToMethodsIn = function(obj)
	{
		for (var key in obj)
			if (obj[key] instanceof Function) //&& Object.keys(obj[key]).length == 0) //&& obj[key].toString().Contains("VO"))
			{
				var oldMethod = obj[key];
				var newMethod = function()
				{
					try { return arguments.callee.originalMethod.apply(this, arguments); }
					catch(error) { HandleError(error); }
				};
				newMethod.originalMethod = oldMethod;
				newMethod.prototype = oldMethod.prototype;
				//newMethod.__proto__ = oldMethod.__proto__;
				for (var key2 in oldMethod)
					newMethod[key2] = oldMethod[key2];
				obj[key] = newMethod;
			}
	};
};