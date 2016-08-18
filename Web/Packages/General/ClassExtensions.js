// Object: base
// ==================

// the below lets you do stuff like this: Array.prototype._AddFunction(function AddX(value) { this.push(value); }); []._AddX("newItem");
Object.defineProperty(Object.prototype, "_AddItem", { // note; these functions should by default add non-enumerable properties/items
	//configurable: true,
	enumerable: false,
	value: function(name, value, forceAdd) {
		if (this[name])
			delete this[name];
		if (!this[name] || forceAdd) // workaround for some properties not being deleted
			Object.defineProperty(this, name, {
				enumerable: false,
				value: value
			});
	}
});
Object.prototype._AddItem("_AddFunction", function(name, func) {
	//this._AddItem(func.name || func.toString().match(/^function\s*([^\s(]+)/)[1], func);
	this._AddItem(name, func);
});

// the below lets you do stuff like this: Array.prototype._AddGetterSetter("AddX", null, function(value) { this.push(value); }); [].AddX = "newItem";
Object.prototype._AddFunction("_AddGetterSetter", function(name, getter, setter) {
	//var name = (getter || setter).name || (getter || setter).toString().match(/^function\s*([^\s(]+)/)[1];
	if (this[name])
		delete this[name];
	if (!this[name]) // workaround for some properties not being deleted
		if (getter && setter)
			Object.defineProperty(this, name, {enumerable: false, get: getter, set: setter});
		else if (getter)
			Object.defineProperty(this, name, {enumerable: false, get: getter});
		else
			Object.defineProperty(this, name, {enumerable: false, set: setter});
});

// the below lets you do stuff like this: Array.prototype._AddFunction_Inline = function AddX(value) { this.push(value); }; [].AddX = "newItem";
Object.prototype._AddGetterSetter("_AddFunction_Inline", null, function(func) { this._AddFunction(func.name, func); }); // maybe make-so: these use func.GetName()
Object.prototype._AddGetterSetter("_AddGetter_Inline", null, function(func) { this._AddGetterSetter(func.name, func, null); });
Object.prototype._AddGetterSetter("_AddSetter_Inline", null, function(func) { this._AddGetterSetter(func.name, null, func); });

// alias for _AddFunction_Inline, since now we need to add functions to the "window" object relatively often
Object.prototype._AddGetterSetter("AddFunc", null, function(func) { this._AddFunction(func.name, func); });

// Function (early)
// ==========

//Function.prototype._AddFunction_Inline = function GetName() { return this.name || this.name_fake || this.toString().match(/^function\s*([^\s(]+)/)[1]; };
Function.prototype._AddFunction_Inline = function GetName() { return this.name_fake || this.name || this.toString().match(/^function\s*([^\s(]+)/)[1]; };
Function.prototype._AddFunction_Inline = function SetName(name) { this.name_fake = name; return this; };
// probably make-so: SetName_Temp function exists
//Function.prototype._AddFunction_Inline = function Call_Silent(self) { this.apply(self, V.Slice(arguments, 1)); return this; }
//Function.prototype._AddFunction_Inline = function Call_Silent() { this.apply(this, arguments); return this; }

// Object: C# polyfills/emulators
// ==================

/*Object.prototype._AddGetterSetter("AddMethod", null, function(func) { // for steamlined prototype-method-adding, that doesn't overwrite the method if it already exists (maybe just for use in this project)
	if (this.prototype[func.GetName()] == null)
		this._AddFunction(func.GetName(), func);
});*/
Object.prototype._AddSetter_Inline = function AddMethod(func) { // for steamlined prototype-method-adding, that doesn't overwrite the method if it already exists (maybe just for use in this project)
	if (this[func.GetName()] == null)
		this._AddFunction(func.GetName(), func);
};
// maybe temp; shorthand version (i.e.: p.method = function MethodName() {};)
/*Object.prototype._AddSetter_Inline = function method(func) //Method, add, Add,
{
	if (this[func.GetName()] == null)
		this._AddFunction(func.GetName(), func);
};*/

Object.prototype._AddFunction_Inline = function SetBaseClass(baseClassFunc) {
	this.prototype.__proto__ = baseClassFunc.prototype; // makes "(new ThisClass()) instanceof BaseClass" be true
	//self.constructor = List; // makes "(new List()).constructor == List" be true

	var name = this.GetName();
	if (name != "")
		// this only runs on class constructor functions, so if function has name (i.e. name sucked in for self-knowledge purposes), create a variable by that name for global access
		window[name] = this;
};
Object.prototype._AddSetter_Inline = function SetAsBaseClassFor(derivedClassFunc) {
	derivedClassFunc.SetBaseClass(this);
	//window[derivedClassFunc.GetName()] = derivedClassFunc;
};
Object.prototype._AddFunction_Inline = function CallBaseConstructor(constructorArgs___) {
	//return this.prototype.__proto__.apply(this, V.AsArray(arguments));
	//this.__proto__.__proto__.constructor.apply(this, V.AsArray(arguments));
	arguments.callee.caller.prototype.__proto__.constructor.apply(this, V.AsArray(arguments));
	return this;
};

// probably temp; helper so 'p' function is usable on objects that aren't Node's (e.g. to declare property types)
Object.prototype._AddFunction_Inline = function AddHelpers(obj) {
	this.p = Node_p;
	return this;
};

Object.prototype._AddFunction_Inline = function GetVDFTypeInfo() { return VDFTypeInfo.Get(this.GetTypeName()); };

//Object.prototype._AddFunction_Inline = function GetType() { return this.constructor; };
Object.prototype._AddFunction_Inline = function GetTypeName(/*o:*/ vdfTypeName) { //, simplifyForVScriptSystem)
	vdfTypeName = vdfTypeName != null ? vdfTypeName : true;

	/*var result = this.constructor.name;
	if (allowProcessing) 	{
		if (result == "String")
			result = "string";
		else if (result == "Boolean")
			result = "bool";
		else if (result == "Number")
			result = this.toString().contains(".") ? "double" : "int";
	}
	return result;*/


	/*var result = vdfTypeName ? VDF.GetTypeNameOfObject(this) : this.constructor.name;
	//if (simplifyForVScriptSystem)
	//	result = SimplifyTypeName(result);
	return result;*/
	if (vdfTypeName) {
		/*if (this instanceof Multi)
			return "Multi(" + this.itemType + ")";*/
		return VDF.GetTypeNameOfObject(this);
	}
	return this.constructor.name;
};
Object.prototype._AddFunction_Inline = function GetType(/*o:*/ simplifyForVScriptSystem) {
	var result = window.GetType(this.GetTypeName());
	if (simplifyForVScriptSystem)
		result = SimplifyType(result);
	return result;
};
// probably temp
/*function SimplifyTypeName(typeName) {
	var result = typeName;
	if (result.startsWith("List("))
		result = "IList";
	return result;
}*/
function SimplifyType(type) {
	if (type.name.startsWith("List("))
		return GetType("IList");
	if (type.name.startsWith("Dictionary("))
		return GetType("IDictionary");
	return type;
}

// Object: normal
// ==================

//Object.prototype._AddSetter_Inline = function ExtendWith_Inline(value) { this.ExtendWith(value); };
//Object.prototype._AddFunction_Inline = function ExtendWith(value) { $.extend(this, value); };
/*Object.prototype._AddFunction_Inline = function GetItem_SetToXIfNull(itemName, /*;optional:*#/ defaultValue) {
	if (!this[itemName])
		this[itemName] = defaultValue;
	return this[itemName];
};*/
//Object.prototype._AddFunction_Inline = function CopyXChildrenAsOwn(x) { $.extend(this, x); };
//Object.prototype._AddFunction_Inline = function CopyXChildrenToClone(x) { return $.extend($.extend({}, this), x); };

Object.prototype._AddFunction_Inline = function Extend(x) {
	for (var name in x) {
		var value = x[name];
		//if (value !== undefined)
        this[name] = value;
    }
	return this;
};

// as replacement for C#'s 'new MyClass() {prop = true}'
Object.prototype._AddFunction_Inline = function Init(x) { return this.Extend(x); };
Object.prototype._AddFunction_Inline = function Init_VTree(x) { // by default, uses set_self method
    for (var name in x)
    	this.a(name).set_self = x[name];
	return this;
};

Object.prototype._AddFunction_Inline = function Set_Normal(x) { return this.Extend(x); };
Object.prototype._AddFunction_Inline = function Set_VTree(x) { return this.Init_VTree(x); };

Object.prototype._AddFunction_Inline = function Extended(x) {
	var result = {};
	for (var name in this)
		result[name] = this[name];
	if (x)
    	for (var name in x)
    		result[name] = x[name];
    return result;
};

/*Object.prototype._AddFunction_Inline = function Keys() {
	var result = [];
	for (var key in this)
		if (this.hasOwnProperty(key))
			result.push(key);
	return result;
};*/
//Object.prototype._AddFunction_Inline = function Keys() { return Object.keys(this); }; // 'Keys' is already used for Dictionary prop
//Object.prototype._AddGetter_Inline = function VKeys() { return Object.keys(this); }; // 'Keys' is already used for Dictionary prop
Object.prototype._AddFunction_Inline = function VKeys() { return Object.keys(this); }; // 'Keys' is already used for Dictionary prop
Object.prototype._AddGetter_Inline = function Props() {
	var result = [];
	var i = 0;
	for (var propName in this)
		result.push({name: propName, value: this[propName], index: i++});
	return result;
}; // like Pairs for Dictionary, except for Object
/*Object.defineProperty(Object.prototype, "Keys", {
	enumerable: false,
	configurable: true,
	get: function() { return Object.keys(this); }
	//get: Object.keys
});*/
/*Object.prototype._AddFunction_Inline = function Items() {
	var result = [];
	for (var key in this)
		if (this.hasOwnProperty(key))
			result.push(this[key]);
	return result;
};*/
//Object.prototype._AddFunction_Inline = function ToJson() { return JSON.stringify(this); };

Object.prototype._AddFunction_Inline = function AddProp(name, value) {
	this[name] = value;
	return this;
};

/*Object.prototype._AddFunction_Inline = function GetVSData(context) {
	this[name] = value;
	return this;
};*/

Object.prototype._AddFunction_Inline = function VAct(action) {
	action.call(this);
	return this;
};

// Function
// ==========

Function.prototype._AddFunction_Inline = function AddTag(tag) {
	if (this.tags == null)
		this.tags = [];
	this.tags.push(tag);
	return this;
};
/*Function.prototype._AddFunction_Inline = function AddTags(/*o:*#/ tags___) { // (already implemented in VDF.js file)
	if (this.tags == null)
		this.tags = [];
	for (var i in arguments)
		this.tags.push(arguments[i]);
	return this;
};*/
/*function AddTags() {
	var tags = V.Slice(arguments, 0, arguments.length - 1);
	var func = V.Slice(arguments).Last();
	func.AddTags.apply(func, tags);
};*/
Function.prototype._AddFunction_Inline = function GetTags(/*o:*/ type) { return (this.tags || []).Where(function() { return type == null || this instanceof type; }); };

Function.prototype._AddFunction_Inline = function AsStr(args___) { return V.Multiline.apply(null, [this].concat(V.AsArray(arguments))); };

Function.prototype._AddFunction_Inline = function RunThenReturn(args___) { this.apply(null, arguments); return this; };

// Number
// ==========

//Number.prototype._AddFunction_Inline = function RoundToMultipleOf(step) { return Math.round(new Number(this) / step) * step; }; //return this.lastIndexOf(str, 0) === 0; };
Number.prototype._AddFunction_Inline = function RoundToMultipleOf(step) {
	var integeredAndRounded = Math.round(new Number(this) / step);
	var result = (integeredAndRounded * step).toFixed(step.toString().TrimStart("0").length); // - 1);
	if (result.contains("."))
		result = result.TrimEnd("0").TrimEnd(".");
	return result;
};

Number.prototype._AddFunction_Inline = function KeepAtLeast(step) {
	return Math.max(min, this);
};
Number.prototype._AddFunction_Inline = function KeepAtMost(step) {
	return Math.min(max, this);
};
Number.prototype._AddFunction_Inline = function KeepBetween(min, max) {
	return Math.min(max, Math.max(min, this));
};

// String
// ==========

String.prototype._AddFunction_Inline = function TrimEnd(chars___) {
	var chars = V.Slice(arguments);

	var result = "";
	var doneTrimming = false;
	for (var i = this.length - 1; i >= 0; i--)
		if (!chars.Contains(this[i]) || doneTrimming) {
			result = this[i] + result;
			doneTrimming = true;
		}
	return result;
};

String.prototype._AddFunction_Inline = function startsWith(str) { return this.indexOf(str) == 0; }; //return this.lastIndexOf(str, 0) === 0; };
String.prototype._AddFunction_Inline = function endsWith(str) { var pos = this.length - str.length; return this.indexOf(str, pos) === pos; };
String.prototype._AddFunction_Inline = function contains(str, /*;optional:*/ startIndex) { return -1 !== String.prototype.indexOf.call(this, str, startIndex); };
String.prototype._AddFunction_Inline = function hashCode() {
	var hash = 0;
	for (var i = 0; i < this.length; i++) {
		var char = this.charCodeAt(i);
		hash = ((hash << 5) - hash) + char;
		hash |= 0; // convert to 32-bit integer
	}
	return hash;
};
String.prototype._AddFunction_Inline = function Matches(strOrRegex) {
	if (typeof strOrRegex == "string") {
		var str = strOrRegex;
		var result = [];
		var lastMatchIndex = -1;
		while (true) {
			var matchIndex = this.indexOf(str, lastMatchIndex + 1);
			if (matchIndex == -1) // if another match was not found
				break;
			result.push({index: matchIndex});
			lastMatchIndex = matchIndex;
		}
		return result;
	}

	var regex = strOrRegex;
	if (!regex.global)
		throw new Error("Regex must have the 'g' flag added. (otherwise an infinite loop occurs)");

	var result = [];
	var match;
	while (match = regex.exec(this))
		result.push(match);
	return result;
};
/*String.prototype._AddFunction_Inline = function matches_group(regex, /*o:*#/ groupIndex)
{
	if (!regex.global)
		throw new Error("Regex must have the 'g' flag added. (otherwise an infinite loop occurs)");

	groupIndex = groupIndex || 0; // default to the first capturing group
	var matches = [];
	var match;
	while (match = regex.exec(this))
		matches.push(match[groupIndex]);
	return matches;
};*/
/*String.prototype._AddFunction_Inline = function lastIndexOf(str)
{
    for (var i = this.length - 1; i >= 0; i--)
        if (this.substr(i).startsWith(str))
            return i;
    return -1;
};*/
String.prototype._AddFunction_Inline = function IndexOf_X(x, str) // (0-based)
{
	var currentPos = -1;
	for (var i = 0; i <= x; i++)
	{
		var subIndex = this.indexOf(str, currentPos + 1);
		if (subIndex == -1)
			return -1; // no such xth index
		currentPos = subIndex;
	}
	return currentPos;
};
String.prototype._AddFunction_Inline = function IndexOf_XFromLast(x, str) // (0-based)
{
	var currentPos = (this.length - str.length) + 1; // index just after the last-index-where-match-could-occur
	for (var i = 0; i <= x; i++)
	{
		var subIndex = this.lastIndexOf(str, currentPos - 1);
		if (subIndex == -1)
			return -1; // no such xth index
		currentPos = subIndex;
	}
	return currentPos;
};
String.prototype._AddFunction_Inline = function indexOfAny()
{
	if (arguments[0] instanceof Array)
		arguments = arguments[0];

	var lowestIndex = -1;
	for (var i = 0; i < arguments.length; i++)
	{
		var indexOfChar = this.indexOf(arguments[i]);
		if (indexOfChar != -1 && (indexOfChar < lowestIndex || lowestIndex == -1))
			lowestIndex = indexOfChar;
	}
	return lowestIndex;
};
String.prototype._AddFunction_Inline = function startsWithAny() { return this.indexOfAny.apply(this, arguments) == 0; };
String.prototype._AddFunction_Inline = function containsAny()
{
	for (var i = 0; i < arguments.length; i++)
		if (this.contains(arguments[i]))
			return true;
	return false;
};
String.prototype._AddFunction_Inline = function splitByAny()
{
	if (arguments[0] instanceof Array)
		arguments = arguments[0];

	var splitStr = "/";
	for (var i = 0; i < arguments.length; i++)
		splitStr += (splitStr.length > 1 ? "|" : "") + arguments[i];
	splitStr += "/";

	return this.split(splitStr);
};
String.prototype._AddFunction_Inline = function splice(index, removeCount, insert) { return this.slice(0, index) + insert + this.slice(index + Math.abs(removeCount)); };

// Array
// ==========

//Array.prototype._AddFunction_Inline = function Contains(str) { return this.indexOf(str) != -1; };
Array.prototype._AddFunction_Inline = function Indexes()
{
	var result = {};
	for (var i = 0; i < this.length; i++)
		result[i] = null; //this[i]; //null;
	return result;
};
Array.prototype._AddFunction_Inline = function Strings() // not recommended, because it doesn't allow for duplicates
{
	var result = {};
	for (var key in this)
		if (this.hasOwnProperty(key))
			result[this[key]] = null;
	return result;
};

Array.prototype._AddFunction_Inline = function Add(item) { return this.push(item); };
Array.prototype._AddFunction_Inline = function CAdd(item) { this.push(item); return this; }; // CAdd = ChainAdd
Array.prototype._AddFunction_Inline = function TAdd(item) { this.push(item); return item; }; // TAdd = TransparentAdd
Array.prototype._AddFunction_Inline = function AddRange(array)
{
	for (var i in array)
		this.push(array[i]);
};
Array.prototype._AddFunction_Inline = function Remove(item)
{
	/*for (var i = 0; i < this.length; i++)
		if (this[i] === item)
			return this.splice(i, 1);*/
	var itemIndex = this.indexOf(item);
	this.splice(itemIndex, 1);
};
Array.prototype._AddFunction_Inline = function RemoveAt(index) { return this.splice(index, 1); };
Array.prototype._AddFunction_Inline = function Insert(index, obj) { this.splice(index, 0, obj); }

Object.prototype._AddFunction_Inline = function AsRef() { return new NodeReference_ByPath(this); }

// Linq replacements
// ----------

Array.prototype._AddFunction_Inline = function Any(matchFunc)
{
    for (var i in this.Indexes())
        if (matchFunc.call(this[i], this[i]))
            return true;
    return false;
};
Array.prototype._AddFunction_Inline = function All(matchFunc)
{
    for (var i in this.Indexes())
        if (!matchFunc.call(this[i], this[i]))
            return false;
    return true;
};
Array.prototype._AddFunction_Inline = function Where(matchFunc)
{
	var result = [];
	for (var i in this)
		if (matchFunc.call(this[i], this[i])) // call, having the item be "this", as well as the first argument
			result.push(this[i]);
	return result;
};
Array.prototype._AddFunction_Inline = function Select(selectFunc)
{
	var result = [];
	//for (var i in this)
	for (var i in this) // need this for VDF List's, which also use this function (since they derive from the Array class)
		result.Add(selectFunc.call(this[i], this[i]));
	return result;
};
//Array.prototype._AddFunction_Inline = function Count(matchFunc) { return this.Where(matchFunc).length; };
//Array.prototype._AddFunction_Inline = function Count(matchFunc) { return this.Where(matchFunc).length; }; // needed for items to be added properly to custom classes that extend Array
Array.prototype._AddGetter_Inline = function Count() { return this.length; }; // needed for items to be added properly to custom classes that extend Array
Array.prototype._AddFunction_Inline = function VCount(matchFunc) { return this.Where(matchFunc).length; };
/*Array.prototype._AddFunction_Inline = function Clear()
{
	while (this.length > 0)
		this.pop();
};*/
Array.prototype._AddFunction_Inline = function First(matchFunc) { return this.Where(matchFunc || function() { return true; })[0]; };
//Array.prototype._AddFunction_Inline = function FirstWithPropValue(propName, propValue) { return this.Where(function() { return this[propName] == propValue; })[0]; };
Array.prototype._AddFunction_Inline = function FirstWith(propName, propValue) { return this.Where(function() { return this[propName] == propValue; })[0]; };
Array.prototype._AddFunction_Inline = function Last() { return this[this.length - 1]; };
Array.prototype._AddFunction_Inline = function XFromLast(x) { return this[(this.length - 1) - x]; };

// since JS doesn't have basic 'foreach' system
Array.prototype._AddFunction_Inline = function ForEach(func) {
	for (var i in this)
		func.call(this[i], this[i], i); // call, having the item be "this", as well as the first argument
};

Array.prototype._AddFunction_Inline = function Move(item, newIndex)
{
	var oldIndex = this.indexOf(item);
	this.RemoveAt(oldIndex);
	if (oldIndex < newIndex) // new-index is understood to be the position-in-list to move the item to, as seen before the item started being moved--so compensate for remove-from-old-position list modification
		newIndex--;
	this.Insert(newIndex, item);
};

Array.prototype._AddFunction_Inline = function ToList(itemType) { return List.apply(null, [itemType || "object"].concat(this)); }
Array.prototype._AddFunction_Inline = function ToDictionary(keyFunc, valFunc)
{
	var result = new Dictionary();
	for (var i in this)
		result.Add(keyFunc(this[i]), valFunc(this[i]));
	return result;
}
Array.prototype._AddFunction_Inline = function Skip(count)
{
	var result = [];
	for (var i = count; i < this.length; i++)
		result.push(this[i]);
	return result;
};
Array.prototype._AddFunction_Inline = function Take(count)
{
	var result = [];
	for (var i = 0; i < count && i < this.length; i++)
		result.push(this[i]);
	return result;
};
Array.prototype._AddFunction_Inline = function FindIndex(matchFunc)
{
	for (var i in this)
		if (matchFunc.call(this[i], this[i])) // call, having the item be "this", as well as the first argument
			return i;
	return -1;
};
Array.prototype._AddFunction_Inline = function OrderBy(valFunc)
{
	var temp = this.ToList();
	temp.sort(function(a, b) { return valFunc(a) - valFunc(b); });
	return temp;
};
Array.prototype._AddFunction_Inline = function Distinct()
{
	var result = [];
	for (var i in this)
		if (!result.Contains(this[i]))
			result.push(this[i]);
	return result;
};
//Array.prototype._AddFunction_Inline = function JoinUsing(separator) { return this.join(separator);};

// ArgumentsArray
// ==========

/*ArgumentsArray.prototype.Slice = Array.prototype.Slice;
ArgumentsArray.prototype.AsArray = function() { return this.Slice(0); };*/

CanvasRenderingContext2D.prototype.clear = CanvasRenderingContext2D.prototype.clear || function(/*o:*/ preserveTransform)
{
	if (preserveTransform)
	{
		this.save();
		this.setTransform(1, 0, 0, 1, 0, 0);
    }

    this.clearRect(0, 0, this.canvas.width, this.canvas.height);

    if (preserveTransform)
		this.restore();
};
CanvasRenderingContext2D.prototype.clearAndFillRect = function(x, y, width, height)
{
	this.clearRect(x, y, width, height);
	this.fillRect(x, y, width, height);
};

// Node
// ==========

//Node.prototype._AddFunction_Inline = function p(propName, propType_orFirstTag) { return Prop(this, propName, propType_orFirstTag); }
//Object.prototype._AddFunction_Inline = function p(propName, /*o:*/ propType_orFirstTag, additionalTags___) { return Prop(this, propName, propType_orFirstTag); }
//Object.prototype._AddFunction_Inline = function p(propName, /*o:*/ propType_orFirstTag, additionalTags___) { return Prop.apply(null, [this].concat(V.AsArray(arguments))); };

function PropDeclarationWrapper_JustForSettingValue(obj, propName)
{
	var s = this;
	s.obj = obj;
	s.propName = propName;
}
PropDeclarationWrapper_JustForSettingValue.prototype._AddSetter_Inline = function set(value)
{
	var s = this;
	s.obj[s.propName] = value;
};
//Object.prototype._AddFunction_Inline = function p(propName, /*o:*/ propType_orFirstTag, tags___)
//Node.prototype._AddFunction_Inline = function p(propName, /*o:*/ propType_orFirstTag, tags___)
g.Node_p = function p(propName, /*o:*/ propType_orFirstTag, tags___)
{
	var tags = V.Slice(arguments, 2);
	if (propType_orFirstTag != null && typeof propType_orFirstTag != "string")
		return p.apply(this, [propName, null, propType_orFirstTag].concat(tags));

	var type = this instanceof Function ? this : this.constructor;
	var typeInfo = VDFTypeInfo.Get(type.GetName());
	if (typeInfo.props[propName] == null)
	{
		var propTag = {};
		for (var i in tags)
			if (tags[i] instanceof VDFProp)
				propTag = tags[i];
		typeInfo.props[propName] = new VDFPropInfo(propName, propType_orFirstTag, tags, propTag);
	}

	return new PropDeclarationWrapper_JustForSettingValue(this, propName);
};

// [offset construct] (e.g. {left: 10, top: 10})
// ==========

Object.prototype._AddFunction_Inline = function plus(offset) { return { left: this.left + offset.left, top: this.top + offset.top }; };