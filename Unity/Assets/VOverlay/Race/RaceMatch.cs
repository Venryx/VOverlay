using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
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

namespace VTree.BiomeDefenseN.MatchesN {
	public class RaceMatch : Match {
		RaceMatch s;

		[IgnoreStartData] void paused_PostSet() { UpdateRigidbodyFreeze(); }
		void UpdateRigidbodyFreeze() {
			/*var constraints = paused ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
			foreach (var rigidbody in map.obj.GetComponentsInChildren<Rigidbody>())
				rigidbody.constraints = constraints;*/
			//Time.timeScale = paused ? 0 : 1;
			foreach (var rigidbody in map.obj.GetComponentsInChildren<Rigidbody>())
				if (!_Running) {
					rigidbody.SetMeta("oldVelocity", rigidbody.velocity);
					rigidbody.Sleep();
				}
				else {
					rigidbody.WakeUp();
					rigidbody.velocity = rigidbody.GetMeta<Vector3>("oldVelocity");
				}
		}
		[IgnoreStartData] void finished_PostSet() {
			if (finished)
				UpdateRigidbodyFreeze();
		}
		
		public void StartBuilding() {
			var S = M.GetCurrentMethod().Profile_AllFrames();

			obj = new GameObject("LiveMatch");
			obj.transform.parent = VO.main.race._gameObject.transform;
			var previewObjectHolder = new GameObject("PreviewObjectHolder");
			previewObjectHolder.transform.parent = obj.transform;

			BuildMap(map);
			
			S._____(null);
		}
		void BuildMap(Map map) {
			//V.ShowLoadingScreen("Building map...", captureMouseFocus: false);

			var mapObj = map.BuildGameObject();
			mapObj.transform.parent = obj.transform;
			mapObj.layer = LayerMask.NameToLayer("TransparentFX");

			map.StartBuilding();
			//map.RunXOnBuildCompleted(V.HideLoadingScreen);
		}

		// general
		// ==========

		void AddPlayer(string username, string emoji_encodedStr) {
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

			var member = VO.main.GetChatMember(username);
			member.emojiStr = emoji;
			var player = new Player(member);
			map.a(a=>a.players).add = player;

			var jumperUnitType = VO.main.objects.objects.First(a=>a.name == "Jumper");
			var unit = jumperUnitType.Clone();
			unit.map = map;
			unit.owner = player;
			unit.transform.Position = new VVector3(Random.Range(0, (float)VO.GetSize_WorldMeter().x), 0, VO.unitSize / 2); // only x and z are used for 2d
			//unit.emojiStr = player.chatMember.emojiStr;
			unit.emojiStr = player.chatMember.emojiStr;
			map.a(a=>a.units).add = unit;
		}

		// update stuff
		// ==========

		//void PreViewFrameTick()
		Action _preViewFrameTick; void _PreRemoveFromMainTree_EM1() { VO.main.RemoveExtraMethod("PreViewFrameTick", _preViewFrameTick); }
		[ToMainTree] void _PostAdd_EM1(){VO.main.AddExtraMethod("PreViewFrameTick",_preViewFrameTick=()=> {
			CallMethod("PreViewFrameTick");
		});}

		// main view-frame-tick handler
		void PreViewFrameTick_EM1() {
			foreach (VObject obj in map.GetObjects())
				//if (obj.IsConnectedToMainTree())
				if (obj.Parent != null) // if object hasn't been destroyed/removed during looping
					obj.PreViewFrameTick(VO.main.viewFrame);
		}
		
		[IgnoreStartData, NoAutoProfile] void dataFrame_PostSet() {
			var S = M.GetCurrentMethod().Profile_LastDataFrame();
			// we know this method's block is already added from the CallMethod call, so just grab it
			//var S = Profiler_LastDataFrame.CurrentBlock;

			//BroadcastMessage(ContextGroup.Local_CS, "PostDataFrameTick", dataFrame);
			/*S._____("players");
			foreach (var player in map.players)
				player.PostDataFrameTick(dataFrame);*/
			S._____("objects");
			foreach (VObject obj in map.GetObjects()) {
				Debug.Assert(obj.attachPoint != null, "Object should be attached to map at time of its data-frame-update. (i.e. not removed by external source)");
				obj.PostDataFrameTick(dataFrame);
			}

			S._____("checks");
			CheckForVictoryConditions();

			S._____(null);
		}
		void CheckForVictoryConditions() {
			// make-so: this ends the game a while after someone reaches the target
		}
	}
}