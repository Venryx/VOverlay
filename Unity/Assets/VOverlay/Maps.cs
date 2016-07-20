using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using VDFN;
using VTree.BiomeDefenseN.MapsN;
using VTree_Structures;
using Random = System.Random;

namespace VTree.OverlayN {
	public class Maps : Node {
		Maps s;
		[VDFPreDeserialize] public Maps() { _gameObject = VO.main.gameObject.GetChild("Maps"); }

		public GameObject _gameObject;

		void LoadFileSystemData() {
			var S = M.GetCurrentMethod().Profile_AllFrames();

			var mapsFolder = FileManager.GetFolder("VOverlay/Maps/Maps");

			var S2 = S._____2("maps");
			foreach (DirectoryInfo folder in mapsFolder.GetDirectories_Safe("*", SearchOption.AllDirectories))
				if (folder.GetFile("Main.vdf").Exists && !("/" + folder.VFullName(mapsFolder)).Contains("/@")) { // if map folder, and not under ignored folder
					S2._____(folder.Name);
					var map = Map.Load_Part1(folder);
					s.a(a=>a.maps).add = map;
				}

			S._____(null);
		}

		[NotTo("file")] public bool visible;
		void visible_PostSet() { _gameObject.SetActive(visible); }

		// maps
		// =========

		[NotTo("file")] public List<Map> maps = new List<Map>();

		public Map GetRandomMapType() { return maps[new Random().Next(maps.Count)]; }
	}
}