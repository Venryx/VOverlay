using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using VDFN;
using VectorStructExtensions;
using VTree.BiomeDefenseN.MapsN.MapN;
using VTree.BiomeDefenseN.MatchesN;
using VTree.BiomeDefenseN.ObjectsN;
using VTree.VOverlayN.MapsN.MapN;
using VTree_Structures;
using Object = UnityEngine.Object;

/*public class MapSaveInfoPack {
	public MapSaveInfoPack(DirectoryInfo folder) { this.folder = folder; }
	public DirectoryInfo folder;
}
public class MapLoadInfoPack {
	public MapLoadInfoPack(DirectoryInfo folder) { this.folder = folder; }
	public DirectoryInfo folder;
}*/

public class MapInfo {
	[VDFProp] public Guid id = Guid.NewGuid();
	[VDFProp] public string name;
}

namespace VTree.BiomeDefenseN.MapsN {
	[VDFType(popOutL1: true)] public class Map : Node {
		Map s;
		[VDFPreDeserialize] public Map() {}

		// loading
		// ==========

		public static Map Load_Part1(DirectoryInfo folder) {
			var result = LoadStub(folder.Name, folder);
			return result;
		}
		public void Load_Part2(bool allowDelayedLoad = true) { LoadMainData(); }

		public DirectoryInfo _folder;
		//[NotToFile] FolderNode folder; // for ui
		[NotTo("file")] public string path; // for ui
		static Map LoadStub(string name, DirectoryInfo folder) {
			var result = (Map)VDFNode.CreateNewInstanceOfType(typeof(Map));

			// stuff that Map needs initialized, for its Node base // maybe todo: move this sort of handling into the Node class itself
			result.s = result;
			//result._parent = BD.main.maps;
			//result._pathNode = new NodePathNode("maps", BD.main.maps.maps.Count); // - 1);
			result._children = new List<Node>();
			result._changeOrMessageSubmissionToUIAllowed = true;
			result._childPlaceholders = new Dictionary<string, NodePlaceholder>();
			result._extraMethods = new Dictionary<string, List<Delegate>>();

			result.name = name;
			result._folder = folder;
			//result.folder = new FolderNode(folder);
			result.path = folder.VFullName();
			return result;
		}
		public bool _mainDataLoaded;
		void LoadMainData() {
			var vdf = File.ReadAllText(_folder.GetFile("Main.vdf").FullName);
            VConvert.FromVDFInto(vdf, this, new VDFLoadOptions(new List<object> {"Map>from file"}));
			FakeAdd(attachPoint); // make sure descendents are attached to this map/sub-tree (now part of main tree), and that map/sub-tree is attached to main tree

			_mainDataLoaded = true;
		}

		// general
		// ==========

		[P(false)] public Match match;
		[ToMainTree] void _PostAdd() {
			if (Parent is Match)
				match = Parent as Match;
		}

		[P(false)] public GameObject obj;
		public string name;

		public VTerrain terrain = new VTerrain();

		// general
		// ==========

		public GameObject BuildGameObject() {
			var result = new GameObject("Map");
			obj = result;
			terrain.BuildGameObject();
			return result;
		}
		public void StartBuilding() {
			foreach (var obj in GetObjects())
				obj.Manifest();

			NotifyBuildCompleted();
		}

		public bool _initialBuildCompleted;
		List<Action> _onBuildCompletedActions = new List<Action>();
		public void RunXOnBuildCompleted(Action action) { _onBuildCompletedActions.Add(action); }
		public void RunXOnBuildCompleted_Early(Action action) { _onBuildCompletedActions.Insert(0, action); }
		//public void RunXOnBuildCompleted_WithDelay(int delayFrames, Action action) { _onBuildCompletedActions.Add(()=>V.WaitXFramesThenCall(delayFrames, action)); }
		public void NotifyBuildCompleted() {
			_initialBuildCompleted = true;
			while (_onBuildCompletedActions.Count != 0) {
				_onBuildCompletedActions[0]();
				_onBuildCompletedActions.RemoveAt(0);
			}
		}

		public Map Clone() {
			if (!_mainDataLoaded)
				Load_Part2(false);

			var options = new VDFSaveOptionsV(new List<object> {"Map>clone"}, toMap: true);
			options.messages.Add("for match");
			var cloneVDF = VConvert.ToVDF(this, options: options);
			var result = VConvert.FromVDF<Map>(cloneVDF);
			result._mainDataLoaded = true;

			return result;
		}

		[VDFProp(popOutL2: true)] public List<Player> players = new List<Player>();

		[VDFProp(popOutL2: true)] public List<VObject> plants = new List<VObject>();
		[IgnoreStartData] void plants_PostAdd(VObject plant, Change change) {
			if (match != null && VO.main.race.liveMatch == match)
				plant.Manifest(obj.GetChild("Plants/" + plant.type.name, true));
		}

		[VDFProp(popOutL2: true)] public List<VObject> structures = new List<VObject>();
		[IgnoreStartData] void structures_PostAdd(VObject structure, Change change) {
			if (match != null && VO.main.race.liveMatch == match)
				structure.Manifest(obj.GetChild("Structures/" + structure.type.name, true));
		}

		[VDFProp(popOutL2: true)] public List<VObject> units = new List<VObject>();
		[IgnoreStartData] void units_PostAdd(VObject unit, Change change) {
			if (match != null && VO.main.race.liveMatch == match)
				unit.Manifest(obj.GetChild("Units/" + unit.type.name, true));
		}

		[VDFProp(popOutL2: true)] public List<VObject> projectiles = new List<VObject>();
		[IgnoreStartData] void projectiles_PostAdd(VObject projectile, Change change) {
			if (match != null && VO.main.race.liveMatch == match)
				projectile.Manifest(obj.GetChild("Projectiles/" + projectile.type.name, true));
		}

		public void AddObject(VObject obj) {
			if (obj.type.objType == ObjectType.Plant)
				s.a(a=>a.plants).add = obj;
			else if (obj.type.objType == ObjectType.Structure)
				s.a(a=>a.structures).add = obj;
			else if (obj.type.objType == ObjectType.Unit)
				s.a(a=>a.units).add = obj;
			else //if (obj.type.objType == ObjectType.Other)
				s.a(a=>a.projectiles).add = obj;
		}

		public List<VObject> GetObjects(bool plants = true, bool structures = true, bool units = true, bool projectiles = true) {
			var result = new List<VObject>();
			if (plants)
				result.AddRange(this.plants);
			if (structures)
				result.AddRange(this.structures);
			if (units)
				result.AddRange(this.units);
			if (projectiles)
				result.AddRange(this.projectiles);
			return result;
		}
		public List<VObject> GetObjects_OptIn(bool plants = false, bool structures = false, bool units = false, bool projectiles = false) {
			return GetObjects(plants, structures, units, projectiles);
		}
	}
}