//Type(new VDFType("^[^_]")).set = Node = function Node()
// maybe temp; exclude "root" prop explicitly
//Type(new VDFType("^(?!_)(?!s$)(?!root$)")).set = Node = function Node() // base node class (when not overriden, acts as empty/heirarchy/container node)
Type(new VDFType("^(?!s$)(?!root$)[a-z]")).set = Node = function Node() { // base node class (when not overriden, acts as empty/heirarchy/container node)
	// maybe temp; used by VObject>type-stuff
	if (Node.baseObj == null) {
		Node.baseObj = {};
		Node.baseObj = new Node();
	}

	var s = this;

	/*s._postDeserializeMethods = [];
	s.AddPostDeserializeMethod = function(method) { s._postDeserializeMethods.push(method); };*/

	s.PreDeserialize = function(node, path, options) {
		var oldDisconnectedRootReference = options.messages.First(function() { return this instanceof DisconnectedRootReference; });
		var disconnectedRootReference = oldDisconnectedRootReference || new DisconnectedRootReference(this);
		if (oldDisconnectedRootReference == null)
			options.messages.Add(disconnectedRootReference);
		else
			if (path.currentNode.prop != null)
				path.parentNode.obj[path.currentNode.prop.name] = this;
			else if (path.currentNode.list_index != -1)
				path.parentNode.obj.Add(this);
			else if (path.currentNode.map_key != null)
				path.parentNode.obj.Add(path.currentNode.map_key, this);
	}.AddTags(new VDFPreDeserialize());
	s.PostDeserialize = function(node, path, options) {
		var disconnectedRootReference = options.messages.First(function() { return this instanceof DisconnectedRootReference; });
		if (disconnectedRootReference != null && disconnectedRootReference.node == this)
			options.messages.Remove(disconnectedRootReference);

		/*for (var i in s._postDeserializeMethods)
			s._postDeserializeMethods[i]();*/
	}.AddTags(new VDFPostDeserialize());

	s.Serialize = function(path, options) {
		/*if (s instanceof GetterSetter) // if a GetterSetter, always serialize normally (they're always attached 'rawly'--never as reference)
			return VDF.NoActionTaken;*/

		var prop = (path.GetNodeWithProp() || {}).prop;
		//if (prop != null && prop.tags.Any(function() { return this instanceof ByPath; }) && !prop.tags.First(function() { return this instanceof ByPath; }).saveNormallyForParentlessNode && VO != s && s.Parent == null)
		//	throw new Error("Property '" + prop.name + "' of type '" + parent.GetType().name + "' references a parentless node.");

		//var tryToSaveByReference = prop && prop.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; });
		var sourceProp = prop;
		if (path.parentNode != null && path.parentNode.obj instanceof Change && (prop.name == "value" || prop.name == "item"))
			sourceProp = path.parentNode.obj.propInfo; //VDFTypeInfo.Get(path.parentNode.obj.obj.GetType()).GetProp(path.parentNode.obj.propInfo.name);
		var propWantsByReference = prop && prop.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; });
		var sourcePropWantsByReference = sourceProp && sourceProp.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; });

		var selfIsKey = path.currentNode.map_keyIndex != -1; // (if self is a key, always serialize by reference)

		/*if (prop && prop.tags && prop.tags.Any(function() { return this instanceof ByPath; })) //&& s.IsConnectedToMainTree()) // if self should be saved by-path
			if (VO == s || s.Parent != null) // and self is connected to v-tree (i.e. saving-by-path is valid)
				return VDFSaver.ToVDFNode(s.GetPath_Absolute());
			else if (!prop.tags.First(function() { return this instanceof ByPath; }).saveNormallyForParentlessNode)
				throw new Error("Node should be saved by path, but path is invalid.");
		if (prop != null && prop.tags.Any(function() { return this instanceof ByName; }))
			return VDFSaver.ToVDFNode(this.GetType().name + ">" + this.name, undefined, undefined, path);*/
		if (propWantsByReference || sourcePropWantsByReference || selfIsKey) {
			var referenceTagProp = sourcePropWantsByReference ? sourceProp : prop;
			//var newOptions = new VDFSaveOptions();
			// if self should be saved by-path, and this prop-path is not where self is attached (since, if this were the attach-point, the generated reference would point to... itself)
			if ((referenceTagProp && referenceTagProp.tags.Any(function() { return this instanceof ByPath; }) && (path.GetNodeWithProp() == null || s.attachPoint == null || path.GetNodeWithProp().prop != s.attachPoint.prop)) || selfIsKey) {
				// first, try to get a relative path
				var treePath = s.GetPath_Relative(path);
				// if path to value is same as path to anchor-point/prop, that's not usable for a relative-path (it wouldn't point to any actual data), so nullify it
				// old: maybe temp; take 'saveNormallyForParentlessNode' as hint that we should use absolute-path
				/*if (referenceTagProp.tags.Any(function(a) { return a instanceof ByPath && a.saveNormallyForParentlessNode; }))
					treePath = null;*/
				/*if (treePath != null && (treePath.toString() == path.ToNodePath().toString() || (treePath.nodes.Count == 2 && treePath.rootNode.vdfRoot)))
					treePath = null;*/

				// if no valid relative path found, try to get an absolute path
				if (treePath == null)
					treePath = s.GetPath_Absolute();

				if (treePath != null) { // if found valid path (i.e. self is connected to main-tree, or internal to the being-serialized pack)
					// maybe make-so: the to-local-anchor converter below is used
					/*if (path.parentNode.obj instanceof Node)
						treePath = treePath.TryAsFrom(path.parentNode.obj);*/
					//return VDFSaver.ToVDFNode(treePath, null, newOptions);
					return VDFSaver.ToVDFNode(treePath);
				}
				if (selfIsKey || !referenceTagProp.tags.First(function() { return this instanceof ByPath; }).saveNormallyForParentlessNode) {
					debugger;
					throw new Error("Node should be saved by path, but path is invalid. (i.e. the node of type '" + s.GetTypeName() + "' is not attached to main-tree, and is not internal to being-serialized pack)");
				}
			}
			else if (referenceTagProp && referenceTagProp.tags.Any(function() { return this instanceof ByName; })) {
				//var typeStr = options.messages.Contains("to cs") ? s.GetTypeName() + ">" : "";
				var typeStr = s.GetTypeName() + ">"; // (for now, js side just always includes type-name, for by-name nodes)
				//return VDFSaver.ToVDFNode(typeStr + this.name, undefined, undefined, path);
				return VDFSaver.ToVDFNode(typeStr + this.name, undefined, undefined);
			}
		}
		return VDF.NoActionTaken;
	}.AddTags(new VDFSerialize());
	s.PostSerialize = function(node, path, options) {
		// maybe temp; manually correct metadata (you can either set the metadata to "Node", or specify the type for the "obj" property on the JS side)
		//if (node["nodes"])
		if (node.metadata == "NodePath")
			node.metadata = "Node"; //NodePath";
		else if (typeof node.primitiveValue == "string")
			node.metadata = "Node"; //NodePath";
	}.AddTags(new VDFPostSerialize());
	Node.lastMapKeyPlaceholder_index = Node.lastMapKeyPlaceholder_index != null ? Node.lastMapKeyPlaceholder_index : -1;
	//var byNameTypes = ["Map", "Soil", "Biome", "VObject_Type", "Module"];
	Node.Deserialize = function(node, path, options) { //s.VDFDeserialize = function(node, prop, options)
		// if (typeof node.primitiveValue == "string" && prop && prop.tags && prop.tags.Any(function() { return this instanceof ByPath; }))
		var nodeAsStr = typeof node.primitiveValue == "string" ? node.primitiveValue : null;
		if (nodeAsStr != null && (nodeAsStr.startsWith("VO") || nodeAsStr == "#" || nodeAsStr.startsWith("#/") || nodeAsStr == "@" || nodeAsStr.startsWith("@/") || nodeAsStr.startsWith("^") || nodeAsStr.startsWith("/"))) {
			//return VO.GetNodeByNodePath(node.ToObject("NodePath", options), path, options);

			// special case for when this-node-is-the-root-node (e.g. deserialize node-path string directly as a Node)
			if (path.parentNode == null)
				return VO.GetNodeByNodePath(node.ToObject("NodePath", FinalizeFromVDFOptions(new VDFLoadOptions())), path, options);

			// maybe temp; resolve reference after the normal deserialization has completed
			if (path.parentNode.obj instanceof Change && path.currentNode.prop.name == "obj") // if Change.obj prop, evaluate right away, since it's needed for other deserialize methods
				return VO.GetNodeByNodePath(node.ToObject("NodePath", FinalizeFromVDFOptions(new VDFLoadOptions())), path, options);

			var placeholder = path.currentNode.map_keyIndex != -1 ? "mapKeyPlaceholder_" + (++Node.lastMapKeyPlaceholder_index) : null;
			// maybe temp; specially-add map_key data, for delayed SetFinalNodeValue method
			if (path.currentNode.map_keyIndex != -1)
				//path.currentNode.map_key = path.nodes.XFromLast(2).obj[path.parentNode.prop.name].keys[path.currentNode.map_keyIndex];
				path.currentNode.map_key = placeholder;
			options.AddObjPostDeserializeFunc(path.nodes[0].obj, function() { path.SetFinalNodeValue(VO.GetNodeByNodePath(node.ToObject("NodePath", options), path, options)); }, true);
			return placeholder;
		}
		if (nodeAsStr != null) { // if by-name reference
			var attachObj = path.GetNodeWithParent() && path.GetNodeWithParent().obj instanceof Node ? path.GetNodeWithParent().obj : null;
			var attachProp = path.GetNodeWithProp() && path.GetNodeWithProp().prop;
			var attachIndexOrKeyNode = path.GetNodeWithIndexOrKey();
			var attachIndex = attachIndexOrKeyNode != null && attachIndexOrKeyNode.list_index != -1 ? attachIndexOrKeyNode.list_index : -1;
			var attachKeyIndex = -1;
			var attachKey = attachIndexOrKeyNode != null && attachIndexOrKeyNode.map_key != null ? attachIndexOrKeyNode.map_key : null;
			var attachPoint = new NodeAttachPoint(attachObj, attachProp, attachIndex, attachKeyIndex, attachKey);

			var finalAttachProp = attachProp;
			if (path.parentNode != null && path.parentNode.obj instanceof Change && path.parentNode.obj.prop)
				/*attachObj = path.parentNode.obj.obj;
				attachProp = path.parentNode.obj.prop;
				attachIndex = path.parentNode.obj instanceof Change_Add_List ? path.parentNode.obj.index : -1;
				attachKey = path.parentNode.obj instanceof Change_Add_Dictionary ? path.parentNode.obj.key : -1;*/
				finalAttachProp = path.parentNode.obj.prop; // can be null if this Node being deserialized by-name is itself part of the above Change object (since its PostInit method hasn't been called yet)

			var typeName;
			if (node.metadata != "Node") // try from metadata
				//typeName = node.metadata;
				typeName = node.metadata == "VMap" ? "Map" : node.metadata;
			if (typeName == null) // try from node-text
				typeName = node.primitiveValue.contains(">") ? node.primitiveValue.substr(0, node.primitiveValue.indexOf(">")) : null;
			if (typeName == null) { // try from prop-info
				var typeName_fromPropInfo = null;
				if (attachProp)
					if (attachProp.typeName && attachProp.typeName.startsWith("List("))
						typeName_fromPropInfo = VDF.GetGenericArgumentsOfType(attachProp.typeName)[0];
					else if (attachProp.typeName && attachProp.typeName.startsWith("Dictionary("))
						typeName_fromPropInfo = VDF.GetGenericArgumentsOfType(attachProp.typeName)[1];
					else
						typeName_fromPropInfo = attachProp.typeName;
				typeName = typeName_fromPropInfo;
			}
			typeName = typeName || "object";
			//var type = typeName && VDF.GetTypeByName(typeName, VConvert.FinalizeFromVDFOptions(new VDFLoadOptions()));
			var name = node.primitiveValue.substr(node.primitiveValue.indexOf(">") + 1);
			if (typeName == "Map")
				return VO.LoadLazy_Map(attachPoint, name);
			if (typeName == "Soil")
				return VO.LoadLazy_Soil(attachPoint, name);
			if (typeName == "Biome")
				return VO.LoadLazy_Biome(attachPoint, name);
			// js doesn't have type-aliases, so just except either (and hope JS doesn't sent and such to C#, as that would make it inconsistent (though probably still work))
			//if (typeName == "VOT" || typeName == "VObject_Type")
			if (typeName == "VObject_Type")
				return VO.LoadLazy_VObject_Type(attachPoint, name);
			if (typeName == "Module")
				return VO.LoadLazy_Module(attachPoint, name);
			debugger;
			throw new Error("No handler found for Node to load string data \"" + node.primitiveValue + "\".");
		}
		return VDF.NoActionTaken;
	}.AddTags(new VDFDeserialize(true));

	s.p("attachPoint", new VDFProp(false)).set = null;
	s._AddGetter_Inline = function Parent() { return s.attachPoint ? s.attachPoint.parent : null; };
	s.GetAncestors = function() {
		if (s.Parent == null)
			return [];
		var result = [s.Parent];
		while (result[0].Parent != null)
			result.Insert(0, result[0].Parent);
		return result;
	};
	s.SetAttachPoint = function(newAttachPoint) {
		if (s.Parent && newAttachPoint && newAttachPoint.parent && s.Parent != newAttachPoint.parent)
			throw new Error("Cannot set parent more than once. Existing attach-point: " + s.attachPoint);
		if (newAttachPoint && newAttachPoint.parent == this)
			throw new Error("Cannot set self as own parent! Existing attach-point: " + s.attachPoint);

		if (s.Parent)
			s.attachPoint.parent._children.Remove(this);
		s.attachPoint = newAttachPoint;
		if (s.Parent)
			s.attachPoint.parent._children.Add(this);
	};
	s._children = [];

	s.GetPath_Absolute = function() {
		// check if we're the root of a path-finding call-up-chain
		if (VO == this) // if vo-root (i.e. game's main-tree root), return as path root
			return new NodePath([new NodePathNode().Init({voRoot: true})]);

		if (s.Parent == null) // if we can't move up any more, and we still haven't reached vo-root, then return null/no-path
			return null;

		// start or continue a path-finding call-up-chain
		var path = s.Parent.GetPath_Absolute();
		if (path != null)
			path.nodes.AddRange(s.attachPoint.ToPathNodes());
		return path;
	};
	s.GetPath_Relative = function(arg1, initialCall) {
		if (arg1 instanceof VDFNodePath) {
			var relativeVDFPath = arg1;
			initialCall = initialCall != null ? initialCall : true;

			// check if we're the root of a path-finding call-up-chain
			var ancestors = s.GetAncestors();
			//if (relativeVDFPath != null && (s.Parent == null || !relativeVDFPath.nodes.Any(function(a) { return a.obj == s.Parent; }))) // if we have no in-vdf-path Node parent (i.e. if we might be the node-root)
			//if (s.Parent == null || !relativeVDFPath.nodes.Any(function(a) { return a.obj == s.Parent; })) // if we have no in-vdf-path Node parent (i.e. if we might be the node-root)
			// if we have no in-vdf-path Node ancestor (i.e. if we might be the node-root)
			if (s.Parent == null || !relativeVDFPath.nodes.Any(function(a) { return a instanceof Node && ancestors.Contains(a.obj); })) {
				// and we're at the initial-call, (and there's no copy of self higher up in call-chain), then this object is (within this being-serialized-pack) 'attached' at this position; return null/no-path
				if (initialCall && relativeVDFPath.nodes.VCount(function(a) { return a.obj == this; }) <= 1)
					return null;
				// if not deepest vdf-path-obj (i.e. if this call is a call-up) and not shallowest vdf-path-obj (i.e. if not same as vdf-root), we're a valid node-root, so return as path root
				if (this != relativeVDFPath.currentNode.obj && this != relativeVDFPath.rootNode.obj)
					return new NodePath([new NodePathNode().Init({ nodeRoot: true })]);
				// else, return the path from vdf-root to self
				var posInVDFPath = relativeVDFPath.nodes.FindIndex(function(a) { return a.obj == s; });
				var fromVDFRootToSelf = relativeVDFPath.nodes.Take(posInVDFPath + 1);
				return new VDFNodePath(fromVDFRootToSelf).ToNodePath(true);
			}

			// start or continue a path-finding call-up-chain
			var path = s.Parent.GetPath_Relative(relativeVDFPath, false);
			if (path != null)
				//path.nodes.AddRange(s.attachPoint.ToPathNodes(relativeVDFPath));
				path.nodes.AddRange(s.attachPoint.ToPathNodes(relativeVDFPath.nodes.Where(function(a) { return a.obj instanceof Node; }).Select(function(a) { return a.obj; }).First()));
			return path;
		}
		else {
			var nodeRoot = arg1;

			// check if we're the root of a path-finding call-up-chain
			if (this == nodeRoot)
				return new NodePath([new NodePathNode().Init({ nodeRoot: true })]);
			if (s.Parent == null) // if failed to get up to node-root
				return null;

			// start or continue a path-finding call-up-chain
			var path = s.Parent.GetPath_Relative(nodeRoot, false);
			if (path != null)
				path.nodes.AddRange(s.attachPoint.ToPathNodes(nodeRoot));
			return path;
		}
	};

	s.IsConnectedToMainTree = function() { return VO == this || (s.Parent != null && s.Parent.IsConnectedToMainTree()); }

	s.PreAdd = function(newAttachPoint, /*o:*/ info) {
		//if (!parent.IsConnectedToMainTree())
		//	throw new Error("New parent is not connected to v-tree!");
		if (s.attachPoint != null && !s.attachPoint.Equals(newAttachPoint))
			throw new Error("Node of type \"" + s.GetTypeName() + "\" cannot be attached to v-tree in more than one place (other than by reference). Existing attach-point: " + s.attachPoint + " Attempted attach-point: " + newAttachPoint);

		// if real attachment (i.e. if caller wasn't the FakeAdd method)
		if (newAttachPoint) {
			if (info == null)
				info = new SubtreeAddInfo(newAttachPoint.parent, newAttachPoint.parent == null || newAttachPoint.parent.IsConnectedToMainTree(), newAttachPoint.prop, this);

			if (info.anchorInMainTree)
				s.CallMethod("_PreAdd", newAttachPoint, info);
			else
				s.CallMethod(ToMainTree, "_PreAdd", newAttachPoint, info);
			if (info.anchorObj != newAttachPoint.parent && s.Parent == null) // anchor stuff *within* subtree during the PreAdd phase; anchor subtree itself during the PostAdd phase
				s.SetAttachPoint(newAttachPoint);

			if (newAttachPoint.parent._childPlaceholders[newAttachPoint.prop.name]) {
				newAttachPoint.parent._childPlaceholders[newAttachPoint.prop.name].TransferDataTo(s);
				delete newAttachPoint.parent._childPlaceholders[newAttachPoint.prop.name];
			}
		}
		else
			if (info == null)
				info = new SubtreeAddInfo(null, true, null, this);

		var typeInfo = s.GetVDFTypeInfo();
		for (var propName in s)
			if (!(s[propName] instanceof Function) && propName != "s" && !propName.startsWith("_") && !typeInfo.GetProp(propName).tags.Any(function(a) { return a instanceof VDFProp && !a.includeL2; })) {
				//var prop = VDFTypeInfo.Get(s.constructor.GetName()).props[propName];
				var prop = VDFTypeInfo.Get(s.constructor.GetName()).GetProp(propName);
				var propValue = s[propName];
				if (!prop || !prop.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; })) // if not by reference
					if (propValue instanceof Array)
						for (var i in propValue) {
							if (propValue[i] instanceof Node)
								propValue[i].PreAdd(new NodeAttachPoint(this, prop, i), info);
						}
					else if (propValue instanceof Dictionary)
						for (var i = 0, pair = null, pairs = propValue.Pairs; i < pairs.length && (pair = pairs[i]) ; i++) {
							/*if (pair.key instanceof Node)
								pair.key.PreAdd(new NodeAttachPoint(this, prop, null, i), info);*/
							if (pair.value instanceof Node)
								pair.value.PreAdd(new NodeAttachPoint(this, prop, null, null, pair.key), info);
						}
					else if (propValue instanceof Node)
						propValue.PreAdd(new NodeAttachPoint(this, prop), info);
			}
	};
	s.PostAdd = function(newAttachPoint, /*o:*/ info) {
		// if real attachment (i.e. if caller wasn't the FakeAdd method)
		if (newAttachPoint) {
			if (info == null) {
				info = new SubtreeAddInfo(newAttachPoint.parent, newAttachPoint.parent == null || newAttachPoint.parent.IsConnectedToMainTree(), newAttachPoint.prop, this);
				s.SetAttachPoint(newAttachPoint); // anchor stuff *within* subtree during the PreAdd phase; anchor subtree itself during the PostAdd phase
			}
		}
		else if (info == null)
			info = new SubtreeAddInfo(null, true, null, this);

		if (info.anchorInMainTree)
			s.CallMethod("_PostAdd_Early", newAttachPoint, info);
		else
			s.CallMethod(ToMainTree, "_PostAdd_Early", newAttachPoint, info);

		var typeInfo = s.GetVDFTypeInfo();
		for (var propName in s)
			if (!(s[propName] instanceof Function) && propName != "s" && !propName.startsWith("_") && !typeInfo.GetProp(propName).tags.Any(function(a) { return a instanceof VDFProp && !a.includeL2; })) {
				var prop = VDFTypeInfo.Get(s.constructor.GetName()).props[propName];
				var propValue = s[propName];
				var byReference = prop && prop.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; });
				if (propValue instanceof Array) {
					for (var i in propValue) {
						var item = propValue[i];
						if (info.anchorInMainTree)
							s.CallMethod(IgnoreStartData, propName + "_PostAdd_Early", item);
						if (item instanceof Node && !byReference)
							item.PostAdd(new NodeAttachPoint(this, prop, i), info);
						if (info.anchorInMainTree)
							s.CallMethod(IgnoreStartData, propName + "_PostAdd", item);
					}
				}
				else if (propValue instanceof Dictionary) {
					for (var i = 0, pair = null, pairs = propValue.Pairs; i < pairs.length && (pair = pairs[i]) ; i++) {
						/*if (pair.key instanceof Node && !byReference)
							pair.key.PostAdd(new NodeAttachPoint(this, prop, null, i), info);*/

						if (info.anchorInMainTree)
							s.CallMethod(IgnoreStartData, propName + "_PostAdd_Early", pair.key, pair.value);
						if (pair.value instanceof Node && !byReference)
							pair.value.PostAdd(new NodeAttachPoint(this, prop, null, null, pair.key), info);
						if (info.anchorInMainTree)
							s.CallMethod(IgnoreStartData, propName + "_PostAdd", pair.key, pair.value);
					}
				}
				else {
					if (info.anchorInMainTree)
						s.CallMethod(IgnoreStartData, propName + "_PostSet_Early", propValue);
					if (propValue instanceof Node && !byReference)
						propValue.PostAdd(new NodeAttachPoint(this, prop), info);
					if (info.anchorInMainTree)
						s.CallMethod(IgnoreStartData, propName + "_PostSet", propValue);
				}
			}

		if (info.anchorInMainTree)
			s.CallMethod("_PostAdd", newAttachPoint, info);
		else
			s.CallMethod(ToMainTree, "_PostAdd", newAttachPoint, info);
	};
	s.FakeAdd = function(newAttachPoint) {
		s.PreAdd(newAttachPoint);
		s.PostAdd(newAttachPoint);
	};

	s.BroadcastMessage = function(group, methodName, args___) {
		var args = V.Slice(arguments, 2);

		if (group == ContextGroup.Local_CSAndUI || group == ContextGroup.Local_CS) //targetContext == "all" || targetContext == "cs")
			//CSBridge.CallCS("VO.main.Node_BroadcastMessage", [s.GetPath_Absolute(), "cs", methodName].concat(args));
			CSBridge.CallCS.apply(null, ["VO.main.Node_BroadcastMessage", s.GetPath_Absolute(), ContextGroup.Local_CS, methodName].concat(args));
		if (group == ContextGroup.Local_CSAndUI || group == ContextGroup.Local_UI) {
			s.SendMessage(ContextGroup.Local_UI, methodName, args);
			for (var i in s._children)
				s._children[i].BroadcastMessage.apply(s._children[i], arguments); //[ContextGroup.Local_UI, methodName].concat(args));
		}
	};
	s.SendMessage = function(group, methodName, args___) { // if last arg is a function, it will be used as the callback
		var args = V.Slice(arguments, 2);

		if (!(group instanceof ContextGroup)) //(group in ContextGroup))
			throw new Error("Group '" + group + "' is invalid.");

		var result = null;
		if (group == ContextGroup.Local_CS || group == ContextGroup.Local_CSAndUI) //targetContext == "all" || targetContext == "cs")
			//CSBridge.CallCS("VO.main.Node_SendMessage", [s.GetPath_Absolute(), "cs", methodName].concat(args));
			CSBridge.CallCS.apply(null, ["VO.main.Node_SendMessage", s.GetPath_Absolute(), ContextGroup.Local_CS, methodName].concat(args));
		if (group == ContextGroup.Local_CSAndUI || group == ContextGroup.Local_UI)
			result = s.CallMethod.apply(s, [methodName].concat(args)); // maybe todo: have this trigger error if the method is not present
		return result;
	};

	// maybe temp
	//s.SubmitState = function(group) { SubmitState(ToVDF(this), group); }
	s.SubmitState = function(stateVDF, group) {
		group = group || ContextGroup.Local_CSAndUI;
		//s.CallMethod("PreSubmitState");
		//if (group == ContextGroup.Local_CSAndUI || group == ContextGroup.Local_CS) // if CS is part of target
		//	s.SendMessage(ContextGroup.Local_CS, "VO.SubmitState", stateVDF, ContextGroup.Local_CS); // send state submission to CS v-tree
		if (group == ContextGroup.Local_CSAndUI || group == ContextGroup.Local_UI) {
			/*var options = FinalizeFromVDFOptions(new VDFLoadOptions());
			var stateNode = VDFLoader.ToVDFNode(stateVDF, VDF.GetTypeNameOfObject(s), options);
			stateNode.IntoObject(s, options);
            for (var propName in stateNode.mapChildren.Keys)
			{
            	var prop = VDFTypeInfo.Get(VDF.GetTypeNameOfObject(s)).GetProp(propName);
				VO.SubmitChange(new Change_Set(this, prop, s[propName]), ContextGroup.Local_UI);
			}*/
			FromVDFInto(stateVDF, s);
			s.FakeAdd(s.attachPoint);

			s.CallMethod("PostSubmitState");
		}
	};

	s.a = function(propName) { return new PropertyWrapper(this, propName); };

	s._childPlaceholders = {};
	s.GetSafeChild = function(childName) {
		if (s[childName])
			return s[childName];
		if (!s._childPlaceholders[childName])
			s._childPlaceholders[childName] = new NodePlaceholder();
		return s._childPlaceholders[childName];
	};

	s._extraMethods = {};
	s.AddExtraMethod = function(methodName, method, /*o:*/ allowAddingDuplicate) {
		allowAddingDuplicate = allowAddingDuplicate != null ? allowAddingDuplicate : true;

		if (!allowAddingDuplicate && s._extraMethods[methodName] && s._extraMethods[methodName].Any(function() { return this.toString() == method.toString(); })) // if any extra-method exists with the same code, consider the method not-new
			return;

		if (s._extraMethods[methodName] == null)
			s._extraMethods[methodName] = [];
		s._extraMethods[methodName].push(method);
	};
	s.RemoveExtraMethod = function(methodName, method) {
		if (s._extraMethods[methodName])
			for (var i = 0; i < s._extraMethods[methodName].length; i++) {
				var method2 = s._extraMethods[methodName][i];
				if (method2.toString() == method.toString()) {
					s._extraMethods[methodName].Remove(method2);
					return;
				}
			}
	};
	s.RemoveExtraMethod_ByRef = function(methodName, method) {
		if (s._extraMethods[methodName])
			for (var i = 0; i < s._extraMethods[methodName].length; i++) {
				var method2 = s._extraMethods[methodName][i];
				if (method2 == method) {
					s._extraMethods[methodName].Remove(method2);
					return;
				}
			}
	};
	// shorthand version (i.e.: p.extraMethod = function prop_PostSet() {};)
	s._AddSetter_Inline = function extraMethod(func) { s.AddExtraMethod(func.GetName(), func); };
	s._AddSetter_Inline = function extraMethod_ifNew(func) { s.AddExtraMethod(func.GetName(), func, false); };
	s._AddSetter_Inline = function removeExtraMethod(func) { s.RemoveExtraMethod(func.GetName(), func); };
	s._AddSetter_Inline = function removeExtraMethod_byRef(func) { s.RemoveExtraMethod_ByRef(func.GetName(), func); };

	/*s.HasMethod = function(methodName, excludeTag)
		{ return (s[methodName] instanceof Function && !s[methodName].GetTags(excludeTag).length) || (s._extraMethods[methodName] && s._extraMethods[methodName].Any(function() { return !this.GetTags(excludeTag).length; })); };*/
	s.CallMethod = function(excludeTag_orMethodName, methodName, args___) {
		var excludeTag;
		if (typeof excludeTag_orMethodName == "string") {
			//return s.CallMethod.apply(this, [null, methodName, V.Slice(arguments, 2)]);
			excludeTag = null;
			if (V.Slice(arguments).Last() instanceof Change && V.Slice(arguments).Last().messages.Contains("from file"))
				excludeTag = IgnoreStartData;
			return s.CallMethod.apply(this, [excludeTag].concat(V.Slice(arguments)));
		}
		excludeTag = excludeTag_orMethodName;

		var args = V.Slice(arguments, 2);
		var method = s[methodName];
		if (method instanceof Function && (excludeTag == null || method.tags == null || !method.tags.Any(function() { return this instanceof excludeTag; })))
			var result = method.apply(s, args); // for now, only main method can send back result
		if (s._extraMethods[methodName] != null)
			for (var i in s._extraMethods[methodName]) {
				var method2 = s._extraMethods[methodName][i];
				if (excludeTag == null || method2.tags == null || !method2.tags.Any(function() { return this instanceof excludeTag; }))
					method2.apply(null, args); //s, args);
			}

		// number-suffix-based extra-method system not added to JS, since JS has a good alternative already

		return result;
	};
};

Node.prototype._AddFunction_Inline = Node_p;