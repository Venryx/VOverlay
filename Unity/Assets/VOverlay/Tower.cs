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

			var oldPlayers = new List<Player>();
			if (liveMatch != null) {
				oldPlayers = liveMatch.map.players;
				s.a(a=>a.liveMatch).set = null;
			}

			S._____("clone map");
			var match = new TowerMatch();
			var mapType = VO.main.maps.GetRandomMapType();
			var map = mapType.Clone();

			S._____("basic setup");
            map.name = "[live match map]";
			match.map = map;
			map.match = match;

			// re-add old players (as new Player instances)
			// ==========

			foreach (var oldPlayer in oldPlayers) {
				var player = new Player(oldPlayer.chatMember);
				map.players.Add(player);
			}
			if (oldPlayers.Count > 0)
				match.currentPlayer = oldPlayers[0];

			// set up terrain
			// ==========

			map.terrain.sideAndTopBoundaries = false;

			// add tower-blocks
			// ==========

			var towerCenter_x = 52;
			var blockType = VO.main.objects.objects.First(a=>a.name == "Block");
			var size = new Vector2(3, 1.5f);
			var half = size / 2;
			for (var z = 0; z < 20; z++) {
				var rowOffset = Random.Range(0, .7f) * size.x;
				for (var i = 0; i < 3; i++) {
					var block = blockType.Clone();
					block.map = map;
					block.block.number = match.nextBlockNumber++;

					var cellOffset = i * size.x;
					cellOffset += Random.Range(0, .3f) * size.x;
					block.transform.Position = new VVector3(towerCenter_x + rowOffset + cellOffset, 0, .5 + (z * size.y));

					var distanceMultiplier = .95f;
					block.block.vertexes = new List<Vector2> {
						new Vector2(-half.x * distanceMultiplier, half.y * distanceMultiplier), // left top
						new Vector2(half.x * distanceMultiplier, half.y * distanceMultiplier), // right top
						new Vector2(half.x * distanceMultiplier, -half.y * distanceMultiplier), // right down
						new Vector2(-half.x * distanceMultiplier, -half.y * distanceMultiplier) // left down
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

			S._____(null);
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