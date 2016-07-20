using UnityEngine;
using VDFN;
using VTree_Structures;

namespace VTree.BiomeDefenseN.MapsN.MapN {
	public class VTerrain : Node {
		VTerrain s;
		[VDFPreDeserialize] public VTerrain() {}

		public Map _map;
		[ToMainTree] void _PostAdd() { _map = Parent as Map; }
		public GameObject _obj;

		public void BuildGameObject() {
			_obj = new GameObject("Terrain");
			_obj.transform.parent = _map.obj.transform;
			_obj.layer = LayerMask.NameToLayer("Terrain");
		}
	}
}