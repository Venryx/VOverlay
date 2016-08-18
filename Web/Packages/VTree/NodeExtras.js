// override VDF "Type(...).set = " function
// ==========

// for some reason, we need this here; perhaps "with(window)" doesn't work for "function funcName()"?
g.AddFunc = function TypeDeclarationWrapper(tags) { this.tags = tags; } // redefine constructor, since "set" prop-setter was unconfigurable
TypeDeclarationWrapper.prototype._AddSetter_Inline = function set(type) {
	var s = this;
	type = type instanceof Function ? type : type.constructor;
	window[type.GetName()] = type; // custom
	var typeInfo = VDFTypeInfo.Get(type.GetName()); // custom changed

	//typeInfo.props = typeInfo.props || {}; // custom

	var typeTag = {};
	for (var i in s.tags)
		if (s.tags[i] instanceof VDFType)
			typeTag = s.tags[i];
	typeInfo.tags = s.tags;
	typeInfo.typeTag.AddDataOf(typeTag);
};

// override VDF "Prop(...).set = " function (to make the Prop(...).set call actually add the type/type-constructor to the global-context/window-object)
// ==========

g.AddFunc = function PropDeclarationWrapper(typeOrObj, propName, propType_orFirstTag, tags) {
	if (propType_orFirstTag != null && typeof propType_orFirstTag != "string")
		return Prop.apply(this, [typeOrObj, propName, null, propType_orFirstTag].concat(tags));
	var propType = propType_orFirstTag;

	var s = this;
	s.type = typeOrObj instanceof Function ? typeOrObj : typeOrObj.constructor;
	s.propName = propName;
	s.propType = propType;
	s.tags = tags;
}
PropDeclarationWrapper.prototype._AddSetter_Inline = function set(value) {
	var s = this;
	window[s.type.GetName()] = s.type; // custom
	var typeInfo = VDFTypeInfo.Get(s.type.GetName()); // custom changed
	if (typeInfo.props[this.propName] == null) {
		var propTag = {};
		for (var i in s.tags)
			if (s.tags[i] instanceof VDFProp)
				propTag = s.tags[i];
		typeInfo.props[this.propName] = new VDFPropInfo(s.propName, s.propType, s.tags, propTag);
	}
};

// normal
// ==========

// wrappers
// ----------

g.AddFunc = function NodeReference_ByPath(node) {
	var s = this;
	s.node = Prop(s, "node", new ByPath()).set = node;

	//s.Serialize = function() { return new VDFNode(s.node.GetPath_Absolute().Serialize().primitiveValue); }.AddTags(new VDFSerialize());
	s.Serialize = function() { return new VDFNode(s.node.GetPath_Absolute().toString()); }.AddTags(new VDFSerialize());
	s.PostSerialize = function(node, path, options) {
		// maybe temp; manually correct metadata
		if (typeof node.primitiveValue == "string")
			node.metadata = "Node"; //NodePath";
	}.AddTags(new VDFPostSerialize());
	NodeReference_ByPath.Deserialize = function(node, path, options) { return VO.GetNodeByNodePath(s.node.node.ToObject("NodePath", options), path, options); }.AddTags(new VDFDeserialize(true));
}

// property tags
// ----------

g.AddFunc = function ByPath(/*o:*/ saveNormallyForParentlessNode) {
	var s = this;
	s.saveNormallyForParentlessNode = saveNormallyForParentlessNode != null ? saveNormallyForParentlessNode : false;
}
g.AddFunc = function ByName() {}

// method tags
// ----------

g.AddFunc = function IgnoreStartData() {}
g.AddFunc = function IgnoreSetItem() {}

// classes
// ----------

g.AddFunc = Type(new VDFType(VDF.PropRegex_Any)).set = function NodeAttachPoint(parent, prop, /*o:*/ list_index, map_keyIndex, map_key) {
	list_index = list_index != null ? list_index : -1;
	map_keyIndex = map_keyIndex != null ? map_keyIndex : -1;

	var s = this;

	if (prop && !(prop instanceof VDFPropInfo))
		prop.tags = []; // do this just so the errors on checking 'tags' prop don't happen

	s.parent = parent;
	s.prop = prop;
	s.list_index = list_index;
	s.map_keyIndex = map_keyIndex;
	s.map_key = map_key;

	s.Equals = function(other) { return s.toString() == other.toString(); };
	s.toString = function() { return s.parent + ")" + s.prop.name + (s.list_index != -1 ? "[" + s.list_index + "]" : "") + (s.map_keyIndex != -1 ? ".keys[" + s.map_keyIndex + "]" : "") + (s.map_key != null ? "[" + s.map_key + "]" : ""); };

	s.ToPathNodes = function(/*o:*/ nodeRoot) {
		var result = [];
		if (s.prop != null)
			result.Add(new NodePathNode(s.prop));
		if (s.list_index != -1)
			result.Add(new NodePathNode().Init({listIndex: s.list_index}));
		if (s.map_keyIndex != -1)
			result.Add(new NodePathNode().Init({mapKeyIndex: s.map_keyIndex}));
		if (s.map_key != null)
			if (nodeRoot != null)
				result.Add(new NodePathNode(null, map_key, nodeRoot));
			else
				result.Add(new NodePathNode(null, s.map_key));
		return result;
	};
}
g.AddFunc = Type(new VDFType(VDF.PropRegex_Any)).set = function NodePathNode(/*o:*/ prop, mapKey, nodeRoot) {
	var s = this;

	// local-context cache, essentially
	s.prop = prop;
	s.mapKey = mapKey;

	s.voRoot = false; // 'VO'
	s.vdfRoot = false; // '#'
	s.nodeRoot = false; // '@'
	s.currentParent = false; // ''
	s.moveUp = false; // '^'
	//public string prop_str; // ex: 'p:age'
	s.prop_str = prop ? prop.name : null; // ex: 'age' (note: the 'default' type)
	s.listIndex = -1; // ex: 'i:15'
	s.mapKeyIndex = -1; // ex: 'ki:15'
	//s.mapKey_str = mapKey ? mapKey.toString() : null; // ex: 'k:melee'
	s.mapKey_str = null;
	if (mapKey) {
		/*var mapKeyNode = VDFSaver.ToVDFNode(mapKey); // stringify-attempt-1: use exporter
		if (typeof mapKeyNode.primitiveValue == "string")
			s.mapKey_str = mapKeyNode.primitiveValue;
		else // if stringify-attempt-1 failed (i.e. exporter did not return string), use stringify-attempt-2
			s.mapKey_str = mapKey.toString();*/

		if (mapKey instanceof Type)
			//s.mapKey_str = "Type>'" + mapKey + "'";
			s.mapKey_str = ToVDF(mapKey).replace(/"/g, "'");
		else if (mapKey instanceof Node)
			if (nodeRoot != null && mapKey.GetPath_Relative(nodeRoot) != null)
				s.mapKey_str = "[embedded path]" + mapKey.GetPath_Relative(nodeRoot).toString().replace(/\//g, "[fs]");
			else
				s.mapKey_str = "[embedded path]" + mapKey.GetPath_Absolute().toString().replace(/\//g, "[fs]");
			//s.mapKey_str = "[embedded path]" + mapKey.GetPath_Relative(mapKey.GetPath_Absolute().ToVDFNodePath()).toString().replace(/\//g, "[fs]");
		else
			s.mapKey_str = mapKey.toString();
	}

	s.ToVDFNodePathNode = function() { return new VDFNodePathNode(null, s.prop, s.listIndex, s.mapKeyIndex, s.mapKey_str); };

	s.Equals = function(other) { return s.toString() == other.ToString(); };
	s.toString = function() {
		if (s.voRoot)
			return "VO";
		if (s.vdfRoot)
			return "#";
		if (s.nodeRoot)
			return "@";
		if (s.currentParent)
			return "";
		if (s.moveUp)
			return "^";
		if (s.listIndex != -1)
			return "i:" + s.listIndex;
		if (s.mapKeyIndex != -1)
			return "ki:" + s.mapKeyIndex;
		if (s.mapKey_str != null)
			return "k:" + s.mapKey_str;
		//return "p:" + s.prop_str;
		return s.prop_str;
	};
};
NodePathNode.Parse = function(str) {
	var result = new NodePathNode();
	if (str == "VO")
		result.voRoot = true;
	else if (str == "#")
		result.vdfRoot = true;
	else if (str == "@")
		result.nodeRoot = true;
	else if (str == "")
		result.currentParent = true;
	else if (str == "^")
		result.moveUp = true;
	else if (str.StartsWith("i:"))
		result.listIndex = parseInt(str.substr(2));
	else if (str.StartsWith("ki:"))
		result.mapKeyIndex = parseInt(str.substr(3));
	else if (str.StartsWith("k:"))
		result.mapKey_str = str.substr(2);
	else
		//result.prop_str = str.substr(2);
		result.prop_str = str;
	return result;
};
g.AddFunc = Type(new VDFType(VDF.PropRegex_Any)).set = function NodePath(nodes) {
	var s = this;
	s.nodes = new List("NodePathNode"); //[];
	if (nodes)
		for (var i in nodes)
			s.nodes[i] = nodes[i];

	s.Serialize = function() { return new VDFNode(s.toString()); }.AddTags(new VDFSerialize());
	s.Equals = function(other) { return s.toString() == other.toString(); };
	s.toString = function() {
		var result = "";
		var nodesCopy = s.nodes.ToList();
		for (var i = 0; i < nodesCopy.Count; i++) {
			var str = nodesCopy[i].toString();
			if (nodesCopy[i].moveUp)
				while (nodesCopy[i + 1] && nodesCopy[i + 1].moveUp) {
					str += "^";
					nodesCopy.RemoveAt(i + 1);
				}
			result += (i > 0 ? "/" : "") + str;
		}
		return result;
	};
	NodePath.Deserialize = function(node) {
		var nodes = [];
		var parts = node.primitiveValue.split("/");
		for (var i in parts) {
			var part = parts[i];
			if (part.startsWith("^"))
				for (var i2 = 0; i2 < part.length; i2++)
					nodes.Add(NodePathNode.Parse("^"));
			else
				nodes.Add(NodePathNode.Parse(part));
		}
		return new NodePath(nodes);
	}.AddTags(new VDFDeserialize(true));

	s._AddGetter_Inline = function rootNode() { return s.nodes.First(); };
	s._AddGetter_Inline = function parentNode() { return s.nodes.length >= 2 ? s.nodes[s.nodes.length - 2] : null; };
	s._AddGetter_Inline = function currentNode() { return s.nodes.Last(); };
	s.FromLast = function(indexOffset) {
		indexOffset = indexOffset != null ? indexOffset : 0;
		return s.nodes[(s.nodes.length - 1) + indexOffset];
	};

	// gets a transformed path that is local-anchor-based (i.e. adds move-up path-nodes till reaches shared-ancestor, then the second part of the original path)
	s.AsFrom = function(attachPointParent) {
		var attachPointParentPath = attachPointParent.GetPath_Absolute();
		var treePathToSelf_sharedNodes = [];
		for (var i = 0; i < s.nodes.Count && i < attachPointParentPath.nodes.Count; i++)
			if (s.nodes[i].Equals(attachPointParentPath.nodes[i]))
				treePathToSelf_sharedNodes.Add(s.nodes[i]);
		var treePathToSelf_unsharedNodes = s.nodes.Skip(treePathToSelf_sharedNodes.Count);

		var fromAttachParentToLastShared_nodes = [];
		var fromObjWithPropToLastShared_nodesNeeded = attachPointParentPath.nodes.Count - treePathToSelf_sharedNodes.Count;
		for (var i = 0; i < fromObjWithPropToLastShared_nodesNeeded; i++)
			fromAttachParentToLastShared_nodes.Add(new NodePathNode().Init({ moveUp: true }));

		return new NodePath(fromAttachParentToLastShared_nodes.concat(treePathToSelf_unsharedNodes));
	};
	s.TryAsFrom = function(attachPointParent) {
		var attachPointParentPath = attachPointParent.GetPath_Absolute();
		if (attachPointParentPath && attachPointParentPath.rootNode.Equals(rootNode))
			return AsFrom(attachPointParent);
		return this;
	};
}

g.AddFunc = function DisconnectedRootReference(node) {
	var s = this;
	s.node = node;
};

g.ContextGroup = V.CreateEnum("ContextGroup", {
	Local_CS:{},
	Local_CSAndUI:{},
	Local_UI:{}
});

g.AddFunc = function ToMainTree() {};
g.AddFunc = function SubtreeAddInfo(anchorObj, anchorInMainTree, subtreeProp, subtreeObj) {
	var s = this;
	s.anchorObj = anchorObj;
	s.anchorInMainTree = anchorInMainTree;
	s.subtreeProp = subtreeProp;
	s.subtreeObj = subtreeObj;
};

// Node extension-methods
// ==========

g.AddFunc = Node.SetAsBaseClassFor = function NodePlaceholder() { // used for attaching extra-methods before the host object even exists
	var s = this.CallBaseConstructor();
	s.TransferDataTo = function(node) {
		for (var methodName in s._extraMethods)
			for (var i in s._extraMethods[methodName])
				node.AddExtraMethod(methodName, s._extraMethods[methodName][i]);
		for (var childName in s._childPlaceholders)
			if (node[childName] instanceof Node)
				s._childPlaceholders[childName].TransferDataTo(node[childName]);
			else {
				node._childPlaceholders[childName] = new NodePlaceholder();
				s._childPlaceholders[childName].TransferDataTo(node._childPlaceholders[childName]);
			}
	};
};

// maybe todo: update code using this system, to only use it when it's helpful (i.e. when there's actually something listening for changes)
// probably todo: create tag for properties that lets you specify how to serialize/deserialize them (for example, just using the ID/by reference)
g.AddFunc = function PropertyWrapper(obj, propName) {
	var s = this;

	/*s.obj = obj;
	s.propName = propName;*/

	//if (propName.StartsWith("_"))
	//	throw new Error("Cannot use the VTree change system for a non-shared VTree property.");

	var propInfo = obj.GetVDFTypeInfo().GetProp(propName);

	s.Set = function(value, group, message) { VO.SubmitChange(new Change_Set(obj, propInfo, value).AddMessages(message), group || ContextGroup.Local_CSAndUI); };
	s.Set_IfDifferent = function(value, group, message)
	{
		if (obj[propName] != value)
			VO.SubmitChange(new Change_Set(obj, propInfo, value).AddMessages(message), group || ContextGroup.Local_CSAndUI);
	};

	// lists and maps
	s.SetItem = function(index, newItem, group, message) { VO.SubmitChange(new Change_SetItem_List(obj, propInfo, index, newItem).AddMessages(message), group || ContextGroup.Local_CSAndUI); };
	// overload-1 (if prop is an Array): newItem, index, group, message
	// overload-2 (if prop is a Dictionary): key, value, overwrite, group, message
	s.Add = function(arg1, arg2, arg3, arg4, arg5) {
		/*if (arg2 instanceof ContextGroup)
			return s.Add(arg1, -1, arg2, arg3);*/
		if (obj[propName] instanceof Array) {
			var newItem = arg1, index = arg2, group = arg3, message = arg4;
			VO.SubmitChange(new Change_Add_List(obj, propInfo, newItem, index).AddMessages(message), group || ContextGroup.Local_CSAndUI);
		}
		else if (obj[propName] instanceof Dictionary) {
			var key = arg1, value = arg2, overwrite = arg3 != null ? arg3 : false, group = arg4, message = arg5;
			VO.SubmitChange(new Change_Add_Dictionary(obj, propInfo, key, value, overwrite).AddMessages(message), group || ContextGroup.Local_CSAndUI);
		}
		else
			throw new Error("Cannot call Add for prop of type: " + (obj[propName] ? obj[propName].GetTypeName() : "[null]"));
	};
	s.Remove = function(indexOrItemOrKey, group, message) {
		if (obj[propName] instanceof Array)
			VO.SubmitChange(new Change_Remove_List(obj, propInfo, indexOrItemOrKey).AddMessages(message), group || ContextGroup.Local_CSAndUI);
		else if (obj[propName] instanceof Dictionary)
			VO.SubmitChange(new Change_Remove_Dictionary(obj, propInfo, indexOrItemOrKey).AddMessages(message), group || ContextGroup.Local_CSAndUI);
		else
			throw new Error("Cannot call Remove for prop of type: " + (obj[propName] ? obj[propName].GetTypeName() : "[null]"));
	};
	s.Clear = function(group, message) {
		if (obj[propName] instanceof Array)
			VO.SubmitChange(new Change_Clear_List(obj, propInfo).AddMessages(message), group || ContextGroup.Local_CSAndUI);
		else if (obj[propName] instanceof Dictionary)
			VO.SubmitChange(new Change_Clear_Dictionary(obj, propInfo).AddMessages(message), group || ContextGroup.Local_CSAndUI);
		else
			throw new Error("Cannot call Clear for prop of type: " + (obj[propName] ? obj[propName].GetTypeName() : "[null]"));
	};

	// numbers
	s.Increase = function(amount, group, message) { VO.SubmitChange(new Change_Increase_Number(obj, propInfo, amount).AddMessages(message), group || ContextGroup.Local_CSAndUI); }

	s._AddSetter_Inline = function set(value) { s.Set(value); };
	s._AddSetter_Inline = function set_self(value) { s.Set(value, ContextGroup.Local_UI); };
	s._AddSetter_Inline = function set_ifDifferent(value) { s.Set_IfDifferent(value); };
	s._AddSetter_Inline = function increase(value) { s.Increase(value); };
	s._AddSetter_Inline = function add(value) { s.Add(value); };
	s._AddSetter_Inline = function remove(indexOrItem) { s.Remove(indexOrItem); };
};