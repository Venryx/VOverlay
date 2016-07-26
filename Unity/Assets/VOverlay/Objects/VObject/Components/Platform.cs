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
	public class Platform : VComponent {
		Platform typeComp;
		[VDFPreDeserialize] public Platform() {}

		public VVector3 size;

		[P(false)] public VTextureDrawer textureDrawer;
		void Manifest() {
			if (obj.map != null && obj.map.match != null) {
				var collider = obj.gameObject.AddComponent<BoxCollider2D>();
				collider.size = size.ToVector2();
				obj.colliders.Add(collider);
			}

			textureDrawer = new VTextureDrawer();
			textureDrawer.transform.pivot = new Vector2(.5f, .5f);
			textureDrawer.rect = new VRect(0, 0, size.x * VO.pixelsPerMeter, size.z * VO.pixelsPerMeter);
			//textureDrawer.texture = VO.main.emojiAdderScript.emojiFont_atlas.texture;
			textureDrawer.color = Color.gray;
			//textureDrawer.imageComp_raw.uvRect = EmojiAdder.GetUVRectForEmojiChar(obj.emojiStr).Value; // emoji-str must be only one emoji-char, atm
			textureDrawer.enabled = true;
		}
		void Unmanifest() { textureDrawer.Destroy(); }

		public void PreViewFrameTick() {
			textureDrawer.transform.transform.position = obj.transform.Position.ToVector3();
			textureDrawer.transform.rotation = obj.transform.transform.rotation;
		}
	}
}