using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VTree;

public class VTextureDrawer {
	public static GameObject canvasObjOverride;

	/*public VTexture(Texture2D texture, Color color, Rect rect)
	{
		this.texture = texture;
		this.color = color;
		this.rect = rect;
	}*/
	public VTextureDrawer(bool useL2Camera = false, bool useMaterial = false) {
		obj = new GameObject("VTexture");
		if (canvasObjOverride)
			obj.transform.parent = canvasObjOverride.transform;
		else
			//obj.transform.parent = VO.main.gameObject.GetChild(useL2Camera ? "#General/UnityUICamera_L2/Canvas" : "#General/UnityUICamera_L1/Canvas").transform;
			obj.transform.parent = VO.main.gameObject.GetChild("@General/UnityUICamera_L1/Canvas").transform;
		obj.SetActive(false);
		obj.SetLayer("UI");
		//obj.transform.localPosition = new Vector3(rect.x - (Screen.width / 2), rect.y - (Screen.height / 2), 1);

		transform = obj.AddComponent<RectTransform>();
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;

		/* //transform.anchoredPosition = new Vector2(0, -10); // fix for strange offset
		transform.anchoredPosition = Vector2.zero;
		transform.sizeDelta = Vector2.one;
		transform.pivot = Vector2.zero; // not sure what this is for*/

		/*transform.anchorMin = Vector2.zero;
		transform.anchorMax = Vector2.one;
		transform.anchoredPosition = Vector2.zero;
		//transform.sizeDelta = Vector2.zero;
		transform.sizeDelta = new Vector2(-Screen.width, -Screen.height); // start the size out as 0 (so it doesn't show up till the rect property is set in below props/methods)
		transform.pivot = Vector2.zero; // not sure what this is for*/

		transform.pivot = Vector2.zero;
		transform.anchorMin = transform.anchorMax = Vector2.zero;
		transform.anchoredPosition = Vector2.zero;
		transform.sizeDelta = Vector2.zero;

		obj.AddComponent<CanvasRenderer>();
		if (!useMaterial) {
			imageComp_raw = obj.AddComponent<RawImage>();
			imageComp_raw.texture = texture;
		}
		else {
			imageComp_material = obj.AddComponent<Image>();
			imageComp_material.sprite = sprite;
		}
	}

	// for if useMaterial is false
	public Texture texture {
		get { return imageComp_raw.texture; }
		set {
			if (imageComp_raw != null)
				imageComp_raw.texture = value;
			else
				throw new Exception("Use VTextureDrawer.sprite, for drawer's with useMaterial set to true.");
		}
	}

	// for if useMaterial is true
	public Sprite sprite {
		get { return imageComp_material.sprite; }
		set {
			if (imageComp_material != null)
				imageComp_material.sprite = value;
			else
				throw new Exception("Use VTextureDrawer.sprite, for drawer's with useMaterial set to true.");
		}
	}
	public Material material {
		get { return imageComp_raw ? imageComp_raw.material : imageComp_material.material; }
		set {
			if (imageComp_raw != null)
				imageComp_raw.material = value;
			else
				imageComp_material.material = value;
		}
	}

	Color _color = Color.white;
	public Color color {
		get { return _color; }
		set {
			_color = value;
			if (imageComp_raw != null)
				imageComp_raw.color = color;
			else
				imageComp_material.color = color;
		}
	}
	VRect _rect;
	public VRect rect {
		get { return _rect; }
		set {
			_rect = value;
			/*transform.anchorMin = new Vector2((float)(_rect.x / Screen.width), (float)(_rect.y / Screen.height));
			transform.anchorMax = new Vector2((float)((_rect.x + _rect.width - 1) / Screen.width), (float)((_rect.y + _rect.height - 1) / Screen.height));*/
			/*transform.anchoredPosition = new Vector2((float)_rect.x, (float)_rect.y);
			transform.sizeDelta = new Vector2((float)rect.width - Screen.width, (float)rect.height - Screen.height);*/

			/*transform.pivot = Vector2.one / 2;
			transform.anchorMin = transform.anchorMax = Vector2.zero;*/
			transform.anchoredPosition = _rect.position.ToVector2(false);
			transform.sizeDelta = _rect.size.ToVector2(false);
		}
	}

	public GameObject obj;
	public RectTransform transform;
	public RawImage imageComp_raw;
	public Image imageComp_material;

	public bool enabled {
		get { return obj.activeSelf; }
		set { obj.SetActive(value); }
	}

	public void Destroy() { V.Destroy(obj); }

	// can be used to set to index-0, making it display earlier than other VUI elements (thus making the other ones show up on top)
	public void SetIndex(int index) { transform.SetSiblingIndex(index); }
}

/*public class VLineDrawer
{
	public VLineDrawer(bool useL2Camera = false)
	{
		obj = new GameObject("VLineDrawer");
		obj.transform.parent = VO.main.gameObject.GetChild(useL2Camera ? "#General/UnityUICamera_L2/Canvas" : "#General/UnityUICamera_L1/Canvas").transform;
		obj.SetActive(false);
		obj.SetLayer("UI");
		//obj.transform.localPosition = new Vector3(rect.x - (Screen.width / 2), rect.y - (Screen.height / 2), 1);

		transform = obj.AddComponent<RectTransform>();
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;

		transform.anchorMin = Vector2.zero;
		transform.anchorMax = Vector2.one;
		//transform.anchoredPosition = new Vector2(0, -10); // fix for strange offset
		transform.anchoredPosition = Vector2.zero;
		transform.sizeDelta = Vector2.zero;
		transform.pivot = Vector2.zero;

		obj.AddComponent<CanvasRenderer>();
		imageComp = obj.AddComponent<RawImage>();
		imageComp.texture = texture;
	}

	Texture2D _texture;
	public Texture2D texture
	{
		get { return _texture; }
		set
		{
			_texture = value;
			imageComp.texture = _texture;
		}
	}
	Color _color;
	public Color color
	{
		get { return _color; }
		set
		{
			_color = value;
			imageComp.color = color;
		}
	}
	/*public void SetStartAndEndPointsAndLineWidth(Vector2 startPoint, Vector2 endPoint, float lineThickness)
	{
		Vector3 differenceVector = endPoint - startPoint;
		transform.sizeDelta = new Vector2(differenceVector.magnitude, lineThickness);
		transform.pivot = new Vector2(0, .5f);
		transform.position = startPoint;
		float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
		transform.localRotation = Quaternion.Euler(0, 0, angle);
	}*#/
	public void SetStartAndEndPointsAndLineWidth(VVector2 startPoint, VVector2 endPoint, float lineThickness, VRect? constrainmentRect = null)
	{
		if (constrainmentRect.HasValue)
		{
			if (!constrainmentRect.Value.Contains(startPoint) && !constrainmentRect.Value.Contains(endPoint) && !VGeometry.FindClosestLineRectangleIntersection(startPoint, endPoint, constrainmentRect.Value).HasValue)
			{
				transform.localScale = Vector3.zero;
				return;
			}

			if (!constrainmentRect.Value.Contains(startPoint))
				startPoint = VGeometry.ConstrainVectorToRect(startPoint, endPoint, constrainmentRect.Value).Value;
			if (!constrainmentRect.Value.Contains(endPoint))
				endPoint = VGeometry.ConstrainVectorToRect(endPoint, startPoint, constrainmentRect.Value).Value;
		}
		
		VVector2 differenceVector = endPoint - startPoint;
		transform.sizeDelta = new Vector2(-Screen.width + (float)differenceVector.magnitude, -Screen.height + lineThickness);
		float angle = Mathf.Atan2((float)differenceVector.y, (float)differenceVector.x) * Mathf.Rad2Deg;

		VVector2 offset = VVector2.zero;
		if (angle >= 45 && angle < 135) // toward bottom-right (or after)
			offset = new VVector2(0, 1);
		else if (angle >= 135 && angle < 225) // toward bottom-left (or after)
			offset = new VVector2(1, 1);
		else if (angle >= 225 && angle < 315) // toward top-left (or after)
			offset = new VVector2(1, 0);
		startPoint += offset;
		//endPoint += offset;

		transform.anchoredPosition = startPoint.ToVector2(false);
		transform.localRotation = Quaternion.Euler(0, 0, angle);
		transform.localScale = Vector3.one;
	}
	public void SetStartAndEndPointsAndLineWidth(Camera cam, VVector3 startPoint_world, VVector3 endPoint_world, float lineThickness, VRect? constrainmentRect = null)
		{ SetStartAndEndPointsAndLineWidth(cam.WorldToScreenPoint(startPoint_world.ToVector3()).ToVVector3(false).ToVVector2(), cam.WorldToScreenPoint(endPoint_world.ToVector3()).ToVVector3(false).ToVVector2(), lineThickness, constrainmentRect); }

	public GameObject obj;
	public RectTransform transform;
	public RawImage imageComp;

	public bool enabled
	{
		get { return obj.activeSelf; }
		set { obj.SetActive(value); }
	}

	public void Destroy() { V.Destroy(obj); }
}*/

/*public class BoundsDrawer
{
	public BoundsDrawer()
	{
		for (var i = 0; i < 12; i++)
			lineDrawers.Add(new VLineDrawer());
	}

	List<VLineDrawer> lineDrawers = new List<VLineDrawer>(); 
	public Color color
	{
		get { return lineDrawers[0].color; }
		set
		{
			foreach (VLineDrawer lineDrawer in lineDrawers)
				lineDrawer.color = color;
		}
	}
	public void SetBounds(VBounds bounds, float lineThickness)
	{
		var leftBackDown = bounds.position;
		var leftForwardUp = bounds.position.NewY(bounds.max.y).NewZ(bounds.max.z);
		var rightBackUp = bounds.position.NewX(bounds.max.x).NewZ(bounds.max.z);
		var rightForwardDown = bounds.position.NewX(bounds.max.x).NewY(bounds.max.y);

		lineDrawers[0].SetStartAndEndPointsAndLineWidth(Camera.main, leftBackDown, leftBackDown.NewX(bounds.max.x), lineThickness);
		lineDrawers[1].SetStartAndEndPointsAndLineWidth(Camera.main, leftBackDown, leftBackDown.NewY(bounds.max.y), lineThickness);
		lineDrawers[2].SetStartAndEndPointsAndLineWidth(Camera.main, leftBackDown, leftBackDown.NewZ(bounds.max.z), lineThickness);
		lineDrawers[3].SetStartAndEndPointsAndLineWidth(Camera.main, leftForwardUp, leftForwardUp.NewX(bounds.max.x), lineThickness);
		lineDrawers[4].SetStartAndEndPointsAndLineWidth(Camera.main, leftForwardUp, leftForwardUp.NewY(bounds.position.y), lineThickness);
		lineDrawers[5].SetStartAndEndPointsAndLineWidth(Camera.main, leftForwardUp, leftForwardUp.NewZ(bounds.position.z), lineThickness);
		lineDrawers[6].SetStartAndEndPointsAndLineWidth(Camera.main, rightBackUp, rightBackUp.NewX(bounds.position.x), lineThickness);
		lineDrawers[7].SetStartAndEndPointsAndLineWidth(Camera.main, rightBackUp, rightBackUp.NewY(bounds.max.y), lineThickness);
		lineDrawers[8].SetStartAndEndPointsAndLineWidth(Camera.main, rightBackUp, rightBackUp.NewZ(bounds.position.z), lineThickness);
		lineDrawers[9].SetStartAndEndPointsAndLineWidth(Camera.main, rightForwardDown, rightForwardDown.NewX(bounds.position.x), lineThickness);
		lineDrawers[10].SetStartAndEndPointsAndLineWidth(Camera.main, rightForwardDown, rightForwardDown.NewY(bounds.position.y), lineThickness);
		lineDrawers[11].SetStartAndEndPointsAndLineWidth(Camera.main, rightForwardDown, rightForwardDown.NewZ(bounds.max.z), lineThickness);
	}

	public bool enabled
	{
		get { return lineDrawers[0].enabled; }
		set
		{
			foreach (VLineDrawer lineDrawer in lineDrawers)
				lineDrawer.enabled = value;
		}
	}

	public void Destroy()
	{
		foreach (VLineDrawer lineDrawer in lineDrawers)
			lineDrawer.Destroy();
	}
}

// probably todo: make all currently-using-OnGUI classes use this instead
public class VUI : MonoBehaviour
{
    /*static Dictionary<int, HashSet<Action>> layerCalls = new Dictionary<int, HashSet<Action>>();
	public static void AddCall(Action call, int layer = 0)
	{
	    if (layerCalls.ContainsKey(layer))
	        layerCalls[layer].Add(call); // add the call
	    else
	        layerCalls[layer] = new HashSet<Action> {call};
	}
	public static void RemoveCall(Action listener, int layer = 0)
	{
		if (layerCalls.ContainsKey(layer))
			layerCalls[layer].Remove(listener);
	}
	
	public static void CallOnVGUI()
	{
		if(layerCalls != null)
		    foreach (int layer in layerCalls.Keys.OrderBy(layer=>layer))
		        foreach (Action call in layerCalls[layer])
                    if (!(call.Target is Behaviour) || ((Behaviour)call.Target).enabled)
		                call();
	}

    bool updateRanLast;
	void Update() { updateRanLast = true; }
	void OnGUI()
	{
        if (updateRanLast)
	    {
	        updateRanLast = false;
	        return;
	    }
		CallOnVGUI();
	}*#/

    public static int Layer 
    {
        get { return -GUI.depth; }
        set { GUI.depth = -value; }
    }

	/*List<VTexture> textures;
	public void AddTexture(VTexture texture) { textures.Add(texture); }
	public void RefreshTexture(VTexture texture) { textures.Add(texture); }
	public void RemoveTexture(VTexture texture) { textures.Remove(texture); }*#/
}*/