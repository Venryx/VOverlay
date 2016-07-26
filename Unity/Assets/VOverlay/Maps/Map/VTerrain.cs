using System.Collections.Generic;
using UnityEngine;
using VDFN;
using VTree.VOverlayN.MapsN;
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

			var borderThickness = 1000f;

			var bottomCollider = _obj.AddComponent<BoxCollider2D>();
			bottomCollider.size = new Vector2((float)VO.GetSize_WorldMeter().x, borderThickness);
			bottomCollider.offset = new Vector2(bottomCollider.size.x / 2, -(borderThickness / 2));

			var topCollider = _obj.AddComponent<BoxCollider2D>();
			topCollider.size = new Vector2((float)VO.GetSize_WorldMeter().x, borderThickness);
			topCollider.offset = new Vector2(bottomCollider.size.x / 2, (float)(VO.GetSize_WorldMeter().z + (borderThickness / 2)));

			var leftCollider = _obj.AddComponent<BoxCollider2D>();
			leftCollider.size = new Vector2(borderThickness, borderThickness + (float)VO.GetSize_WorldMeter().z + borderThickness);
			leftCollider.offset = new Vector2(-(borderThickness / 2), (float)(VO.GetSize_WorldMeter().z / 2));

			var rightCollider = _obj.AddComponent<BoxCollider2D>();
			rightCollider.size = new Vector2(borderThickness, borderThickness + (float)VO.GetSize_WorldMeter().z + borderThickness);
			rightCollider.offset = new Vector2((float)(VO.GetSize_WorldMeter().x + (borderThickness / 2)), (float)(VO.GetSize_WorldMeter().z / 2));

			var colliders = new List<BoxCollider2D> {bottomCollider, topCollider, leftCollider, rightCollider};
			foreach (var collider in colliders)
				collider.sharedMaterial = VO.main.script.physicsMaterials[0];
		}
	}
}