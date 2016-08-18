//List.prototype._AddFunction_Inline = Array.prototype.filter;

// old: probably todo: automize this
//Array.prototype._AddFunction("Any", List.prototype.Any);
//Array.prototype._AddFunction("All", List.prototype.All);

// VDF
// ==========

// VDFNodePath
VDFNodePath.prototype._AddFunction_Inline = function GetNodeWithProp() {
	var result = null;
	if (this.currentNode.prop != null) // if node zero up has prop-info
		result = this.currentNode;
	else if (this.parentNode != null && this.parentNode.prop != null) // if node one up has prop
		result = this.parentNode;
	else if (this.nodes.Count >= 3 && this.nodes.XFromLast(2).prop != null) // if node two up has prop
		result = this.nodes.XFromLast(2);
	return result;
};
VDFNodePath.prototype._AddFunction_Inline = function GetNodeWithParent() {
	var nodeWithProp = this.GetNodeWithProp();
	return nodeWithProp != null ? this.nodes[this.nodes.indexOf(nodeWithProp) - 1] : null;
};
VDFNodePath.prototype._AddFunction_Inline = function GetNodeWithIndexOrKey() {
	var nodeWithProp = this.GetNodeWithProp();
	return nodeWithProp != null ? this.nodes[this.nodes.indexOf(nodeWithProp) + 1] : null;
};

VDFNodePath.prototype._AddFunction_Inline = function ToNodePath(addNodeForSelf) {
	var result = new NodePath([]);
	if (addNodeForSelf)
		result.nodes.Add(new NodePathNode().Init({ vdfRoot: true }));
	for (var i = 0; i < this.nodes.length; i++) {
		var node = this.nodes[i];
		if (node.prop != null)
			result.nodes.Add(new NodePathNode(node.prop));
		else if (node.list_index != -1)
			result.nodes.Add(new NodePathNode().Init({listIndex: node.list_index}));
		else if (node.map_key != null)
			result.nodes.Add(new NodePathNode(null, node.map_key));
		/*else
			//result.nodes.Add(new NodePathNode().Init({currentParent: true}));
			result.nodes.Add(new NodePathNode().Init({vdfRoot: true}));*/
	}
	return result;
};

VDFNodePath.prototype._AddFunction_Inline = function GetFinalNodeValue() {
	var s = this;
	var parentObj = s.parentNode.obj;
	if (s.currentNode.prop != null) // if final-node is prop
		return parentObj[s.currentNode.prop.name];
	//var propValue = s.nodes.XFromLast(2).obj[s.parentNode.prop.name];
	if (s.currentNode.list_index != -1) // if final-node is list index
		return parentObj[s.currentNode.list_index];
	if (s.currentNode.map_keyIndex != -1) // if final-node is map key-index
		return parentObj.keys[s.currentNode.map_keyIndex];
	// final-node must be map key
	return parentObj.Get(s.currentNode.map_key);
};
VDFNodePath.prototype._AddFunction_Inline = function SetFinalNodeValue(value) {
	var s = this;
	var parentObj = s.parentNode.obj;
	if (s.currentNode.prop != null) // if final-node is prop
		parentObj[s.currentNode.prop.name] = value;
	else if (s.currentNode.list_index != -1) // if final-node is list index
		parentObj[s.currentNode.list_index] = value;
	else if (s.currentNode.map_keyIndex != -1) // if final-node is map key-index
	{
		/*var oldPair = propValue.Pairs[s.currentNode.map_keyIndex];
		propValue.keys[s.currentNode.map_keyIndex] = value;
		delete propValue[oldPair.key];*/

		// remove old-pair, add new pair (with old-pair's value), and have keys update their attach-point data
		/*var oldPair = propValue.Pairs[s.currentNode.map_keyIndex];
		propValue.Remove(oldPair.key);
		propValue.Add(value, oldPair.value);
		// old: make-so: keys update their attach-point data*/

		// maybe temp; rely on specially-added map_key data, to find old-pair (s.currentNode.map_keyIndex might be out-of-date by now, if dictionary was modified since this path's creation)
		var map_keyIndex = parentObj.keys.indexOf(s.currentNode.map_key);
		var oldPair = parentObj.Pairs[map_keyIndex];
		parentObj.Remove(s.currentNode.map_key);
		parentObj.Add(value, oldPair.value);
		delete parentObj[s.currentNode.map_key];
	}
	else // else, final-node must be map key
		parentObj.Set(s.currentNode.map_key, value);
};

// Dictionary
// ==================

Dictionary.prototype._AddFunction_Inline = function Clone() {
	var result = new Dictionary(this.keyType, this.valueType);
	for (var key in this.Keys)
		result.Add(key, this[key]);
	return result;
};
Dictionary.prototype._AddFunction_Inline = function AddDictionary(other) {
	for (var i = 0; i < other.Count; i++)
		this.Add(other.keys[i], other.values[i]);
};

Dictionary.prototype._AddFunction_Inline = function KeyMatching(keyStr) {
	for (var i = 0; i < this.Count; i++)
		if (this.keys[i].toString() == keyStr)
			return this.keys[i];
	return null;
};
Dictionary.prototype._AddFunction_Inline = function Get_ByString(keyStr) { return this.Get(this.KeyMatching(keyStr)); };
/*Dictionary.prototype._AddFunction_Inline = function Add_ByString(str) {
	arguments[0] = this.keys.First(function() { return this.toString() == str; });
	this.Add.apply(this, arguments);
};
Dictionary.prototype._AddFunction_Inline = function Remove_ByString(str) {
	arguments[0] = this.keys.First(function() { return this.toString() == str; });
	this.Remove.apply(this, arguments);
};*/