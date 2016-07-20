using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VTree;
using Object = UnityEngine.Object;

public static class VCoroutine
{
	public static Coroutine Start(IEnumerator action) { return (VO.main.script ?? Object.FindObjectOfType<MonoBehaviour>()).StartCoroutine(action); }
	public static Coroutine Start(params IEnumerator[] actions) { return (VO.main.script ?? Object.FindObjectOfType<MonoBehaviour>()).StartCoroutine(Chain(actions)); }

	/**
      * Usage: StartCoroutine(VCoroutine.Chain(...))
      * For example:
      *     StartCoroutine(VCoroutine.Chain(
      *         VCoroutine.Do(() => Debug.Log("A")),
      *         VCoroutine.WaitForSeconds(2),
      *         VCoroutine.Do(() => Debug.Log("B"))));
      */
	public static IEnumerator Chain(params IEnumerator[] actions) // maybe todo: let Action's be passed as well, not just IEnumerators
	{
		foreach (IEnumerator action in actions)
			yield return Start(action);
	}

	// combos
	// ==========

	// general
	// ==========

	/**
	 * Usage: StartCoroutine(VCoroutine.DelaySeconds(action, delay))
	 * For example:
	 *     StartCoroutine(VCoroutine.DelaySeconds(
	 *         () => DebugUtils.Log("2 seconds past"),
	 *         2);
	 */
	public static IEnumerator Do(Action action)
	{
		action();
		yield return null;
		//if (false)
		//	yield return null;
	}
	/*public static IEnumerator WhileX_YieldReturnNull(Func<bool> xExpressionFunc)
	{
		while (xExpressionFunc())
			yield return null;
	}*/
	public static IEnumerator WaitForEndOfFrame() { yield return new WaitForEndOfFrame(); }
	public static IEnumerator WaitForSeconds(float time) { yield return new WaitForSeconds(time); }
	public static IEnumerator WaitTill(Func<bool> xExpressionFunc)
	{
		while (!xExpressionFunc())
			yield return null;
	}

	/*public static IEnumerator DelaySeconds(Action action, float delay)
	{
		yield return new WaitForSeconds(delay);
		action();
	}*/
}