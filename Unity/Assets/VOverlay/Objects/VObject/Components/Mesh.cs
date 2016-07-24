﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking.Match;
using VDFN;
using VectorStructExtensions;
using VTree.BiomeDefenseN.MapsN;
using VTree.BiomeDefenseN.MapsN.MapN;
using VTree_Structures;

namespace VTree.BiomeDefenseN.ObjectsN.ObjectN.ComponentsN {
	public class MeshComp : VComponent {
		MeshComp typeComp;
		[VDFPreDeserialize] public MeshComp() {}

		[P(false)] GameObject gameObject;
		public void PostObjGameObjectSet() { // maybe make-so: named MidManifest
			gameObject = obj.gameObject;
		}

		[P(false)] public VTextureDrawer textureDrawer; // for e.g. platforms
		//[P(false)] public VTextureDrawer emojiStrTextComp;
		[P(false)] public VTextureDrawer emojiCharDrawer;
		void PostManifest() {
			if (obj.type.objType == ObjectType.Structure) {
				textureDrawer = new VTextureDrawer();
				textureDrawer.transform.pivot = new Vector2(.5f, .5f);
				textureDrawer.rect = new VRect(0, 0, obj.size.x * VO.pixelsPerMeter, obj.size.z * VO.pixelsPerMeter);
				//textureDrawer.texture = VO.main.emojiAdderScript.emojiFont_atlas.texture;
				textureDrawer.color = Color.gray;
				//textureDrawer.imageComp_raw.uvRect = EmojiAdder.GetUVRectForEmojiChar(obj.emojiStr).Value; // emoji-str must be only one emoji-char, atm
				textureDrawer.enabled = true;
			}
			else if (obj.type.objType == ObjectType.Unit) {
				// add text-comp for emoji-str
				/*var text = gameObject.AddComponent<Text>();
				text.font = VO.main.script.font;
				text.text = emojiStr;
				text.AddEmojiToText();*/

				// add raw-image for emoji-char
				emojiCharDrawer = new VTextureDrawer();
				emojiCharDrawer.transform.pivot = new Vector2(.5f, .5f);
				emojiCharDrawer.rect = new VRect(0, 0, VO.unitSize * VO.pixelsPerMeter, VO.unitSize * VO.pixelsPerMeter);
				emojiCharDrawer.material = VO.main.emojiAdderScript.emojiFont_material;
				emojiCharDrawer.texture = VO.main.emojiAdderScript.emojiFont_atlas.texture;
				emojiCharDrawer.imageComp_raw.uvRect = EmojiAdder.GetUVRectForEmojiChar(obj.emojiStr).Value; // emoji-str must be only one emoji-char, atm
				emojiCharDrawer.enabled = true;
			}
		}
		public void Unmanifest() {
			if (obj.type.objType == ObjectType.Structure)
				textureDrawer.Destroy();
			else if (obj.type.objType == ObjectType.Unit) {
				//if (emojiCharDrawer != null)
				emojiCharDrawer.Destroy();
			}
		}

		public void PreViewFrameTick() {
			if (obj.type.objType == ObjectType.Structure) {
				textureDrawer.transform.transform.position = obj.transform.Position.ToVector3();
				textureDrawer.transform.rotation = obj.transform.transform.rotation;
			}
			else if (obj.type.objType == ObjectType.Unit) {
				/*VVector2 posOnScreen = Camera.main.WorldToScreenPoint(obj.transform.Position);
				//emojiCharDrawer.rect = new VRect(posOnScreen.x - 1, posOnScreen.y - 1, 2, 2);
				emojiCharDrawer.rect = emojiCharDrawer.rect.NewPosition(posOnScreen);*/
				/*var posOnCanvas = RectTransformUtility.WorldToScreenPoint(Camera.main, obj.transform.Position.ToVVector2().ToVector2(false));
				emojiCharDrawer.rect = emojiCharDrawer.rect.NewPosition(posOnCanvas.ToVVector2(false));*/
				//emojiCharDrawer.transform.transform.position = (obj.transform.Position * VO.pixelsPerMeter).ToVector3();
				emojiCharDrawer.transform.transform.position = obj.transform.Position.ToVector3();
				emojiCharDrawer.transform.rotation = obj.transform.transform.rotation;
			}
		}
		public void PostDataFrameTick() {}
	}
}