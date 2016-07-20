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
		if (message == "!race")
			VO.main.race.StartNewRace();
	}
}