using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Assertions.Must;
using VDFN;
using VectorStructExtensions;
using VTree_Structures;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace VTree.BiomeDefenseN.ObjectsN {
	[VDFType(popOutL1: true)] public class VObject_Type : VObject {
		public static new VObject_Type baseState = new VObject_Type(null);

		// VDF
		// ==========

		VObject_Type s;
		[VDFPreDeserialize] public VObject_Type() {
			/*lods = new List<VLOD> {
				new VLOD {vertexKeepPercent = 1},
				new VLOD {vertexKeepPercent = .66},
				new VLOD {vertexKeepPercent = .33}
			};*/
		}
		public VObject_Type(string name) { this.name = name; }

		[VDFPreSerializeProp] protected override VDFNode PreSerializeProp(VDFNodePath path, VDFSaveOptions options) {
			var prop = path.currentNode.prop;
			if (ShouldIgnoreProp(prop, options as VDFSaveOptionsV)) // run quick checks first
				return VDF.CancelSerialize;
			//var propName = prop.memberInfo.Name;

			// if prop-value is same as base-state prop-value, no point serializing
			if (baseState != null)
				if (V.Equals(prop.GetValue(this), prop.GetValue(baseState), trueIfSameItems: true))
					return VDF.CancelSerialize;

			return null;
		}

		// loading/saving
		// ==========

		public static VObject_Type Load(DirectoryInfo folder) {
			var result = LoadMainData(folder);
			return result;
		}
		public void Save() {
			SaveMainData();
		}

		[P(false)] public DirectoryInfo folder;
		public static VObject_Type LoadMainData(DirectoryInfo folder) {
			var S = M.GetCurrentMethod().Profile_AllFrames();

			S._____("VDF loading");
			var result = VConvert.FromVDF<VObject_Type>(File.ReadAllText(folder.GetFile("Main.vdf").FullName));
			result.name = folder.Name;
			result.folder = folder;
			S.EndLastSection();

			if (folder.Parent.Name == "Plants")
				result.objType = ObjectType.Plant;
			else if (folder.Parent.Name == "Structures")
				result.objType = ObjectType.Structure;
			else if (folder.Parent.Name == "Units")
				result.objType = ObjectType.Unit;
			else if (folder.Parent.Name == "Projectiles")
				result.objType = ObjectType.Projectile;

			result.LoadModel();

			S._____(null);
			return result;
		}
		void LoadModel() {
			gameObject = new GameObject(name);
			gameObject.transform.SetParent(VO.main.objects.obj.GetChild("TypeObjects").transform, false);

			transform.obj = s; // early set
			transform.PostObjGameObjectSet();

			mesh.obj = s; // early set
		}
		public void SaveMainData() {
			var vdf = VConvert.ToVDF(this, options: new VDFSaveOptionsV(toFile: true, toObjType: true));
			File.WriteAllText(folder.VCreate().GetFile("Main.vdf").FullName, vdf);
		}

		// general
		// ==========

		// probably make-so: this is cleared, when the type's props are changed
		//string _vdfForCreatingInstance;
		VDFNode _vdfNodeForCreatingInstance;
		VDFSaveOptions Clone_options = new VDFSaveOptionsV(toFile: true, toObjType: true);
		public VObject Clone(bool quickInit = true) {
			var S = M.GetCurrentMethod().Profile_AllFrames();
			//var result = VConvert.FromVDF<VObject>(VConvert.ToVDF(this, options: new VDFSaveOptions(new List<object> {"clone"})));
			
			// if vdf hasn't been generated, refresh clone-vdf
			/*if (_vdfForCreatingInstance == null) {
				S._____("to vdf");
				_vdfForCreatingInstance = VConvert.ToVDF(this, options: new VDFSaveOptions(new List<object> {"VObject>cloneToCreateInstance"}));
			}
			S._____("from vdf");
			var result = VConvert.FromVDF<VObject>(_vdfForCreatingInstance, new VDFLoadOptions {profile = true});*/
			if (_vdfNodeForCreatingInstance == null) {
				S._____("to vdf node");
				//_vdfNodeForCreatingInstance = VConvert.ToVDF(this, options: new VDFSaveOptions(new List<object> {"VObject>cloneToCreateInstance"}));
				_vdfNodeForCreatingInstance = VConvert.ToVDFNode(this, options: Clone_options);
			}
			S._____("from vdf node");
			var result = VConvert.FromVDFNode<VObject>(_vdfNodeForCreatingInstance, new VDFLoadOptions {profile = true});

			S._____("part 1");
			result.type = this;
			if (quickInit)
				result.NotAddedToVTree_QuickInit(); // some callers of Clone() use our result VObject before it's added to VTree, so just call this ahead of time

			S._____(null);
			return result;
		}

		// general
		// ==========

		[P(false)] public new bool type; // this is just here to block-out/warn-about-usage-of the base VObject.type prop
		[NotTo("file")] public ObjectType objType;
		[NotTo("file")] public string name;
		[IgnoreStartData] void name_PostSet(string oldValue) {
			var newFolder = folder.Parent.GetSubfolder(name);
			folder.MoveTo(newFolder.FullName);
			folder = newFolder;
		}
	}
}