using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking.Match;
using VDFN;
using VectorStructExtensions;
using VTree.BiomeDefenseN.MapsN;
using VTree.BiomeDefenseN.MapsN.MapN;
using VTree.BiomeDefenseN.ObjectsN.ObjectN;
using VTree.BiomeDefenseN.ObjectsN.ObjectN.ComponentsN;
using VTree.VOverlayN.TowerN;
using VTree_Structures;

namespace VTree.VOverlayN.ObjectsN.ObjectN.ComponentsN {
	public class Block : VComponent {
		Block typeComp;
		[VDFPreDeserialize] public Block() {}

		public int number;
		public List<Vector2> vertexes;
		public List<int> triangleVertexIndexes;
		MeshData meshData;
		//MeshData meshData = new MeshData(); // make-so: this works

		MeshFilter filter;
		MeshRenderer renderer;
		PolygonCollider2D collider;
		//BoxCollider2D collider;
		Rigidbody2D rigidbody;

		GameObject textSub;
		TextMesh textMesh;

		void Manifest() {
			filter = obj.gameObject.AddComponent<MeshFilter>();
			filter.sharedMesh = new Mesh();
			UpdateMesh();

			renderer = obj.gameObject.AddComponent<MeshRenderer>();
			UpdateMaterial();

			collider = obj.gameObject.AddComponent<PolygonCollider2D>();
			//collider = obj.gameObject.AddComponent<BoxCollider2D>();
			UpdateCollider();

			// add rigidbody
			if (obj.map != null && obj.map.match != null) {
				rigidbody = obj.gameObject.AddComponent<Rigidbody2D>();
				rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
				rigidbody.mass = (float)Quick.GetDouble("tower_block_mass", 1);
				rigidbody.gravityScale = (float)Quick.GetDouble("tower_block_gravityScale", 10);
				rigidbody.drag = (float)Quick.GetDouble("tower_block_drag", 3);
				rigidbody.angularDrag = (float)Quick.GetDouble("tower_block_angularDrag", 1);
			}

			// add block-number text
			textSub = new GameObject("NumberMarker");
			textSub.transform.parent = obj.gameObject.transform;
			textSub.transform.localPosition = Vector3.zero;
			textSub.transform.localScale = Vector3.one * .3f;
			textMesh = textSub.AddComponent<TextMesh>();
			textMesh.font = VO.main.script.mainFont;
			textMesh.fontSize = 30;
			textMesh.anchor = TextAnchor.MiddleCenter;
			textMesh.alignment = TextAlignment.Center;
			textMesh.color = Color.red;
			textMesh.text = number.ToString();
		}
		void UpdateMesh() {
			meshData = new MeshData();
			meshData.vertexes = vertexes.Select(a=>a.ToVVector3_()).ToList();
			meshData.submeshTriangleVertexIndexes = new List<List<int>> {triangleVertexIndexes};
			meshData.uvs = meshData.vertexes.Select(a=>new VVector2(a.x, a.z)).ToList();
			meshData.normals = meshData.vertexes.Select(a=>new VVector3(0, -1, 0)).ToList();

			BuildHelper.CalculateTangents(meshData);

			meshData.PrepareArrays();
			meshData.ToMesh(filter.sharedMesh);
		}
		void UpdateMaterial() { renderer.sharedMaterial = VO.main.script.tower_blockMaterial; }
		void UpdateCollider() {
			collider.sharedMaterial = VO.main.script.physicsMaterials[0];
			collider.points = vertexes.ToArray();
			//collider.size = new Vector2(3, 1);
		}

		bool fontRefreshed;
		public void PreViewFrameTick() {
			if (!fontRefreshed) {
				/*textMesh.font = null;
				textMesh.font = VO.main.script.font;*/
				/*textMesh.font = Font.CreateDynamicFontFromOSFont("Cambria", textMesh.fontSize);
				textMesh.font.RequestCharactersInTexture("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890", textMesh.fontSize, textMesh.fontStyle);*/
				//textMesh.font = VO.main.script.font;
				textMesh.GetComponent<MeshRenderer>().sharedMaterial = VO.main.script.mainFont.material;

				//textMesh.font = V.Clone(VO.main.script.font);
				//textMesh.FixFontAtlas();
				/*var realText = textMesh.text;
				textMesh.SetUpFontAtlas();
				textMesh.text = realText;*/

				/*Font.textureRebuilt += font=> {
					if (font == textMesh.font) {
						Debug.Log("Rebuilt");
						textMesh.text = textMesh.text;
						textMesh.font = null;
						textMesh.font = VO.main.script.font;
					}
				};*/
				//textMesh.FixFontAtlas(true);

				fontRefreshed = true;
			}

			var renderer = obj.gameObject.GetChild("NumberMarker").GetComponent<MeshRenderer>();
			renderer.material = renderer.material;

			if (rigidbody.velocity.magnitude < 0.05)
				rigidbody.drag = 2f;
			else
				rigidbody.drag = 0f;

			if (IsTouchingGround() && number > 2) {
				obj.Remove();
				(obj.map.match as TowerMatch).fallenAndRemovedBlocks++;
			}
		}
		public bool IsTouchingGround() { return obj.gameObject.GetBounds().ToBounds().min.y < .3; }
	}
}