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
using VTree.BiomeDefenseN.ObjectsN.ObjectN;
using VTree.BiomeDefenseN.ObjectsN.ObjectN.ComponentsN;
using VTree_Structures;

namespace VTree.VOverlayN.ObjectsN.ObjectN.ComponentsN {
	public class Emoji : VComponent {
		Emoji typeComp;
		[VDFPreDeserialize] public Emoji() {}
		
		//[P(false)] public VTextureDrawer emojiStrTextComp;
		[P(false)] public VTextureDrawer emojiCharDrawer;
		void Manifest() {
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

			// add collider
			// ==========

			// probably make-so: this is in its own component
			if (obj.map != null && obj.map.match != null) {
				var collider = obj.gameObject.AddComponent<CircleCollider2D>();
				//collider.offset = new Vector2(.25f, .25f);
				collider.radius = VO.unitSize / 2;
				obj.colliders.Add(collider);
			}

			// add rigidbody
			// ==========

			if (obj.map != null && obj.map.match != null) {
				var rigidbody = obj.gameObject.AddComponent<Rigidbody2D>();
				rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
				rigidbody.mass = (float)Quick.GetDouble("mass", 1);
				rigidbody.gravityScale = (float)Quick.GetDouble("gravityScale", 10);
				rigidbody.drag = (float)Quick.GetDouble("drag", 3);
				rigidbody.angularDrag = (float)Quick.GetDouble("angularDrag", 1);
			}
		}
		void Unmanifest() {
			//if (emojiCharDrawer != null)
			emojiCharDrawer.Destroy();
		}

		public void PreViewFrameTick() {
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
}