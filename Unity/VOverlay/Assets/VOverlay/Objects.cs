using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using VDFN;
using VTree.BiomeDefenseN.ObjectsN;
using VTree_Structures;
using Object = UnityEngine.Object;

namespace VTree.VOverlayN {
	public class Objects : Node {
		Objects s;
		[VDFPreDeserialize] public Objects() { obj = VO.main.gameObject.GetChild("Objects"); }

		[P(false)] public GameObject obj;

		void LoadFileSystemData() {
			var S = M.GetCurrentMethod().Profile_AllFrames();

			var S2 = S._____2("objects");
			var mainFolder = FileManager.GetFolder("VOverlay/Objects/Objects");
			foreach (var folder in mainFolder.GetDirectories_Safe("*", SearchOption.AllDirectories))
				if (folder.GetFile("Main.vdf").Exists && folder.Name != "CacheL3" && !("/" + folder.VFullName(mainFolder)).Contains("/@")) // if object folder, and not under ignored folder
					try {
						//SubmitAdd(Item.LoadMainData(this, folder));
						var S3 = S2._____2(folder.Name);
						S3._____("VObject_Type.Load");
						var obj = VObject_Type.Load(folder);
						S3._____("Add object to main-tree");
						s.a(a=>a.objects).add = obj;
					}
					catch (Exception ex) {
						ex.AddToMessage("\nObject:" + folder.Name + "\n");
						throw;
					}

			S._____(null);
		}

		[NotTo("file")] public bool visible;
		/*void visible_PostSet() {
			obj.SetActive(visible);
		}*/

		// general
		// ==========

		[NotTo("file")] public List<VObject_Type> objects = new List<VObject_Type>();
	}
}