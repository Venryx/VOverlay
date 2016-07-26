using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VDFN;
using VTree.BiomeDefenseN.MapsN;
using VTree_Structures;
using Random = UnityEngine.Random;

namespace VTree.VOverlayN.MapsN.MapN {
	public class ChatMember {
		public ChatMember(string name, VColor color) {
			this.name = name;
			this.color = color;
		}

		public string name;
		public VColor color;
		public string emojiStr;
	}

	public class Player : Node {
		Player s;
		[VDFPreDeserialize] Player() {}

		[VDFProp(false)] public Map map;
		[ToMainTree] void _PostAdd_Early() { map = Parent as Map; }

		public Player(ChatMember chatMember) { this.chatMember = chatMember; }

		// general
		public ChatMember chatMember;

		// tower
		public double tower_score;
		public double tower_lastActionTime;
		public void Tower_IncreaseScore(double amount, Vector2 actionPos) {
			tower_score += amount;
			
			var scoreMarker = new VTextDrawer();
			scoreMarker.transform.position = actionPos;
			scoreMarker.transform.pivot = new Vector2(.5f, .5f);
			scoreMarker.transform.rotation = Quaternion.AngleAxis(Random.Range(-10, 10), Vector3.forward) * scoreMarker.transform.rotation;
			scoreMarker.transform.sizeDelta = new Vector2(100, 100);
			scoreMarker.textComp.font = VO.main.script.mainFont;
			scoreMarker.textComp.fontStyle = FontStyle.Bold;
			scoreMarker.textComp.fontSize = 32;
			scoreMarker.textComp.alignment = TextAnchor.MiddleCenter;
			scoreMarker.textComp.text = "+" + amount.RoundToMultipleOf(.01);
			scoreMarker.enabled = true;
			scoreMarker.DestroyIn(5);
		}
	}
}