﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace VDFN {
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)] public class VDFType : Attribute {
		public string propIncludeRegexL1;
		public bool popOutL1;
		public VDFType(string propIncludeRegexL1 = null, bool popOutL1 = false) {
			this.propIncludeRegexL1 = propIncludeRegexL1;
			this.popOutL1 = popOutL1;
		}
	}
	public class VDFTypeInfo {
		static Dictionary<Type, VDFTypeInfo> cachedTypeInfo = new Dictionary<Type, VDFTypeInfo>();
		public static VDFTypeInfo Get(Type type) {
			if (!cachedTypeInfo.ContainsKey(type)) {
				var result = new VDFTypeInfo();
				foreach (MemberInfo member in type.GetMembers_Full(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
					if (member is FieldInfo) {
						var field = member as FieldInfo;
						if (!field.Name.StartsWith("<")) // anonymous types will have some extra field names starting with '<'
							result.props[field.Name] = VDFPropInfo.Get(field);
					}
					else if (member is PropertyInfo) {
						var property = member as PropertyInfo;
						result.props[property.Name] = VDFPropInfo.Get(property);
					}
				foreach (MethodBase method in type.GetMembers_Full(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).OfType<MethodBase>()) { // include constructors
					var methodName = method.Name;
					if (result.methods.ContainsKey(methodName))
						methodName += "(from base type: " + method.DeclaringType.Name + ")";
					if (!result.methods.ContainsKey(methodName))
						result.AddMethod(methodName, VDFMethodInfo.Get(method));
				}

				result.typeTag = type.GetCustomAttributes(true).OfType<VDFType>().FirstOrDefault() ?? new VDFType();
				if (VDF.GetIsTypeAnonymous(type))
					result.typeTag.propIncludeRegexL1 = VDF.PropRegex_Any;

				var currentType = type.BaseType;
				while (currentType != null && currentType.GetCustomAttributes(typeof(VDFType), true).Length > 0) {
					var typeTag2 = currentType.GetCustomAttributes(typeof(VDFType), true).OfType<VDFType>().First();
					if (result.typeTag.propIncludeRegexL1 == null)
						result.typeTag.propIncludeRegexL1 = typeTag2.propIncludeRegexL1;
					currentType = currentType.BaseType;
				}

				cachedTypeInfo[type] = result;
			}
			return cachedTypeInfo[type];
		}

		public Dictionary<string, VDFPropInfo> props = new Dictionary<string, VDFPropInfo>();
		public Dictionary<string, VDFMethodInfo> methods = new Dictionary<string, VDFMethodInfo>();
		// deserializing
		public List<VDFMethodInfo> methods_preDeserialize_constructor = new List<VDFMethodInfo>();
		public List<VDFMethodInfo> methods_preDeserialize_method = new List<VDFMethodInfo>();
		public List<VDFMethodInfo> methods_deserialize_fromParent = new List<VDFMethodInfo>();
		public List<VDFMethodInfo> methods_deserialize_notFromParent = new List<VDFMethodInfo>();
		public List<VDFMethodInfo> methods_postDeserialize_constructor = new List<VDFMethodInfo>();
		public List<VDFMethodInfo> methods_postDeserialize_method = new List<VDFMethodInfo>();
		// serializing
		public List<VDFMethodInfo> methods_preSerialize = new List<VDFMethodInfo>();
		public List<VDFMethodInfo> methods_serialize = new List<VDFMethodInfo>();
		public List<VDFMethodInfo> methods_preSerializeProp = new List<VDFMethodInfo>();
		public List<VDFMethodInfo> methods_postSerialize = new List<VDFMethodInfo>();
		void AddMethod(string methodName, VDFMethodInfo methodInfo) {
			methods.Add(methodName, methodInfo);
			// deserializing
			if (methodInfo.preDeserializeTag != null && methodInfo.memberInfo is ConstructorInfo)
				methods_preDeserialize_constructor.Add(methodInfo);
			if (methodInfo.preDeserializeTag != null && methodInfo.memberInfo is MethodInfo)
				methods_preDeserialize_method.Add(methodInfo);
			if (methodInfo.deserializeTag != null && methodInfo.deserializeTag.fromParent)
				methods_deserialize_fromParent.Add(methodInfo);
			if (methodInfo.deserializeTag != null && !methodInfo.deserializeTag.fromParent)
				methods_deserialize_notFromParent.Add(methodInfo);
			if (methodInfo.postDeserializeTag != null && methodInfo.memberInfo is ConstructorInfo)
				methods_postDeserialize_constructor.Add(methodInfo);
			if (methodInfo.postDeserializeTag != null && methodInfo.memberInfo is MethodInfo)
				methods_postDeserialize_method.Add(methodInfo);
			// serializing
			if (methodInfo.preSerializeTag != null)
				methods_preSerialize.Add(methodInfo);
			if (methodInfo.serializeTag != null)
				methods_serialize.Add(methodInfo);
			if (methodInfo.preSerializePropTag != null)
				methods_preSerializeProp.Add(methodInfo);
			if (methodInfo.postSerializeTag != null)
				methods_postSerialize.Add(methodInfo);
		}
		public VDFType typeTag;

		//public delegate void Action<A1, A2, A3, A4, A5>(A1 a1, A2 a2, A3 a3, A4 a4, A5 a5);
		//public delegate R Func<A1, A2, A3, A4, A5, R>(A1 a1, A2 a2, A3 a3, A4 a4, A5 a5);

		public void AddExtraMethod_Base(Delegate method, List<Attribute> tags) {
			var methodInfo = new VDFMethodInfo();
			methodInfo.method = method;
			methodInfo.memberInfo = method.Method;
			methodInfo.preSerializeTag = tags.OfType<VDFPreSerialize>().FirstOrDefault();
			methodInfo.serializeTag = tags.OfType<VDFSerialize>().FirstOrDefault();
			methodInfo.postSerializeTag = tags.OfType<VDFPostSerialize>().FirstOrDefault();
			methodInfo.preDeserializeTag = tags.OfType<VDFPreDeserialize>().FirstOrDefault();
			methodInfo.deserializeTag = tags.OfType<VDFDeserialize>().FirstOrDefault();
			methodInfo.postDeserializeTag = tags.OfType<VDFPostDeserialize>().FirstOrDefault();
			AddMethod(method.Method.Name, methodInfo);
		}
		public void AddExtraMethod(Action method, params Attribute[] tags) { AddExtraMethod_Base(method, tags.ToList()); }
		public void AddExtraMethod<A>(Action<A> method, params Attribute[] tags) { AddExtraMethod_Base(method, tags.ToList()); }
		public void AddExtraMethod<A1, A2>(Action<A1, A2> method, params Attribute[] tags) { AddExtraMethod_Base(method, tags.ToList()); }
		public void AddExtraMethod<A1, A2, A3>(Action<A1, A2, A3> method, params Attribute[] tags) { AddExtraMethod_Base(method, tags.ToList()); }
		public void AddExtraMethod<A1, A2, A3, A4>(Action<A1, A2, A3, A4> method, params Attribute[] tags) { AddExtraMethod_Base(method, tags.ToList()); }
		//public void AddExtraMethod<A1, A2, A3, A4, A5>(Action<A1, A2, A3, A4, A5> method, params Attribute[] tags) { AddExtraMethod_Base(method, tags.ToList()); }
		public void AddExtraMethod<R>(Func<R> method, params Attribute[] tags) { AddExtraMethod_Base(method, tags.ToList()); }
		public void AddExtraMethod<A, R>(Func<A, R> method, params Attribute[] tags) { AddExtraMethod_Base(method, tags.ToList()); }
		public void AddExtraMethod<A1, A2, R>(Func<A1, A2, R> method, params Attribute[] tags) { AddExtraMethod_Base(method, tags.ToList()); }
		public void AddExtraMethod<A1, A2, A3, R>(Func<A1, A2, A3, R> method, params Attribute[] tags) { AddExtraMethod_Base(method, tags.ToList()); }
		public void AddExtraMethod<A1, A2, A3, A4, R>(Func<A1, A2, A3, A4, R> method, params Attribute[] tags) { AddExtraMethod_Base(method, tags.ToList()); }
		//public void AddExtraMethod<A1, A2, A3, A4, A5, R>(Func<A1, A2, A3, A4, A5, R> method, params Attribute[] tags) { AddExtraMethod_Base(method, tags.ToList()); }

		public static void AddSerializeMethod<T>(Func<T, VDFNode> method, params Attribute[] tags) { AddSerializeMethod<T>((obj, path, options)=>method(obj), tags); }
		public static void AddSerializeMethod<T>(Func<T, VDFNodePath, VDFNode> method, params Attribute[] tags) { AddSerializeMethod<T>((obj, path, options)=>method(obj, path), tags); }
		public static void AddSerializeMethod<T>(Func<T, VDFNodePath, VDFSaveOptions, VDFNode> method, params Attribute[] tags) {
			var finalTags = tags.ToList();
			if (!finalTags.Any(a=>a is VDFSerialize))
				finalTags.Add(new VDFSerialize());
			Get(typeof(T)).AddExtraMethod(method, finalTags.ToArray());
		}
		public static void AddDeserializeMethod<T>(Action<T, VDFNode> method, params Attribute[] tags) { AddDeserializeMethod<T>((obj, node, path, options)=>method(obj, node), tags); }
		public static void AddDeserializeMethod<T>(Action<T, VDFNode, VDFNodePath> method, params Attribute[] tags) { AddDeserializeMethod<T>((obj, node, path, options)=>method(obj, node, path), tags); }
		public static void AddDeserializeMethod<T>(Action<T, VDFNode, VDFNodePath, VDFLoadOptions> method, params Attribute[] tags) {
			var finalTags = tags.ToList();
			if (!finalTags.Any(a=>a is VDFDeserialize))
				finalTags.Add(new VDFDeserialize());
			Get(typeof(T)).AddExtraMethod(method, finalTags.ToArray());
		}
		public static void AddDeserializeMethod_WithReturn<T>(Func<T, VDFNode, object> method, params Attribute[] tags) { AddDeserializeMethod_WithReturn<T>((obj, node, path, options)=>method(obj, node), tags); }
		public static void AddDeserializeMethod_WithReturn<T>(Func<T, VDFNode, VDFNodePath, object> method, params Attribute[] tags) { AddDeserializeMethod_WithReturn<T>((obj, node, path, options)=>method(obj, node, path), tags); }
		public static void AddDeserializeMethod_WithReturn<T>(Func<T, VDFNode, VDFNodePath, VDFLoadOptions, object> method, params Attribute[] tags) {
			var finalTags = tags.ToList();
			if (!finalTags.Any(a=>a is VDFDeserialize))
				finalTags.Add(new VDFDeserialize());
			Get(typeof(T)).AddExtraMethod(method, finalTags.ToArray());
		}
		public static void AddDeserializeMethod_FromParent<T>(Func<VDFNode, T> method, params Attribute[] tags) { AddDeserializeMethod_FromParent((node, path, options)=>method(node), tags); }
		public static void AddDeserializeMethod_FromParent<T>(Func<VDFNode, VDFNodePath, T> method, params Attribute[] tags) { AddDeserializeMethod_FromParent((node, path, options)=>method(node, path), tags); }
		public static void AddDeserializeMethod_FromParent<T>(Func<VDFNode, VDFNodePath, VDFLoadOptions, T> method, params Attribute[] tags) {
			var finalTags = tags.ToList();
			if (!finalTags.Any(a=>a is VDFDeserialize))
				finalTags.Add(new VDFDeserialize(fromParent: true));
			Get(typeof(T)).AddExtraMethod(method, finalTags.ToArray());
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)] public class VDFProp : Attribute {
		public bool includeL2;
		public bool popOutL2;
		// maybe old: issue: attribute constructors can't have nullables, so if tag is added, its would-be-optional override-props (popOutL2) are as well
		public VDFProp(bool includeL2 = true, bool popOutL2 = false) {
			this.includeL2 = includeL2;
			this.popOutL2 = popOutL2;
		}
	}
	public class P : VDFProp { // alias for VDFProp
		//public P() {}
		public P(bool includeL2 = true, bool popOutL2 = false) : base(includeL2, popOutL2) {}
	}
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)] public class DefaultValue : Attribute {
		public object defaultValue;
		public DefaultValue() { defaultValue = D.DefaultDefault; }
		public DefaultValue(object defaultValue) { this.defaultValue = defaultValue; }
	}
	public class D : DefaultValue { // alias for DefaultValue
		//public const string NoDefault = "DefaultValue_NoDefault"; // i.e. the prop has no default, so whatever value it has is always saved [commented out, since: if you want no default, just don't add the D tag]
		public const string DefaultDefault = "DefaultValue_DefaultDefault"; // i.e. the default value for the type (not the prop) ['false' for a bool, etc.]
		public const string NullOrEmpty = "DefaultValue_NullOrEmpty"; // i.e. null, or an empty string or collection
		public const string Empty = "DefaultValue_Empty"; // i.e. an empty string or collection

		public D() {}
		public D(object defaultValue) : base(defaultValue) {}
	}
	public class VDFPropInfo {
		static Dictionary<MemberInfo, VDFPropInfo> cachedPropInfo = new Dictionary<MemberInfo, VDFPropInfo>();
		public static VDFPropInfo Get(MemberInfo prop) {
			if (!cachedPropInfo.ContainsKey(prop))
				cachedPropInfo[prop] = new VDFPropInfo {memberInfo = prop, propTag = prop.GetCustomAttributes(true).OfType<VDFProp>().FirstOrDefault(), defaultValueTag = prop.GetCustomAttributes(true).OfType<DefaultValue>().FirstOrDefault()};
			return cachedPropInfo[prop];
		}

		public MemberInfo memberInfo;
		public VDFProp propTag;
		public DefaultValue defaultValueTag;

		public Type GetPropType() { return memberInfo is PropertyInfo ? ((PropertyInfo)memberInfo).PropertyType : ((FieldInfo)memberInfo).FieldType; }
		public bool ShouldValueBeSaved(object val) {
			//if (defaultValueTag == null || defaultValueTag.defaultValue == D.NoDefault)
			if (defaultValueTag == null)
				return true;

			if (defaultValueTag.defaultValue as string == D.DefaultDefault) {
				if (val == null) // if null
					return false;
				if (GetPropType().IsValueType && val.Equals(Activator.CreateInstance(GetPropType()))) //x == Activator.CreateInstance(GetPropType())) // if struct, and equal to struct's default value
					return false;
			}
			if (defaultValueTag.defaultValue as string == D.NullOrEmpty && val == null)
				return false;
			if (defaultValueTag.defaultValue as string == D.NullOrEmpty || defaultValueTag.defaultValue as string == D.Empty) {
				if (val is string && ((string)val).Length == 0) // if string, and empty
					return false;
				if (val is IList && ((IList)val).Count == 0) // if list, and empty
					return false;
			}
			if (Equals(val, defaultValueTag.defaultValue))
				return false;

			return true;
		}
		public object GetValue(object objParent) {
			if (memberInfo is FieldInfo)
				return ((FieldInfo)memberInfo).GetValue(objParent);
			return ((PropertyInfo)memberInfo).GetValue(objParent, null);
		}
		public void SetValue(object objParent, object value) {
			if (memberInfo is FieldInfo)
				((FieldInfo)memberInfo).SetValue(objParent, value);
			else
				((PropertyInfo)memberInfo).SetValue(objParent, value, null);
		}
	}

	[AttributeUsage(AttributeTargets.Method)] public class VDFPreSerializeProp : Attribute {}
	/*[AttributeUsage(AttributeTargets.Method)] public class VDFSerializeProp : Attribute {} // maybe todo: add these, for consistency's/completeness' sake
	[AttributeUsage(AttributeTargets.Method)] public class VDFPostSerializeProp : Attribute {}
	[AttributeUsage(AttributeTargets.Method)] public class VDFPreDeserializeProp : Attribute {}
	[AttributeUsage(AttributeTargets.Method)] public class VDFDeserializeProp : Attribute {}
	[AttributeUsage(AttributeTargets.Method)] public class VDFPostDeserializeProp : Attribute {}*/

	[AttributeUsage(AttributeTargets.Method)] public class VDFPreSerialize : Attribute {}
	[AttributeUsage(AttributeTargets.Method)] public class VDFSerialize : Attribute {}
	[AttributeUsage(AttributeTargets.Method)] public class VDFPostSerialize : Attribute {}
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)] public class VDFPreDeserialize : Attribute {}
	[AttributeUsage(AttributeTargets.Method)] public class VDFDeserialize : Attribute {
		public bool fromParent;
		public VDFDeserialize(bool fromParent = false) { this.fromParent = fromParent; }
	}
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)] public class VDFPostDeserialize : Attribute {
	}
	public class VDFMethodInfo {
		static Dictionary<MemberInfo, VDFMethodInfo> cachedMethodInfo = new Dictionary<MemberInfo, VDFMethodInfo>();
		public static VDFMethodInfo Get(MethodBase method) {
			if (!cachedMethodInfo.ContainsKey(method)) {
				var result = new VDFMethodInfo();
				result.memberInfo = method;
				result.preSerializePropTag = method.GetCustomAttributes(true).OfType<VDFPreSerializeProp>().FirstOrDefault();
				result.preSerializeTag = method.GetCustomAttributes(true).OfType<VDFPreSerialize>().FirstOrDefault();
				result.serializeTag = method.GetCustomAttributes(true).OfType<VDFSerialize>().FirstOrDefault();
				result.postSerializeTag = method.GetCustomAttributes(true).OfType<VDFPostSerialize>().FirstOrDefault();
				result.preDeserializeTag = method.GetCustomAttributes(true).OfType<VDFPreDeserialize>().FirstOrDefault();
				result.deserializeTag = method.GetCustomAttributes(true).OfType<VDFDeserialize>().FirstOrDefault();
				result.postDeserializeTag = method.GetCustomAttributes(true).OfType<VDFPostDeserialize>().FirstOrDefault();
				cachedMethodInfo[method] = result;
			}
			return cachedMethodInfo[method];
		}

		public MethodBase memberInfo;
		public VDFPreSerializeProp preSerializePropTag;
		public VDFPreSerialize preSerializeTag;
		public VDFSerialize serializeTag;
		public VDFPostSerialize postSerializeTag;
		public VDFPreDeserialize preDeserializeTag;
		public VDFDeserialize deserializeTag;
		public VDFPostDeserialize postDeserializeTag;

		public Delegate method; // if a method delegate/lambda was supplied (i.e. if an extension method)

		public object Call(object objParent, params object[] args) {
			var hasSelfFirstArg = method != null && memberInfo.GetParameters().First().ParameterType != typeof(VDFNode); // if accepts a "self" argument (e.g. an added-as-extra-method standard Deserialize method);
			if (hasSelfFirstArg)
				args = new[] {objParent}.Concat(args).ToArray();
			if (args.Length > memberInfo.GetParameters().Length)
				args = args.Take(memberInfo.GetParameters().Length).ToArray();

			/*if (memberInfo.Name.Contains("<"))  // if anonymous/lambda method
				if (memberInfo.GetParameters().FirstOrDefault().ParameterType != typeof(VDFNode)) // if accepts a "self" argument (e.g. an added-as-extra-method standard Deserialize method)
					return memberInfo.Invoke(null, new[] {objParent}.Concat(args).ToArray());
				else
					return memberInfo.Invoke(FormatterServices.GetUninitializedObject(memberInfo.DeclaringType), args.ToArray());*/
			if (method != null) // if anonymous/lambda method
				return method.DynamicInvoke(args.ToArray());
			return memberInfo.Invoke(objParent, args);
		}
	}
}