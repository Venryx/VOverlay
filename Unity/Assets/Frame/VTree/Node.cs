using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using VDFN;
using VTree;

// normal
// ==========

namespace VTree_Structures {
	// maybe todo: use a more consistent inclusion system (currently, the idea is to use _... for shared-with-none (or rather, not part of tree management), basic not-_... for shared-with-all, and the [NoUI], [NoSave], and [NoPeers] tags for what's in-between)
	//[VDFType(propIncludeRegexL1: "^(?!_)(?!s$)")] public class Node // base node class (when not overriden, acts as empty/heirarchy/container node)
	//[VDFType(propIncludeRegexL1: "^(?!s$)[a-z]")] public class Node { // base node class (when not overriden, acts as empty/heirarchy/container node)
	// probably make-so: typeComp exclusion is only for VComponent class
	[VDFType(propIncludeRegexL1: "^(?!(s|typeComp)$)[a-z]")] public class Node { // base node class (when not overriden, acts as empty/heirarchy/container node)
		//static Regex nodeDefaultPropRegex = new Regex("^(?!(s|typeComp)$)[a-z]");
		[VDFPreDeserialize] public Node() {
			var sProp = VTypeInfo.Get(GetType()).props.GetValueOrX("s");
			if (sProp != null)
				sProp.SetValue(this, this);

			//VO.main.a(a=>a.nodes).Add(nodeID, new WeakOrStrongReference<Node>(this));
		}

		/*List<Action> _postDeserializeMethods = new List<Action>();
		public void AddPostDeserializeMethod(Action method) { _postDeserializeMethods.Add(method); }*/

		// probably todo: remove (DisconnectedRootReference class is probably no longer needed, since we have VDFNodePaths)
		[VDFPreDeserialize] protected void Node_PreDeserialize(VDFNode node, VDFNodePath path, VDFLoadOptions options) {
			var oldDisconnectedRootReference = options.messages.OfType<DisconnectedRootReference>().FirstOrDefault();
			var disconnectedRootReference = oldDisconnectedRootReference ?? new DisconnectedRootReference(this);
			if (oldDisconnectedRootReference == null)
				options.messages.Add(disconnectedRootReference);
			else
				if (path.currentNode.prop != null)
					path.currentNode.prop.SetValue(path.parentNode.obj, this);
				else if (path.currentNode.list_index != -1)
					(path.parentNode.obj as IList).Add(this);
				else if (path.currentNode.map_key != null)
					(path.parentNode.obj as IDictionary).Add(path.currentNode.map_key, this);
		}
		[VDFPostDeserialize] protected void Node_PostDeserialize(VDFNode node, VDFNodePath path, VDFLoadOptions options) {
			var disconnectedRootReference = options.messages.OfType<DisconnectedRootReference>().FirstOrDefault();
			if (disconnectedRootReference != null && disconnectedRootReference.node == this)
				options.messages.Remove(disconnectedRootReference);

			/*foreach (var method in _postDeserializeMethods)
				method();*/
		}

		[VDFPreSerializeProp] protected VDFNode Node_PreSerializeProp(VDFNodePath propPath, VDFSaveOptions options) {
			var optionsV = options as VDFSaveOptionsV;
			if (optionsV) {
				var prop = propPath.GetNodeWithProp().prop;
				if (prop != null && ShouldIgnoreProp(prop, optionsV))
					return VDF.CancelSerialize;
			}
			return null;
		}
		// maybe make-so: Node class auto-checks path for objects of the specified type, rather than relying on custom settings of VDFSaveOptionsV props
		protected bool ShouldIgnoreProp(VDFPropInfo prop, VDFSaveOptionsV optionsV) {
			if (optionsV == null)
				return false;
			var propV = prop.VInfo();
			var notTo = propV.notTo;
			return notTo != null
				&& ((notTo.objType && optionsV.toObjType)
					|| (notTo.obj && optionsV.toObj)
					|| (notTo.file && optionsV.toFile)
					|| (notTo.map && optionsV.toMap)
					|| (notTo.js && optionsV.toJS)
				);
		}

		static VDFSaveOptions genericSaveOptions = VConvert.FinalizeToVDFOptions(new VDFSaveOptions()); // maybe make-so: this is standardized somehow
		[VDFSerialize] protected VDFNode Node_Serialize(VDFNodePath path, VDFSaveOptions options) {
			var optionsV = options as VDFSaveOptionsV;
			var S = optionsV && optionsV.profile ? M.GetCurrentMethod().Profile_LastDataFrame() : M.None_SameType;

			S._____("part 1");
			var prop = path.GetNodeWithProp() != null ? path.GetNodeWithProp().prop : null;
			//if (prop != null && prop.VInfo().tags.OfType<ByPath>().Any() && !prop.VInfo().tags.OfType<ByPath>().First().allowReferenceToParentlessNode && VO.main != this && Parent == null)
			//	throw new Exception("Property '" + prop.memberInfo.Name + "' of type '" + prop.memberInfo.DeclaringType + "' references a parentless node.");

			S._____("part 2");
			var sourceProp = prop;
			if (path.parentNode != null && path.parentNode.obj is Change && (prop.memberInfo.Name == "value" || prop.memberInfo.Name == "item"))
				sourceProp = (path.parentNode.obj as Change).propInfo.VDFInfo();
			var propWantsByReference = prop != null && prop.VInfo().tags.Any(a=>a is ByPath || a is ByName);
			var sourcePropWantsByReference = sourceProp != null && sourceProp.VInfo().tags.Any(a=>a is ByPath || a is ByName);

			S._____("part 3");
			var selfIsKey = path.currentNode.map_keyIndex != -1; // (if self is a key, always serialize by reference)
			if (propWantsByReference || sourcePropWantsByReference || selfIsKey) {
				var referenceTagProp = sourcePropWantsByReference ? sourceProp : prop;
				var newOptions = genericSaveOptions;
				S._____("part 3.5");
				if ((
						// if self should be saved by-path
						referenceTagProp != null && referenceTagProp.VInfo().tags.Any(a=>a is ByPath)
						&& S._____2("part 3.7") != null
						// and this prop-path is not where self is attached (since, if this were the attach-point, the generated reference would point to... itself)
						&& (path.GetNodeWithProp() == null || attachPoint == null || path.GetNodeWithProp().prop.VInfo() != attachPoint.prop)
					)
					|| selfIsKey
				) {
					S._____("part 4");
					// first, try to get a relative path
					var treePath = GetPath_Relative(path);
					// if no valid relative path found, try to get an absolute path
					if (treePath == null)
						treePath = GetPath_Absolute();

					S._____("part 5");
					if (treePath != null) // if found valid path (i.e. self is connected to main-tree, or internal to the being-serialized pack)
						// maybe make-so: the to-local-anchor converter below is used
						/*if (path.parentNode.obj is Node)
							treePath = treePath.TryAsFrom(path.parentNode.obj as Node);*/
						//return VDFSaver.ToVDFNode(treePath, null, newOptions).EndProfileBlock(S);
						return new VDFNode(treePath.ToString()).EndProfileBlock(S);
					if (selfIsKey || !referenceTagProp.VInfo().tags.OfType<ByPath>().First().saveNormallyForParentlessNode) {
						// for debugging
						/*V.Break();
						var a = GetPath_Relative(path);
						var b = GetPath_Absolute();*/
						throw new Exception("Node should be saved by path, but path is invalid. (i.e. the node of type '" + GetType().Name + "' is not attached to main-tree, and is not internal to being-serialized pack)");
					}
				}
				else if (referenceTagProp != null && referenceTagProp.VInfo().tags.Any(a=>a is ByName)) {
					S._____("part 5 [b]");
					var typeStr = options.messages.Contains("to ui") ? VDF.GetNameOfType(GetType(), newOptions) + ">" : "";
					//return VDFSaver.ToVDFNode(typeStr + VDFTypeInfo.Get(GetType()).props["name"].GetValue(this), null, newOptions).EndProfileBlock(S); //, path);
					return new VDFNode(typeStr + VDFTypeInfo.Get(GetType()).props["name"].GetValue(this)).EndProfileBlock(S);
				}
			}
			return VDF.NoActionTaken.EndProfileBlock(S);
		}
		[VDFPostSerialize] protected void Node_PostSerialize(VDFNode node, VDFNodePath path, VDFSaveOptions options) {
			if (options.messages.Contains("to ui")) { // maybe todo: mirror for JS side
				// maybe temp; manually correct metadata (you can either set the metadata to "Node", or specify the type for the "obj" property on the JS side)
				//if (node["nodes"] != null)
				if (node.metadata == "NodePath")
					node.metadata = "Node"; //NodePath";
				else if (node.primitiveValue is string)
					node.metadata = "Node"; //NodePath";
			}
		}
		static int lastMapKeyPlaceholder_index = -1;
		[VDFDeserialize(fromParent: true)] protected static object Node_Deserialize_FromParent(VDFNode node, VDFNodePath path, VDFLoadOptions options) {
			//if (prop != null && prop.memberInfo.GetCustomAttributes(true).Any(a=>a is ByPath))
			// todo: make sure this doesn't catch e.g. "BDNotANodePathSoilName"
			var nodeAsStr = node.primitiveValue as string;
			if (nodeAsStr != null && (nodeAsStr.StartsWith("VO") || nodeAsStr == "#" || nodeAsStr.StartsWith("#/") || nodeAsStr == "@" || nodeAsStr.StartsWith("@/") || nodeAsStr.StartsWith("^") || nodeAsStr.StartsWith("/"))) {
				//return VOverlay.GetNodeByNodePath(node.ToObject<NodePath>(VConvert.FinalizeFromVDFOptions(new VDFLoadOptions())), path, options);

				// special case for when this-node-is-the-root-node (e.g. deserialize node-path string directly as a Node)
				if (path.parentNode == null)
					return VOverlay.GetNodeByNodePath(node.ToObject<NodePath>(VConvert.FinalizeFromVDFOptions(new VDFLoadOptions())), path, options);

				// maybe temp; resolve reference after the normal deserialization has completed
				if (path.parentNode.obj is Change && path.currentNode.prop.memberInfo.Name == "obj") // if Change.obj prop, evaluate right away, since it's needed for other deserialize methods
					return VOverlay.GetNodeByNodePath(node.ToObject<NodePath>(VConvert.FinalizeFromVDFOptions(new VDFLoadOptions())), path, options);

				var placeholder = path.currentNode.map_keyIndex != -1 ? "mapKeyPlaceholder_" + (++lastMapKeyPlaceholder_index) : null;
				// maybe temp; specially-add map_key data, for delayed SetFinalNodeValue method
				if (path.currentNode.map_keyIndex != -1)
					//path.currentNode.map_key = (path.parentNode.prop.GetValue(path.nodes.XFromLast(2).obj) as IDictionary).Keys.ToList_Object()[path.currentNode.map_keyIndex];
					path.currentNode.map_key = placeholder;
				options.AddObjPostDeserializeFunc(path.nodes[0].obj, ()=>path.SetFinalNodeValue(VOverlay.GetNodeByNodePath(node.ToObject<NodePath>(VConvert.FinalizeFromVDFOptions(new VDFLoadOptions())), path, options)), true);
				return placeholder;
			}
			/*if (nodeAsStr != null) { // if by-name reference
				var attachObj = path.GetNodeWithParent() != null && path.GetNodeWithParent().obj is Node ? (Node)path.GetNodeWithParent().obj : null;
				var attachProp = path.GetNodeWithProp() != null ? path.GetNodeWithProp().prop.VInfo() : null;
				var attachIndexOrKeyNode = path.GetNodeWithIndexOrKey();
				var attachIndex = attachIndexOrKeyNode != null && attachIndexOrKeyNode.list_index != -1 ? attachIndexOrKeyNode.list_index : -1;
				var attachKeyIndex = -1;
				var attachKey = attachIndexOrKeyNode != null && attachIndexOrKeyNode.map_key != null ? attachIndexOrKeyNode.map_key : null;
				var attachPoint = new NodeAttachPoint(attachObj, attachProp, attachIndex, attachKeyIndex, attachKey);

				var finalAttachProp = attachProp;
				if (path.parentNode != null && path.parentNode.obj is Change && (path.parentNode.obj as Change).propInfo != null)
					/*attachObj = (path.parentNode.obj as Change).obj;
					attachProp = (path.parentNode.obj as Change).propInfo;
					attachIndex = path.parentNode.obj is Change_Add_List ? (path.parentNode.obj as Change_Add_List).index : -1;
					attachKey = path.parentNode.obj is Change_Add_Dictionary ? (path.parentNode.obj as Change_Add_Dictionary).key : -1;*#/
					finalAttachProp = (path.parentNode.obj as Change).propInfo; // can be null if this Node being deserialized by-name is itself part of the above Change object (since its PostInit method hasn't been called yet)

				string typeName = null;
				if (node.metadata != "Node") // try from metadata
					typeName = node.metadata;
				if (typeName == null) // try from node-text
					typeName = node.primitiveValue.ToString().Contains(">") ? node.primitiveValue.ToString().Substring(0, node.primitiveValue.ToString().IndexOf(">")) : null;
				if (typeName == null) { // try from prop-info
					Type type_fromPropInfo;
					if (typeof(IList).IsAssignableFrom(finalAttachProp.GetPropType()))
						type_fromPropInfo = finalAttachProp.GetPropType().GetGenericArguments()[0];
					else if (typeof(IDictionary).IsAssignableFrom(finalAttachProp.GetPropType()))
						type_fromPropInfo = finalAttachProp.GetPropType().GetGenericArguments()[1];
					else
						type_fromPropInfo = finalAttachProp.GetPropType();
					typeName = type_fromPropInfo.Name;
				}
				//typeName = typeName ?? "object";

				var type = VDF.GetTypeByName(typeName, VConvert.FinalizeFromVDFOptions(new VDFLoadOptions()));
				var name = node.primitiveValue.ToString().Substring(node.primitiveValue.ToString().IndexOf(">") + 1);
				if (type == typeof(Map))
					return VO.main.LoadLazy_Map(attachPoint, name); //, true);
				if (type == typeof(Soil))
					return VO.main.LoadLazy_Soil(attachPoint, name);
				if (type == typeof(Biome))
					return VO.main.LoadLazy_Biome(attachPoint, name);
				//if (type == typeof(VObject))
				//if (type == typeof(VOT) || type == typeof(VObject_Type))
				if (type == typeof(VObject_Type))
					return VO.main.LoadLazy_VObject_Type(attachPoint, name);
				if (type == typeof(Module))
					return VO.main.LoadLazy_Module(attachPoint, name);
				throw new Exception("No handler found for Node to load string data. \"" + node.primitiveValue + "\".");
			}*/
			return VDF.NoActionTaken;
		}

		[VDFProp(false)] public NodeAttachPoint attachPoint;
		[VDFProp(false)] public Node Parent { get { return attachPoint != null ? attachPoint.parent : null; }}
		public List<Node> GetAncestors() {
			if (Parent == null)
				return new List<Node>();
			var result = new List<Node> {Parent};
			while (result[0].Parent != null)
				result.Insert(0, result[0].Parent);
			return result;
		}
		public void SetAttachPoint(NodeAttachPoint newAttachPoint) {
			if (Parent != null && newAttachPoint != null && newAttachPoint.parent != null && Parent != newAttachPoint.parent)
				throw new Exception("Cannot set parent more than once. Existing attach-point: " + attachPoint);
			if (newAttachPoint != null && newAttachPoint.parent == this)
				throw new Exception("Cannot set self as own parent! Existing attach-point: " + attachPoint);

			if (Parent != null)
				attachPoint.parent._children.Remove(this);
			attachPoint = newAttachPoint;
			if (Parent != null)
				attachPoint.parent._children.Add(this);
		}
		public List<Node> _children = new List<Node>();

		// probably todo: add way to specify child by a match signature/function (e.g. BD/maps/maps/[name:Test1]), and replace LoadLazy system with that
		public NodePath GetPath_Absolute() {
			// check if we're the root of a path-finding call-up-chain
			if (VO.main == this) // if bd-root (i.e. game's main-tree root), return as path root
				return new NodePath(new List<NodePathNode> {new NodePathNode {voRoot = true}});

			if (Parent == null) // if we can't move up any more, and we still haven't reached bd-root, then return null/no-path
				return null;

			// start or continue a path-finding call-up-chain
			var path = Parent.GetPath_Absolute();
			if (path != null)
				path.nodes.AddRange(attachPoint.ToPathNodes());
			return path;
		}
		public NodePath GetPath_Relative(VDFNodePath relativeVDFPath, bool initialCall = true) {
			// check if we're the root of a path-finding call-up-chain
			var ancestors = GetAncestors();
			//if (Parent == null || !relativeVDFPath.nodes.Any(a=>a.obj == Parent)) // if we have no in-vdf-path Node parent (i.e. if we might be the node-root)
			// if we have no in-vdf-path Node ancestor (i.e. if we might be the node-root)
			if (Parent == null || !relativeVDFPath.nodes.Any(a=>a.obj is Node && ancestors.Contains((Node)a.obj))) {
				// and we're at the initial-call, (and there's no copy of self higher up in call-chain), then this object is (within this being-serialized-pack) 'attached' at this position; return null/no-path
				if (initialCall && relativeVDFPath.nodes.Count(a=>a.obj == this) <= 1)
					return null;
				// if not deepest vdf-path-obj (i.e. if this call is a call-up) and not shallowest vdf-path-obj (i.e. if not same as vdf-root), we're a valid node-root, so return as path root
				if (this != relativeVDFPath.currentNode.obj && this != relativeVDFPath.rootNode.obj)
					return new NodePath(new List<NodePathNode> {new NodePathNode {nodeRoot = true}});
				// else, return the path from vdf-root to self
				var posInVDFPath = relativeVDFPath.nodes.FindIndex(a=>a.obj == this);
				var fromVDFRootToSelf = relativeVDFPath.nodes.Take(posInVDFPath + 1).ToList();
				return new VDFNodePath(fromVDFRootToSelf).ToNodePath(true);
			}

			// start or continue a path-finding call-up-chain
			var path = Parent.GetPath_Relative(relativeVDFPath, false);
			if (path != null)
				path.nodes.AddRange(attachPoint.ToPathNodes(relativeVDFPath.nodes.Where(a=>a.obj is Node).Select(a=>a.obj as Node).FirstOrDefault()));
			return path;
		}
		public NodePath GetPath_Relative(Node nodeRoot, bool initialCall = true) {
			// check if we're the root of a path-finding call-up-chain
			if (this == nodeRoot)
				return new NodePath(new List<NodePathNode> {new NodePathNode {nodeRoot = true}});
			if (Parent == null) // if failed to get up to node-root
				return null;

			// start or continue a path-finding call-up-chain
			var path = Parent.GetPath_Relative(nodeRoot, false);
			if (path != null)
				path.nodes.AddRange(attachPoint.ToPathNodes(nodeRoot));
			return path;
		}

		public bool IsConnectedToMainTree() { return VO.main == this || (Parent != null && Parent.IsConnectedToMainTree()); }
		// maybe temp; maybe add to JS
		public bool _changeOrMessageSubmissionToUIAllowed = true;
		public bool IsChangeOrMessageSubmissionToUIAllowed() { return _changeOrMessageSubmissionToUIAllowed && (Parent != null ? Parent.IsChangeOrMessageSubmissionToUIAllowed() : VO.main == this); }

		// maybe todo: have this also call the Pre[...] methods
		public void PreAdd(NodeAttachPoint newAttachPoint, SubtreeAddInfo info = null) {
			//if (!parent.IsConnectedToVTree())
			//	throw new Exception("New parent is not connected to v-tree!");
			if (attachPoint != null && !attachPoint.Equals(newAttachPoint))
				throw new Exception("Node of type \"" + GetType().Name + "\" cannot be attached to v-tree in more than one place (other than by reference). Existing attach-point: " + attachPoint + " Attempted attach-point: " + newAttachPoint);

			// if real attachment (i.e. if caller wasn't the FakeAdd method)
			if (newAttachPoint != null) {
				if (info == null)
					info = new SubtreeAddInfo(newAttachPoint.parent, newAttachPoint.parent == null || newAttachPoint.parent.IsConnectedToMainTree(), newAttachPoint.prop, this);

				if (info.anchorInMainTree)
					CallMethod("_PreAdd", newAttachPoint, info);
				else
					CallMethod(typeof(ToMainTree), "_PreAdd", newAttachPoint, info);
				if (info.anchorObj != newAttachPoint.parent && Parent == null) // anchor stuff *within* subtree during the PreAdd phase; anchor subtree itself during the PostAdd phase
					SetAttachPoint(newAttachPoint);

				if (newAttachPoint.parent._childPlaceholders.ContainsKey(newAttachPoint.prop.Name)) {
					newAttachPoint.parent._childPlaceholders[newAttachPoint.prop.Name].TransferDataTo(this);
					newAttachPoint.parent._childPlaceholders.Remove(newAttachPoint.prop.Name);
				}
			}
			else
				if (info == null)
					info = new SubtreeAddInfo(null, true, null, this);

			var typeInfo = VTypeInfo.Get(GetType());
			foreach (KeyValuePair<string, VPropInfo> pair in typeInfo.props) {
				var propName = pair.Key;
				var prop = pair.Value;
				//if (!nodeDefaultPropRegex.IsMatch(pair.Key) || !(pair.Value.memberInfo is FieldInfo) || pair.Value.tags.Any(a=>a is VDFProp && !(a as VDFProp).includeL2))
				//if (propName == "s" || propName == "typeComp" || propName.StartsWith("_") || char.IsUpper(propName[0]) || !(prop.memberInfo is FieldInfo) || prop.tags.Any(a=>a is VDFProp && !(a as VDFProp).includeL2))
				if (propName == "s" || propName == "typeComp" || propName.StartsWith("_") || !(prop.memberInfo is FieldInfo) || prop.tags.Any(a=>a is VDFProp && !(a as VDFProp).includeL2))
					continue;

				//if (nodeDefaultPropRegex.IsMatch(pair.Key) && pair.Value.memberInfo is FieldInfo && !pair.Value.tags.Any(a => a is VDFProp && !(a as VDFProp).includeL2)) {
				var propValue = prop.GetValue(this);
				if (!prop.tags.Any(a=>a is ByPath || a is ByName)) // if not by reference
					if (propValue is IList)
						for (var i = 0; i < (propValue as IList).Count; i++) {
							var item = (propValue as IList)[i];
							if (item is Node)
								(item as Node).PreAdd(new NodeAttachPoint(this, prop, i), info);
						}
					else if (propValue is IDictionary) {
						//var index = -1;
						foreach (KeyValuePair<object, object> pair2 in (propValue as IDictionary).Pairs())
							/*index++;
							if (pair2.Key is Node)
								(pair2.Key as Node).PreAdd(new NodeAttachPoint(this, prop, map_keyIndex: index), info);*/
							if (pair2.Value is Node)
								(pair2.Value as Node).PreAdd(new NodeAttachPoint(this, prop, map_key: pair2.Key), info);
					}
					else if (propValue is Node)
						(propValue as Node).PreAdd(new NodeAttachPoint(this, prop), info);
			}
		}
		public void PostAdd(NodeAttachPoint newAttachPoint, SubtreeAddInfo info = null) { //, Change change = null) // old: maybe temp; allow Change object, so messages can be sent through to triggered event methods
			// if real attachment (i.e. if caller wasn't the FakeAdd method)
			if (newAttachPoint != null) {
				if (info == null) {
					info = new SubtreeAddInfo(newAttachPoint.parent, newAttachPoint.parent == null || newAttachPoint.parent.IsConnectedToMainTree(), newAttachPoint.prop, this);
					SetAttachPoint(newAttachPoint); // anchor stuff *within* subtree during the PreAdd phase; anchor subtree itself during the PostAdd phase
				}
			}
			else if (info == null)
				info = new SubtreeAddInfo(null, true, null, this);

			if (info.anchorInMainTree)
				CallMethod("_PostAdd_Early", newAttachPoint, info);
			else
				CallMethod(typeof(ToMainTree), "_PostAdd_Early", newAttachPoint, info);

			var typeInfo = VTypeInfo.Get(GetType());
			foreach (KeyValuePair<string, VPropInfo> pair in typeInfo.props) {
				var propName = pair.Key;
				var prop = pair.Value;
				if (propName == "s" || propName == "typeComp" || propName.StartsWith("_") || !(prop.memberInfo is FieldInfo) || prop.tags.Any(a=>a is VDFProp && !(a as VDFProp).includeL2))
					continue;
				
				var propValue = prop.GetValue(this);
				var byReference = prop.tags.Any(a=>a is ByPath || a is ByName);
				if (propValue is IList) {
					for (var i = 0; i < (propValue as IList).Count; i++) {
						var item = (propValue as IList)[i];
						if (info.anchorInMainTree)
							CallMethod(typeof(IgnoreStartData), prop.Name + "_PostAdd_Early", item, null);
						if (item is Node && !byReference)
							(item as Node).PostAdd(new NodeAttachPoint(this, prop, i), info);
						if (info.anchorInMainTree)
							CallMethod(typeof(IgnoreStartData), prop.Name + "_PostAdd", item, null);
					}
				}
				else if (propValue is IDictionary) {
					//var index = -1;
					foreach (KeyValuePair<object, object> pair2 in (propValue as IDictionary).Pairs()) // for now we assume the key is not a Node
					{
						/*index++;
						if (pair2.Key is Node && !byReference)
							(pair2.Key as Node).PostAdd(new NodeAttachPoint(this, prop, map_keyIndex: index), info);*/

						if (info.anchorInMainTree)
							CallMethod(typeof(IgnoreStartData), prop.Name + "_PostAdd_Early", pair2.Key, pair2.Value, null);
						if (pair2.Value is Node && !byReference)
							(pair2.Value as Node).PostAdd(new NodeAttachPoint(this, prop, map_key: pair2.Key), info);
						if (info.anchorInMainTree)
							CallMethod(typeof(IgnoreStartData), prop.Name + "_PostAdd", pair2.Key, pair2.Value, null);
					}
				}
				else {
					if (info.anchorInMainTree)
						CallMethod(typeof(IgnoreStartData), prop.Name + "_PostSet_Early", propValue, null);
					if (propValue is Node && !byReference)
						(propValue as Node).PostAdd(new NodeAttachPoint(this, prop), info);
					if (info.anchorInMainTree)
						CallMethod(typeof(IgnoreStartData), prop.Name + "_PostSet", propValue, null);
				}
			}

			if (info.anchorInMainTree)
				CallMethod("_PostAdd", newAttachPoint, info);
			else
				CallMethod(typeof(ToMainTree), "_PostAdd", newAttachPoint, info);
		}
		public void FakeAdd(NodeAttachPoint newAttachPoint) {
			PreAdd(newAttachPoint);
			PostAdd(newAttachPoint);
		}

		public void BroadcastMessage(ContextGroup group, string methodName, params object[] args) {
			//if (IsConnectedToMainTree() && (group == ContextGroup.Local_CSAndUI || group == ContextGroup.Local_UI)) //targetContext == "all" || targetContext == "ui")
			/*if (IsChangeOrMessageSubmissionToUIAllowed() && (group == ContextGroup.Local_CSAndUI || group == ContextGroup.Local_UI))
				JSBridge.CallJS("BD.Node_BroadcastMessage", new object[] {GetPath_Absolute(), ContextGroup.Local_UI, methodName}.Concat(args).ToArray());*/
			if (group == ContextGroup.Local_CSAndUI || group == ContextGroup.Local_CS) {
				SendMessage(ContextGroup.Local_CS, methodName, args);
				foreach (var pair in _children.Pairs())
					if (_children.Contains(pair.item, pair.index)) // if child still exists
						pair.item.BroadcastMessage(ContextGroup.Local_CS, methodName, args);
			}
		}
		/// <summary>If the last argument is a delegate (e.g. Action{string}), it will be taken as the 'callback', and will be called after the JS code's completion.</summary>
		public object SendMessage(ContextGroup group, string methodName, params object[] args) {
			object result = null;
			//if (IsConnectedToMainTree() && (group == ContextGroup.Local_CSAndUI || group == ContextGroup.Local_UI)) //targetContext == "all" || targetContext == "ui")
			/*if (IsChangeOrMessageSubmissionToUIAllowed() && (group == ContextGroup.Local_CSAndUI || group == ContextGroup.Local_UI))
				JSBridge.CallJS("BD.Node_SendMessage", new object[] {GetPath_Absolute(), ContextGroup.Local_UI, methodName}.Concat(args).ToArray());*/
			if (group == ContextGroup.Local_CSAndUI || group == ContextGroup.Local_CS)
				result = CallMethod(methodName, args);
			return result;
		}

		// maybe temp
		public void SubmitState(ContextGroup? group = null) {
			/*var oldAttachPoint = attachPoint;
			attachPoint = null;*/
			var stateVDF = VConvert.ToVDF(this, true, VDFTypeMarking.External, new VDFSaveOptions(new List<object> {"to ui"})); // maybe temp; assume we're sending to UI
			//attachPoint = oldAttachPoint;
			SubmitState(stateVDF, group);
		}
		public void SubmitState(string stateVDF, ContextGroup? group = null) {
			group = group ?? ContextGroup.Local_CSAndUI;
			//CallMethod("PreSubmitState");
			if (group == ContextGroup.Local_CSAndUI || group == ContextGroup.Local_UI) // if UI is part of target
				SendMessage(ContextGroup.Local_UI, "SubmitState", stateVDF, ContextGroup.Local_UI); // send state submission to UI v-tree
			if (group == ContextGroup.Local_CSAndUI || group == ContextGroup.Local_CS) {
				/*var options = VConvert.FinalizeFromVDFOptions(new VDFLoadOptions());
                var stateNode = VDFLoader.ToVDFNode(stateVDF, GetType(), options);
				stateNode.IntoObject(this, options);
                foreach (string propName in stateNode.mapChildren.Keys)
				{
					var prop = VTypeInfo.Get(GetType()).props[propName];
					BiomeDefense.SubmitChange(new Change_Set(this, prop, prop.GetValue(this)), ContextGroup.Local_CS);
				}*/
				VConvert.FromVDFInto(stateVDF, this);
				PreAdd(attachPoint);
				PostAdd(attachPoint);

				CallMethod("PostSubmitState");
			}
		}

		public Dictionary<string, NodePlaceholder> _childPlaceholders = new Dictionary<string, NodePlaceholder>();
		public Node GetSafeChild(string childName) {
			if (VTypeInfo.Get(GetType()).props[childName].GetValue(this) != null)
				return (Node)VTypeInfo.Get(GetType()).props[childName].GetValue(this);
			if (!_childPlaceholders.ContainsKey(childName))
				_childPlaceholders[childName] = new NodePlaceholder();
			return _childPlaceholders[childName];
		}

		// maybe todo: add way for extra-methods to be tagged, so objects can remove their extra-methods, once they're no longer needed
		public Dictionary<string, List<Delegate>> _extraMethods = new Dictionary<string, List<Delegate>>();
		public void AddExtraMethod_Base(string methodName, Delegate method, bool allowAddingDuplicate = true) {
			if (!allowAddingDuplicate && _extraMethods.ContainsKey(methodName) && _extraMethods[methodName].Contains(method))
				return;

			if (!_extraMethods.ContainsKey(methodName))
				_extraMethods[methodName] = new List<Delegate>();
			_extraMethods[methodName].Add(method);
		}
		public void AddExtraMethod(string methodName, Action method, bool allowAddingDuplicate = true) { AddExtraMethod_Base(methodName, method, allowAddingDuplicate); }
		public void AddExtraMethod<A1>(string methodName, Action<A1> method, bool allowAddingDuplicate = true) { AddExtraMethod_Base(methodName, method, allowAddingDuplicate); }
		public void AddExtraMethod<A1, A2>(string methodName, Action<A1, A2> method, bool allowAddingDuplicate = true) { AddExtraMethod_Base(methodName, method, allowAddingDuplicate); }
		public void AddExtraMethod<A1, A2, A3>(string methodName, Action<A1, A2, A3> method, bool allowAddingDuplicate = true) { AddExtraMethod_Base(methodName, method, allowAddingDuplicate); }
		public void RemoveExtraMethod_Base(string methodName, Delegate method) {
			if (_extraMethods.ContainsKey(methodName))
				_extraMethods[methodName].Remove(method);
		}
		public void RemoveExtraMethod(string methodName, Action method) { RemoveExtraMethod_Base(methodName, method); }
		public void RemoveExtraMethod<A1>(string methodName, Action<A1> method) { RemoveExtraMethod_Base(methodName, method); }
		public void RemoveExtraMethod<A1, A2>(string methodName, Action<A1, A2> method) { RemoveExtraMethod_Base(methodName, method); }
		public void RemoveExtraMethod<A1, A2, A3>(string methodName, Action<A1, A2, A3> method) { RemoveExtraMethod_Base(methodName, method); }

		// removes extra-method based on class it was declared in (for removing lambdas)
		// example: void _PreRemoveFromMainTree_EM1() { VO.main.RemoveExtraMethod("PreViewFrameTick", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType); }
		/*public void RemoveExtraMethod(string methodName, Type type) {
			foreach (Delegate extraMethod in _extraMethods[methodName].ToList())
				//if (extraMethod is Action && extraMethod.Method.ReflectedType == type)
				if (extraMethod is Action && extraMethod.Method.DeclaringType == type)
					RemoveExtraMethod(methodName, (Action)extraMethod);
		}*/

		/*public bool HasMethod(string methodName, Type excludeTag)
			{ return (VTypeInfo.Get(GetType()).methods.ContainsKey(methodName) && !VTypeInfo.Get(GetType()).methods[methodName].tags.Any(excludeTag.IsInstanceOfType)) || _extraMethods.ContainsKey(methodName); }*/
		public object CallMethod(string methodName, params object[] args) {
			if (args.LastOrDefault() is Change && (args.Last() as Change).messages.Contains("from file"))
				return CallMethod(typeof(IgnoreStartData), methodName, args);
			return CallMethod(null, methodName, args);
		}
		//public static bool autoProfileEnabled = true;
		public object CallMethod(Type excludeTag, string methodName, params object[] args) { // required arg comes just before free-arg-list, so that this full version isn't used unless the args are filled
			//var S = M.GetCurrentMethod().Profile_CurrentRun(methodName);
			BlockRunInfo S = null;
			/*if (autoProfileEnabled)
				S = Profiler_LastDataFrame.CurrentBlock.StartMethod(methodName);*/

			try {
				object result = null;
				var typeInfo = VTypeInfo.Get(GetType());
				var method = typeInfo.GetMethod(methodName);
				//var method_profile = method != null && !method.tags.Any(typeof(NoAutoProfile).IsInstanceOfType);
				/*if (method == null || method_profile)
					S = Profiler_LastDataFrame.CurrentBlock.StartMethod(methodName);*/

				if (method != null && (excludeTag == null || !method.tags.Any(excludeTag.IsInstanceOfType)))
					//result = method.Call_Advanced(this, method_profile, args); // for now, only main method can send back result
					result = method.Call_Advanced(this, !method.tags.Any(typeof(NoAutoProfile).IsInstanceOfType), args); // for now, only main method can send back result
				if (_extraMethods.ContainsKey(methodName))
					//foreach (Delegate method2 in _extraMethods[methodName]) // maybe todo: add way to "tag" extra-methods (so we can exclude the ones with the given exclude-tag)
					foreach (Delegate method2 in _extraMethods[methodName].ToList())
						//method2.Call_Advanced(method_profile, args);
						method2.Call_Advanced(true, args);

				var extraMethodNumber = 1;
				while ((method = typeInfo.GetMethod(methodName + "_EM" + extraMethodNumber)) != null) {
					if (method != null && (excludeTag == null || !method.tags.Any(excludeTag.IsInstanceOfType)))
						method.Call(this, args);
					if (_extraMethods.ContainsKey(methodName))
						//foreach (Delegate method2 in _extraMethods[methodName]) // maybe todo: add way to "tag" extra-methods (so we can exclude the ones with the given exclude-tag)
						foreach (Delegate method2 in _extraMethods[methodName].ToList())
							//method2.Call_Advanced(method_profile, args);
							method2.Call_Advanced(true, args);

					extraMethodNumber++;
				}

				return result;
			}
			catch (Exception ex) { // probably old: error should not stop the other parts of the tree from getting the message
				//ex.AddToMessage("\nCallMethod) NodePath:" + VConvert.ToVDF(GetPath_Absolute()) + " MethodName:" + methodName + "\n");
				//throw; //VDebug.LogException(ex);
				VDebug.RethrowException(ex, "\nCallMethod) NodePath:" + VConvert.ToVDF(GetPath_Absolute()) + " MethodName:" + methodName + "\n");
				throw null; // this never actually runs, but lets method compile
			}
			/*catch (TargetInvocationException ex) {
				//ex.AddToMessage("\nCallMethod) NodePath:" + VConvert.ToVDF(GetPath_Absolute()) + " MethodName:" + methodName + "\n");
				//throw; //VDebug.LogException(ex);
				VDebug.RethrowInnerExceptionOf(ex, "\nCallMethod) NodePath: " + VConvert.ToVDF(GetPath_Absolute()) + " MethodName: " + methodName + "\n");
				throw null; // this never actually runs, but lets method compile
			}*/
			/*finally {
				if (S != null)
					S._____(null);
			}*/
		}
	}
}