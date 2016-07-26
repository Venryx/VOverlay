using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;
using VDFN;
using VectorStructExtensions;
using VTree;
using VTree.BiomeDefenseN.MapsN;
using VTree.BiomeDefenseN.MapsN.MapN;
using VTree.BiomeDefenseN.MatchesN;
using VTree.BiomeDefenseN.ObjectsN.ObjectN.ComponentsN;
using VTree.VOverlayN.MapsN;
using VTree.VOverlayN.MapsN.MapN;
using VTree.VOverlayN.ObjectsN.ObjectN.ComponentsN;
using VTree.VOverlayN.SharedN;
using VTree_Structures;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace VTree.BiomeDefenseN.ObjectsN {
	[VDFType(popOutL1: true)] public class VObject : Node {
		public static VObject baseState = new VObject();

		// VDF
		// ==========

		VObject s;
		[VDFPreDeserialize] public VObject() {}

		VDFNode _deserializeNode;
		[VDFPostDeserialize] void PostDeserialize(VDFNode node) { _deserializeNode = node; }
		//static string[] toJS_baseProps = {""};
		[VDFPreSerializeProp] protected virtual VDFNode PreSerializeProp(VDFNodePath path, VDFSaveOptions options) {
			var prop = path.currentNode.prop;
			if (ShouldIgnoreProp(prop, options as VDFSaveOptionsV)) // run quick checks first
				return VDF.CancelSerialize;
			//var propName = prop.memberInfo.Name;
			var optionsV = options as VDFSaveOptionsV;

			// if saving to map-file
			if (optionsV && optionsV.toMap) {
				if (V.Equals(prop.GetValue(this), prop.GetValue(type), trueIfSameItems: true))
					return VDF.CancelSerialize;
			}
			// if saving to js (as part of map)
			else {
				//if (options.messages.Contains("VObject>send full data")) {

				/*if (propName == "tasks" && tasks.Count == 0)
				return VDF.CancelSerialize;*/

				if (V.Equals(prop.GetValue(this), prop.GetValue(type), trueIfSameItems: true))
					return VDF.CancelSerialize;

				/*}
				else*/
			}

			return null;
		}
		/*[VDFPostSerialize] void PostSerialize_VObject(VObject obj, VDFNode result, VDFNodePath path, VDFSaveOptions options) {
			if (!(obj is VObject_Type))
				Debug.Log("Serialized VObject:" + result.ToVDF(options));
		}*/

		// general
		// ==========

		//public Map _map { get { return Parent is Map ? Parent as Map : (Parent as VTerrain).map; } }
		[P(false)] public Match match; // maybe temp
		[P(false)] public Map map;
		[ToMainTree] void _PostAdd_Early() {
			if (Parent is Map)
				map = Parent as Map;
			else if (Parent is VTerrain)
				map = (Parent as VTerrain)._map;
			if (map != null)
				match = map.match;
		}

		// call this to initialize core stuff, for VObject's that are not added to VTree yet (and maybe never)
		public void NotAddedToVTree_QuickInit() {
			transform.obj = s;
			mesh.obj = s;
		}

		//public MeshData _mesh;
		// probably make-so: these are in Mesh component
		[P(false)] public GameObject gameObject;
		[P(false)] public List<Collider2D> colliders = new List<Collider2D>();

		public bool _hasBeenManifested;
		public GameObject Manifest(GameObject parent = null) {
			var S = M.GetCurrentMethod().Profile_AllFrames();

			S._____("part 1");
			if (parent == null)
				if (attachPoint.prop.Name == "plants")
					parent = map.obj.GetChild("Plants/" + type.name, true);
				else if (attachPoint.prop.Name == "structures")
					parent = map.obj.GetChild("Structures/" + type.name, true);
				else if (attachPoint.prop.Name == "units")
					parent = map.obj.GetChild("Units/" + type.name, true);
				else //if (attachPoint.prop.Name == "projectiles")
					parent = map.obj.GetChild("Projectiles/" + type.name, true);

			S._____("instantiate game-object");
			GameObject result = Object.Instantiate(type.gameObject);
			S._____("part 2");
			result.transform.parent = parent.transform;
			result.SetLayer("Default", true); //"Item", true);
			result.AddComponent<ObjectScript>().obj = this;
			gameObject = result;

			// todo: make sure use of local/world is consistent
			S._____("part 2");
			transform.PostObjGameObjectSet();
			s.transform.ApplyTransform();
			//transform.PostTransformChange();

			/*S._____("add collider");
			if (map != null && map.match != null)
				if (type.objType == ObjectType.Structure || type.objType == ObjectType.Plant) {
					var collider = gameObject.AddComponent<BoxCollider2D>();
					collider.size = size.ToVector2();
					colliders.Add(collider);
				}
				else if (type.objType == ObjectType.Unit) {
					var collider = gameObject.AddComponent<CircleCollider2D>();
					//collider.offset = new Vector2(.25f, .25f);
					collider.radius = VO.unitSize / 2;
					colliders.Add(collider);
				}*/

			_hasBeenManifested = true;

			S._____("manifest descendents");
			foreach (var child in _children)
				child.BroadcastMessage(ContextGroup.Local_CS, "Manifest");

			S._____(null);
			return result;
		}
		public void Unmanifest() {
			V.Destroy(gameObject);
			foreach (var child in _children)
				child.BroadcastMessage(ContextGroup.Local_CS, "Unmanifest");
		}
		void _PreRemoveFromMainTree() { Unmanifest(); }

		[NotTo("objType,js")] public VTransform transform = new VTransform();
		[NotTo("obj")] public MeshComp mesh = new MeshComp();
		[NotTo("obj")] public Platform platform;
		[NotTo("obj")] public Emoji emoji;
		[NotTo("obj")] public Block block;

		// update
		// ==========
		
		public void PreViewFrameTick(int frame) { // uses simple method, to be consistent with PostDataFrameTick method's call-approach
			if (map == null || map.match == null || !map._initialBuildCompleted)
				return;

			//S._____("ripple call to components");
			if (transform != null) transform.PreViewFrameTick();
			if (platform != null) platform.PreViewFrameTick();
			if (emoji != null) emoji.PreViewFrameTick();
			if (block != null) block.PreViewFrameTick();
		}
		
		public int lastProcessedFrame = -1;
		public void PostDataFrameTick(int frame) {
			lastProcessedFrame = frame;

			// ripple call to components
			if (platform != null) platform.PreViewFrameTick();
			if (emoji != null) emoji.PreViewFrameTick();
			if (isProjectile != null) isProjectile.PostDataFrameTick();

			transform.PostDataFrameTick(); // call this last, since it should update bounds only once at end of frame
		}
		
		// general
		// ==========

		[ByName] public VObject_Type type;
		public VObject_Type Type_Safe {
			get {
				var selfAsType = this as VObject_Type;
				var str = this.GetType().Name;
				return selfAsType ?? type;
			}
		}
		void type_PostSet(VObject oldValue, Change_Set change) {
			// for each property that hasn't been set from VDF, set it's value equal to the type-v-object's
			if (type != null)
				LoadDataFromType();
		}
		// this runs during load-from-file, to copy prop-values from type to instance, for objects which had props trimmed (as happens during saving of map, when prop-values are same as type's)
		public void LoadDataFromType() {
			foreach (var pair in VTypeInfo.Get(GetType()).props) {
				var propName = pair.Key;
				var prop = pair.Value;
				//if (!nodeDefaultPropRegex.IsMatch(pair.Key) || !(pair.Value.memberInfo is FieldInfo) || pair.Value.tags.Any(a=>a is VDFProp && !(a as VDFProp).includeL2))
				if (propName == "s" || propName == "typeComp" || propName.StartsWith("_") || !(prop.memberInfo is FieldInfo) || prop.tags.Any(a=>a is VDFProp && !(a as VDFProp).includeL2))
					continue;
				var propValue_type = prop.GetValue(type);

				// if object was loaded from VDF, but this prop was set only for the type-v-object
				//var valueDifferentThanDefault = Equals(prop.GetValue(this), prop.GetValue(defaultObj));
				var valueInTypeDiffersFromDefault = type._deserializeNode[prop.memberInfo.Name] != null || propName == "name" || propName == "objType";
				if (!valueInTypeDiffersFromDefault) // if type prop-value is no different than the class default, no point transferring
					continue;

				var valueInInstanceSetFromVDF = _deserializeNode == null || _deserializeNode[prop.memberInfo.Name] != null;

				// if prop-value is [a Node] or [a collection with Nodes], and not by-reference
				if ((propValue_type is Node
						|| (propValue_type is IList && (propValue_type as IList).ToList_Object().Any(a=>a is Node))
						|| (propValue_type is IDictionary && (propValue_type as IDictionary).Values.ToList_Object().Any(a=>a is Node)))
					&& !prop.tags.Any(a=>a is ByPath || a is ByName)) {
				}
				else {
					if (valueInInstanceSetFromVDF) // if instance prop-value was set from vdf, that overrides the type prop-value, so don't transfer
						continue;
					prop.SetValue(this, propValue_type);
				}
			}
		}
		[ByPath] public Player owner;
		public bool IsControllableBy(Player player) { return owner == player; }
		public int attachFrame = -1;
		void _PostAdd_EM1() {
			if (!(Parent is Map) || (Parent as Map).match == null)
				return;
			if (attachFrame == -1)
				s.a(a=>a.attachFrame).set_self = (Parent as Map).match.dataFrame;
		}

		public void Remove() {
			if (attachPoint != null)
				if (attachPoint.prop.Name == "plants")
					map.a(a=>a.plants).remove = this;
				else if (attachPoint.prop.Name == "structures")
					map.a(a=>a.structures).remove = this;
				else if (attachPoint.prop.Name == "units")
					map.a(a=>a.units).remove = this;
				else if (attachPoint.prop.Name == "projectiles")
					map.a(a=>a.projectiles).remove = this;
		}

		// plant
		// ==========
		
		// structure
		// ==========

		// unit
		// ==========

		public string emojiStr;

		public void ReceiveAttack(Player attacker) { Remove(); }

		// other
		// ==========

		public IsProjectile isProjectile;

		// maybe temp
		/*[P(false)] bool postAdd_fullyDone;
		void _PostAdd_EM3() { postAdd_fullyDone = true; }*/
	}
}