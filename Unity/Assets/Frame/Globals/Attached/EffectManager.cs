using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VTree;

public enum EffectType {
	CFX_Hit_C_White_33,
	CFX2_SparksHit_50
}
public class EffectManager : MonoBehaviour {
	public static EffectManager GetLive() { return VO.main.gameObject.GetComponent<EffectManager>(); }

	public static GameObject effectHolder;
	public static void EnsureEffectsHolderBuilt() {
		if (!effectHolder) {
			effectHolder = new GameObject("Effects");
			effectHolder.transform.parent = VO.main.race._gameObject.transform;
			effectHolder.transform.position = Vector3.zero;
		}
	}
	public static GameObject SpawnEffect(EffectType effectType, Vector3 position)
	{
		EnsureEffectsHolderBuilt();

		var result = (GameObject)Instantiate(VO.main.gameObject.GetComponent<EffectManager>().effectPrefabs.First(a=>a.name == effectType.ToString()), position, Quaternion.identity);
		result.transform.parent = effectHolder.transform;
		return result;
	}

	/*public static GameObject SpawnTrailFollowing(GameObject target)
	{
		var live = GetLive();

		var trailObj = new GameObject("Trail");
		trailObj.transform.parent = effectHolder.transform;

		var trail = trailObj.AddComponent<Trail>();
		trail.material = live.trailMaterial;
		trail.colors = new List<Color> {live.trailColor};
		trail.widths = new List<float> {live.trailBaseWidth};
		trail.lifeTime = live.trailTime;
		trail.target = target;

		return trailObj;
	}*/

	public List<GameObject> effectPrefabs;

	public Material trailMaterial;
	public Color trailColor;
	public float trailBaseWidth;
	public float trailTime;
}