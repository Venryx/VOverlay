using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace VDFN {
	public enum VDFTypeMarking {
		None,
		Internal,
		External,
		ExternalNoCollapse // maybe temp
	}
	public class VDFSaveOptions {
		public VDFSaveOptions(List<object> messages = null, VDFTypeMarking typeMarking = VDFTypeMarking.Internal,
			bool useMetadata = true, bool useChildPopOut = true, bool useStringKeys = false, bool useNumberTrimming = true, bool useCommaSeparators = false, 
			Dictionary<MemberInfo, bool> propInclusionL3 = null, Dictionary<string, string> namespaceAliasesByName = null, Dictionary<Type, string> typeAliasesByType = null
		) {
			this.messages = messages ?? new List<object>();
			this.typeMarking = typeMarking;
			this.useMetadata = useMetadata;
			this.useChildPopOut = useChildPopOut;
			this.useStringKeys = useStringKeys;
			this.useNumberTrimming = useNumberTrimming;
			this.useCommaSeparators = useCommaSeparators;
			this.propInclusionL3 = propInclusionL3 ?? new Dictionary<MemberInfo, bool>();
			this.namespaceAliasesByName = namespaceAliasesByName ?? new Dictionary<string, string>();
			this.typeAliasesByType = typeAliasesByType ?? new Dictionary<Type, string>();
		}

		public List<object> messages;
		public VDFTypeMarking typeMarking;

		// for JSON compatibility
		public bool useMetadata;
		public bool useChildPopOut;
		public bool useStringKeys;
		public bool useNumberTrimming; // e.g. trims 0.123 to .123
		public bool useCommaSeparators; // currently only applies to non-popped-out children

		// CS only
		public Dictionary<MemberInfo, bool> propInclusionL3;
		public Dictionary<string, string> namespaceAliasesByName;
		public Dictionary<Type, string> typeAliasesByType;

		public VDFSaveOptions ForJSON() { // helper function for JSON compatibility
			useMetadata = false;
			useChildPopOut = false;
			useStringKeys = true;
			useNumberTrimming = false;
			useCommaSeparators = true;
			return this;
		}
	}

	public static class VDFSaver
	{
		public static VDFNode ToVDFNode<T>(object obj, VDFSaveOptions options = null) { return ToVDFNode(obj, typeof(T), options); }
		public static VDFNode ToVDFNode(object obj, VDFSaveOptions options) { return ToVDFNode(obj, null, options); }
		public static VDFNode ToVDFNode(object obj, Type declaredType = null, VDFSaveOptions options = null, VDFNodePath path = null, bool declaredTypeInParentVDF = false) {
			declaredType = declaredType != null ? declaredType.ToVDFType() : null;
			options = options ?? new VDFSaveOptions();
			path = path ?? new VDFNodePath(new VDFNodePathNode(obj));

			Type type = obj != null ? obj.GetVDFType() : null;
			var typeGenericArgs = VDF.GetGenericArgumentsOfType(type);
			var typeInfo = type != null ? VDFTypeInfo.Get(type) : null; //VDFTypeInfo.Get(type) : null; // so anonymous object can be recognized

			if (obj != null)
				foreach (VDFMethodInfo method in VDFTypeInfo.Get(type).methods_preSerialize)
					if (method.Call(obj, path, options) == VDF.CancelSerialize)
						return VDF.CancelSerialize;

			VDFNode result = null;
			bool serializedByCustomMethod = false;
			if (obj != null)
				foreach (VDFMethodInfo method in VDFTypeInfo.Get(type).methods_serialize) {
					object serializeResult = method.Call(obj, path, options);
					if (serializeResult != VDF.NoActionTaken) {
						result = (VDFNode)serializeResult;
						serializedByCustomMethod = true;
					}
				}

			if (!serializedByCustomMethod) {
				result = new VDFNode();
				if (obj == null) {} //result.primitiveValue = null;}
				else if (VDF.GetIsTypePrimitive(type))
					result.primitiveValue = obj;
				else if (type.IsEnum) // helper exporter for enums
					result.primitiveValue = obj.ToString();
				else if (obj is IList) { // this saves arrays also
					result.isList = true;
					var objAsList = (IList)obj;
					for (var i = 0; i < objAsList.Count; i++) {
						var itemNode = ToVDFNode(objAsList[i], typeGenericArgs[0], options, path.ExtendAsListItem(i, objAsList[i]), true);
						if (itemNode == VDF.CancelSerialize)
							continue;
						result.listChildren.Add(itemNode);
					}
				}
				else if (obj is IDictionary) {
					result.isMap = true;
					var objAsDictionary = (IDictionary)obj;
					var index = 0;
					foreach (object key in objAsDictionary.Keys) {
						index++;
						var keyNode = ToVDFNode(key, typeGenericArgs[0], options, path.ExtendAsMapKey(index, key), true); // stringify-attempt-1: use exporter
						if (!(keyNode.primitiveValue is string)) // if stringify-attempt-1 failed (i.e. exporter did not return string), use stringify-attempt-2
							//throw new VDFException("A map key object must either be a string or have an exporter that converts it into a string.");
							keyNode = new VDFNode(key.ToString());
						var valueNode = ToVDFNode(objAsDictionary[key], typeGenericArgs[1], options, path.ExtendAsMapItem(key, objAsDictionary[key]), true);
						if (valueNode == VDF.CancelSerialize)
							continue;
						result.mapChildren.Add(keyNode, valueNode);
					}
				}
				else { // if an object, with properties
					result.isMap = true;
					foreach (string propName in typeInfo.props.Keys)
						try {
							VDFPropInfo propInfo = typeInfo.props[propName];
							//bool include = (typeInfo.typeTag.propIncludeRegexL1 != null ? new Regex("^" + typeInfo.propIncludeRegexL1 + "$")..IsMatch(propName) : false);
							/*bool include = (typeInfo.typeTag.propIncludeRegexL1 != null ? new Regex(typeInfo.typeTag.propIncludeRegexL1).IsMatch(propName) : false);
							include = propInfo.propTag != null ? propInfo.propTag.includeL2 : include;
							include = options.propIncludesL3.Contains(propInfo.memberInfo) || options.propIncludesL3.Contains(VDF.AnyMember) ? true : include;
							include = options.propExcludesL4.Contains(propInfo.memberInfo) || options.propExcludesL4.Contains(VDF.AnyMember) ? false : include;
							include = options.propIncludesL5.Contains(propInfo.memberInfo) || options.propIncludesL5.Contains(VDF.AnyMember) ? true : include;*/
							bool include = options.propInclusionL3.GetValueOrX(propInfo.memberInfo) ?? options.propInclusionL3.GetValueOrX(VDF.AnyMember)
								?? (propInfo.propTag != null ? propInfo.propTag.includeL2 : (bool?)null)
								?? typeInfo.typeTag.propIncludeRegexL1 != null && new Regex(typeInfo.typeTag.propIncludeRegexL1).IsMatch(propName);
							if (!include)
								continue;

							object propValue = propInfo.GetValue(obj);
							if (!propInfo.ShouldValueBeSaved(propValue))
								continue;

							VDFNodePath childPath = path.ExtendAsChild(propInfo, propValue);
							var canceled = false;
							foreach (VDFMethodInfo method in typeInfo.methods_preSerializeProp)
								if (method.Call(obj, childPath, options) == VDF.CancelSerialize)
									canceled = true;
							if (canceled)
								continue;

							var propNameNode = new VDFNode(propName);
							// if obj is an anonymous type, considers its props' declared-types to be null, since even internal loading doesn't have a class declaration it can look up
							var propValueNode = ToVDFNode(propValue, !type.Name.Contains("<") ? propInfo.GetPropType() : null, options, childPath);
							if (propValueNode == VDF.CancelSerialize)
								continue;
							propValueNode.childPopOut = options.useChildPopOut && (propInfo.propTag != null ? propInfo.propTag.popOutL2 : propValueNode.childPopOut);
							result.mapChildren.Add(propNameNode, propValueNode);
						}
						catch (Exception ex) { throw new VDFException("Error saving property '" + propName + "'.", ex); }/**/finally{}
						/*catch (Exception ex)
						{
							var field = ex.GetType().GetField("message", BindingFlags.NonPublic | BindingFlags.Instance) ?? ex.GetType().GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance);
							field.SetValue(ex, ex.Message + "\n==================\nRethrownAs) " + ("Error saving property '" + propName + "'.") + "\n");
							throw;
						}*/
				}
			}

			if (declaredType == null)
				if (result.isList || result.listChildren.Count > 0)
					declaredType = typeof(List<object>);
				else if (result.isMap || result.mapChildren.Count > 0)
					declaredType = typeof(Dictionary<object, object>);
				else
					declaredType = typeof(object);
			if (options.useMetadata && type != null && !VDF.GetIsTypeAnonymous(type) && (
				(options.typeMarking == VDFTypeMarking.Internal && !VDF.GetIsTypePrimitive(type) && type != declaredType)
				|| (options.typeMarking == VDFTypeMarking.External && !VDF.GetIsTypePrimitive(type) && (type != declaredType || !declaredTypeInParentVDF))
				|| options.typeMarking == VDFTypeMarking.ExternalNoCollapse
			))
				result.metadata = VDF.GetNameOfType(type, options);

			if (result.metadata_override != null)
				result.metadata = result.metadata_override;
					
			if (options.useChildPopOut && typeInfo != null && typeInfo.typeTag.popOutL1)
				result.childPopOut = true;

			if (obj != null)
				foreach (VDFMethodInfo method in VDFTypeInfo.Get(type).methods_postSerialize)
					method.Call(obj, result, path, options);

			return result;
		}
	}
}