using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using VDFN;
using VTree;

namespace VTree_Structures
{
	//[VDFType(propIncludeRegexL1: VDF.PropRegex_Any)] public class Change
	[VDFType(propIncludeRegexL1: "^(?!_)")] public class Change
	{
		//public Change() { sourceContext = "cs_" + Environment.UserName; } // todo: obviously, this needs to be changed to be based on a unique (and probably persistent) user-id
		public Change(Node obj, VPropInfo prop)
		{
			//sourceContext = "cs_" + Environment.UserName;

			this.obj = obj;
			propInfo = prop;
		}

		[VDFPreDeserialize] protected void PreDeserialize() { messages = messages ?? new List<object>(); }
		/*[VDFDeserialize] protected object Deserialize(VDFNode node)
		{
			//obj = node["obj"].ToObject<Node>(VConvert.FinalizeFromVDFOptions(new VDFLoadOptions()));
			//node.mapChildren.Remove(node["obj"]);

			//var typeInfo = VTypeInfo.Get(obj.GetType());
			//var typeInfo = VTypeInfo.Get(VDF.GetTypeByName(node["obj"].metadata, VConvert.FinalizeFromVDFOptions(new VDFLoadOptions())));
			var typeInfo = VTypeInfo.Get(BiomeDefense.GetNodeByNodePath(node["obj"].ToObject<NodePath>(VConvert.FinalizeFromVDFOptions(new VDFLoadOptions()))).GetType());
			if (!typeInfo.props.ContainsKey(node["propName"]))
				throw new Exception("Type " + typeInfo.props.Values.First().memberInfo.DeclaringType + " does not contain a property named " + node["propName"] + ".");
			propInfo = typeInfo.props[node["propName"]];
			node.mapChildren.Remove("propName");

			return VDF.NoActionTaken;
		}*/
		//[VDFPostDeserialize] public void PostDeserialize(VDFNode node, VDFNodePath path, VDFLoadOptions options) { options.AddObjPostDeserializeFunc(path.rootNode.obj, PostInit); } // have this run after v-tree references are resolved
		[VDFPostDeserialize] public void PostDeserialize() { PostInit(); } // this can run before normal v-tree-reference-resolution phase, since Change.obj is resolved early
		public void PostInit()
		{
			if (propName == null) // maybe make-so: you're sure whether this is needed
				return;

			var typeInfo = VTypeInfo.Get(obj.GetType());
			if (!typeInfo.props.ContainsKey(propName))
				throw new Exception("Type " + obj.GetType() + " does not contain a property named " + propName + ".");
			propInfo = typeInfo.props[propName];
		}
		[VDFPreSerialize] protected void PreSerialize(VDFNodePath path, VDFSaveOptions options)
		{
			propName = propInfo.memberInfo.Name;
			//options.messages.Add(propInfo); // send the "real"/contained-by-change prop-info, for custom Serialize methods to use
		}

		public string sourceContext = "cs_" + Environment.UserName;
		[ByPath] public Node obj;
		[VDFProp(false)] public VPropInfo propInfo;
		protected string propName;
		public string GetPropName() { return propInfo != null ? propInfo.memberInfo.Name : propName; }
		[D(D.Empty)] public List<object> messages = new List<object>();
		public Change AddMessages(params object[] messages) // does not add messages that are null
		{
			this.messages.AddRange(messages.Where(a=>a != null));
			return this;
		}
		//public int gameTime; // if specified, change is not applied until a change-submission is received (from each source) for a later game-time

		public virtual void PreApply() {} //throw new NotImplementedException(); }
		public virtual void Apply() { throw new NotImplementedException(); }
	}
	//public class Change_Set<TProp> : Change // removed generics on the Change classes, to make them more easily compatible with the JS versions (generics aren't very helpful for Change objects anyway)
	public class Change_Set : Change
	{
		public Change_Set(Node obj, VPropInfo prop, object value) : base(obj, prop) { this.value = value; }

		[ByPath(true)] public object value;

		//[VDFProp(false)] public bool allowSerializeValueByPath = true; // maybe todo: add to JS

		public override void PreApply()
		{
			// maybe temp
			var oldValueAsNode = propInfo.GetValue(obj) as Node;
			if (oldValueAsNode != null && oldValueAsNode.Parent == obj && oldValueAsNode.attachPoint.prop == propInfo)
			{
				oldValueAsNode.CallMethod("_PreRemoveFromParent", this);
				if (obj.IsConnectedToMainTree())
					oldValueAsNode.BroadcastMessage(ContextGroup.Local_CS, "_PreRemoveFromMainTree", this);
			}

			obj.CallMethod(propInfo.memberInfo.Name + "_PreSet", value, this);

			//var newToVTree = obj.IsConnectedToMainTree() && !propInfo.tags.Any(a=>a is ByPath || a is ByName); // new to v-tree, assuming no parent is set (if there is one set, an error will occur)
			if (value is Node && !propInfo.tags.Any(a=>a is ByPath || a is ByName))
				(value as Node).PreAdd(new NodeAttachPoint(obj, propInfo)); //, this);
		}
		public override void Apply()
		{
			//obj.CallMethod(propInfo.memberInfo.Name + "_PreSet", value, this);

			var oldValue = propInfo.GetValue(obj);

			// special handling for doubles/floats
			if (value is double && propInfo.GetPropType() == typeof(float))
				//propInfo.SetValue(obj, (float)(double)value);
				propInfo.SetValue(obj, Convert.ChangeType(value, propInfo.GetPropType()));
			else
				propInfo.SetValue(obj, value);

			var oldValueAsNode = oldValue as Node;
			if (oldValueAsNode != null && oldValueAsNode.Parent == obj && oldValueAsNode.attachPoint.prop == propInfo)
				oldValueAsNode.SetAttachPoint(null);
			//var newToMainTree = obj.IsConnectedToMainTree() && !propInfo.tags.Any(a=>a is ByPath || a is ByName);
			//if (newToVTree) //&& propInfo.memberInfo.GetCustomAttributes(true).Any(a=>a is ByPath))
			//	(value as Node).OnAddedToVTree_Early(obj, new NodePathNode(propInfo.memberInfo.Name)); //, this);

			obj.CallMethod(propInfo.memberInfo.Name + "_PostSet_Early", oldValue, this);
			if (value is Node && !propInfo.tags.Any(a=>a is ByPath || a is ByName))
				(value as Node).PostAdd(new NodeAttachPoint(obj, propInfo));
			obj.CallMethod(propInfo.memberInfo.Name + "_PostSet", oldValue, this);
		}
	}

	// while we could just send Set changes with the final value, an Add change helps keep meaning clear for logs, as well as clashes less with other changes (i.e. two Add changes can be confidently combined, but not two Set changes)
	// (however, it's fine to apply the change using a newly-created Set change, since the two advantages above are then already applied)
	public class Change_Increase_Number : Change {
		/*[VDFDeserialize] Change_Increase_Number Deserialize(VDFNode node) {
			return node.
		}*/
		/*public string ToShortString() {
			return "7," + 
		}*/

		public Change_Increase_Number(Node obj, VPropInfo prop, int amount) : base(obj, prop) {
			amount_int = amount;
			PostInit();
		}
		public Change_Increase_Number(Node obj, VPropInfo prop, double amount) : base(obj, prop) {
			amount_double = amount;
			PostInit();
		}
		[VDFPostDeserialize] new void PostInit() {
			base.PostInit();
			//_subchange = amount_int.HasValue ? new Change_Set(obj, propInfo, (int)propInfo.GetValue(obj) + amount_int) : new Change_Set(obj, propInfo, propInfo.GetValue(obj).ToDouble() + amount_double);
			_subchange = amount_int.HasValue ? new Change_Set(obj, propInfo, (int)propInfo.GetValue(obj) + amount_int.Value) : new Change_Set(obj, propInfo, propInfo.GetValue(obj).ToDouble() + amount_double.Value);
		}

		Change_Set _subchange;
		//[VDFPostDeserialize] public Change_Add_Int() { _subchange = new Change_Set(obj, propInfo, (int)propInfo.GetValue(obj) + amount); }

		[D(null)] public int? amount_int;
		[D(null)] public double? amount_double;

		public override void PreApply() { _subchange.PreApply(); }
		public override void Apply() { _subchange.Apply(); }
	}

	public class Change_SetItem_List : Change
	{
		public Change_SetItem_List(Node obj, VPropInfo prop, int index, object item) : base(obj, prop)
		{
			this.index = index;
			this.item = item;
		}

		public int index;
		[ByPath(true)] public object item;

		public override void PreApply()
		{
			obj.CallMethod(propInfo.memberInfo.Name + "_PreSetItem", this, index, item);
			if (item is Node && !propInfo.tags.Any(a=>a is ByPath || a is ByName))
				(item as Node).PreAdd(new NodeAttachPoint(obj, propInfo, index));
		}
		public override void Apply()
		{
			(propInfo.GetValue(obj) as IList)[index] = item;
			obj.CallMethod(typeof(IgnoreSetItem), propInfo.memberInfo.Name + "_PostSetItem_Early", this, index, item);
			if (item is Node && !propInfo.tags.Any(a=>a is ByPath || a is ByName))
				(item as Node).PostAdd(new NodeAttachPoint(obj, propInfo, index));
			obj.CallMethod(propInfo.memberInfo.Name + "_PostSetItem", this, index, item);
		}
	}
	public class Change_Add_List : Change {
		public Change_Add_List(Node obj, VPropInfo prop, object item, int index = -1) : base(obj, prop) {
			this.index = index;
			this.item = item;
		}

		public int index = -1;
		[ByPath(true)] public object item;

		public override void PreApply() {
			obj.CallMethod(propInfo.memberInfo.Name + "_PreAdd", item, this);

			//var newToVTree = obj.IsConnectedToVTree() && item is Node && (item as Node)._parent == null;
			//var newToVTree = obj.IsConnectedToMainTree() && !propInfo.tags.Any(a=>a is ByPath || a is ByName);
			if (item is Node && !propInfo.tags.Any(a=>a is ByPath || a is ByName))
				(item as Node).PreAdd(new NodeAttachPoint(obj, propInfo, index != -1 ? index : (propInfo.GetValue(obj) as IList).Count)); //, this);
		}
		public override void Apply() {
			//obj.CallMethod(propInfo.memberInfo.Name + "_PreAdd", item, this);

			var list = propInfo.GetValue(obj) as IList;
			if (index != -1)
				list.Insert(index, item);
			else
				list.Add(item);
			//var newToMainTree = obj.IsConnectedToMainTree() && !propInfo.tags.Any(a=>a is ByPath || a is ByName);
			//if (newToVTree) //&& propInfo.memberInfo.GetCustomAttributes(true).Any(a=>a is ByPath))
			//	(item as Node).OnAddedToVTree_Early(obj, new NodePathNode(propInfo.memberInfo.Name, list.Count - 1)); //, this);

			obj.CallMethod(propInfo.memberInfo.Name + "_PostAdd_Early", item, this);
			if (item is Node && !propInfo.tags.Any(a=>a is ByPath || a is ByName))
				(item as Node).PostAdd(new NodeAttachPoint(obj, propInfo, index != -1 ? index : list.Count - 1));

			// if we're inserting an item, update the list-index values of any Node list items after the insert point (in their path-node structures)
			if (index != -1) {
				var isAnchorForNodeChildren = obj.IsConnectedToMainTree() && !propInfo.tags.Any(a=>a is ByPath || a is ByName);
				if (isAnchorForNodeChildren)
					for (var i = index + 1; i < list.Count; i++)
						if (list[i] is Node)
							(list[i] as Node).attachPoint.list_index++;
			}

			obj.CallMethod(propInfo.memberInfo.Name + "_PostAdd", item, this);
		}
	}
	public class Change_Remove_List : Change {
		public Change_Remove_List(Node obj, VPropInfo prop, int index) : base(obj, prop) { this.index = index; }
		public Change_Remove_List(Node obj, VPropInfo prop, object item) : base(obj, prop) { this.item = item; }

		public int index = -1;
		[ByPath(true)] public object item;

		public override void PreApply() {
			// maybe temp
			var itemAsNode = item as Node;
			var list = propInfo.GetValue(obj) as IList;
			var itemIndex = index != -1 ? index : list.IndexOf(item);
			if (itemAsNode != null && itemAsNode.Parent == obj && itemAsNode.attachPoint.prop == propInfo && itemAsNode.attachPoint.list_index == itemIndex) {
				itemAsNode.CallMethod("_PreRemoveFromParent", this);
				if (obj.IsConnectedToMainTree())
					itemAsNode.BroadcastMessage(ContextGroup.Local_CS, "_PreRemoveFromMainTree", this);
			}

			obj.CallMethod(propInfo.memberInfo.Name + "_PreRemove", item, this);
		}
		public override void Apply() {
			//obj.CallMethod(propInfo.memberInfo.Name + "_PreRemove", item, this);

			var list = propInfo.GetValue(obj) as IList;
			var itemIndex = index != -1 ? index : list.IndexOf(item);
			list.RemoveAt(itemIndex);
			var itemAsNode = item as Node;
			if (itemAsNode != null && itemAsNode.Parent == obj && itemAsNode.attachPoint.prop == propInfo && itemAsNode.attachPoint.list_index == itemIndex)
				itemAsNode.SetAttachPoint(null);

			// if list children are attached to the v-tree through this list, update list-index values of the other list items (in their path-node structures)
			var isAnchorForNodeChildren = obj.IsConnectedToMainTree() && !propInfo.tags.Any(a=>a is ByPath || a is ByName);
			if (isAnchorForNodeChildren)
				for (var i = itemIndex; i < list.Count; i++)
					if (list[i] is Node)
						(list[i] as Node).attachPoint.list_index--;

			obj.CallMethod(propInfo.memberInfo.Name + "_PostRemove", item, this);
		}
	}
	public class Change_Clear_List : Change {
		public Change_Clear_List(Node obj, VPropInfo prop) : base(obj, prop) { PostInit(); }
		[VDFPostDeserialize] new void PostInit() {
			base.PostInit();
			_subchanges = new List<Change_Remove_List>();
			var list = propInfo.GetValue(obj) as IList;
			foreach (var item in list)
				_subchanges.Add(new Change_Remove_List(obj, propInfo, item));
		}

		List<Change_Remove_List> _subchanges;

		public override void PreApply() {
			foreach (var change in _subchanges)
				change.PreApply();
		}
		public override void Apply() {
			foreach (var change in _subchanges)
				change.Apply();
		}
	}

	public class Change_Add_Dictionary : Change {
		public Change_Add_Dictionary(Node obj, VPropInfo prop, object key, object value, bool overwrite) : base(obj, prop) {
			this.key = key;
			this.value = value;
			this.overwrite = overwrite;
		}
		[VDFPostDeserialize] new void PostInit() {
			base.PostInit();
			value = V.FixAmbiguousFromJSValue(value, propInfo.GetPropType().GetGenericArguments()[1]);
		}

		//public object key;
		[ByPath(true)] public object key;
		[ByPath(true)] public object value;
		public bool overwrite;

		public override void PreApply() {
			obj.CallMethod(propInfo.memberInfo.Name + "_PreAdd", key, value, this);

			//var newToVTree = obj.IsConnectedToMainTree() && !propInfo.tags.Any(a=>a is ByPath || a is ByName);
			if (value is Node && !propInfo.tags.Any(a=>a is ByPath || a is ByName))
				(value as Node).PreAdd(new NodeAttachPoint(obj, propInfo, map_key: key)); //, this);
		}
		public override void Apply() {
			//obj.CallMethod(propInfo.memberInfo.Name + "_PreAdd", key, value, this);

			if (overwrite)
				(propInfo.GetValue(obj) as IDictionary)[key] = value;
			else
				(propInfo.GetValue(obj) as IDictionary).Add(key, value);
			//var newToMainTree = obj.IsConnectedToMainTree() && !propInfo.tags.Any(a=>a is ByPath || a is ByName);
			//if (newToVTree)
			//	(value as Node).OnAddedToVTree_Early(obj, new NodePathNode(propInfo.memberInfo.Name, -1, key)); //, this);

			obj.CallMethod(propInfo.memberInfo.Name + "_PostAdd_Early", key, value, this);
			if (value is Node && !propInfo.tags.Any(a=>a is ByPath || a is ByName))
				(value as Node).PostAdd(new NodeAttachPoint(obj, propInfo, map_key: key));
			obj.CallMethod(propInfo.memberInfo.Name + "_PostAdd", key, value, this);
		}
	}
	public class Change_Remove_Dictionary : Change
	{
		public Change_Remove_Dictionary(Node obj, VPropInfo prop, object key) : base(obj, prop) { this.key = key; }

		[ByPath(true)] public object key;

		public override void PreApply()
		{
			// maybe temp
			var oldValueAsNode = (propInfo.GetValue(obj) as IDictionary)[key] as Node;
			if (oldValueAsNode != null && oldValueAsNode.Parent == obj && oldValueAsNode.attachPoint.prop == propInfo && oldValueAsNode.attachPoint.map_key == key)
			{
				oldValueAsNode.CallMethod("_PreRemoveFromParent", this);
				if (obj.IsConnectedToMainTree())
					oldValueAsNode.BroadcastMessage(ContextGroup.Local_CS, "_PreRemoveFromMainTree", this);
			}

			obj.CallMethod(propInfo.memberInfo.Name + "_PreRemove", key, this);
		}
		public override void Apply()
		{
			//obj.CallMethod(propInfo.memberInfo.Name + "_PreRemove", key, this);

			var value = (propInfo.GetValue(obj) as IDictionary)[key];
			(propInfo.GetValue(obj) as IDictionary).Remove(key);
			var valueAsNode = value as Node;
			if (valueAsNode != null && valueAsNode.Parent == obj && valueAsNode.attachPoint.prop == propInfo && valueAsNode.attachPoint.map_key == key)
				valueAsNode.SetAttachPoint(null);

			obj.CallMethod(propInfo.memberInfo.Name + "_PostRemove", key, this);
		}
	}
	public class Change_Clear_Dictionary : Change
	{
		public Change_Clear_Dictionary(Node obj, VPropInfo prop) : base(obj, prop) { PostInit(); }
		[VDFPostDeserialize] new void PostInit()
		{
			base.PostInit();
			_subchanges = new List<Change_Remove_Dictionary>();
			var dictionary = propInfo.GetValue(obj) as IDictionary;
			foreach (var key in dictionary.Keys)
				_subchanges.Add(new Change_Remove_Dictionary(obj, propInfo, key));
		}

		List<Change_Remove_Dictionary> _subchanges;

		public override void PreApply()
		{
			foreach (var change in _subchanges)
				change.PreApply();
		}
		public override void Apply()
		{
			foreach (var change in _subchanges)
				change.Apply();
		}
	}
}