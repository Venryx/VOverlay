using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using VTree;
using System.Linq;
using VDFN;
using VTree_Structures;

public static class MessageHandler {
	// called from v-bot (and from local chat-input box, simulating chat-message-forwarding by v-bot)
	public static void OnMessageAdded(string message) {
		Debug.Log("Chat message:" + message);

		// message shortcuts - dev
		if (message.StartsWith("r"))
			message = "!race";

		// message shortcuts
		if (message.StartsWith("m."))
			message = "!move ." + message.Substring("m.".Length);
		else if (message.StartsWith("!move."))
			message = "!move ." + message.Substring("!move.".Length);

		if (message == "!race")
			VO.main.race.StartNewRace();
		else if (message.StartsWith("!move ")) {
			if (VO.main.race.liveMatch == null)
				return;

			var parts = message.Split('.', '#');
			var x = double.Parse("." + parts[1]);
			var z = double.Parse("." + parts[2]);
			var targetPos_normalized = new VVector3(x, 0, z);
			var targetPos = targetPos_normalized * VO.GetSize_WorldMeter();

			// temp
			var strength = message.Contains("#") ? double.Parse(parts[3]) : 10;

			var match = VO.main.race.liveMatch;
			var unit = match.map.units[0];
			var rigidbody = unit.gameObject.GetComponent<Rigidbody2D>();
			var posDif = targetPos - unit.transform.Position;

			//var forceVector = posDif.ToVector2().normalized * 10;
			var forceVector = posDif.ToVector2().normalized * (float)strength;
			Debug.Log(targetPos + "===" + posDif + "===" + forceVector);
			rigidbody.AddForce(forceVector, ForceMode2D.Force);
		}
	}
}