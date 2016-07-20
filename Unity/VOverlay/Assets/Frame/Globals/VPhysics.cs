using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class VPhysics
{
	public static int GetLayerMask(List<string> includeLayers = null, string excludeLayer1 = "Ignore Raycast", string excludeLayer2 = "[none]", params string[] extraExcludeLayers)
	{
		int result = 0; // no layers
		if (includeLayers == null)
			result = ~0; // all layers
		else
			foreach (string layer in includeLayers)
				result = result | (1 << LayerMask.NameToLayer(layer)); // add layer
		if (excludeLayer1 != null)
			result = result & ~(1 << LayerMask.NameToLayer(excludeLayer1)); // remove layer
		if (excludeLayer2 != null)
			result = result & ~(1 << LayerMask.NameToLayer(excludeLayer2)); // remove layer
		foreach (string layer in extraExcludeLayers)
			result = result & ~(1 << LayerMask.NameToLayer(layer)); // remove layer
		return result;
	}
	public static Vector3? CastRay_Point(VRay ray, float distance = float.MaxValue, List<string> includeLayers = null, string excludeLayer1 = "Ignore Raycast", string excludeLayer2 = "[none]", params string[] extraExcludeLayers)
	{
		var rayHit = CastRay(ray, distance, includeLayers, excludeLayer1, excludeLayer2, extraExcludeLayers);
		if (rayHit.HasValue)
			return rayHit.Value.point;
		return null;
	}
	public static RaycastHit? CastRay(VRay ray, float distance = float.MaxValue, List<string> includeLayers = null, string excludeLayer1 = "Ignore Raycast", string excludeLayer2 = "[none]", params string[] extraExcludeLayers)
	{
		RaycastHit? result = null;
		RaycastHit temp;
		if (Raycast(ray, out temp, distance, includeLayers, excludeLayer1, excludeLayer2, extraExcludeLayers))
			result = temp;
		return result;
	}
	public static List<RaycastHit> CastRay_MultiHit(VRay ray, float distance = float.MaxValue, List<string> includeLayers = null, string excludeLayer1 = "Ignore Raycast", string excludeLayer2 = "[none]", params string[] extraExcludeLayers)
		{ return Physics.RaycastAll(ray.ToRay(), distance, GetLayerMask(includeLayers, excludeLayer1, excludeLayer2, extraExcludeLayers)).ToList(); }

	// old style (using an out-parameter)
	public static bool Raycast(VRay ray, out RaycastHit hit, float distance = float.MaxValue, List<string> includeLayers = null, string excludeLayer1 = "Ignore Raycast", string excludeLayer2 = "[none]", params string[] extraExcludeLayers)
		{ return Physics.Raycast(ray.ToRay(), out hit, distance, GetLayerMask(includeLayers, excludeLayer1, excludeLayer2, extraExcludeLayers)); }
}