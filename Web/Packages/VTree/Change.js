Type(new VDFType(VDF.PropRegex_Any)).set = function Change() {
	var s = this;

	//s.Change_PostDeserialize = function(node, path, options) { options.AddObjPostDeserializeFunc(path.rootNode.obj, s.PostInit); }.AddTags(new VDFPostDeserialize()); // have this run after v-tree references are resolved
	s.Change_PostDeserialize = function() { s.PostInit(); }.AddTags(new VDFPostDeserialize()); // this can run before normal v-tree-reference-resolution phase, since Change.obj is resolved early
	s.PostInit = function() { s.propInfo = s.propInfo || s.obj.GetVDFTypeInfo().GetProp(s.propName); };
	s.Change_PreSerialize = function() { s.propName = s.propInfo.name; }.AddTags(new VDFPreSerialize());

	s.sourceContext = "ui"; // maybe todo: add user-id to end of id/name/key
	s.obj = Prop(s, "obj", new ByPath()).set = null;
	s.propInfo = Prop(s, "propInfo", new VDFProp(false)).set = null;
	s.propName = null;
	s.GetPropName = function() { return s.propInfo != null ? s.propInfo.name : s.propName; };
	s.p("messages", new D(D.Empty)).set = [];
	s.AddMessages = function(messages___) { // does not add messages that are null
		var messages = V.Slice(arguments);
		s.messages.AddRange(messages.Where(function(a) { return a != null; }));
		return this;
	};
	//s.gameTime = 0; //-1; // if specified, change is not applied until a change-submission is received (from each source) for a later game-time

	//s.PreApply = function() { throw new NotImplementedException(); };
	//s.Apply = function() { throw new NotImplementedException(); };
};
Change.prototype.AddHelpers();

//[VDFType(propIncludeRegexL1: "^(?!_)")] // JS side doesn't need this, since 'private' props of these classes aren't actually even attached to the objects
Change.SetAsBaseClassFor = function Change_Set(obj, propInfo, value) {
	var s = this.CallBaseConstructor();
	s.value = Prop(s, "value", new ByPath(true)).set = null;
	if (obj) {
		s.obj = obj;
		s.propInfo = propInfo;
		s.value = value;
		s.PostInit();
	}

	s.PreApply = function() {
		var oldValue = s.obj[s.propInfo.name];
		if (oldValue instanceof Node && oldValue.Parent == s.obj && oldValue.attachPoint.prop == s.propInfo) {
			oldValue.CallMethod("_PreRemoveFromParent", s);
			if (s.obj.IsConnectedToMainTree())
				oldValue.BroadcastMessage(ContextGroup.Local_UI, "_PreRemoveFromMainTree", this);
		}

		s.obj.CallMethod(s.propInfo.name + "_PreSet", s.value, s);

		//var prop_byReference = s.propInfo.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; });
		//var newToVTree = s.obj.IsConnectedToMainTree() && !prop_byReference && s.value instanceof Node && s.value.Parent == null;

		//var newToVTree = s.obj.IsConnectedToMainTree() && !s.propInfo.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; }); // new to v-tree, assuming no parent is set (if there is one set, an error will occur)
		if (s.value instanceof Node && !s.propInfo.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; }))
			s.value.PreAdd(new NodeAttachPoint(s.obj, s.propInfo));
	};
	s.Apply = function() {
		//s.obj.CallMethod(s.propInfo.name + "_PreSet", s.value, s);

		var oldValue = s.obj[s.propInfo.name];
		s.obj[s.propInfo.name] = s.value;
		if (oldValue instanceof Node && oldValue.Parent == s.obj && oldValue.attachPoint.prop == s.propInfo)
			oldValue.SetAttachPoint(null);
		//var newToVTree = s.obj.IsConnectedToMainTree() && !s.propInfo.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; });
		//if (newToVTree) //s.propInfo.tags.Any(function() { return this instanceof ByPath; }))
		//	s.value.OnAddedToVTree_Early(s.obj, new NodePathNode(s.propInfo.name));

		s.obj.CallMethod(s.propInfo.name + "_PostSet_Early", oldValue, s);
		if (s.value instanceof Node && !s.propInfo.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; }))
			s.value.PostAdd(new NodeAttachPoint(s.obj, s.propInfo));
		s.obj.CallMethod(s.propInfo.name + "_PostSet", oldValue, s);
	};
};

Change.SetAsBaseClassFor = function Change_Increase_Number(obj, propInfo, amount) {
	var s = this.CallBaseConstructor();
	s.p("amount_int", new D(null)).set = null;
	s.p("amount_double", new D(null)).set = null;
	if (obj) {
		s.obj = obj;
		s.propInfo = propInfo;
		if (IsInt(s.amount_int))
			s.amount_int = amount;
		else
			s.amount_double = amount;
		s.PostInit();
	}

	var _subchange;
	var postInit = function() { _subchange = new Change_Set(s.obj, s.propInfo, s.obj[s.propInfo.name] + (s.amount_int != null ? s.amount_int : s.amount_double)); };
	s.PostDeserialize = postInit.AddTags(new VDFPostDeserialize());
	if (obj) // if created specifically, rather than by VDF system
		postInit();

	s.PreApply = function() { _subchange.PreApply(); };
	s.Apply = function() { _subchange.Apply(); };
};

Change.SetAsBaseClassFor = function Change_SetItem_List(obj, propInfo, index, item) {
	var s = this.CallBaseConstructor();
	s.index = 0;
	s.item = Prop(s, "item", new ByPath(true)).set = null;
	if (obj) {
		s.obj = obj;
		s.propInfo = propInfo;
		s.index = index;
		s.item = item;
		s.PostInit();
	}

	s.PreApply = function()
	{
		s.obj.CallMethod(s.propInfo.name + "_PreSetItem", s, s.index, s.item);
		if (s.item instanceof Node && !s.propInfo.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; }))
			s.item.PreAdd(new NodeAttachPoint(s.obj, s.propInfo, s.index));
	};
	s.Apply = function()
	{
		s.obj[s.propInfo.name][s.index] = s.item;
		s.obj.CallMethod(s.propInfo.name + "_PostSetItem_Early", s, s.index, s.item);
		if (s.item instanceof Node && !s.propInfo.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; }))
			s.item.PostAdd(new NodeAttachPoint(s.obj, s.propInfo, s.index));
		s.obj.CallMethod(s.propInfo.name + "_PostSetItem", s, s.index, s.item);
	};
};
Change.SetAsBaseClassFor = function Change_Add_List(obj, propInfo, item, index)
{
	var s = this.CallBaseConstructor();
	s.index = -1;
	s.item = Prop(s, "item", new ByPath(true)).set = null;
	if (obj)
	{
		s.obj = obj;
		s.propInfo = propInfo;
		s.index = index != null ? index : -1;
		s.item = item;
		s.PostInit();
	}

	s.PreApply = function()
	{
		s.obj.CallMethod(s.propInfo.name + "_PreAdd", s.item, s);

		//var newToVTree = s.obj.IsConnectedToMainTree() && !s.propInfo.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; });
		if (s.item instanceof Node && !s.propInfo.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; }))
			s.item.PreAdd(new NodeAttachPoint(s.obj, s.propInfo, s.index != -1 ? s.index : s.obj[s.propInfo.name].length));
	};
	s.Apply = function()
	{
		//s.obj.CallMethod(s.propInfo.name + "_PreAdd", s.item, s);

		var list = s.obj[s.propInfo.name];
		if (s.index != -1)
			list.Insert(s.index, s.item);
		else
			list.Add(s.item);
		//var newToVTree = s.obj.IsConnectedToMainTree() && !s.propInfo.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; });
		//if (newToVTree) //&s.propInfo.tags.Any(function() { return this instanceof ByPath; }))
		//	s.item.OnAddedToVTree_Early(s.obj, new NodePathNode(s.propInfo.name, list.length - 1));

		s.obj.CallMethod(s.propInfo.name + "_PostAdd_Early", s.item, s);
		if (s.item instanceof Node && !s.propInfo.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; }))
			s.item.PostAdd(new NodeAttachPoint(s.obj, s.propInfo, s.index != -1 ? s.index : list.length - 1));

		// if we're inserting an item, update the list-index values of any Node list items after the insert point (in their path-node structures)
		if (s.index != -1)
		{
			var isAnchorForNodeChildren = s.obj.IsConnectedToMainTree() && !s.propInfo.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; });
			if (isAnchorForNodeChildren)
				for (var i = s.index + 1; i < list.Count; i++)
					if (list[i] instanceof Node)
						list[i].attachPoint.list_index++;
		}

		s.obj.CallMethod(s.propInfo.name + "_PostAdd", s.item, s);
	};
};
Change.SetAsBaseClassFor = function Change_Remove_List(obj, propInfo, indexOrItem) {
	var s = this.CallBaseConstructor();
	s.index = -1;
	s.p("item", new ByPath()).set = null;
	if (obj) {
		var index = indexOrItem.GetTypeName() == "int" ? indexOrItem : -1;
		var item = indexOrItem.GetTypeName() != "int" ? indexOrItem : null;

		s.obj = obj;
		s.propInfo = propInfo;
		s.index = index;
		s.item = item;
		s.PostInit();
	}

	s.PreApply = function() {
		// maybe temp
		var list = s.obj[s.propInfo.name];
		var itemIndex = s.index != -1 ? s.index : list.indexOf(s.item);
		if (s.item instanceof Node && s.item.Parent == s.obj && s.item.attachPoint.prop == s.propInfo && s.item.attachPoint.list_index == itemIndex) {
			s.item.CallMethod("_PreRemoveFromParent", s);
			if (s.obj.IsConnectedToMainTree())
				s.item.BroadcastMessage(ContextGroup.Local_UI, "_PreRemoveFromMainTree", this);
		}

		s.obj.CallMethod(s.propInfo.name + "_PreRemove", s.item, s);
	};
	s.Apply = function() {
		//s.obj.CallMethod(s.propInfo.name + "_PreRemove", s.item, s);

		var list = s.obj[s.propInfo.name];
		var itemIndex = s.index != -1 ? s.index : list.indexOf(s.item);
		list.RemoveAt(itemIndex);
		if (s.item instanceof Node && s.item.Parent == s.obj && s.item.attachPoint.prop == s.propInfo && s.item.attachPoint.list_index == itemIndex)
			s.item.SetAttachPoint(null);

		// if list children are attached to the v-tree through this list, update list-index values of the other list items (in their path-node structures)
		var isAnchorForNodeChildren = s.obj.IsConnectedToMainTree() && !s.propInfo.tags.Any(function () { return this instanceof ByPath || this instanceof ByName; });
		if (isAnchorForNodeChildren)
			for (var i = itemIndex; i < list.Count; i++)
				if (list[i] instanceof Node)
					list[i].attachPoint.list_index--;

		s.obj.CallMethod(s.propInfo.name + "_PostRemove", s.item, s);
	};
};
Change.SetAsBaseClassFor = function Change_Clear_List(obj, propInfo)
{
	var s = this.CallBaseConstructor();
	if (obj)
	{
		s.obj = obj;
		s.propInfo = propInfo;
		s.PostInit();
	}

	var _subchanges = [];
	var postInit = function()
	{
		for (var i in s.obj[s.propInfo.name])
			_subchanges.Add(new Change_Remove_List(s.obj, s.propInfo, s.obj[s.propInfo.name][i]));
	};
	s.PostDeserialize = postInit.AddTags(new VDFPostDeserialize());
	if (obj) // if created specifically, rather than by VDF system
		postInit();

	s.PreApply = function()
	{
		for (var i in _subchanges)
			_subchanges[i].PreApply();
	};
	s.Apply = function()
	{
		for (var i in _subchanges)
			_subchanges[i].Apply();
	};
};

Change.SetAsBaseClassFor = function Change_Add_Dictionary(obj, propInfo, key, value, overwrite)
{
	var s = this.CallBaseConstructor();
	s.key = Prop(s, "key", new ByPath(true)).set = null;
	s.value = Prop(s, "value", new ByPath(true)).set = null;
	if (obj)
	{
		s.obj = obj;
		s.propInfo = propInfo;
		s.key = key;
		s.value = value;
		s.overwrite = overwrite;
		s.PostInit();
	}

	s.PreApply = function()
	{
		s.obj.CallMethod(s.propInfo.name + "_PreAdd", s.key, s.value, s);

		//var newToVTree = s.obj.IsConnectedToMainTree() && !s.propInfo.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; });
		if (s.value instanceof Node && !s.propInfo.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; }))
			s.value.PreAdd(new NodeAttachPoint(s.obj, s.propInfo, null, null, s.key));
	};
	s.Apply = function()
	{
		//s.obj.CallMethod(s.propInfo.name + "_PreAdd", s.key, s.value, s);

		if (s.overwrite)
			//if (s.obj[s.propInfo.name] instanceof Dictionary)
			s.obj[s.propInfo.name].Set(s.key, s.value);
			/*else
				s.obj[s.propInfo.name][s.key] = s.value;*/
		else
			s.obj[s.propInfo.name].Add(s.key, s.value);
		//var newToVTree = s.obj.IsConnectedToMainTree() && !s.propInfo.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; });
		//if (newToVTree) //&& s.propInfo.tags.Any(function() { return this instanceof ByPath; }))
		//	s.value.OnAddedToVTree_Early(s.obj, new NodePathNode(s.propInfo.name, null, s.key));

		s.obj.CallMethod(s.propInfo.name + "_PostAdd_Early", s.key, s.value, s);
		if (s.value instanceof Node && !s.propInfo.tags.Any(function() { return this instanceof ByPath || this instanceof ByName; }))
			s.value.PostAdd(new NodeAttachPoint(s.obj, s.propInfo, null, null, s.key));
		s.obj.CallMethod(s.propInfo.name + "_PostAdd", s.key, s.value, s);
	};
};
Change.SetAsBaseClassFor = function Change_Remove_Dictionary(obj, propInfo, key)
{
	var s = this.CallBaseConstructor();
	s.key = Prop(s, "key", new ByPath()).set = null;
	if (obj)
	{
		s.obj = obj;
		s.propInfo = propInfo;
		s.key = key;
		s.PostInit();
	}

	s.PreApply = function()
	{
		// maybe temp
		var value = s.obj[s.propInfo.name][s.key];
		if (value instanceof Node && value.Parent == s.obj && value.attachPoint.prop == s.propInfo && value.attachPoint.map_key == s.key)
		{
			value.CallMethod("_PreRemoveFromParent", s);
			if (s.obj.IsConnectedToMainTree())
				value.BroadcastMessage(ContextGroup.Local_UI, "_PreRemoveFromMainTree", this);
		}

		s.obj.CallMethod(s.propInfo.name + "_PreRemove", s.key, s);
	};
	s.Apply = function()
	{
		//s.obj.CallMethod(s.propInfo.name + "_PreRemove", s.key, s);

		var value = s.obj[s.propInfo.name][s.key];
		s.obj[s.propInfo.name].Remove(s.key);
		if (value instanceof Node && value.Parent == s.obj && value.attachPoint.prop == s.propInfo && value.attachPoint.map_key == s.key)
			value.SetAttachPoint(null);

		s.obj.CallMethod(s.propInfo.name + "_PostRemove", s.key, s);
	};
};
Change.SetAsBaseClassFor = function Change_Clear_Dictionary(obj, propInfo)
{
	var s = this.CallBaseConstructor();
	if (obj)
	{
		s.obj = obj;
		s.propInfo = propInfo;
		s.PostInit();
	}

	var _subchanges = [];
	var postInit = function()
	{
		for (var key in s.obj[s.propInfo.name])
			_subchanges.Add(new Change_Remove_Dictionary(s.obj, s.propInfo, s.key));
	};
	s.PostDeserialize = postInit.AddTags(new VDFPostDeserialize());
	if (obj) // if created specifically, rather than by VDF system
		postInit();

	s.PreApply = function()
	{
		for (var i in _subchanges)
			_subchanges[i].PreApply();
	};
	s.Apply = function()
	{
		for (var i in _subchanges)
			_subchanges[i].Apply();
	};
};

// make sure "obj" props of Change objects are never serialized
/*for (var key in window)
	if (key.startsWith("Change"))
		Prop(window[key], "obj", null, new VDFProp(false)).set = null;*/

// make sure "obj" props of Change objects are always serialized ByPath
/*for (var key in window)
	if (key == "Change" || key.startsWith("Change_"))
	{
		//Prop(window[key], "obj", "Node", new ByPath()).set = null; // for type-specification, you can either specify it for this "obj" property, or set the metadata to "Node" on the C# side
		Prop(window[key], "obj", new ByPath()).set = null;
		Prop(window[key], "key", new ByPath(true)).set = null;
		Prop(window[key], "value", new ByPath(true)).set = null;
		Prop(window[key], "item", new ByPath(true)).set = null;
	}*/