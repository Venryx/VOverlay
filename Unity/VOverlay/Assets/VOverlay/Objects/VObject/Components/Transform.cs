using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking.Match;
using VDFN;
using VectorStructExtensions;
using VTree.BiomeDefenseN.MapsN.MapN;
using VTree_Structures;

namespace VTree.BiomeDefenseN.ObjectsN.ObjectN.ComponentsN {
	public class VTransform : VComponent {
		VTransform typeComp;
		[VDFPreDeserialize] public VTransform() {}

		[P(false)] GameObject gameObject;
		public void PostObjGameObjectSet() {
			//this.obj = obj; // must set obj here as well, for case where obj is not part of main VTree (i.e. where _PostAdd() above is not called)
			gameObject = obj.gameObject;
		}

		[NotTo("js")] VVector3 position = VVector3.zero; // don't reference directly // maybe make-so: this is a VVector2
		[NotTo("js")] public bool posOutflowsNeedUpdate = true; // maybe make-so: this is merged with boundsNeedsUpdate var
		//[IgnoreStartData] void position_PostSet() {
		public VVector3 Position {
			get { return position; }
			set {
				if (value == position) // if no change, return
					return;
				position = value;
				posOutflowsNeedUpdate = true;
				//if (attachPoint == null)
				if (gameObject == null)
					return;
				gameObject.transform.position = Position.ToVector3();
				PostTransformChange();
				//gameObject.transform.position = (Position - (GetBounds().size / 2).NewZ(0)).ToVector3();
			}
		}
		//public VVector2 Pos2D { get { return position.ToVVector2(); } }
		[NotTo("js")] VVector4 rotation = VVector4.identity; // don't reference directly
		//[IgnoreStartData] void rotation_PostSet() {
		public VVector4 Rotation {
			get { return rotation; }
			set {
				if (value == Rotation) // if no change, return
					return;
				rotation = value;
				if (gameObject == null)
					return;
				if (gameObject.transform.GetMeta("rotation_original") == null)
					gameObject.transform.SetMeta("rotation_original", gameObject.transform.localRotation);
				gameObject.transform.localRotation = gameObject.transform.GetMeta<Quaternion>("rotation_original") * Rotation.ToQuaternion();
				PostTransformChange();
			}
		}
		[NotTo("js")] VVector3 scale = VVector3.one; // don't reference directly
		//[IgnoreStartData] void scale_PostSet() {
		public VVector3 Scale {
			get { return scale; }
			set {
				if (value == Scale) // if no change, return
					return;
				scale = value;
				if (gameObject == null)
					return;
				//_gameObject.transform.localScale = Scale.ToVector3();
				//var baseScale = obj.type.gameObject.transform.localScale.ToVVector3();
				var baseScale = 1;
				gameObject.transform.localScale = (baseScale * Scale).ToVector3();
				PostTransformChange();
			}
		}

		// called by VObject.Manifest(), for initial values
		public void ApplyTransform() {
			gameObject.transform.position = Position.ToVector3();
			if (gameObject.transform.GetMeta("rotation_original") == null)
				gameObject.transform.SetMeta("rotation_original", gameObject.transform.localRotation);
			gameObject.transform.localRotation = gameObject.transform.GetMeta<Quaternion>("rotation_original") * Rotation.ToQuaternion();
			//var baseScale = obj.type.gameObject.transform.localScale.ToVVector3();
			var baseScale = 1;
			gameObject.transform.localScale = (baseScale * Scale).ToVector3();
		}

		public bool IsRotationDiagonal(VVector4? rotationOverride = null) {
			// if not structure, return false (only structures can be "diagonal")
			if (obj != null && obj.Type_Safe.objType != ObjectType.Structure) // (obj is null for object-placer)
				return false;
			var rotation_final = rotationOverride ?? Rotation;
			var rotation_aroundUpAxis = (int)Math.Round(rotation_final.GetRotation_AroundAxis(VVector3.up));
			return rotation_aroundUpAxis.RoundToMultipleOf(45) % 90 != 0;
		}

		void PostTransformChange() {}
		public void PostDataFrameTick() {}
	}
}