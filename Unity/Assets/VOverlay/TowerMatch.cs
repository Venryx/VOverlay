using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEditor;
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

namespace VTree.VOverlayN.TowerN {
	public class TowerMatch : Match {
		TowerMatch s;

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

		// main view-frame-tick handler
		void PreViewFrameTick_EM1() {
			foreach (VObject obj in map.GetObjects())
				//if (obj.IsConnectedToMainTree())
				if (obj.Parent != null) // if object hasn't been destroyed/removed during looping
					obj.PreViewFrameTick(VO.main.viewFrame);
		}

		// general
		// ==========

		public int nextBlockNumber;

		void AddPlayer(string username) {
			// if player already exists, remove first
			var oldPlayer = map.players.FirstOrDefault(a => a.chatMember.name == username);
			if (oldPlayer != null) {
				map.a(a=>a.units).remove = map.units.First(a=>a.owner.chatMember.name == username);
				map.a(a=>a.players).remove = oldPlayer;
			}

			var member = new ChatMember(username, VColor.Blue, "");
			var player = new Player(member);
			map.a(a=>a.players).add = player;
		}

		void Break(string username, int blockNumber) {
			var block = map.structures.FirstOrDefault(a=>a.block.number == blockNumber);
			if (block != null)
				block.Remove();
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