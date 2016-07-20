using System.Linq;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using VectorStructExtensions;

public enum VKeyCode
{
	Control,
	Shift,
	Alt
}

public enum MouseButtonState
{	
	IsDown_First,
	IsDown,
	IsUp_First,
	IsUp_First_Click, // down and up both occurred within event zone
	IsUp // default state
}
public enum KeyState
{
	IsDown_First,
	IsDown,
	IsUp_First,
	IsUp_First_Press, // down and up both occurred within event zone
	IsUp // default state
}
public class VInput : MonoBehaviour
{
	static Dictionary<int, MouseButtonState> mouseButtonStates = new Dictionary<int, MouseButtonState>();
	static Dictionary<KeyCode, KeyState> keyStates = new Dictionary<KeyCode, KeyState>();
	public static HashSet<int> MouseButtonsForWhichMouseFocusHasBeenCapturedSinceLastMouseDown = new HashSet<int>();
	public static HashSet<KeyCode> KeysForWhichKeyboardFocusHasBeenCapturedSinceLastKeyDown = new HashSet<KeyCode>();
	public static bool UnityUIHasMouseFocus = false;
	public static bool UnityUIHasKeyboardFocus = false;
	public static bool WebUIHasMouseFocus = false;
	public static bool WebUIHasKeyboardFocus = false;
	
	// class methods: Unity
	// ==========

	static int lastFrameProcessed = -1;
	static void UpdateInputData()
	{
		if (lastFrameProcessed == Time.frameCount)
			return;
		lastFrameProcessed = Time.frameCount;

		if (GetIsMouseFocusTaken())
			foreach (int mouseButton in mouseButtonStates.Keys)
				MouseButtonsForWhichMouseFocusHasBeenCapturedSinceLastMouseDown.Add(mouseButton);
		if (GetIsKeyboardFocusTaken())
			foreach (KeyCode key in keyStates.Keys)
				KeysForWhichKeyboardFocusHasBeenCapturedSinceLastKeyDown.Add(key);

		if (!mouseStateFrozen) // maybe temp
			foreach (int button in mouseButtonStates.Keys.ToArray())
			{
				if (Input.GetMouseButtonDown(button) && !GetIsMouseFocusTaken()) // if new mouse-down event, and focus isn't currently taken
					MouseButtonsForWhichMouseFocusHasBeenCapturedSinceLastMouseDown.Remove(button); // reset, since this was a mouse-down event
				// combine "Input.Get..." with UI focus flags, to calculate mouse states right here
				if (Input.GetMouseButton(button)) // if mouse is currently "down"
				{
					if (!GetIsMouseFocusTaken() && !MouseButtonsForWhichMouseFocusHasBeenCapturedSinceLastMouseDown.Contains(button)) // if we have focus, and we've had focus since last mouse down
					{
						if (mouseButtonStates[button] == MouseButtonState.IsUp || mouseButtonStates[button] == MouseButtonState.IsUp_First || mouseButtonStates[button] == MouseButtonState.IsUp_First_Click) // if last-frame's state was "up"
							mouseButtonStates[button] = MouseButtonState.IsDown_First;
						else
							mouseButtonStates[button] = MouseButtonState.IsDown;
					}
				}
				else // if mouse is currently "up"
				{
					if (mouseButtonStates[button] == MouseButtonState.IsUp || mouseButtonStates[button] == MouseButtonState.IsUp_First || mouseButtonStates[button] == MouseButtonState.IsUp_First_Click) // if last-frame's state was "up"
						mouseButtonStates[button] = MouseButtonState.IsUp;
					else
						mouseButtonStates[button] = MouseButtonsForWhichMouseFocusHasBeenCapturedSinceLastMouseDown.Contains(button) ? MouseButtonState.IsUp_First : MouseButtonState.IsUp_First_Click;
				}
			}
		foreach (KeyCode key in keyStates.Keys.ToArray())
		{
			if (Input.GetKeyDown(key) && !GetIsKeyboardFocusTaken()) // if new key-down event, and focus isn't currently taken
				KeysForWhichKeyboardFocusHasBeenCapturedSinceLastKeyDown.Remove(key); // reset, since this was a key-down event
			// combine "Input.Get..." with UI focus flags, to calculate key states right here
			if (Input.GetKey(key)) // if key is currently "down"
			{
				if (!GetIsKeyboardFocusTaken() && !KeysForWhichKeyboardFocusHasBeenCapturedSinceLastKeyDown.Contains(key)) // if we have focus, and we've had focus since last key down
				{
					if (keyStates[key] == KeyState.IsUp || keyStates[key] == KeyState.IsUp_First || keyStates[key] == KeyState.IsUp_First_Press) // if last-frame's state was "up"
						keyStates[key] = KeyState.IsDown_First;
					else
						keyStates[key] = KeyState.IsDown;
				}
				else // maybe temp; have input 'forced' into 'up' state when focus is captured
					keyStates[key] = KeyState.IsUp_First;
			}
			else // if key is currently "up"
			{
				if (keyStates[key] == KeyState.IsUp || keyStates[key] == KeyState.IsUp_First || keyStates[key] == KeyState.IsUp_First_Press) // if last-frame's state was "up"
					keyStates[key] = KeyState.IsUp;
				else
					keyStates[key] = KeysForWhichKeyboardFocusHasBeenCapturedSinceLastKeyDown.Contains(key) ? KeyState.IsUp_First : KeyState.IsUp_First_Press;
			}
		}
	}
	void LateUpdate() // make sure we 'progress' key-states AFTER they've already had initial processing
	{
		if (!mouseStateFrozen) // maybe temp
			foreach (int button in mouseButtonStates.Keys.ToArray())
			{
				if (mouseButtonStates[button] == MouseButtonState.IsDown_First)
					mouseButtonStates[button] = MouseButtonState.IsDown;
				else if (mouseButtonStates[button] == MouseButtonState.IsUp_First)
					mouseButtonStates[button] = MouseButtonState.IsUp;
				else if (mouseButtonStates[button] == MouseButtonState.IsUp_First_Click)
					mouseButtonStates[button] = MouseButtonState.IsUp;
			}
		foreach (KeyCode key in keyStates.Keys.ToArray())
		{
			if (keyStates[key] == KeyState.IsDown_First)
				keyStates[key] = KeyState.IsDown;
			else if (keyStates[key] == KeyState.IsUp_First)
				keyStates[key] = KeyState.IsUp;
			else if (keyStates[key] == KeyState.IsUp_First_Press)
				keyStates[key] = KeyState.IsUp;
		}

		// note; should figure out why this blocked clicks when it shouldn't have, and add it back
		//UnityUIHasMouseFocus = GUIUtility.hotControl != 0; // true when we're over a Unity GUI control
		//UnityUIHasMouseFocus = GUIUtility.hotControl != 0; // todo; fix this blocking keyboard input even when not having UnityUI-textbox focus
	}

	// static methods: setters
	// ==========

	public static void SetWebUIHasMouseFocus(bool hasFocus) { WebUIHasMouseFocus = hasFocus; }
	public static void SetWebUIHasKeyboardFocus(bool hasFocus) { WebUIHasKeyboardFocus = hasFocus; }

	// removed; we now rely entirely on the above focus-flags
	/*public static void OnMouseDown(int button) { mouseButtonStates[button] = MouseButtonState.IsDown_First; }
	public static void OnMouseUp(int button)
	{
		// temp; fix for "clicks-not-registering in standalone player" issue
		if (mouseButtonStates.ContainsKey(button) && mouseButtonStates[button] == MouseButtonState.IsUp_First_Click)
			return; // don't overwrite "click" event with basic "mouse up" event
		mouseButtonStates[button] = MouseButtonState.IsUp_First;
	}
	public static void OnMouseClick(int button) { mouseButtonStates[button] = MouseButtonState.IsUp_First_Click; }*/

	// static methods: getters
	// ==========

	// maybe make-so: this returns Vector2i instead of VVector2
	static VVector2 lastReturnedMousePosition = VVector2.Null; // maybe temp
	public static VVector2 mousePosition { get { return mouseStateFrozen && lastReturnedMousePosition != VVector2.Null ? lastReturnedMousePosition : lastReturnedMousePosition = Input.mousePosition.ToVVector3(false).ToVVector2(); } }
	//public static Vector3 GetMousePositionAsScreenPoint() { return new Vector3(mousePosition.x, mousePosition.y, 0); }

	public static bool GetIsMouseFocusTaken() { return UnityUIHasMouseFocus || WebUIHasMouseFocus; }
	public static bool GetIsKeyboardFocusTaken() { return UnityUIHasKeyboardFocus || WebUIHasKeyboardFocus; }

	public static MouseButtonState GetMouseButtonState(int button)
	{
		UpdateInputData();
		if (!mouseButtonStates.ContainsKey(button))
			mouseButtonStates[button] = MouseButtonState.IsUp;
		return mouseButtonStates[button];
	}
	public static KeyState GetKeyState(KeyCode key)
	{
		UpdateInputData();
		if (!keyStates.ContainsKey(key))
			keyStates[key] = KeyState.IsUp;
		return keyStates[key];
	}

	/// <summary>Returns true if the mouse button was pushed down during this frame, or was pushed down in an earlier frame and is being held down.</summary>
	public static bool GetMouseButton(int button) { return GetMouseButtonState(button) == MouseButtonState.IsDown_First || GetMouseButtonState(button) == MouseButtonState.IsDown; }
	public static bool GetMouseButtonDown(int button) { return GetMouseButtonState(button) == MouseButtonState.IsDown_First; }
	public static bool GetMouseButtonUp(int button) { return GetMouseButtonState(button) == MouseButtonState.IsUp_First || GetMouseButtonState(button) == MouseButtonState.IsUp_First_Click; }
	public static bool GetMouseButtonClick(int button) { return GetMouseButtonState(button) == MouseButtonState.IsUp_First_Click; }

	/*public static bool leftAltDown_override;
	public static bool rightAltDown_override;*/
	public static bool GetKey(KeyCode key) { return GetKeyState(key) == KeyState.IsDown; }
	public static bool GetKey(VKeyCode key)
	{
		if (key == VKeyCode.Control)
			return GetKeyState(KeyCode.LeftControl) == KeyState.IsDown || GetKeyState(KeyCode.RightControl) == KeyState.IsDown;
		if (key == VKeyCode.Shift)
			return GetKeyState(KeyCode.LeftShift) == KeyState.IsDown || GetKeyState(KeyCode.RightShift) == KeyState.IsDown;
		if (key == VKeyCode.Alt)
			//return GetKeyState(KeyCode.LeftAlt) == KeyState.IsDown || leftAltDown_override || GetKeyState(KeyCode.RightAlt) == KeyState.IsDown || rightAltDown_override;
			return GetKeyState(KeyCode.LeftAlt) == KeyState.IsDown || GetKeyState(KeyCode.RightAlt) == KeyState.IsDown;
		return false;
	}
	public static bool GetKeyDown(KeyCode key) { return GetKeyState(key) == KeyState.IsDown_First; }
	public static bool GetKeyUp(KeyCode key) { return GetKeyState(key) == KeyState.IsUp_First; }

	public static bool GetKey(string key) { return GetKey((KeyCode)Enum.Parse(typeof(KeyCode), key)); }
	public static bool GetKeyDown(string key) { return GetKeyDown((KeyCode)Enum.Parse(typeof (KeyCode), key)); }
	public static bool GetKeyUp(string key) { return GetKeyUp((KeyCode)Enum.Parse(typeof (KeyCode), key)); }

	public static bool GetKey_Any(params KeyCode[] keys)
	{
		foreach (KeyCode key in keys)
			if (GetKey(key))
				return true;
		return false;
	}
	public static bool GetKeyDown_Any(params KeyCode[] keys)
	{
		foreach (KeyCode key in keys)
			if (GetKeyDown(key))
				return true;
		return false;
	}
	public static bool GetKeyUp_Any(params KeyCode[] keys)
	{
		foreach (KeyCode key in keys)
			if (GetKeyUp(key))
				return true;
		return false;
	}
	
	// static methods
	// ==========

	// maybe temp
	static bool mouseStateFrozen;
	public static void ToggleMouseStateFrozen() { mouseStateFrozen = !mouseStateFrozen; }
}