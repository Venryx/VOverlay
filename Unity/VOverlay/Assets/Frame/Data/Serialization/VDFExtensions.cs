using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System;
using System.Reflection;
using VDFN;
using VTree_Structures;
using Object = UnityEngine.Object;

public class ResizeForUI : Attribute {
	public ResizeForUI(int maxWidthAndHeight) { this.maxWidthAndHeight = maxWidthAndHeight; }
	public int maxWidthAndHeight;
}

/*public class VDFSaveOptions_VInfo {
	public VDFSaveOptions_VInfo(bool forFile = false, bool forInstance_cs = false, bool forInstance_js = false, bool forJS = false, bool profile = false) {
		this.forFile = forFile;
		this.forInstance_cs = forInstance_cs;
		this.forInstance_js = forInstance_js;
		this.forJS = forJS;

		this.profile = profile;
	}
	// serialization-flags, which props can be set to not serialize to (using [No(...)] attribute)
	public bool forFile;
	public bool forInstance_cs;
	public bool forInstance_js;
	public bool forJS;
	// others
	public bool profile;
}*/
public class VDFSaveOptionsV : VDFSaveOptions {
	public VDFSaveOptionsV(List<object> messages = null, VDFTypeMarking typeMarking = VDFTypeMarking.Internal,
		bool useMetadata = true, bool useChildPopOut = true, bool useStringKeys = false, bool useNumberTrimming = true, bool useCommaSeparators = false,
		Dictionary<MemberInfo, bool> propInclusionL3 = null, Dictionary<string, string> namespaceAliasesByName = null, Dictionary<Type, string> typeAliasesByType = null
		// custom
		, bool toFile = false, bool toJS = false, bool toMap = false, bool toObjType = false, bool toObj = false
		, bool profile = false
	)
	: base(messages, typeMarking, useMetadata, useChildPopOut, useStringKeys, useNumberTrimming, useCommaSeparators, propInclusionL3, namespaceAliasesByName, typeAliasesByType)
	{
		this.toFile = toFile;
		this.toJS = toJS;
		this.toMap = toMap;
		this.toObjType = toObjType;
		this.toObj = toObj;

		this.profile = profile;
	}

	// serialization-flags, which match with the [No] attribute props
	public bool toFile;
	public bool toJS;
	public bool toMap;
	public bool toObjType;
	public bool toObj;

	// general
	public bool profile;

	public static implicit operator bool(VDFSaveOptionsV s) { return s != null; }
}

public static class VDFExtensions {
	public static void Init() { } // forces the static initializer below to run
	static VDFExtensions() { // one time registration of custom exporters/importers/tags
		// type exporter-importer pairs
		// ==========

		/*VDFTypeInfo.AddSerializeMethod<int>(a=>new VDFNode(a));
		VDFTypeInfo.AddDeserializeMethod_FromParent<int>(node=>int.Parse(node));
		VDFTypeInfo.AddSerializeMethod<int?>(a=>a.HasValue ? new VDFNode(a.Value) : null);
		VDFTypeInfo.AddDeserializeMethod_FromParent<int?>(node=>node.primitiveValue != null ? (int?)int.Parse(node) : null);*/

		VDFTypeInfo.AddSerializeMethod<Guid>(a=>a.ToString());
		VDFTypeInfo.AddDeserializeMethod_FromParent<Guid>(node=>new Guid(node));

		VDFTypeInfo.AddSerializeMethod<Vector2>(obj=>obj.x + " " + obj.y);
		VDFTypeInfo.AddDeserializeMethod_FromParent<Vector2>(node=> {
			var parts = ((string)node).Split(new[] {" "}, StringSplitOptions.None); // get 2 parts of the vector into separate strings
			return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
		});

		VDFTypeInfo.AddSerializeMethod<Vector3>(obj=>obj.x + " " + obj.y + " " + obj.z);
		VDFTypeInfo.AddDeserializeMethod_FromParent<Vector3>(node=> {
			var parts = ((string)node).Split(new[] {" "}, StringSplitOptions.None); // get 3 parts of the vector into separate strings
			return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
		});

		VDFTypeInfo.AddSerializeMethod<Vector3i>(obj=>obj.x + " " + obj.y + " " + obj.z);
		VDFTypeInfo.AddDeserializeMethod_FromParent<Vector3i>(node=> {
			var parts = ((string)node).Split(new[] {" "}, StringSplitOptions.None); // get 3 parts of the vector into separate strings
			return new Vector3i(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
		});

		VDFTypeInfo.AddSerializeMethod<Vector4>(obj=>obj.x + " " + obj.y + " " + obj.z + " " + obj.w);
		VDFTypeInfo.AddDeserializeMethod_FromParent<Vector4>(node=> {
			var parts = ((string)node).Split(new[] {" "}, StringSplitOptions.None); // get 4 parts of the vector into separate strings
			return new Vector4(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
		});

		VDFTypeInfo.AddSerializeMethod<Quaternion>(obj=>obj.x + " " + obj.y + " " + obj.z + " " + obj.w);
		VDFTypeInfo.AddDeserializeMethod_FromParent<Quaternion>(node=> {
			var parts = ((string)node).Split(new[] {" "}, StringSplitOptions.None); // get 4 parts of the vector into separate strings
			return new Quaternion(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
		});

		//VDFTypeInfo.AddSerializeMethod<Texture2D>(obj=>VConvert.TextureToPNGString(obj));
		/*VDFTypeInfo.AddSerializeMethod<Texture2D>((self, path, options)=> {
			var prop = path.GetSourceProp(); //currentNode.prop;
			if (options.messages.Any(a=>a is VPropInfo && (a as VPropInfo).GetPropType() == typeof(Texture2D)))
				prop = (options.messages.First(a=>a is VPropInfo && (a as VPropInfo).GetPropType() == typeof(Texture2D)) as VPropInfo).VDFInfo();

			var resizeTag = prop != null ? (ResizeForUI)prop.memberInfo.GetCustomAttributes(true).FirstOrDefault(a=>a is ResizeForUI) : null;
			if (resizeTag != null) {
				var scalePercentage = Math.Min(1, self.width > self.height ? (double)resizeTag.maxWidthAndHeight / self.width : (double)resizeTag.maxWidthAndHeight / self.height); // do not scale up
				if (scalePercentage != 1) {
					var resizedTexture = Object.Instantiate(self);
					TextureScaler.Resize_Bilinear(resizedTexture, (int)(self.width * scalePercentage), (int)(self.height * scalePercentage));
					//return VDFSaver.ToVDFNode<Texture2D>(resizedTexture, options); //(resizedTexture, options);
					return VConvert.TextureToPNGString(resizedTexture);
				}
			}

			return VConvert.TextureToPNGString(self);
		});
		VDFTypeInfo.AddDeserializeMethod_FromParent<Texture2D>(node=>node.primitiveValue != null ? VConvert.PNGStringToTexture(node) : null);*/

		// probably todo: get the generic version working
		/*VDFTypeInfo.AddSerializeMethod<Enum>(a=>a.ToString());
		VDFTypeInfo.AddDeserializeMethod_FromParent<Enum>((node, path)=>(Enum)Enum.Parse(path.currentNode.prop.GetPropType(), node.primitiveValue.ToString()));*/
		// note: I don't think these are needed, actually, for UI-to-CS messages; they are needed to receive trimmed from-file VDF data, though
		VDFTypeInfo.AddSerializeMethod<ContextGroup>(a=>a.ToString());
		VDFTypeInfo.AddDeserializeMethod_FromParent<ContextGroup>(node=>(ContextGroup)Enum.Parse(typeof(ContextGroup), node.primitiveValue.ToString()));
		/*VDFTypeInfo.AddSerializeMethod<VColor>(a=>a.ToString());
		VDFTypeInfo.AddDeserializeMethod_FromParent<VColor>(node=>(VColor)Enum.Parse(typeof(VColor), node.primitiveValue.ToString()));*/
		VDFTypeInfo.AddSerializeMethod<KeyCode>(a=>a.ToString());
		VDFTypeInfo.AddDeserializeMethod_FromParent<KeyCode>(node=>V.ParseEnum<KeyCode>(node.primitiveValue.ToString()));

		// for VScript system
		//VDFTypeInfo.AddSerializeMethod<Type>(a=>a.Name);
		//VDFTypeInfo.AddDeserializeMethod_FromParent<Type>(node=>Type.GetType(node.primitiveValue.ToString()));
		//VDFTypeInfo.Get(Type.GetType("System.MonoType")).AddExtraMethod((Type a)=>new VDFNode(a.Name), new VDFSerialize());
		//VDFTypeInfo.Get(Type.GetType("System.MonoType")).AddExtraMethod((VDFNode node)=>Type.GetType(node.primitiveValue.ToString()), new VDFDeserialize(true));
		/*VDFTypeInfo.Get(Type.GetType("System.MonoType")).AddExtraMethod((Type a, VDFNodePath path, VDFSaveOptions options)=> {
			var typeStr = VDF.GetNameOfType(a, VConvert.FinalizeToVDFOptions(new VDFSaveOptions()));
			//if (typeStr == "System.MonoType" && options.messages.Contains("to ui")) // if serializing a Type to the ui, serialize as string instead (JS side treats type-names as types)
			//	typeStr = "string";
			if (typeStr == "System.MonoType" && options.messages.Contains("to ui")) // if serializing a Type to the ui, serialize as string instead (JS side treats type-names as types)
				typeStr = "Type";
			return new VDFNode(typeStr);
		}, new VDFSerialize());*/
		if (V.inUnitTests) // if in unit-tests, return here cause it can't (try and fail to) access the below types
			return;
		//VDFTypeInfo.Get(Type.GetType("System.MonoType")).AddExtraMethod((Type a)=>new VDFNode(VDF.GetNameOfType(a, VConvert.FinalizeToVDFOptions(new VDFSaveOptions())), "Type"), new VDFSerialize());
		VDFTypeInfo.Get(Type.GetType("System.MonoType")).AddExtraMethod((Type a)=>new VDFNode(VConvert.GetNameOfType(a)) {metadata_override = "Type"}, new VDFSerialize());
		VDFTypeInfo.Get(Type.GetType("System.MonoType")).AddExtraMethod((VDFNode node)=>VConvert.GetTypeByName(node.primitiveValue.ToString()), new VDFDeserialize(true));
		/*VDFTypeInfo.Get(Type.GetType("System.MonoType")).AddExtraMethod((VDFNode node)=> {
			/*if (node.primitiveValue.ToString() == "Type") // maybe temp; fix for that finding typeof(Type) dynamically causes crash
				return typeof(Type);*#/
			return VDF.GetTypeByName(node.primitiveValue.ToString(), VConvert.FinalizeFromVDFOptions(new VDFLoadOptions()));
		}, new VDFDeserialize(true));*/
		/*VDFTypeInfo.Get(Type.GetType("System.RuntimeType")).AddExtraMethod((Type a)=>new VDFNode(a.Name), new VDFSerialize());
		VDFTypeInfo.Get(Type.GetType("System.RuntimeType")).AddExtraMethod((VDFNode node)=>Type.GetType(node.primitiveValue.ToString()), new VDFDeserialize(true));*/

		// for processing node with the from-js "Type" metadata
		//VDFTypeInfo.Get(typeof(Type)).AddExtraMethod((VDFNode node)=>VDF.GetTypeByName(node.primitiveValue.ToString(), VConvert.FinalizeFromVDFOptions(new VDFLoadOptions())), new VDFDeserialize(true));

		// type/prop/method tags
		// ==========

		VDFPropInfo.Get(typeof(GameObject).GetProperty("transform")).propTag = new VDFProp();
		VDFPropInfo.Get(typeof(Transform).GetProperty("localPosition")).propTag = new VDFProp();
		VDFPropInfo.Get(typeof(Transform).GetProperty("localRotation")).propTag = new VDFProp();
		VDFPropInfo.Get(typeof(Transform).GetProperty("localScale")).propTag = new VDFProp();
	}
}