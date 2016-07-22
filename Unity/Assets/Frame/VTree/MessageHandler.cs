using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using VTree;
using System.Linq;
using VDFN;
using VTree.VOverlayN.MapsN.MapN;
using VTree_Structures;
using WebSocketSharp.Net;
using Random = UnityEngine.Random;

public static class MessageHandler {
	public static void StartNewRace() {
		VO.main.race.StartNewRace();
	}
	public static void AddPlayer(string username, string emoji_encodedStr) {
		var match = VO.main.race.liveMatch;
		var map = match.map;
		// if player already exists, remove first
		var oldPlayer = map.players.FirstOrDefault(a=>a.chatMember.name == username);
		if (oldPlayer != null) {
			map.a(a=>a.units).remove = map.units.First(a=>a.owner.chatMember.name == username);
			map.a(a=>a.players).remove = oldPlayer;
		}

		//var emoji = HttpUtility.UrlDecode(emoji_encodedStr.SubstringSE(1, emoji_encodedStr.Length - 1));
		var emoji = Uri.UnescapeDataString(emoji_encodedStr.SubstringSE(1, emoji_encodedStr.Length - 1));
		if (!EmojiAdder.GetUVRectForEmojiChar(emoji).HasValue)
			return;

		var member = new ChatMember(username, VColor.Blue, emoji);
		var player = new Player(member);
		map.a(a=>a.players).add = player;

		var jumperUnitType = VO.main.objects.objects.First(a=>a.name == "Jumper");
		var unit = jumperUnitType.Clone();
		unit.map = map;
		unit.owner = player;
		unit.transform.Position = new VVector3(Random.Range(0, (float)VO.GetSize_WorldMeter().x), VO.unitSize / 2, 0); // only x and z are used for 2d
		//unit.emojiStr = player.chatMember.emojiStr;
		unit.emojiStr = player.chatMember.emojiStr;
		map.a(a=>a.units).add = unit;
	}
	public static void PlayerJump(string username, double x, double z, double strength) {
		var targetPos_normalized = new VVector3(x, 0, z) / 100;
		var targetPos = targetPos_normalized * VO.GetSize_WorldMeter();

		var match = VO.main.race.liveMatch;
		var unit = match.map.units.FirstOrDefault(a=>a.owner.chatMember.name == username);
		if (unit == null)
			return;
		var rigidbody = unit.gameObject.GetComponent<Rigidbody2D>();
		var posDif = targetPos - unit.transform.Position;

		//var forceVector = posDif.ToVector2().normalized * 10;
		var forceVector = posDif.ToVector2().normalized * (float)strength;
		//Debug.Log("Adding force. TargetPos: " + targetPos + " === PosDif: " + posDif + " === ForceVector: " + forceVector);
		rigidbody.AddForce(forceVector, ForceMode2D.Force);
	}
}