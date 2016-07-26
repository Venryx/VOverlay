using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;
using VDFN;
using VectorStructExtensions;
using VTree.BiomeDefenseN.MapsN;
using VTree.BiomeDefenseN.ObjectsN;
using VTree.BiomeDefenseN.ObjectsN.ObjectN;
using VTree.VOverlayN.MapsN;
using VTree.VOverlayN.MapsN.MapN;
using VTree.VOverlayN.SharedN;
using VTree_Structures;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace VTree.VOverlayN.TowerN {
	public class TowerMatch : Match {
		TowerMatch s;

		[IgnoreStartData] void started_PostSet() {
			SetPhysicsActive(true);
			if (started)
				V.WaitXSecondsThenCall(5, ()=>SetPhysicsActive(false));
		}
		//[IgnoreStartData] void paused_PostSet() { SetPhysicsActive(); }
		//bool physicsActive;
		//public void SetPhysicsActive(bool? active = null) {
		public void SetPhysicsActive(bool active) {
			/*var constraints = paused ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
			foreach (var rigidbody in map.obj.GetComponentsInChildren<Rigidbody>())
				rigidbody.constraints = constraints;*/
			//Time.timeScale = paused ? 0 : 1;

			foreach (var rigidbody in map.obj.GetComponentsInChildren<Rigidbody2D>())
				if (active) {
					if (rigidbody.IsSleeping() && rigidbody.GetMeta("oldVelocity") != null) {
						rigidbody.WakeUp();
						rigidbody.velocity = rigidbody.GetMeta<Vector2>("oldVelocity");
					}
				}
				else {
					rigidbody.SetMeta("oldVelocity", rigidbody.velocity);
					rigidbody.Sleep();
				}
			//physicsActive = active;

			// show whether physics is active
			VO.main.gameObject.GetChild("@General/UI/Canvas_L2/PhysicsActive").SetActive(active);
			VO.main.gameObject.GetChild("@General/UI/Canvas_L2/PhysicsNotActive").SetActive(!active);
		}
		[IgnoreStartData] void finished_PostSet() {
			if (finished)
				SetPhysicsActive(false);
		}
		
		public void StartBuilding() {
			obj = new GameObject("LiveMatch");
			obj.transform.parent = VO.main.tower.gameObj.transform;
			BuildMap(map);
		}
		void BuildMap(Map map) {
			//V.ShowLoadingScreen("Building map...", captureMouseFocus: false);

			var mapObj = map.BuildGameObject();
			mapObj.transform.parent = obj.transform;
			mapObj.layer = LayerMask.NameToLayer("TransparentFX");

			map.StartBuilding();
			//map.RunXOnBuildCompleted(V.HideLoadingScreen);
		}

		//void PreViewFrameTick()
		Action _preViewFrameTick; void _PreRemoveFromMainTree_EM1() { VO.main.RemoveExtraMethod("PreViewFrameTick", _preViewFrameTick); }
		[ToMainTree] void _PostAdd_EM1(){VO.main.AddExtraMethod("PreViewFrameTick",_preViewFrameTick=()=> {
			CallMethod("PreViewFrameTick");
		});}

		public int fallenAndRemovedBlocks;
		VTextDrawer endClosenessText;
		VTextDrawer matchEndText;

		// main view-frame-tick handler
		void PreViewFrameTick_EM1() {
			foreach (VObject obj in map.GetObjects())
				//if (obj.IsConnectedToMainTree())
				if (obj.Parent != null) // if object hasn't been destroyed/removed during looping
					obj.PreViewFrameTick(VO.main.viewFrame);

			if (VO.main.viewFrame.IsMultipleOf(10)) {
				// update scores
				var scoresStr = "";
				foreach (var player in map.players) //.OrderByDescending(a=>a.tower_score))
					scoresStr += (scoresStr.Length > 0 ? "\n" : "") + (currentPlayer == player ? "turn >> " : "") + player.chatMember.name + ": " + player.tower_score.RoundToMultipleOf(.01);
				GameObject.Find("Scoreboard").GetComponent<Text>().text = scoresStr;

				// show closeness to match-end (and whether physics is active)
				var fallenBlocks = map.structures.Count(a=>a.block.IsTouchingGround() && a.block.number > 2) + fallenAndRemovedBlocks;
				var totalBlocks_excludingUserRemoved = map.structures.Count + fallenAndRemovedBlocks;
				var blocksFallenToConsiderCollapsed = totalBlocks_excludingUserRemoved * .25;
				var collapsePercent = fallenBlocks / blocksFallenToConsiderCollapsed;
				GameObject.Find("EndCloseness").GetComponent<Text>().text = "Tower " + (int)(collapsePercent * 100).KeepAtMost(100) + "% collapsed";

				// check for match-end
				if (fallenBlocks >= blocksFallenToConsiderCollapsed && _Running) {
					s.a(a=>a.finished).set = true;
					if (currentPlayer != null)
						currentPlayer.tower_score = 0;

					matchEndText = new VTextDrawer();
					matchEndText.transform.position = VO.GetSize_WorldMeter().ToVector2() / 2;
					matchEndText.transform.pivot = new Vector2(.5f, .5f);
					matchEndText.transform.sizeDelta = new Vector2((float)VO.GetSize_WorldPixels().x, 300);
					matchEndText.textComp.font = VO.main.script.mainFont;
					matchEndText.textComp.fontSize = 50;
					matchEndText.textComp.fontStyle = FontStyle.Bold;
					matchEndText.textComp.alignment = TextAnchor.MiddleCenter;
					matchEndText.textComp.color = Color.red;
					matchEndText.enabled = true;
					matchEndText.DestroyIn(15);

					matchEndText.textComp.text = currentPlayer != null
						? "Failure! " + currentPlayer.chatMember.name.Capitalize() + " made the tower fall.\n(game over)"
						: "Tower fell by itself!\n(game over)";
				}
			}
		}
		void _PreRemoveFromMainTree_EM2() {
			GameObject.Find("Scoreboard").GetComponent<Text>().text = "";
			GameObject.Find("EndCloseness").GetComponent<Text>().text = "";
		}

		// general
		// ==========

		public int nextBlockNumber;

		Player AddPlayer(string username) {
			// if player already exists, ignore call
			if (map.players.Any(a=>a.chatMember.name == username)) return null;
			
			var player = new Player(VO.main.GetChatMember(username));
			map.a(a=>a.players).add = player;
			// if first player, or second player (and first has already played)
			if (currentPlayer == null || (currentPlayer.tower_score > 0 && map.players.Count == 2))
				currentPlayer = player;
			return player;
		}
		void Quit(string username) {
			var player = map.players.FirstOrDefault(a=>a.chatMember.name == username);
			if (player != null)
				map.a(a=>a.players).remove = player;
		}

		[ByPath] public Player currentPlayer;
		void SetCurrentPlayer_ByName(string playerName) { SetCurrentPlayer(map.players.FirstOrDefault(a=>a.chatMember.name == playerName)); }
		void SetCurrentPlayer(Player player) {
			currentPlayer = player;
			VBotBridgeScript.main.socket.Emit("OnSetCurrentPlayer", JSONObject.CreateStringObject(player.chatMember.name));
		}
		void SkipCurrentPlayer() {
			if (currentPlayer != null)
				SetCurrentPlayer(map.players.Count > currentPlayer.attachPoint.list_index + 1 ? map.players[currentPlayer.attachPoint.list_index + 1] : map.players[0]);
		}

		void Break(string username, int blockNumber) {
			var player = map.players.FirstOrDefault(a=>a.chatMember.name == username) ?? AddPlayer(username);
			if (player != currentPlayer || Time.time - player.tower_lastActionTime < 3) return;
			var block = map.structures.FirstOrDefault(a=>a.block.number == blockNumber);
			if (block == null) return;

			var blockPos = block.gameObject.transform.position;
			block.Remove();
			var towerHeight = map.structures.Max(a=>a.gameObject.transform.position.y);
			var percentToTop = 1 - (block.gameObject.transform.position.y / towerHeight);
			player.Tower_IncreaseScore(percentToTop, blockPos);
			player.tower_lastActionTime = Time.time;

			SetPhysicsActive(true);

			V.WaitXSecondsThenCall(3, ()=> {
				SetCurrentPlayer(map.players.Count > player.attachPoint.list_index + 1 ? map.players[player.attachPoint.list_index + 1] : map.players[0]);
				SetPhysicsActive(false);
			});
		}
		void Shrink(string username, int blockNumber, double shrinkToPercent) {
			var player = map.players.FirstOrDefault(a=>a.chatMember.name == username) ?? AddPlayer(username);
			if (player != currentPlayer || Time.time - player.tower_lastActionTime < 3) return;
			var block = map.structures.FirstOrDefault(a=>a.block.number == blockNumber);
			if (block == null) return;

			var blockPos = block.gameObject.transform.position;
			block.transform.Scale *= shrinkToPercent;
			var shrinkByPercent = 1 - shrinkToPercent;
			var towerHeight = map.structures.Max(a=>a.gameObject.transform.position.y);
			var percentToTop = 1 - (block.gameObject.transform.position.y / towerHeight);
			player.Tower_IncreaseScore(percentToTop * shrinkByPercent, blockPos.ToVector2());
			player.tower_lastActionTime = Time.time;

			SetPhysicsActive(true);

			V.WaitXSecondsThenCall(3, ()=> {
				SetCurrentPlayer(map.players.Count > player.attachPoint.list_index + 1 ? map.players[player.attachPoint.list_index + 1] : map.players[0]);
				SetPhysicsActive(false);
			});
		}

		// data-frame stuff
		// ==========

		// data-frame-related view-frame-tick handler
		void PreViewFrameTick_EM2() {
			if (!_Running)
				return;

			// for now, data-frame is same as view-frame
			Profiler_LastDataFrame.PreDataFrameTick(); // first call profiler, so it can set up profiling for this upcoming frame
			s.a(a=>a.dataFrame).set = dataFrame + 1;
			Profiler_LastDataFrame.PostDataFrameTick(); // notify end of data-frame portion; rest gets put into "after-data-frame view-frame processing"
		}

		public int dataFrame;
		[IgnoreStartData, NoAutoProfile] void dataFrame_PostSet() {
			foreach (VObject obj in map.GetObjects()) {
				Debug.Assert(obj.attachPoint != null, "Object should be attached to map at time of its data-frame-update. (i.e. not removed by external source)");
				obj.PostDataFrameTick(dataFrame);
			}
			CheckForVictoryConditions();
		}
		void CheckForVictoryConditions() {
			// make-so: this ends the game if tower falls
		}
	}
}