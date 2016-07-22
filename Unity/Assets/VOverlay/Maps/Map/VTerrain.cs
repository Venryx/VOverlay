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

			var bottomCollider = _obj.AddComponent<BoxCollider2D>();
			bottomCollider.size = new Vector2((float)VO.GetSize_WorldMeter().x, 1);
			bottomCollider.offset = new Vector2(bottomCollider.size.x / 2, (float)(VO.GetSize_WorldMeter().z + .5));

			var topCollider = _obj.AddComponent<BoxCollider2D>();
			topCollider.size = new Vector2((float)VO.GetSize_WorldMeter().x, 1);
			topCollider.offset = new Vector2(topCollider.size.x / 2, -.5f);

			var leftCollider = _obj.AddComponent<BoxCollider2D>();
			leftCollider.size = new Vector2(1, (float)VO.GetSize_WorldMeter().z);
			leftCollider.offset = new Vector2(-.5f, leftCollider.size.y / 2);

			var rightCollider = _obj.AddComponent<BoxCollider2D>();
			rightCollider.size = new Vector2(1, (float)VO.GetSize_WorldMeter().z);
			rightCollider.offset = new Vector2((float)(VO.GetSize_WorldMeter().x + .5), rightCollider.size.y / 2);
		}
	}
}