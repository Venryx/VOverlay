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
using VTree_Structures;
using Object = UnityEngine.Object;

namespace VTree.VOverlayN {
	public class Race : Node {
		Race s;
		[VDFPreDeserialize] public Race() { _gameObject = VO.main.gameObject.GetChild("Race"); }

		public GameObject _gameObject;

		[NotTo("file")] public bool visible;
		void visible_PostSet() {
			_gameObject.SetActive(visible);
			if (visible) {
				// make-so: game is launched
			}
		}

		// general
		// ==========

		public void StartNewRace() {
			var S = M.GetCurrentMethod().Profile_AllFrames();

			if (liveMatch != null)
				s.a(a=>a.liveMatch).set = null;
			
			S._____("clone map");
			var match = new Match();
			var mapType = VO.main.maps.GetRandomMapType();
			var map = mapType.Clone();

			// maybe temp
			/*foreach (VObject obj in map.GetObjects())
				obj.LoadDataFromTypeVObject();*/

			S._____("basic setup");
            map.name = "[live match map]";
			match.map = map;
			map.match = match;

			S._____("player init");
			// make-so: this gets populated with the actual chat-members
			var members = new List<ChatMember> {new ChatMember("Venryx", VColor.Blue, "😒") };
			foreach (var member in members) {
				var player = new Player(member);
				map.players.Add(player);
			}

			// activate
			// ==========

			LoadMatch(match);
			StartMatch(match);
		}
		void LoadMatch(Match match) {
			var S = M.GetCurrentMethod().Profile_AllFrames();
			//var map = match.map;

			s.a(a=>a.liveMatch).set = match;

			S._____("add standard objects");
			AddStandardObjects(match);
			S._____("notify post-core-map-init");
			// at this point, core-map-init is officially done; notify post-core-map-init
			match.NotifyPostCoreMapInit();
			
			S._____(null);
		}

		void AddStandardObjects(Match match) {
			var map = match.map;
			var jumperUnitType = VO.main.objects.objects.First(a=>a.name == "Jumper");
			foreach (var player in match.map.players) {
				var unit = jumperUnitType.Clone();
				unit.map = map;
				unit.owner = player;
				unit.transform.Position = new VVector3(0, 0, 1);
				unit.emojiStr = player.chatMember.emojiStr;
				match.map.a(a=>a.units).add = unit;
			}
		}

		void StartMatch(Match match) { match.a(a=>a.started).set = true; }

		[P(false)] public Match liveMatch;
		void liveMatch_PostSet() { liveMatch.StartBuilding(); }
	}
}