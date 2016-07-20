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
		public ChatMember(string name, VColor color, string emojiStr) {
			this.name = name;
			this.color = color;
			this.emojiStr = emojiStr;
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
		// ==========

		public ChatMember chatMember;
	}
}