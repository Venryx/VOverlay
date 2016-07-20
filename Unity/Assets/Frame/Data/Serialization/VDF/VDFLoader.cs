using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VDFN
{
	public class VDFLoadOptions
	{
		public VDFLoadOptions(List<object> message = null, bool allowStringKeys = true, bool allowCommaSeparators = false, Dictionary<string, string> namespaceAliasesByName = null, Dictionary<Type, string> typeAliasesByType = null)
		{
			this.messages = message ?? new List<object>();
			this.allowStringKeys = allowStringKeys;
			this.allowCommaSeparators = allowCommaSeparators;
			this.namespaceAliasesByName = namespaceAliasesByName ?? new Dictionary<string, string>();
			this.typeAliasesByType = typeAliasesByType ?? new Dictionary<Type, string>();
		}

		public List<object> messages;
		public Dictionary<object, List<Action>> objPostDeserializeFuncs_early = new Dictionary<object, List<Action>>();
		public Dictionary<object, List<Action>> objPostDeserializeFuncs = new Dictionary<object, List<Action>>();
		public void AddObjPostDeserializeFunc(object obj, Action func, bool early = false)
		{
			if (early)
			{
				if (!objPostDeserializeFuncs_early.ContainsKey(obj))
					objPostDeserializeFuncs_early.Add(obj, new List<Action>());
				objPostDeserializeFuncs_early[obj].Add(func);
			}
			else
			{
				if (!objPostDeserializeFuncs.ContainsKey(obj))
					objPostDeserializeFuncs.Add(obj, new List<Action>());
				objPostDeserializeFuncs[obj].Add(func);
			}
		}

		// for JSON compatibility
		public bool allowStringKeys;
		public bool allowCommaSeparators;

		// CS only
		public Dictionary<string, string> namespaceAliasesByName;
		public Dictionary<Type, string> typeAliasesByType;
		//public List<string> extraSearchAssemblyNames; // maybe add this option later

		// custom
		public bool profile;

		public VDFLoadOptions ForJSON() // helper function for JSON compatibility
		{
			allowStringKeys = true;
			allowCommaSeparators = true;
			return this;
		}
	}

	public static class VDFLoader
	{
		public static VDFNode ToVDFNode<T>(string text, VDFLoadOptions options = null) { return ToVDFNode(text, typeof(T), options); }
		public static VDFNode ToVDFNode(string text, VDFLoadOptions options) { return ToVDFNode(text, null, options); }
		public static VDFNode ToVDFNode(string text, Type declaredType = null, VDFLoadOptions options = null) { return ToVDFNode(VDFTokenParser.ParseTokens(text, options), declaredType, options); }
		public static VDFNode ToVDFNode(List<VDFToken> tokens, Type declaredType = null, VDFLoadOptions options = null, int firstTokenIndex = 0, int enderTokenIndex = -1)
		{
			options = options ?? new VDFLoadOptions();
			enderTokenIndex = enderTokenIndex != -1 ? enderTokenIndex : tokens.Count;

			// figure out obj-type
			// ==========

			var depth = 0;
			var tokensAtDepth0 = new List<VDFToken>();
			var tokensAtDepth1 = new List<VDFToken>();
			//foreach (VDFToken token in tokens)
			for (var i = firstTokenIndex; i < enderTokenIndex; i++)
			{
				var token = tokens[i];
				if (token.type == VDFTokenType.ListEndMarker || token.type == VDFTokenType.MapEndMarker)
					depth--;
				if (depth == 0)
					tokensAtDepth0.Add(token);
				if (depth == 1)
					tokensAtDepth1.Add(token);
				if (token.type == VDFTokenType.ListStartMarker || token.type == VDFTokenType.MapStartMarker)
					depth++;
			}

			var fromVDFTypeName = "object";
			var firstNonMetadataToken = tokensAtDepth0.First(a=>a.type != VDFTokenType.Metadata);
			if (tokensAtDepth0[0].type == VDFTokenType.Metadata)
				fromVDFTypeName = tokensAtDepth0[0].text;
			else if (firstNonMetadataToken.type == VDFTokenType.Boolean)
				fromVDFTypeName = "bool";
			else if (firstNonMetadataToken.type == VDFTokenType.Number)
				//fromVDFTypeName = firstNonMetadataToken.text.Contains(".") ? "double" : "int";
				fromVDFTypeName = firstNonMetadataToken.text == "Infinity" || firstNonMetadataToken.text == "-Infinity" || firstNonMetadataToken.text.Contains(".") || firstNonMetadataToken.text.Contains("e") ? "double" : "int";
			else if (firstNonMetadataToken.type == VDFTokenType.String)
				fromVDFTypeName = "string";
			else if (firstNonMetadataToken.type == VDFTokenType.ListStartMarker)
				fromVDFTypeName = "List(object)";
			else if (firstNonMetadataToken.type == VDFTokenType.MapStartMarker)
				fromVDFTypeName = "Dictionary(object object)"; //"object";

			Type type = declaredType;
			if (fromVDFTypeName != null && fromVDFTypeName.Length > 0)
			{
				var fromVDFType = VDF.GetTypeByName(fromVDFTypeName, options);
				if (type == null || fromVDFType.IsDerivedFrom(type)) // if there is no declared type, or the from-vdf type is more specific than the declared type
					type = fromVDFType;
			}
			// for keys, force load as string, since we're not at the use-importer stage
			if (firstNonMetadataToken.type == VDFTokenType.Key)
				type = typeof(string);
			var typeGenericArgs = VDF.GetGenericArgumentsOfType(type);
			var typeInfo = VDFTypeInfo.Get(type);

			// create the object's VDFNode, and load in the data
			// ==========

			var node = new VDFNode();
			node.metadata = tokensAtDepth0[0].type == VDFTokenType.Metadata ? fromVDFTypeName : null;
		
			// if primitive, parse value
			if (firstNonMetadataToken.type == VDFTokenType.Null)
				node.primitiveValue = null;
			else if (type == typeof(bool) || type == typeof(bool?))
				node.primitiveValue = bool.Parse(firstNonMetadataToken.text);
			else if (type == typeof(int) || type == typeof(int?))
			{
				//node.primitiveValue = int.Parse(firstNonMetadataToken.text);
				// maybe make-so: changes in other places are done as well, to add good support for long's
				var number = long.Parse(firstNonMetadataToken.text);
				if ((int)number == number)
					node.primitiveValue = (int)number;
				else
					node.primitiveValue = number;
			}
			else if (type == typeof(float) || type == typeof(float?)) // (only occurs if declared-type is float)
				if (firstNonMetadataToken.text == "Infinity")
					node.primitiveValue = float.PositiveInfinity;
				else if (firstNonMetadataToken.text == "-Infinity")
					node.primitiveValue = float.NegativeInfinity;
				else
					node.primitiveValue = float.Parse(firstNonMetadataToken.text);
			else if (type == typeof(double) || type == typeof(double?) || firstNonMetadataToken.type == VDFTokenType.Number)
				if (firstNonMetadataToken.text == "Infinity")
					node.primitiveValue = double.PositiveInfinity;
				else if (firstNonMetadataToken.text == "-Infinity")
					node.primitiveValue = double.NegativeInfinity;
				else //if (firstNonMetadataToken.text.Contains(".") || firstNonMetadataToken.text.Contains("e"))
					node.primitiveValue = double.Parse(firstNonMetadataToken.text);
			//else if (type == typeof(string))
			// have in-vdf string type override declared type, since we're not at the use-importer stage
			else if (type == typeof(string) || firstNonMetadataToken.type == VDFTokenType.String)
				node.primitiveValue = firstNonMetadataToken.text;

			// if list, parse items
			//else if (firstNonMetadataToken.type == VDFTokenType.ListStartMarker)
			else if (type.IsDerivedFrom(typeof(IList)))
			{
				node.isList = true;
				for (var i = 0; i < tokensAtDepth1.Count; i++)
				{
					var token = tokensAtDepth1[i];
					if (token.type != VDFTokenType.ListEndMarker && token.type != VDFTokenType.MapEndMarker)
					{
						var itemFirstToken = tokens[token.index];
						var itemEnderToken = tokensAtDepth1.FirstOrDefault(a=>a.index > itemFirstToken.index + (itemFirstToken.type == VDFTokenType.Metadata ? 1 : 0) && token.type != VDFTokenType.ListEndMarker && token.type != VDFTokenType.MapEndMarker);
						//node.listChildren.Add(ToVDFNode(GetTokenRange_Tokens(tokens, itemFirstToken, itemEnderToken), typeGenericArgs[0], options));
						node.listChildren.Add(ToVDFNode(tokens, typeGenericArgs[0], options, itemFirstToken.index, itemEnderToken != null ? itemEnderToken.index : enderTokenIndex));
						if (itemFirstToken.type == VDFTokenType.Metadata) // if item had metadata, skip an extra token (since it had two non-end tokens)
							i++;
					}
				}
			}

			// if not primitive and not list (i.e. map/object/dictionary), parse pairs/properties
			//else //if (firstNonMetadataToken.type == VDFTokenType.MapStartMarker)
			else //if (!objType.IsDerivedFrom(typeof(IList)))
			{
				node.isMap = true;
				for (var i = 0; i < tokensAtDepth1.Count; i++)
				{
					var token = tokensAtDepth1[i];
					if (token.type == VDFTokenType.Key)
					{
						var propNameFirstToken = i >= 1 && tokensAtDepth1[i - 1].type == VDFTokenType.Metadata ? tokensAtDepth1[i - 1] : tokensAtDepth1[i];
						var propNameEnderToken = tokensAtDepth1[i + 1];
						var propNameType = propNameFirstToken.type == VDFTokenType.Metadata ? typeof(object) : typeof(string);
						if (type.IsDerivedFrom(typeof(IDictionary)) && typeGenericArgs[0] != typeof(object))
							propNameType = typeGenericArgs[0];
						var propNameNode = ToVDFNode(tokens, propNameType, options, propNameFirstToken.index, propNameEnderToken.index);

						Type propValueType;
						if (type.IsDerivedFrom(typeof(IDictionary)))
							propValueType = typeGenericArgs[1];
						else
							propValueType = propNameNode.primitiveValue is string && typeInfo.props.ContainsKey(propNameNode.primitiveValue as string) ? typeInfo.props[propNameNode.primitiveValue as string].GetPropType() : null;
						var propValueFirstToken = tokensAtDepth1[i + 1];
						var propValueEnderToken = tokensAtDepth1.FirstOrDefault(a=>a.index > propValueFirstToken.index && a.type == VDFTokenType.Key);
						//var propValueNode = ToVDFNode(GetTokenRange_Tokens(tokens, propValueFirstToken, propValueEnderToken), propValueType, options);
						var propValueNode = ToVDFNode(tokens, propValueType, options, propValueFirstToken.index, propValueEnderToken != null ? propValueEnderToken.index : enderTokenIndex);

						node.mapChildren.Add(propNameNode, propValueNode);
					}
				}
			}

			return node;
		}
		/*static List<VDFToken> GetTokenRange_Tokens(List<VDFToken> tokens, VDFToken firstToken, VDFToken enderToken)
		{
			//return tokens.GetRange(firstToken.index, (enderToken != null ? enderToken.index : tokens.Count) - firstToken.index).Select(a=>new VDFToken(a.type, a.position - firstToken.position, a.index - firstToken.index, a.text)).ToList();

			var result = new List<VDFToken>(); //(enderToken != null ? enderToken.index : tokens.Count) - firstToken.index);
			for (var i = firstToken.index; i < (enderToken != null ? enderToken.index : tokens.Count); i++)
				result.Add(new VDFToken(tokens[i].type, tokens[i].position - firstToken.position, tokens[i].index - firstToken.index, tokens[i].text));
			return result;
		}*/
	}
}