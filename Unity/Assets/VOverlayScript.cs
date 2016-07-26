using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VTree;

public class VOverlayScript : MonoBehaviour {
	void Awake() {
		var S = M.GetCurrentMethod().Profile_AllFrames();
		S._____("VOverlay init");
		VO.main = new VOverlay(this);
		S._____("VOverlay load-data-and-launch");
		VO.main.LoadDataAndLaunch(); //BD.main.PostAddedToVTree();
		S._____(null);
	}
	void OnApplicationQuit() {
		if (!VO.main._finalizeShutdown) // if triggered by something external (and not just our own shutdown code)
			if (Application.isEditor)
				VO.main.Shutdown(true);
			else {
				Application.CancelQuit(); // cancel quit; we want to control the shutdown schedule
				VO.main.Shutdown();
			}
	}

	void Update() { VO.main.Update(); }
	void LateUpdate() { VO.main.LateUpdate(); }

	public Font mainFont;
	public Font emojiFont;

	//public Font font2;
	public List<GameObject> effects;
	public List<PhysicsMaterial2D> physicsMaterials;
	public Material tower_blockMaterial;
}