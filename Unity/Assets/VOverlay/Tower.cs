using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using VDFN;
using VTree.BiomeDefenseN.MapsN;
using VTree.BiomeDefenseN.MapsN.MapN;
using VTree.BiomeDefenseN.MatchesN;
using VTree.BiomeDefenseN.ObjectsN;
using VTree.VOverlayN.MapsN.MapN;
using VTree.VOverlayN.TowerN;
using VTree_Structures;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace VTree.VOverlayN {
	public class Tower : Node {
		Tower s;
		[VDFPreDeserialize] public Tower() { gameObj = VO.main.gameObject.GetChild("Tower"); }

		[P(false)] public GameObject gameObj;

		[NotTo("file")] public bool visible;
		void visible_PostSet() { gameObj.SetActive(visible); }
		public void MakeVisible() { s.a(a => a.visible).set = true; }

		// general
		// ==========

		public void StartNewMatch() {
			var S = M.GetCurrentMethod().Profile_AllFrames();

			if (liveMatch != null)
				s.a(a=>a.liveMatch).set = null;
			
			S._____("clone map");
			var match = new TowerMatch();
			var mapType = VO.main.maps.GetRandomMapType();
			var map = mapType.Clone();

			S._____("basic setup");
            map.name = "[live match map]";
			match.map = map;
			map.match = match;

			// add tower-blocks
			// ==========

			var towerCenter_x = 32;

			//var blockType = new VObject_Type {name = "Block"};
			var blockType = VO.main.objects.objects.First(a=>a.name == "Block");
			for (var z = 0; z < 32; z++) {
				var rowOffset = Random.Range(0, 1f);
				for (var i = 0; i < 3; i++) {
					var block = blockType.Clone();
					block.map = map;
					block.block.number = match.nextBlockNumber++;

					var cellOffset = i * 2;
					block.transform.Position = new VVector3(towerCenter_x + rowOffset + cellOffset, 0, z + .5);

					var distanceMultiplier = .95f;
					block.block.vertexes = new List<Vector2> {
						new Vector2(-1 * distanceMultiplier, .5f * distanceMultiplier), // left top
						new Vector2(1 * distanceMultiplier, .5f * distanceMultiplier), // right top
						new Vector2(1 * distanceMultiplier, -.5f * distanceMultiplier), // right down
						new Vector2(-1 * distanceMultiplier, -.5f * distanceMultiplier) // left down
					};
					block.block.triangleVertexIndexes = new List<int> {
						0, 1, 3,
						1, 2, 3
					};

					map.structures.Add(block);
				}
			}

			// activate
			// ==========

			LoadMatch(match);
			StartMatch(match);
		}
		void LoadMatch(TowerMatch match) {
			s.a(a=>a.liveMatch).set = match;
			match.NotifyPostCoreMapInit();
		}
		void StartMatch(TowerMatch match) { match.a(a=>a.started).set = true; }

		[P(false)] public TowerMatch liveMatch;
		[IgnoreStartData] void liveMatch_PreSet() {
			if (liveMatch != null) // if live-match is being closed
				V.Destroy(liveMatch.obj);
		}
		void liveMatch_PostSet() {
			if (liveMatch == null)
				return;
			liveMatch.StartBuilding();
		}
	}
}