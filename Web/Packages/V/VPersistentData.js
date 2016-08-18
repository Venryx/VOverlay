// Object: base
// ==================

// the below lets you do stuff like this: Array.prototype._AddFunction(function AddX(value) { this.push(value); }); []._AddX("newItem");
/*Object.defineProperty(Object.prototype, "_AddItem", // note; these functions should by default add non-enumerable properties/items
{
	enumerable: false,
	value: function(name, value)
	{
		Object.defineProperty(this, name,
		{
			enumerable: false,
			value: value
		});
	}
});
Object.prototype._AddItem("_AddFunction", function(func) { this._AddItem(func.name || func.toString().match(/^function\s*([^\s(]+)/)[1], func); });

// the below lets you do stuff like this: Array.prototype._AddGetterSetter("AddX", null, function(value) { this.push(value); }); [].AddX = "newItem";
Object.prototype._AddFunction(function _AddGetterSetter(getter, setter)
{
	var name = (getter || setter).name || (getter || setter).toString().match(/^function\s*([^\s(]+)/)[1];
	if (getter && setter)
		Object.defineProperty(this, name, { enumerable: false, get: getter, set: setter });
	else if (getter)
		Object.defineProperty(this, name, { enumerable: false, get: getter });
	else
		Object.defineProperty(this, name, { enumerable: false, set: setter });
});

// the below lets you do stuff like this: Array.prototype._AddFunction_Inline = function Push(value) { this.push(value); }; [].Push = "newItem";
Object.prototype._AddGetterSetter(null, function _AddFunction_Inline(func) { this._AddFunction(func); });
Object.prototype._AddGetterSetter(null, function _AddGetter_Inline(func) { this._AddGetterSetter(func, null); });
Object.prototype._AddGetterSetter(null, function _AddSetter_Inline(func) { this._AddGetterSetter(null, func); });*/

// VPromise
// ==========

var VPersistentData = function(key, defaultData)
{
	this.key = key;
	if (localStorage[this.key] === undefined)
		localStorage[this.key] = ToJSON(defaultData);
	this.Load();
};
VPersistentData.prototype.Save = function()
{
	var saveData = {};
	for (var key in this)
		saveData[key] = this[key];
	localStorage[this.key] = ToJSON(saveData);
};
VPersistentData.prototype.Load = function()
{
	var loadData = FromJSON(localStorage[this.key]);
	for (var key in loadData)
		this[key] = loadData[key];
}