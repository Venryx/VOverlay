using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using VTree;
using System.Linq;
using VDFN;
using VectorStructExtensions;
using VTree.VOverlayN.MapsN.MapN;
using VTree_Structures;
using WebSocketSharp.Net;
using Random = UnityEngine.Random;

public static class MessageHandler {
	/*public static void PlayerJump(string username, double x, double z, double strength) {
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
	}*/
	public static void PlayerJump(string username, double angle, double strength) {
		strength = strength.KeepBetween(0, 100);

		var match = VO.main.race.liveMatch;
		var unit = match.map.units.FirstOrDefault(a=>a.owner.chatMember.name == username);
		if (unit == null)
			return;
		var rigidbody = unit.gameObject.GetComponent<Rigidbody2D>();
		var posDif = Quaternion.AngleAxis((float)-angle, Vector3.forward) * Vector3.up;

		var finalStrength = strength * Quick.GetDouble("jumpStrength");
		//var forceVector = posDif.ToVector2().normalized * (float)finalStrength;
		var forceVector = posDif.ToVector2().normalized * (float)finalStrength;
		//Debug.Log("Adding force. TargetPos: " + targetPos + " === PosDif: " + posDif + " === ForceVector: " + forceVector);
		rigidbody.AddForce(forceVector, ForceMode2D.Impulse);
	}
}