using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEngine;
using VDFN;
using VTree;

// normal
// ==========

namespace VTree_Structures {
	// old: maybe todo: let tags be added to change-listener methods to have them be triggered for (or the reverse) ancestor events (such as ancestor-line being added to the main v-tree)
	//[AttributeUsage(AttributeTargets.Method)] public class IgnoreDeserialize : Attribute {} // old: todo: add code that uses this

	// wrappers
	// ----------

	public class NodeReference_ByPath {
		public NodeReference_ByPath(Node node) { this.node = node; }

		[ByPath] readonly Node node;

		[VDFSerialize] VDFNode Serialize() { return node.GetPath_Absolute().ToString(); }
		[VDFPostSerialize] void PostSerialize(VDFNode node) {
			// maybe temp; manually correct metadata
			if (node.primitiveValue is string)
				node.metadata = "Node"; //NodePath";
		}
		[VDFDeserialize(fromParent: true)] protected static Node Deserialize_FromParent(VDFNode node, VDFNodePath path, VDFLoadOptions options) {
			//return Node.Node_Deserialize_FromParent(node["node"], path.ExtendAsChild(node, this), options)
			return VOverlay.GetNodeByNodePath(node["node"].ToObject<NodePath>(VConvert.FinalizeFromVDFOptions(new VDFLoadOptions())), path, options);
		}

		// maybe temp; make NodeReference_ByPath objects be equivalent as Dictionary keys, if their target 'node' is the same
		public override int GetHashCode() { return node.GetHashCode(); }
		public override bool Equals(object other) {
			if (!(other is NodeReference_ByPath))
				return false;
			return this == other;
		}
	}

	// property tags
	// ----------

	//[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class ByPath : Attribute {
		public bool saveNormallyForParentlessNode;
		public ByPath(bool saveNormallyForParentlessNode = false) { this.saveNormallyForParentlessNode = saveNormallyForParentlessNode; }
	}
	public class ByName : Attribute {} // save name of Node in prop, rather than the actual data in that Node

	// [old] attachable to prop, meaning: "no serializing this-prop when serializing self-obj [...]"
	// attachable to prop, meaning "do not serialize to within [...]"
	public class NotTo : Attribute {
		public NotTo(string str) {
			str = "," + str + ",";
			file = str.Contains(",file,");
			js = str.Contains(",js,");
			map = str.Contains(",map,");
			objType = str.Contains(",objType,");
			obj = str.Contains(",obj,");
		}
		// order starting from the highest-level/widest/most-able-to-contain-the-others
		public bool file;
		public bool js;
		public bool map;
		public bool objType;
		public bool obj;

		public static implicit operator bool(NotTo s) { return s != null; }
	}

	// method tags
	// ----------

	public class IgnoreStartData : Attribute {} // i.e. IgnoreNonRealChange
	public class IgnoreSetItem : Attribute {}

	public class NoAutoProfile : Attribute {}

	// classes
	// ----------

	[VDFType(propIncludeRegexL1: VDF.PropRegex_Any)]
	public class NodeAttachPoint {
		[VDFPreDeserialize]
		NodeAttachPoint() { }
		public NodeAttachPoint(Node parent, VPropInfo prop, int list_index = -1, int map_keyIndex = -1, object map_key = null) {
			this.parent = parent;
			this.prop = prop;
			this.list_index = list_index;
			this.map_keyIndex = map_keyIndex;
			this.map_key = map_key;
		}

		public Node parent;
		public VPropInfo prop;
		public int list_index = -1;
		public int map_keyIndex = -1;
		public object map_key;

		/*public static bool operator ==(NodeAttachInfo s, NodeAttachInfo b) {}
		public static bool operator !=(NodeAttachInfo s, NodeAttachInfo b) {}*/

		/*public override bool Equals(object obj)
		{
			var other = obj as NodeAttachPoint;
			if (other == null)
				return false;
			return parent == other.parent && prop == other.prop && list_index == other.list_index && map_key == other.map_key;
		}*/
		public override bool Equals(object obj) { return ToString() == obj.ToString(); }
		public override string ToString() { return parent + ")" + prop.Name + (list_index != -1 ? "[" + list_index + "]" : "") + (map_keyIndex != -1 ? ".keys[" + map_keyIndex + "]" : "") + (map_key != null ? "[" + map_key + "]" : ""); }

		public List<NodePathNode> ToPathNodes(Node nodeRoot = null) {
			var result = new List<NodePathNode>();
			if (prop != null)
				result.Add(new NodePathNode(prop: prop));
			if (list_index != -1)
				result.Add(new NodePathNode { listIndex = list_index });
			if (map_keyIndex != -1)
				result.Add(new NodePathNode { mapKeyIndex = map_keyIndex });
			if (map_key != null)
				if (nodeRoot != null)
					result.Add(new NodePathNode(mapKey: map_key, nodeRoot: nodeRoot));
				else
					result.Add(new NodePathNode(mapKey: map_key));
			return result;
		}
	}
	[VDFType(propIncludeRegexL1: VDF.PropRegex_Any)]
	public class NodePathNode {
		public NodePathNode(VPropInfo prop = null, object mapKey = null, Node nodeRoot = null) {
			this.prop = prop;
			if (prop != null)
				prop_str = prop.Name;
			this.mapKey = mapKey;

			/*if (mapKey != null)
				mapKey_str = mapKey.ToString();*/
			if (mapKey != null) {
				/*var mapKeyNode = VDFSaver.ToVDFNode(mapKey); // stringify-attempt-1: use exporter
				if (mapKeyNode.primitiveValue is string)
					mapKey_str = mapKeyNode.primitiveValue as string;
				else // if stringify-attempt-1 failed (i.e. exporter did not return string), use stringify-attempt-2
					mapKey_str = mapKey.ToString();*/

				if (mapKey is Type)
					//mapKey_str = "Type>'" + mapKey + "'";
					mapKey_str = VConvert.ToVDF(mapKey).Replace("\"", "'");
				else if (mapKey is Node)
					if (nodeRoot != null && (mapKey as Node).GetPath_Relative(nodeRoot) != null)
						mapKey_str = "[embedded path]" + (mapKey as Node).GetPath_Relative(nodeRoot).ToString().Replace("/", "[fs]");
					else
						mapKey_str = "[embedded path]" + (mapKey as Node).GetPath_Absolute().ToString().Replace("/", "[fs]");
				//mapKey_str = "[embedded path]" + (mapKey as Node).GetPath_Relative((mapKey as Node).GetPath_Absolute().ToVDFNodePath()).ToString().Replace("/", "[fs]");
				else
					mapKey_str = mapKey.ToString();
			}
		}

		// local-context cache, essentially
		public VPropInfo prop;
		public object mapKey;

		public bool voRoot; // 'VO'
		public bool vdfRoot; // '#'
		public bool nodeRoot; // '@'
		public bool currentParent; // ''
		public bool moveUp; // '^'
							//public string prop_str; // ex: 'p:age'
		public string prop_str; // ex: 'age' (note: the 'default' type)
		public int listIndex = -1; // ex: 'i:15'
		public int mapKeyIndex = -1; // ex: 'ki:15'
		public string mapKey_str; // ex: 'k:melee'

		public VDFNodePathNode ToVDFNodePathNode() { return new VDFNodePathNode(null, prop.VDFInfo(), listIndex, mapKeyIndex, mapKey_str); }

		/*public override bool Equals(object obj)
		{
			var other = obj as NodePathNode;
			if (other == null)
				return false;
			return prop == other.prop && list_index == other.list_index && map_key == other.map_key;
		}*/
		public override bool Equals(object obj) { return ToString() == obj.ToString(); }
		public override string ToString() {
			if (voRoot)
				return "VO";
			if (vdfRoot)
				return "#";
			if (nodeRoot)
				return "@";
			if (currentParent)
				return "";
			if (moveUp)
				return "^";
			if (listIndex != -1)
				return "i:" + listIndex;
			if (mapKeyIndex != -1)
				return "ki:" + mapKeyIndex;
			if (mapKey_str != null)
				return "k:" + mapKey_str;
			//return "p:" + prop_str;
			return prop_str;
		}
		public static NodePathNode Parse(string str) {
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
				result.listIndex = int.Parse(str.Substring(2));
			else if (str.StartsWith("ki:"))
				result.mapKeyIndex = int.Parse(str.Substring(3));
			else if (str.StartsWith("k:"))
				result.mapKey_str = str.Substring(2);
			else
				//result.prop_str = str.Substring(2);
				result.prop_str = str;
			return result;
		}
	}
	[VDFType(propIncludeRegexL1: VDF.PropRegex_Any)]
	public class NodePath {
		public List<NodePathNode> nodes;
		public NodePath(List<NodePathNode> nodes) { this.nodes = nodes; }

		[VDFSerialize] VDFNode Serialize() { return ToString(); }
		public override bool Equals(object obj) { return ToString() == obj.ToString(); }
		/*public override string ToString() {
			var result = "";
			var nodesCopy = nodes.ToList();
			for (var i = 0; i < nodesCopy.Count; i++) {
				var str = nodesCopy[i].ToString();
				if (nodesCopy[i].moveUp)
					while (nodesCopy.HasIndex(i + 1) && nodesCopy[i + 1].moveUp) {
						str += "^";
						nodesCopy.RemoveAt(i + 1);
					}
				result += (i > 0 ? "/" : "") + str;
			}
			return result;
		}*/
		public override string ToString() {
			var resultBuilder = new StringBuilder();
			for (int i = 0, count = nodes.Count; i < count; i++) {
				var node = nodes[i];
				resultBuilder.Append(node);
				if (!node.moveUp && i != count - 1)
				//if (!node.moveUp)
					resultBuilder.Append("/");
			}
			// if has final dangling forward-slash, remove it
			/*if (resultBuilder[resultBuilder.Length - 1] == '/')
				resultBuilder.Length--;*/
			return resultBuilder.ToString();
		}
		[VDFDeserialize(true)] public static NodePath Deserialize(VDFNode node) {
			var nodes = new List<NodePathNode>();
			var parts = ((string)node.primitiveValue).Split('/');
			foreach (var part in parts)
				if (part.StartsWith("^"))
					for (var i = 0; i < part.Length; i++)
						nodes.Add(NodePathNode.Parse("^"));
				else
					nodes.Add(NodePathNode.Parse(part));
			return new NodePath(nodes);
		}

		public NodePathNode rootNode { get { return nodes.First(); } }
		public NodePathNode parentNode { get { return nodes.Count >= 2 ? nodes[nodes.Count - 2] : null; } }
		public NodePathNode currentNode { get { return nodes.Last(); } }
		public NodePathNode FromLast(int indexOffset = 0) { return nodes[(nodes.Count - 1) + indexOffset]; }

		// gets a transformed path that is local-anchor-based (i.e. adds move-up path-nodes till reaches shared-ancestor, then the second part of the original path)
		public NodePath AsFrom(Node attachPointParent) {
			var attachPointParentPath = attachPointParent.GetPath_Absolute();
			var treePathToSelf_sharedNodes = new List<NodePathNode>();
			for (var i = 0; i < nodes.Count && i < attachPointParentPath.nodes.Count; i++)
				if (nodes[i].Equals(attachPointParentPath.nodes[i]))
					treePathToSelf_sharedNodes.Add(nodes[i]);
			var treePathToSelf_unsharedNodes = nodes.Skip(treePathToSelf_sharedNodes.Count).ToList();

			var fromAttachParentToLastShared_nodes = new List<NodePathNode>();
			var fromObjWithPropToLastShared_nodesNeeded = attachPointParentPath.nodes.Count - treePathToSelf_sharedNodes.Count;
			for (var i = 0; i < fromObjWithPropToLastShared_nodesNeeded; i++)
				fromAttachParentToLastShared_nodes.Add(new NodePathNode { moveUp = true });

			return new NodePath(fromAttachParentToLastShared_nodes.Concat(treePathToSelf_unsharedNodes).ToList());
		}
		public NodePath TryAsFrom(Node attachPointParent) {
			var attachPointParentPath = attachPointParent.GetPath_Absolute();
			if (attachPointParentPath != null && attachPointParentPath.rootNode.Equals(rootNode))
				return AsFrom(attachPointParent);
			return this;
		}
	}

	public class DisconnectedRootReference {
		public Node node;
		public DisconnectedRootReference(Node node) { this.node = node; }
	}

	public enum ContextGroup {
		Local_CS, //Self
		Local_CSAndUI,
		Local_UI
		//All
	}

	public class ToMainTree : Attribute { }
	public class SubtreeAddInfo {
		public SubtreeAddInfo(Node anchorObj, bool anchorInMainTree, VPropInfo subtreeProp, Node subtreeObj) {
			this.anchorObj = anchorObj;
			this.anchorInMainTree = anchorInMainTree;
			this.subtreeProp = subtreeProp;
			this.subtreeObj = subtreeObj;
		}
		public Node anchorObj;
		public bool anchorInMainTree;
		public VPropInfo subtreeProp;
		public Node subtreeObj;
	}

	// Node extension-methods
	// ==========

	public static class NodeExtensionMethods {
		// maybe todo: rename to "s", and use "s" for the lambda variable name
		public static PropertyWrapper<TProp> a<TObj, TProp>(this TObj obj, Expression<Func<TObj, TProp>> prop) where TObj : Node {
			return new PropertyWrapper<TProp>(obj, VPropInfo.Get(prop.GetMemberExpression().Member));
		}
		public static PropertyWrapper_Int a<TObj>(this TObj obj, Expression<Func<TObj, int>> prop) where TObj : Node {
			return new PropertyWrapper_Int(obj, VPropInfo.Get(prop.GetMemberExpression().Member));
		}
		public static PropertyWrapper_Double a<TObj>(this TObj obj, Expression<Func<TObj, double>> prop) where TObj : Node {
			return new PropertyWrapper_Double(obj, VPropInfo.Get(prop.GetMemberExpression().Member));
		}
		public static PropertyWrapper_List<TItem> a<TObj, TItem>(this TObj obj, Expression<Func<TObj, IList<TItem>>> prop) where TObj : Node {
			return new PropertyWrapper_List<TItem>(obj, VPropInfo.Get(prop.GetMemberExpression().Member));
		}
		public static PropertyWrapper_List<TItem> a<TObj, TItem>(this TObj obj, Expression<Func<TObj, List<TItem>>> prop) where TObj : Node {
			return new PropertyWrapper_List<TItem>(obj, VPropInfo.Get(prop.GetMemberExpression().Member));
		}
		public static PropertyWrapper_Dictionary<TKey, TValue> a<TObj, TKey, TValue>(this TObj obj, Expression<Func<TObj, IDictionary<TKey, TValue>>> prop) where TObj : Node {
			return new PropertyWrapper_Dictionary<TKey, TValue>(obj, VPropInfo.Get(prop.GetMemberExpression().Member));
		}
		public static PropertyWrapper_Dictionary<TKey, TValue> a<TObj, TKey, TValue>(this TObj obj, Expression<Func<TObj, Dictionary<TKey, TValue>>> prop) where TObj : Node {
			return new PropertyWrapper_Dictionary<TKey, TValue>(obj, VPropInfo.Get(prop.GetMemberExpression().Member));
		}
	}
	public class NodePlaceholder : Node { // used for attaching extra-methods before the host object even exists
		public void TransferDataTo(Node node) {
			foreach (var methodName in _extraMethods.Keys)
				foreach (Delegate extraMethod in _extraMethods[methodName])
					node.AddExtraMethod_Base(methodName, extraMethod);
			foreach (var childName in _childPlaceholders.Keys)
				if (VTypeInfo.Get(node.GetType()).props[childName].GetValue(node) is Node)
					_childPlaceholders[childName].TransferDataTo((Node)VTypeInfo.Get(node.GetType()).props[childName].GetValue(node));
				else {
					node._childPlaceholders[childName] = new NodePlaceholder();
					_childPlaceholders[childName].TransferDataTo(node._childPlaceholders[childName]);
				}
		}
	}

	// maybe todo: update code using this system, to only use it when it's helpful (i.e. when there's actually something listening for changes)
	public class PropertyWrapper<TProp> {
		public Node obj;
		public VPropInfo prop;
		public PropertyWrapper(Node obj, VPropInfo prop) {
			this.obj = obj;
			this.prop = prop;

			//if (prop.memberInfo.Name.StartsWith("_"))
			//	throw new Exception("Cannot use the VTree change system for a non-shared VTree property.");
		}

		// had to use the silly "ContextGroup? group = null" because the Mono compiler apparently can't handle default-value enums
		public void Set(TProp value, ContextGroup? group = null, object message = null) {
			VO.SubmitChange(new Change_Set(obj, prop, value).AddMessages(message), group ?? ContextGroup.Local_CSAndUI);
		}
		public void Set_IfDifferent(TProp value, ContextGroup? group = null, object message = null) { // maybe temp
			var oldValue = prop.GetValue(obj);
			if (oldValue != (object)value && (!(value is string) || (string)oldValue != value as string))
				VO.SubmitChange(new Change_Set(obj, prop, value).AddMessages(message), group ?? ContextGroup.Local_CSAndUI);
		}

		public TProp set { set { Set(value); } }
		public TProp set_self { set { Set(value, ContextGroup.Local_CS); } }
		public TProp set_ifDifferent { set { Set_IfDifferent(value); } }
	}
	public class PropertyWrapper_Int : PropertyWrapper<int> {
		public PropertyWrapper_Int(Node obj, VPropInfo prop) : base(obj, prop) { }

		//public void Set(int value, ContextGroup? group = null, object message = null) { VO.SubmitChange(new Change_Set {obj = obj, propInfo = prop, value = value}.AddMessages(message), group ?? ContextGroup.Local_CSAndUI); }
		public void Increase(int amount, ContextGroup? group = null, object message = null) {
			VO.SubmitChange(new Change_Increase_Number(obj, prop, amount).AddMessages(message), group ?? ContextGroup.Local_CSAndUI);
		}

		//public int set { set { Set(value); } }
		public int increase { set { Increase(value); } }
	}
	public class PropertyWrapper_Double : PropertyWrapper<double> {
		public PropertyWrapper_Double(Node obj, VPropInfo prop) : base(obj, prop) {}

		public void Increase(double amount, ContextGroup? group = null, object message = null) {
			VO.SubmitChange(new Change_Increase_Number(obj, prop, amount).AddMessages(message), group ?? ContextGroup.Local_CSAndUI);

			/*var S = M.GetCurrentMethod().Profile_LastDataFrame();
			S._____("create change object");
			var change = new Change_Increase_Number(obj, prop, amount).AddMessages(message);
			S._____("submit change object");
			VO.SubmitChange(change, group ?? ContextGroup.Local_CSAndUI);
			S._____(null);*/
		}

		public double increase { set { Increase(value); } }
		public double increase_self { set { Increase(value, ContextGroup.Local_CS); } }
	}
	public class PropertyWrapper_List<TItem> : PropertyWrapper<List<TItem>> { //IList<TItem>>
		public PropertyWrapper_List(Node obj, VPropInfo prop) : base(obj, prop) { }

		/*public void Set(IList<TItem> value, ContextGroup? group = null, object message = null) { 
			VO.SubmitChange(new Change_Set { obj = obj, propInfo = prop, value = value, message = message }, group ?? ContextGroup.Local_CSAndUI);
		}*/
		public void SetItem(int index, TItem newItem, ContextGroup? group = null, object message = null) {
			VO.SubmitChange(new Change_SetItem_List(obj, prop, index, newItem).AddMessages(message), group ?? ContextGroup.Local_CSAndUI);
		}
		public void Add(TItem newItem, int index = -1, ContextGroup? group = null, object message = null) {
			VO.SubmitChange(new Change_Add_List(obj, prop, newItem, index).AddMessages(message), group ?? ContextGroup.Local_CSAndUI);
		}
		public void Remove(int index, ContextGroup? group = null, object message = null) {
			VO.SubmitChange(new Change_Remove_List(obj, prop, index).AddMessages(message), group ?? ContextGroup.Local_CSAndUI);
		}
		public void Remove(TItem item, ContextGroup? group = null, object message = null) {
			VO.SubmitChange(new Change_Remove_List(obj, prop, item).AddMessages(message), group ?? ContextGroup.Local_CSAndUI);
		}
		public void Clear(ContextGroup? group = null, object message = null) {
			VO.SubmitChange(new Change_Clear_List(obj, prop).AddMessages(message), group ?? ContextGroup.Local_CSAndUI);
		}

		//public TItem set { set { Set(value); } }
		public TItem add { set { Add(value); } }
		public TItem remove { set { Remove(value); } }

		// maybe temp
		//public TItem add_self { set { Add(value, ContextGroup.Local_CS); } }
	}
	public class PropertyWrapper_Dictionary<TKey, TValue> : PropertyWrapper<Dictionary<TKey, TValue>> { //IDictionary<TKey, TValue>>
		public PropertyWrapper_Dictionary(Node obj, VPropInfo prop) : base(obj, prop) {}

		public void Add(TKey key, TValue value, bool overwrite = false, ContextGroup? group = null, object message = null) {
			VO.SubmitChange(new Change_Add_Dictionary(obj, prop, key, value, overwrite).AddMessages(message), group ?? ContextGroup.Local_CSAndUI);
		}
		public void Remove(TKey key, ContextGroup? group = null, object message = null) {
			VO.SubmitChange(new Change_Remove_Dictionary(obj, prop, key).AddMessages(message), group ?? ContextGroup.Local_CSAndUI);
		}
		public void Clear(ContextGroup? group = null, object message = null) {
			VO.SubmitChange(new Change_Clear_Dictionary(obj, prop).AddMessages(message), group ?? ContextGroup.Local_CSAndUI);
		}
	}
}