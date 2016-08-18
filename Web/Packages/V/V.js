V = new function() {
    var s = this;

	/*function AddClosureFunctionsToX(newHolder) {
		var names = arguments.callee.caller.toString().Matches(/function\s*([\w\d]+)\s*\(/g);
		for (var i in names)
			try { newHolder[names[i]] = eval(names[i]); } catch(e) {}
	}
	AddClosureFunctionsToX(self);*/

	//s.Break = function() { debugger; };

    s.Nothing = function () {};
    var constructorHelper = function () {};
    s.CreateClass = function(baseClass, classMembers)     {
        baseClass = baseClass || Object;

        var result;

        if (classMembers && classMembers.hasOwnProperty("constructor"))
            result = classMembers.constructor;
        else
            result = function () { return baseClass.apply(this, arguments); };

        constructorHelper.prototype = baseClass.prototype;
        result.prototype = new constructorHelper();

        if (classMembers)
            result.prototype.Extend(classMembers);

        result.prototype.constructor = result;
        result.__super__ = baseClass.prototype;

        return result;
    };

	s.CreateEnum = function(enumTypeName, enumNames) {
		//var type = function(name, value)
		// for now at least, auto-add enum as global, since enums are types and VDF system needs types to be global
		var type = window[enumTypeName] = function(name, value)
		{
			var s = this;
			//s.realTypeName = enumTypeName; // old: maybe temp; makes-so VDF system recognizes enumValues as of this enumType
			s.name = name;
			s.value = value;

			s.Serialize = function() { return new VDFNode(s.name, enumTypeName); }.AddTags(new VDFSerialize());
			s.toString = function() { return s.name; };
			//s.valueOf = function() { return s.value; }; // currently removed, since overrides toString for to-primitive use, thus disabling the "player.age == 'Rodent'" functionality
		};
		type.Deserialize = function(node) { return type[node.primitiveValue]; }.AddTags(new VDFDeserialize());
		//type.name = enumTypeName;
		//type.realTypeName = enumTypeName;
		type._IsEnum = 0; // mimic odd enum marker/flag, used by TypeScript

		var values = [];
		if (enumNames instanceof Array)
			for (var i in enumNames)
				values.push(type[enumNames[i]] = new type(enumNames[i], i));
		else
			for (var enumName in enumNames)
				values.push(type[enumName] = new type(enumName, enumNames[enumName].value != null ? enumNames[enumName].value : enumNames.VKeys().indexOf(enumName)));
		type.names = enumNames;
		type.values = values;

		return type;
	};

    s.AnimateSize = function (control, newWidth, newHeight, newMinWidth, newMinHeight, fadeTime)
	{
		var newLeft = parseInt(control.offset().left - ((newWidth - control.width()) / 2));
		if (newLeft < 0)
			newLeft = 0;
		else if (newLeft + newWidth > $(document).width())
			newLeft = $(document).width() - newWidth - (control.outerWidth() - control.width());
		var newTop = parseInt(control.offset().top - ((newHeight - control.height()) / 2));
		if (newTop < 0)
			newTop = 0;
		else if (newTop + newHeight > $(document).height())
			newTop = $(document).height() - newHeight - (control.outerHeight() - control.height());

		control.animate({ minWidth: newMinWidth, minHeight: newMinHeight, width: newWidth, height: newHeight, left: newLeft, top: newTop }, 250);
	};

	//s.CloneObject = function(obj) { return $.extend({}, obj); }; //deep: JSON.parse(JSON.stringify(obj));
	s.CloneObject = function(obj, /*o*/ propMatchFunc, depth)
	{
		depth = depth != null ? depth : 0;
		if (depth > 100)
			debugger;

		if (obj == null)
			return null;
		if (IsPrimitive(obj))
			return obj;
		//if (obj.GetType() == Array)
		if (obj.constructor == Array)
			return s.CloneArray(obj);
		if (obj.constructor == List)
			return List.apply(null, [obj.itemType].concat(s.CloneArray(obj)));

		var result = {};
		for (var propName in obj)
			if ((obj[propName] || {}).constructor != Function && (propMatchFunc == null || propMatchFunc.call(obj, propName, obj[propName])))
				result[propName] = V.CloneObject(obj[propName], propMatchFunc, depth + 1);
		return result;
	};
	s.CloneArray = function(array) { return Array.prototype.slice.call(array, 0); }; //array.slice(0); //deep: JSON.parse(JSON.stringify(array));
	s.IsEqual = function(a, b) {
		function _equals(a, b) { return JSON.stringify(a) === JSON.stringify($.extend(true, {}, a, b)); }
		return _equals(a, b) && _equals(b, a);
	};

	s.CallXAtDepthY = function(func, depth) {
		var currentCallPackage = function() { func(); };
		for (var i = 1; i < depth; i++)
			currentCallPackage = function() { currentCallPackage(); };
		currentCallPackage();
	};

	s.Average = function(numbers___) {
		var numbers = arguments;
		var total = 0;
		for (var i in numbers)
			total += numbers[i];
		return total / numbers.length;
	};

	s.FormatString = function(str /*params:*/) {
		var result = str;
		for (var i = 0; i < arguments.length - 1; i++) {
			var reg = new RegExp("\\{" + i + "\\}", "gm");
			result = result.replace(reg, arguments[i + 1]);
		}
		return result;
	};
	s.CapitalizeWordsInX = function(str, /*o:*/ addSpacesBetweenWords) {
		var result = str.replace(/(^|\W)(\w)/g, function(match) { return match.toUpperCase(); });
		var lowercaseWords = [ // words that are always lowercase (in titles)
			"a", "aboard", "about", "above", "across", "after", "against", "along", "alongside", "amid", "amidst", "among", "amongst", "an", "and", "around", "as", "aside", "astride", "at", "atop",
			"before", "behind", "below", "beneath", "beside", "besides", "between", "beyond", "but", "by", "despite", "during", "except",
			"for", "from", "given", "in", "inside", "into", "minus", "notwithstanding", "of", "off", "on", "onto", "opposite", "or", "out", "over",
			"per", "plus", "regarding", "sans", "since", "than", "through", "throughout", "till", "toward", "towards",
			"under", "underneath", "unlike", "until", "unto", "upon", "versus", "via", "with", "within", "without", "yet"
		];
		lowercaseWords.AddRange(["to"]); // words that are overwhelmingly lowercase
		result = result.replace(new RegExp("(\\s)(" + lowercaseWords.join("|") + ")(\\s|$)", "gi"), function(match) { return match.toLowerCase(); }); // case-insensitive, search-and-make-lowercase call
		if (addSpacesBetweenWords)
			result = result.replace(/(^|[a-z])([A-Z])/g, function(match, group1, group2) { return group1 + " " + group2; });
		return result;
	};
	s.ModifyFirstLetterOfEachWord = function(str, modifierFunc, /*o:*/ firstCharModifierFunc) {
		if (firstCharModifierFunc) {
			var part1 = str.substr(0, 1);
			part1 = firstCharModifierFunc(part1);
			var part2 = str.substr(1);
			part2 = part2.replace(/(?!\W|[a-z])([A-Z])/g, modifierFunc);
			return part1 + part2;
		}
		return str.replace(/(?!^|\W|[a-z])([A-Z])/g, modifierFunc);
	}
	// example:
	// var multilineText = V.Multiline(function() {/*
	//		Text that...
	//		spans multiple...
	//		lines.
	// */});
	//self.Multiline = function(functionWithInCommentMultiline) { return functionWithInCommentMultiline.toString().replace(/^[^\/]+\/\*!?/, '').replace(/\*\/[^\/]+$/, ''); };
	//self.Multiline = function(functionWithInCommentMultiline) { return functionWithInCommentMultiline.toString().replace(/^[^\/]+\/\*/, '').replace(/\*\/(.|\n)*/, ''); };
	s.Multiline = function(functionWithInCommentMultiline, useExtraPreprocessing) {
		useExtraPreprocessing = useExtraPreprocessing != null ? useExtraPreprocessing : true;

		var text = functionWithInCommentMultiline.toString().replace(/\r/g, "");

		// some extra preprocessing
		if (useExtraPreprocessing) {
			text = text.replace(/@@.*/g, ""); // remove single-line comments
			//text = text.replace(/@\**?\*@/g, ""); // remove multi-line comments
			text = text.replace(/@\*/g, "/*").replace(/\*@/g, "*/"); // fix multi-line comments
		}

		var firstCharPos = text.indexOf("\n", text.indexOf("/*")) + 1;
		return text.substring(firstCharPos, text.lastIndexOf("\n"));
	};

	s.StableSort = function(array, compare) { // needed for Chrome
		var array2 = array.map(function(obj, index) { return { index: index, obj: obj }; });
		array2.sort(function(a, b) {
			var r = compare(a.obj, b.obj);
			return r != 0 ? r : V.Compare(a.index, b.index);
		});
		return array2.map(function(pack) { return pack.obj; });
	};
	s.Compare = function(a, b, /*o:*/ caseSensitive) {
		caseSensitive = caseSensitive != null ? caseSensitive : true;
		if (!caseSensitive && typeof a == "string" && typeof b == "string") {
			a = a.toLowerCase();
			b = b.toLowerCase();
		}
		return a < b ? -1 : (a > b ? 1 : 0);
	};

	s.GetAbsolutePath = function(path) {
		var a = $("<a>").attr("href", path);
		return a[0].protocol + "//" + a[0].host + a[0].pathname + a[0].search + a[0].hash;
	};

	//self.GetScreenCenter = function() { return Frame.screenCenter.offset(); };
	/*s.GetScreenWidth = function() { return $("body").width(); }
	s.GetScreenHeight = function() { return $("body").height(); }*/
	s.GetScreenWidth = function() { return VO.root.width(); };
	s.GetScreenHeight = function() { return VO.root.height(); };

	s.GetContentSize = function(content) {
		/*var holder = $("#hiddenTempHolder");
		var contentClone = content.clone();
		holder.append(contentClone);
		var width = contentClone.outerWidth();
		var height = contentClone.outerHeight();
		contentClone.remove();*/

		var holder = $("<div id='hiddenTempHolder2' style='position: absolute; left: -1000; top: -1000; width: 1000; height: 1000; overflow: hidden;'>").appendTo("body");
		var contentClone = content.clone();
		holder.append(contentClone);
		var width = contentClone.outerWidth();
		var height = contentClone.outerHeight();
		holder.remove();

		return {width: width, height: height};
	};
	s.GetContentWidth = function(content) { return s.GetContentSize(content).width; };
	s.GetContentHeight = function(content) { return s.GetContentSize(content).height; };

	s.GetObjectsWithKey_AsMap = function(keyToFind, /*;optional:*/ rootObj, maxDepth, currentDepth)
	{
		rootObj = rootObj || window;
		maxDepth = maxDepth != null ? maxDepth : 10;
		currentDepth = currentDepth != null ? currentDepth : 0;

		var result = {};
		for (var key in rootObj)
			if (key == keyToFind)
				result[key] = "FOUND_HERE";
			else if (rootObj[key] instanceof Object && currentDepth < maxDepth && rootObj[key] != window)
			{
				var matchingDescendantMap = V.GetObjectsWithKey_AsMap(keyToFind, rootObj[key], maxDepth, currentDepth + 1);
				if (matchingDescendantMap)
					result[key] = matchingDescendantMap;
			}
		return result.VKeys().length ? result : null;
	};

	s.GetDescendants = function(rootObj, /*;optional:*/ matchFunc, keyMatchFunc, maxCount, maxDepth, currentDepth, parentObjects)
	{
		matchFunc = matchFunc || function(child) { return child instanceof Object; };
		keyMatchFunc = keyMatchFunc || function(child) { return true; };
		maxCount = maxCount || Number.MAX_VALUE;
		maxDepth = maxDepth != null ? maxDepth : Number.MAX_VALUE;
		currentDepth = currentDepth != null ? currentDepth : 0;
		parentObjects = parentObjects || [];

		var result = [];
		for (var key in rootObj)
		{
			var child = rootObj[key];
			if (!keyMatchFunc(key) || parentObjects.Contains(child)) // no loop-backs
				continue;

			if (matchFunc(child) && result.length < maxCount)
				result.push(child);
			if (result.length < maxCount && child != rootObj && currentDepth < maxDepth)
			{
				var matchingDescendants = V.GetDescendants(child, matchFunc, keyMatchFunc, maxCount, maxDepth, currentDepth + 1, parentObjects.concat([child]));
				for (var i in matchingDescendants)
					if (result.length < maxCount)
						result.push(matchingDescendants[i]);
			}
		}
		return result;
	};

	s.ExtendWith = function(value) { $.extend(s, value); };

    var hScrollBarHeight;
    s.GetHScrollBarHeight = function()
    {
	    if (!hScrollBarHeight)
	    {
		    var outer = $("<div style='visibility: hidden; position: absolute; left: -100; top: -100; height: 100; overflow: scroll;'/>").appendTo('body');
		    var heightWithScroll = $("<div>").css({height: "100%"}).appendTo(outer).outerHeight();
		    outer.remove();
		    hScrollBarHeight = 100 - heightWithScroll;
		    //hScrollBarHeight = outer.children().height() - outer.children()[0].clientHeight;
	    }
	    return hScrollBarHeight;
    }
    var vScrollBarWidth;
    s.GetVScrollBarWidth = function()
    {
	    if (!vScrollBarWidth)
	    {
		    var outer = $("<div style='visibility: hidden; position: absolute; left: -100; top: -100; width: 100; overflow: scroll;'/>").appendTo('body');
		    var widthWithScroll = $("<div>").css({width: "100%"}).appendTo(outer).outerWidth();
		    outer.remove();
		    vScrollBarWidth = 100 - widthWithScroll;
		    //vScrollBarWidth = outer.children().width() - outer.children()[0].clientWidth + 1;
	    }
	    return vScrollBarWidth;
    }
    s.HasScrollBar = function(control) { return HasVScrollBar(control) || HasHScrollBar(control); }
    s.HasVScrollBar = function(control) { return control[0].scrollHeight > control[0].clientHeight; }
    s.HasHScrollBar = function(control) { return control[0].scrollWidth > control[0].clientWidth; }

    s.transparentImageString = "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7";
    s.transparentImageString_full = "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7";
	s.whiteImageString = "R0lGODlhAQABAIAAAP7//wAAACH5BAAAAAAALAAAAAABAAEAAAICRAEAOw==";

    s.ShowMessageBox_Simple = function(title, message) { VMessageBox.ShowMessageBox({ title: title, message: message }); };

    s.AddXToItselfYTimes = function(x, y)
    {
        var result = x;
        for (var i = 0; i < y; i++)
            result += x;
        return result;
    };

	s.CreateGUID = function()
	{
		"xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function(ch)
		{
			var rand = Math.random() * 16 | 0;
			return (ch == 'x' ? rand : (rand & 0x3 | 0x8)).toString(16);
		});
	};

	/*s.SetWood = function(wood) { R("#wood").html(wood.toString().substring(0, wood.toString().indexOf(".") != -1 ? wood.toString().indexOf(".") + 2 : wood.toString().length)); }*/

	s.AsArray = function(args) { return s.Slice(args, 0); };
	//s.ToArray = function(args) { return s.Slice(args, 0); };
	s.Slice = function(args, start, end) { return Array.prototype.slice.call(args, start != null ? start : 0, end); };

	/*s.GetScreenRect = function(control)
	{
		var left = viewport.offset().left / V.GetScreenWidth();
		var top = viewport.offset().top / V.GetScreenHeight();
		var width = viewport.width() / V.GetScreenWidth();
		var height = viewport.height() / V.GetScreenHeight();
		return {x: left, y: (V.GetScreenHeight() - 1) - top, width: width, height: height};
	};*/

	s.GetElementDescendants = function(element, /*o:*/ includeSelf) {
		var result = [];
		if (includeSelf)
			result.Add(element);
		var children = element.children().toArray();
		for (var i in children)
			result.AddRange(s.GetElementDescendants($(children[i]), true));
		return result;
	};

	s.IsTypeABaseOfOrSameAsB = function(typeA, typeB) {
		/*var typeAName = typeA && typeA.name;
		var typeBName = typeB && typeB.name;
		if (typeAName == "Property" && typeBName != null) // todo: add more sophisticated/correct-in-all-cases version of this
			return true;
		if (typeAName == "object" && typeBName != null)
			return true;
		if (typeAName == "IList" && typeBName && typeBName.startsWith("List("))
			return true;
		if (typeAName == typeBName)
			return true;
		return false;*/
		typeA = typeA instanceof Type ? typeA : GetType(typeA);
		typeB = typeB instanceof Type ? typeB : GetType(typeB);
		return typeB && typeB.IsDerivedFrom(typeA);
	};
	s.DoesTypeListContainMatchFor = function(typeList, type) {
		for (var i in typeList)
			if (s.IsTypeABaseOfOrSameAsB(typeList[i], type))
				return true;
		return false;
	};

	s.GetStackTraceStr = function() {
		var result = new Error().stack;
		return result.substr(result.IndexOf_X(1, "\n")); // remove "Error" line and first stack-frame (that of this method)
	};
	s.LogStackTrace = function() { Log(s.GetStackTraceStr()); };
};