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
using VTree_Structures;
using Object = UnityEngine.Object;

namespace VTree.BiomeDefenseN.MatchesN {
	public class Match : Node {
		Match s;

		public bool started;
		public bool paused;
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
		public bool finished;
		[IgnoreStartData] void finished_PostSet() {
			if (finished)
				UpdateRigidbodyFreeze();
		}
		public bool _InProgress { get { return started && !finished; } }
		public bool _Running { get { return started && !paused && !finished; } }

		//public ScriptContext scriptContext;

		[P(false)] public GameObject obj;

		public Map map;
		/*void map_PostSet() {
			var treeRect = map.terrain.GetTreeRect();
			_units_tree = new QuadTreeNode<VObject>(treeRect);
		}*/
		//public List<Player> players = new List<Player>();
		//public Player _LocalHumanPlayer { get { return players.First(a=>a.ai == null); } }

		[P(false)] public bool coreMapInitDone;
		[NoAutoProfile] public void NotifyPostCoreMapInit() { coreMapInitDone = true; }

		public bool fogOfWar;

		public List<VObject> GetObjects(bool plants = true, bool structures = true, bool units = true, bool others = true) {
			var result = new List<VObject>();
			if (plants)
				result.AddRange(map.plants);
			if (structures)
				result.AddRange(map.structures);
			if (units)
				result.AddRange(map.units);
			if (others)
				result.AddRange(map.projectiles);
			return result;
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
			var S = M.GetCurrentMethod().Profile_AllFrames();

			//V.ShowLoadingScreen("Building map...", captureMouseFocus: false);

			var mapObj = map.BuildGameObject();
			mapObj.transform.parent = obj.transform;
			mapObj.layer = LayerMask.NameToLayer("TransparentFX");

			//Chunk.CalculateSoilOpacitiesCache.Start(map);
			map.StartBuilding();
			map.RunXOnBuildCompleted(()=> {
				//Chunk.CalculateSoilOpacitiesCache.Stop();
				//V.HideLoadingScreen();
			});

			S._____(null);
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

		// data-frame stuff
		// ==========

		public int dataFrame;
		public int dataFramesPerSecond = 30;
		public double SecondsPerDataFrame { get { return 1d / dataFramesPerSecond; } }
		public class DataFrameFor_Class {
			public DataFrameFor_Class(Match match) { this.match = match; }
			public Match match;
			public int XSecondsFromNow(double seconds) { return XSecondsFromFrameY(seconds, match.dataFrame); }
			public int XSecondsFromFrameY(double seconds, int frameY) { return frameY + (int)(seconds * match.dataFramesPerSecond); }
		}
		public DataFrameFor_Class DataFrameFor { get { return new DataFrameFor_Class(s); } }
		
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
		
		// data-frame-related view-frame-tick handler
		void PreViewFrameTick_EM2() {
			if (!_Running)
				return;

			// for now, data-frame is same as view-frame
			Profiler_LastDataFrame.PreDataFrameTick(); // first call profiler, so it can set up profiling for this upcoming frame
			s.a(a=>a.dataFrame).set = dataFrame + 1;
			Profiler_LastDataFrame.PostDataFrameTick(); // notify end of data-frame portion; rest gets put into "after-data-frame view-frame processing"
		}
	}
}