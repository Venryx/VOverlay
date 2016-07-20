using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace VDFN {
	public class VDFNode {
		public string metadata;
		public string metadata_override; // maybe temp
		public object primitiveValue;
		public List<VDFNode> listChildren = new List<VDFNode>();
		public Dictionary<VDFNode, VDFNode> mapChildren = new Dictionary<VDFNode, VDFNode>(); // holds object-properties, as well as dictionary-key-value-pairs

		public VDFNode(object primitiveValue = null, string metadata = null) {
			this.primitiveValue = primitiveValue;
			this.metadata = metadata;
		}

		public VDFNode this[int index] {
			get { return listChildren.Count > index ? listChildren[index] : null; }
			set {
				if (listChildren.Count == index) // lets you add new items easily: vdfNode[0] = new VDFNode();
					listChildren.Add(value);
				else
					listChildren[index] = value;
			}
		}
		public VDFNode this[string key] {
			get {
				//return mapChildren.ContainsKey(key) ? mapChildren[key] : null;
				foreach (KeyValuePair<VDFNode, VDFNode> pair in mapChildren)
					if (pair.Key.primitiveValue is string && (string)pair.Key.primitiveValue == key)
						return pair.Value;
				return null;
			}
			set { mapChildren[key] = value; }
		}
		public bool Equals(VDFNode other) { return ToVDF() == other.ToVDF(); } // base equality on whether their 'default output' is the same

		// base-types: ["bool", "char", "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong", "float", "double", "decimal", "string"]
		public T As_Base<T>() { return (T)Convert.ChangeType(primitiveValue, typeof(T)); } //(T)primitiveValue; }
		public static implicit operator bool(VDFNode node) { return node.As_Base<bool>(); ; }
		public static implicit operator VDFNode(bool val) { return new VDFNode(val); }
		public static implicit operator char(VDFNode node) { return node.As_Base<char>(); }
		public static implicit operator VDFNode(char val) { return new VDFNode(val); }
		public static implicit operator byte(VDFNode node) { return node.As_Base<byte>(); }
		public static implicit operator VDFNode(byte val) { return new VDFNode(val); }
		public static implicit operator sbyte(VDFNode node) { return node.As_Base<sbyte>(); }
		public static implicit operator VDFNode(sbyte val) { return new VDFNode(val); }
		public static implicit operator short(VDFNode node) { return node.As_Base<short>(); }
		public static implicit operator VDFNode(short val) { return new VDFNode(val); }
		public static implicit operator ushort(VDFNode node) { return node.As_Base<ushort>(); }
		public static implicit operator VDFNode(ushort val) { return new VDFNode(val); }
		public static implicit operator int(VDFNode node) { return node.As_Base<int>(); }
		public static implicit operator VDFNode(int val) { return new VDFNode(val); }
		public static implicit operator uint(VDFNode node) { return node.As_Base<uint>(); }
		public static implicit operator VDFNode(uint val) { return new VDFNode(val); }
		public static implicit operator long(VDFNode node) { return node.As_Base<long>(); }
		public static implicit operator VDFNode(long val) { return new VDFNode(val); }
		public static implicit operator ulong(VDFNode node) { return node.As_Base<ulong>(); }
		public static implicit operator VDFNode(ulong val) { return new VDFNode(val); }
		public static implicit operator float(VDFNode node) { return node.As_Base<float>(); }
		public static implicit operator VDFNode(float val) { return new VDFNode(val); }
		public static implicit operator double(VDFNode node) { return node.As_Base<double>(); }
		public static implicit operator VDFNode(double val) { return new VDFNode(val); }
		public static implicit operator decimal(VDFNode node) { return node.As_Base<decimal>(); }
		public static implicit operator VDFNode(decimal val) { return new VDFNode(val); }
		public static implicit operator string(VDFNode node) { return node.As_Base<string>(); }
		public static implicit operator VDFNode(string val) { return new VDFNode(val); }
		//public override string ToString() { return this; } // another way of calling the above string cast; equivalent to: (string)vdfNode
		public override string ToString() { return primitiveValue != null ? primitiveValue.ToString() : ""; } // helpful for debugging

		// saving
		// ==========

		static string PadString(string unpaddedString) {
			var result = unpaddedString;
			if (result.StartsWith("<") || result.StartsWith("#"))
				result = "#" + result;
			if (result.EndsWith(">") || result.EndsWith("#"))
				result += "#";
			return result;
		}

		public bool isList; // can also be inferred from use of list-children collection
		public bool isMap; // can also be inferred from use of map-children collection
		public bool childPopOut;
		public string ToVDF(VDFSaveOptions options = null, int tabDepth = 0) { return ToVDF_InlinePart(options, tabDepth) + ToVDF_PoppedOutPart(options, tabDepth); }
		public char[] charsThatNeedEscaping_1 = {'"', '\'', '\n'};
		public char[] charsThatNeedEscaping_2 = {'{', '}', '[', ']', ':'};
		/*public Regex charsThatNeedEscaping_1_regex = new Regex("\"|'|\n|<<|>>");
		public Regex charsThatNeedEscaping_2_regex = new Regex(@"{|}|\[|\]|:");*/
		public string ToVDF_InlinePart(VDFSaveOptions options = null, int tabDepth = 0, bool isKey = false) {
			options = options ?? new VDFSaveOptions();

			var builder = new StringBuilder();

			if (options.useMetadata && metadata != null)
				builder.Append(metadata).Append(">");

			if (primitiveValue == null) {
				if (!isMap && mapChildren.Count == 0 && !isList && listChildren.Count == 0)
					builder.Append("null");
			}
			else if (primitiveValue is bool)
				builder.Append(primitiveValue.ToString().ToLower());
			else if (primitiveValue is string) {
				var unpaddedString = (string)primitiveValue;
				// (the parser doesn't actually need '<<' and '>>' wrapped for single-line strings, but we do so for consistency)
				//var needsEscaping = unpaddedString.Contains("\"") || unpaddedString.Contains("'") || unpaddedString.Contains("\n") || unpaddedString.Contains("<<") || unpaddedString.Contains(">>");
				var needsEscaping = unpaddedString.IndexOfAny(charsThatNeedEscaping_1) != -1 || unpaddedString.Contains("<<") || unpaddedString.Contains(">>");
				//var needsEscaping = charsThatNeedEscaping_1_regex.IsMatch(unpaddedString);
				if (isKey)
					//needsEscaping = needsEscaping || unpaddedString.Contains("{") || unpaddedString.Contains("}") || unpaddedString.Contains("[") || unpaddedString.Contains("]") || unpaddedString.Contains(":");
					needsEscaping = needsEscaping || unpaddedString.IndexOfAny(charsThatNeedEscaping_2) != -1;
					//needsEscaping = needsEscaping || charsThatNeedEscaping_2_regex.IsMatch(unpaddedString);
				if (needsEscaping) {
					var literalStartMarkerString = "<<";
					var literalEndMarkerString = ">>";
					while (unpaddedString.Contains(literalStartMarkerString) || unpaddedString.Contains(literalEndMarkerString)) {
						literalStartMarkerString += "<";
						literalEndMarkerString += ">";
					}
					builder.Append(isKey ? "" : "\"").Append(literalStartMarkerString).Append(PadString(unpaddedString)).Append(literalEndMarkerString).Append(isKey ? "" : "\"");
				}
				else
					builder.Append(isKey ? "" : "\"").Append(unpaddedString).Append(isKey ? "" : "\"");
			}
			else if (VDF.GetIsTypePrimitive(primitiveValue.GetType())) // if number
				builder.Append(options.useNumberTrimming && primitiveValue.ToString().StartsWith("0.") ? primitiveValue.ToString().Substring(1) : primitiveValue);
			else
				builder.Append("\"").Append(primitiveValue).Append("\"");

			if (options.useChildPopOut && childPopOut) {
				if (isMap || mapChildren.Count > 0)
					builder.Append(mapChildren.Count > 0 ? "{^}" : "{}");
				if (isList || listChildren.Count > 0)
					builder.Append(listChildren.Count > 0 ? "[^]" : "[]");
			}
			else {
				if (isMap || mapChildren.Count > 0) {
					builder.Append("{");
					var i = -1;
					foreach (KeyValuePair<VDFNode, VDFNode> pair in mapChildren) {
						i++;
						var keyStr = pair.Key.ToVDF_InlinePart(options, tabDepth, true);
						var valueStr = pair.Value.ToVDF_InlinePart(options, tabDepth);
						builder.Append(i == 0 ? "" : (options.useCommaSeparators ? "," : " ")).Append(options.useStringKeys ? "\"" : "").Append(keyStr).Append(options.useStringKeys ? "\"" : "").Append(":").Append(valueStr);
					}
					builder.Append("}");
				}
				if (isList || listChildren.Count > 0) {
					builder.Append("[");
					for (var i = 0; i < listChildren.Count; i++)
						builder.Append(i == 0 ? "" : (options.useCommaSeparators ? "," : " ")).Append(listChildren[i].ToVDF_InlinePart(options, tabDepth));
					builder.Append("]");
				}
			}

			return builder.ToString();
		}
		public string ToVDF_PoppedOutPart(VDFSaveOptions options = null, int tabDepth = 0) {
			options = options ?? new VDFSaveOptions();

			var builder = new StringBuilder();

			// include popped-out-content of direct children (i.e. a single directly-under group)
			if (options.useChildPopOut && childPopOut) {
				var childTabStr = "";
				for (var i = 0; i < tabDepth + 1; i++)
					childTabStr += "\t";
				if (isMap || mapChildren.Count > 0)
					foreach (KeyValuePair<VDFNode, VDFNode> pair in mapChildren) {
						var keyStr = pair.Key.ToVDF_InlinePart(options, tabDepth, true);
						var valueStr = pair.Value.ToVDF_InlinePart(options, tabDepth + 1);
						builder.Append("\n").Append(childTabStr).Append(options.useStringKeys ? "\"" : "").Append(keyStr).Append(options.useStringKeys ? "\"" : "").Append(":" + valueStr);
						var poppedOutChildText = pair.Value.ToVDF_PoppedOutPart(options, tabDepth + 1);
						if (poppedOutChildText.Length > 0)
							builder.Append(poppedOutChildText);
					}
				if (isList || listChildren.Count > 0)
					foreach (VDFNode item in listChildren) {
						builder.Append("\n").Append(childTabStr).Append(item.ToVDF_InlinePart(options, tabDepth + 1));
						var poppedOutChildText = item.ToVDF_PoppedOutPart(options, tabDepth + 1);
						if (poppedOutChildText.Length > 0)
							builder.Append(poppedOutChildText);
					}
			}
			else { // include popped-out-content of inline-items' descendents (i.e. one or more pulled-up groups)
				var poppedOutChildTexts = new List<string>();
				string poppedOutChildText;
				if (isMap || mapChildren.Count > 0)
					foreach (KeyValuePair<VDFNode, VDFNode> pair in mapChildren)
						if ((poppedOutChildText = pair.Value.ToVDF_PoppedOutPart(options, tabDepth)).Length > 0)
							poppedOutChildTexts.Add(poppedOutChildText);
				if (isList || listChildren.Count > 0)
					foreach (VDFNode item in listChildren)
						if ((poppedOutChildText = item.ToVDF_PoppedOutPart(options, tabDepth)).Length > 0)
							poppedOutChildTexts.Add(poppedOutChildText);
				for (var i = 0; i < poppedOutChildTexts.Count; i++) {
					poppedOutChildText = poppedOutChildTexts[i];
					var insertPoint = 0;
					while (poppedOutChildText[insertPoint] == '\n' || poppedOutChildText[insertPoint] == '\t')
						insertPoint++;
					builder.Append(insertPoint > 0 ? poppedOutChildText.Substring(0, insertPoint) : "").Append(i == 0 ? "" : "^").Append(poppedOutChildText.Substring(insertPoint));
				}
			}

			return builder.ToString();
		}

		// loading
		// ==========

		public static object CreateNewInstanceOfType(Type type) {
			if (type.IsDerivedFrom(typeof(Array))) // if array, we start out with a list, and then turn it into an array at the end
				return Activator.CreateInstance(typeof(List<>).MakeGenericType(type.GetElementType()), true);
			if (type.IsDerivedFrom(typeof(IList)) || type.IsDerivedFrom(typeof(IDictionary))) // special cases, which require that we call the constructor
				return Activator.CreateInstance(type, true);
			return FormatterServices.GetUninitializedObject(type); // preferred (for simplicity/consistency's sake): create an instance of the type, completely uninitialized 
		}

		public T ToObject<T>(VDFLoadOptions options = null) { return (T)ToObject(typeof(T), options); }
		public object ToObject(VDFLoadOptions options) { return ToObject(null, options); }
		public object ToObject(Type declaredType = null, VDFLoadOptions options = null, VDFNodePath path = null) {
			var S = options.profile ? M.GetCurrentMethod().Profile_AllFrames() : M.None_SameType; // custom [as below]
			
			options = options ?? new VDFLoadOptions();
			path = path ?? new VDFNodePath(new VDFNodePathNode());

			S._____("part 1");
			var fromVDFTypeName = "object";
			if (metadata != null)
				fromVDFTypeName = metadata;
			else if (primitiveValue is bool)
				fromVDFTypeName = "bool";
			else if (primitiveValue is int)
				fromVDFTypeName = "int";
			else if (primitiveValue is double)
				fromVDFTypeName = "double";
			else if (primitiveValue is string)
				fromVDFTypeName = "string";
			else if (primitiveValue == null)
				if (isList || listChildren.Count > 0)
					fromVDFTypeName = "List(object)";
				else if (isMap || mapChildren.Count > 0)
					fromVDFTypeName = "Dictionary(object object)"; //"object";

			S._____("get final type");
			Type finalType = declaredType;
			var fromVDFType = VDF.GetTypeByName(fromVDFTypeName, options);
			if (finalType == null || fromVDFType.IsDerivedFrom(finalType)) // if there is no declared type, or the from-vdf type is more specific than the declared type
				finalType = fromVDFType;

			S._____("call deserialize methods_from-parent [type was: " + finalType.Name + "]");
			object result = null;
			bool deserializedByCustomMethod = false;
			foreach (VDFMethodInfo method in VDFTypeInfo.Get(finalType).methods_deserialize_fromParent) {
				object deserializeResult = method.Call(null, this, path, options);
				if (deserializeResult != VDF.NoActionTaken) {
					result = deserializeResult;
					deserializedByCustomMethod = true;
				}
			}

			var S2 = S._____2("deserialize - normal path");
			if (!deserializedByCustomMethod)
				if (finalType == typeof(object)) {} //result = null;
				else if (finalType.IsEnum) // helper importer for enums
					result = Enum.Parse(finalType, primitiveValue.ToString()); //primitiveValue);
				else if (VDF.GetIsTypePrimitive(finalType)) //primitiveValue != null)
					result = Convert.ChangeType(primitiveValue, finalType);
				else if (primitiveValue != null || isList || isMap) {
					S2._____("create instance");
					result = CreateNewInstanceOfType(finalType);
					path.currentNode.obj = result;
					S2._____("into object");
					IntoObject(result, options, path);
					if (finalType.IsDerivedFrom(typeof(Array))) { // if type is array, we created a temp-list for item population; so, now, replace the temp-list with an array
						var newResult = Array.CreateInstance(finalType.GetElementType(), ((IList)result).Count);
						((IList)result).CopyTo(newResult, 0);
							result = newResult;
						}
					}
			path.currentNode.obj = result; // in case post-deserialize method was attached as extra-method to the object, that makes use of the (basically useless) path.currentNode.obj property

			S._____(null);
			return result;
		}
		public void IntoObject(object obj, VDFLoadOptions options = null, VDFNodePath path = null) {
			var type = obj.GetType();
			var S = options.profile ? M.GetCurrentMethod().Profile_AllFrames("VDFNode-IntoObject (type=" + type.Name + ")") : M.None_SameType; // custom [as below]

			options = options ?? new VDFLoadOptions();
			path = path ?? new VDFNodePath(new VDFNodePathNode(obj));

			S._____("part 1");
			//var type = obj.GetType();
			var typeGenericArgs = VDF.GetGenericArgumentsOfType(type);
			var typeInfo = VDFTypeInfo.Get(type);

			S._____("call pre-deserialize constructors");
			// call pre-deserialize constructors before pre-deserialize normal methods
			foreach (VDFMethodInfo method in typeInfo.methods_preDeserialize_constructor)
				method.Call(obj, this, path, options);
			S._____("call pre-deserialize methods");
			foreach (VDFMethodInfo method in typeInfo.methods_preDeserialize_method)
				method.Call(obj, this, path, options);

			S._____("call deserialize methods");
			bool deserializedByCustomMethod2 = false;
			foreach (VDFMethodInfo method in typeInfo.methods_deserialize_notFromParent) {
				object deserializeResult = method.Call(obj, this, path, options);
				if (deserializeResult != VDF.NoActionTaken)
					deserializedByCustomMethod2 = true;
			}

			if (!deserializedByCustomMethod2) {
				S._____("process list items");
				for (var i = 0; i < listChildren.Count; i++)
					if (obj is Array)
						(obj as Array).SetValue(listChildren[i].ToObject(typeGenericArgs[0], options, path.ExtendAsListItem(i, listChildren[i])), i);
					else if (obj is IList) {
						//(obj as IList).Add(listChildren[i].ToObject(typeGenericArgs[0], options, path.ExtendAsListChild((obj as IList).Count, listChildren[i])));
						var item = listChildren[i].ToObject(typeGenericArgs[0], options, path.ExtendAsListItem((obj as IList).Count, listChildren[i]));
						if ((obj as IList).Count == i) // maybe temp; allow child to have already attached itself (by way of the VDF event methods)
							(obj as IList).Add(item);
					}
				var S2 = S._____2("process map children");
				var index = -1;
				foreach (KeyValuePair<VDFNode, VDFNode> pair in mapChildren)
					try {
						S2._____("prop:" + pair.Key);
						index++;
						if (obj is IDictionary) {
							/*var key = VDF.Deserialize("\"" + keyString + "\"", typeGenericArgs[0], options);
							//((IDictionary)obj).Add(key, mapChildren[keyString].ToObject(typeGenericArgs[1], options, path.ExtendAsMapChild(key, null))); // "obj" prop to be filled in at end of ToObject method*/
							var key = pair.Key.ToObject(typeGenericArgs[0], options, path.ExtendAsMapKey(index, null));
							var value = pair.Value.ToObject(typeGenericArgs[1], options, path.ExtendAsMapItem(key, null));
							(obj as IDictionary)[key] = value; // maybe temp; allow child to have already attached itself (by way of the VDF event methods)
						}
						else { // else, must be string prop-name
							var propName = pair.Key.primitiveValue as string;
							if (typeInfo.props.ContainsKey(propName)) { // maybe temp; just ignore props that are missing
								var value = pair.Value.ToObject(typeInfo.props[propName].GetPropType(), options, path.ExtendAsChild(typeInfo.props[propName], null));
								typeInfo.props[propName].SetValue(obj, value);
							}
						}
					}
					/*catch (Exception ex) {
						var field = ex.GetType().GetField("message", BindingFlags.NonPublic | BindingFlags.Instance) ?? ex.GetType().GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance);
						field.SetValue(ex, ex.Message + "\n==================\nRethrownAs) " + ("Error loading map-child with key '" + keyString + "'.") + "\n");
						throw;
					}*/
					catch (Exception ex) { throw new VDFException("Error loading map-child with key " + (pair.Key.primitiveValue is string ? "'" + pair.Key.primitiveValue + "'" : "of type " + pair.Key) + ".", ex); }/**/finally{}
			}

			S._____("call post-deserialize funcs_early (from options)");
			if (options.objPostDeserializeFuncs_early.ContainsKey(obj))
				foreach (Action func in options.objPostDeserializeFuncs_early[obj])
					func();

			S._____("call post-deserialize constructors/methods");
			// call post-deserialize constructors before post-deserialize normal methods
			foreach (VDFMethodInfo method in typeInfo.methods_postDeserialize_constructor)
				method.Call(obj, this, path, options);
			foreach (VDFMethodInfo method in typeInfo.methods_postDeserialize_method)
				method.Call(obj, this, path, options);

			S._____("call post-deserialize funcs (from options)");
			if (options.objPostDeserializeFuncs.ContainsKey(obj))
				foreach (Action func in options.objPostDeserializeFuncs[obj])
					func();

			S._____(null);
		}
	}
}