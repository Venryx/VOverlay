using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using VDFN;
using VectorStructExtensions;
using VTree;
using VTree_Structures;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace VTree.BiomeDefenseN.ObjectsN.ObjectN {
	[VDFType(popOutL1: true)] public class VComponent : Node {
		//[VDFPreDeserialize] public VComponent() {}
		static Dictionary<Type, VComponent> type_baseState = new Dictionary<Type, VComponent>();

		VDFNode _deserializeNode;
		[VDFPostDeserialize] void PostDeserialize(VDFNode node) { _deserializeNode = node; }
		[VDFPreSerializeProp] VDFNode PreSerializeProp(VDFNodePath path, VDFSaveOptions options) {
			/*if (obj == null) // if not child of VObject, don't make any changes (i.e. serialize everything)
				return null;*/
			
			var objProp = path.parentNode.prop;
			/*if (objProp == null) // if being called from VObject>LoadDataFromType() method, don't make any changes (i.e. serialize everything)
				return null;*/
			var prop = path.currentNode.prop;
			if (ShouldIgnoreProp(prop, options as VDFSaveOptionsV)) // run quick checks first
				return VDF.CancelSerialize;

			/*var propV = prop.VInfo();
			if (propV.noUIInstance && !(obj is VObject_Type) && options.messages.Contains("to js"))
				return VDF.CancelSerialize;*/

			VComponent defaultsComp;
			if (obj != null && obj.type != null)
				defaultsComp = (VComponent)objProp.GetValue(obj.type);
			else
				//defaultsComp = type_baseState.GetValueOrX(GetType()) ?? (type_baseState[GetType()] = (VComponent)VDFNode.CreateNewInstanceOfType(GetType()));
				// use actual constructor, so prop-initializers are run
				defaultsComp = type_baseState.GetValueOrX(GetType()) ?? (type_baseState[GetType()] = (VComponent)Activator.CreateInstance(GetType(), true));
				//defaultsComp = type_baseState.GetValueOrX(GetType()) ?? (type_baseState[GetType()] = (VComponent)V.CreateInstance(GetType()));

			// if prop-value is same as default-comp prop-value, no point serializing
			if (V.Equals(prop.GetValue(this), prop.GetValue(defaultsComp), trueIfSameItems: true))
				return VDF.CancelSerialize;

			return null;
		}

		// this runs during load-from-file, to copy prop-values from type-comp to instance-comp, for object-comps which had data trimmed (as happens during saving of map, when prop-values are same as type-comp's)
		/*public void LoadDataFromTypeComponent(VComponent typeComp) { // technically, typeComp clone (i.e. its data is already cloned/instanced for this component instance)
			foreach (VPropInfo prop in VTypeInfo.Get(GetType()).props.Values) {
				//var propName = prop.memberInfo.Name;
				var propValue_type = prop.GetValue(typeComp);

				// if comp was loaded from VDF, but this prop was set only for the type-comp
				//var valueDifferentThanDefault = Equals(prop.GetValue(this), prop.GetValue(defaultObj));
				var valueInTypeDiffersFromDefault = typeComp._deserializeNode[prop.memberInfo.Name] != null;
				if (!valueInTypeDiffersFromDefault) // if type prop-value is no different than the class default, no point transferring
					continue;

				var valueInInstanceSetFromVDF = _deserializeNode == null || _deserializeNode[prop.memberInfo.Name] != null;

				// if prop-value is [a Node] or [a collection with Nodes], and not by-reference
				if ((propValue_type is Node
						|| (propValue_type is IList && (propValue_type as IList).ToList_Object().Any(a=>a is Node))
						|| (propValue_type is IDictionary && (propValue_type as IDictionary).Values.ToList_Object().Any(a=>a is Node)))
					&& !prop.tags.Any(a=>a is ByPath || a is ByName))
				{
					//var propValueClone = VConvert.FromVDF(VConvert.ToVDF(propValue, options: new VDFSaveOptions(new List<object> {"VObject>clone"})), propValue.GetType());
					var propValue_instance = VConvert.FromVDF(VConvert.ToVDF(propValue_type), propValue_type.GetType());

					// if child is a VComponent, and it has data from-vdf, ask it to transfer data from the type-comp's component-clone to itself (rather than just overwriting the VComponent child)
					/*if (propValue_type is VComponent && valueInInstanceSetFromVDF)
						(prop.GetValue(this) as VComponent).LoadDataFromTypeVObjectComponent((VComponent)propValue_instance);
					else // else, just overwrite instance-comp's prop-value with from-type-comp prop-value*#/
					prop.SetValue(this, propValue_instance);
					if (!prop.tags.Any(a=>a is ByPath || a is ByName))
						if (propValue_type is Node)
							//(propValueClone as Node).SetParentAndPathNode(this, VConvert.Clone((propValue as Node)._pathNode));
							//(propValueClone as Node).SetParentAndPathNode(this, new NodePathNode(propName));
							(propValue_instance as Node).FakeAdd(new NodeAttachPoint(this, prop));
						else if (propValue_type is IList && (propValue_type as IList).ToList_Object().Any(a => a is Node))
						{
							foreach (var pair in (propValue_instance as IList).Pairs())
								if (pair.item is Node)
									(pair.item as Node).FakeAdd(new NodeAttachPoint(this, prop, pair.index));
						}
						else //if (propValue is IDictionary && (propValue as IDictionary).Values.ToList_Object().Any(a=>a is Node))
							foreach (var pair in (propValue_instance as IDictionary).Pairs())
								if (pair.Value is Node)
									(pair.Value as Node).FakeAdd(new NodeAttachPoint(this, prop, map_key: pair.Key));
				}
				else
				{
					if (valueInInstanceSetFromVDF) // if instance-comp prop-value was set from vdf, that overrides the type-comp prop-value, so don't transfer
						continue;
					prop.SetValue(this, propValue_type);
				}
			}
		}*/

		//void _PostAddToMainTree() { _obj = Parent as VObject; }
		[ToMainTree] protected void _PostAdd_Early() {
			obj = Parent as VObject;
			//map = obj != null ? obj.map : region.map;

			// you can think of typeComp as a shortcut for obj.type.[comp-prop-name], except it also works when self is itself the type-comp (i.e. has parent-obj that's a type)
			var typeCompProp = VTypeInfo.Get(GetType()).props.GetValueOrX("typeComp");
			if (typeCompProp != null) {
				var typeComp = obj is VObject_Type ? this : attachPoint.prop.GetValue(obj.type); // if parent is type, we are the type-comp
				typeCompProp.SetValue(this, typeComp);
			}
		}

		//[P(false)] public Map map;

		// for if child of VObject
		[P(false)] public VObject obj;
	}
}