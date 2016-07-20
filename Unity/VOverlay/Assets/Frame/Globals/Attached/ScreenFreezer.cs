using UnityEngine;

public class ScreenFreezer : MonoBehaviour
{
	bool screenFrozen;
	Texture2D screenFreezeTexture;
	public float screenFreezeTextureOpacity = 1;
	public void FreezeScreen()
	{
		screenFrozen = true;
		V.GetScreenshot_Async(texture=>screenFreezeTexture = texture);
	}
	public void UnfreezeScreen()
	{
		screenFrozen = false;
		screenFreezeTexture = null;
	}
	void OnGUI()
	{
		if (screenFrozen && screenFreezeTexture != null)
		{
			Color oldColor = GUI.color;
			GUI.color = new Color(1, 1, 1, screenFreezeTextureOpacity);
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), screenFreezeTexture);
			GUI.color = oldColor;

			//GL.PushMatrix();
			//GL.LoadIdentity(); // fix gui-showing-up-in-world-space issue
			//Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), screenFreezeTexture, new Rect(0, 0, 1, 1), 0, 0, 0, 0);
			//GL.PopMatrix();
		}
	}
}