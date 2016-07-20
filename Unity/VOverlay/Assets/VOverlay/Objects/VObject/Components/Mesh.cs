using System;
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

		//[P(false)] public VTextureDrawer emojiStrTextComp;
		[P(false)] public VTextureDrawer emojiCharDrawer;

		void PostManifest() {
			// add text-comp for emoji-str
			/*var text = gameObject.AddComponent<Text>();
			text.font = VO.main.script.font;
			text.text = emojiStr;
			text.AddEmojiToText();*/

			// add raw-image for emoji-char
			emojiCharDrawer = new VTextureDrawer();
			emojiCharDrawer.rect = new VRect(0, 0, 36, 36);
			emojiCharDrawer.material = VO.main.emojiAdderScript.emojiFont_material;
			emojiCharDrawer.texture = VO.main.emojiAdderScript.emojiFont_atlas.texture;
			emojiCharDrawer.imageComp_raw.uvRect = EmojiAdder.GetUVRectForEmojiChar(obj.emojiStr); // emoji-str must be only one emoji-char, atm
			emojiCharDrawer.enabled = true;
		}
		public void Unmanifest() {
			//if (emojiCharDrawer != null)
			emojiCharDrawer.Destroy();
		}

		public void PreViewFrameTick() {
			VVector2 posOnScreen = Camera.main.WorldToScreenPoint(obj.transform.Position);
			//emojiCharDrawer.rect = new VRect(posOnScreen.x - 1, posOnScreen.y - 1, 2, 2);
			emojiCharDrawer.rect = emojiCharDrawer.rect.NewPosition(posOnScreen);
		}
		public void PostDataFrameTick() {}
	}
}