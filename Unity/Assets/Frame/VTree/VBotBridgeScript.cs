using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SocketIO;
using VDFN;

public class VBotBridgeScript : MonoBehaviour {
	public static VBotBridgeScript main;
	public VBotBridgeScript() { main = this; }

	public SocketIOComponent socket;
    
	void Start() {
        GameObject go = GameObject.Find("SocketIO");
        socket = go.GetComponent<SocketIOComponent>();
		
        socket.On("CallMethod", CallMethod);

        Debug.Log("Game started");
	}

	bool wasConnected;
	void Update() {
		/*if (!socket.IsConnected)
			socket.Connect();*/
		if (socket.IsConnected && !wasConnected)
			Debug.Log("Connected to VBot");
		wasConnected = socket.IsConnected;
	}

    void CallMethod(SocketIOEvent obj) {
	    try {
		    var methodName = obj.data.GetField("methodName").str;
		    var argsJSON = obj.data.GetField("argsJSON").str;
		    var args = VConvert.FromVDF<List<object>>(argsJSON, new VDFLoadOptions().ForJSON());

		    Debug.Log("CallingMethod:" + methodName + " === argsLength:" + args.Count + " === argsVDF:" + argsJSON);

		    VBotBridge.CallMethod(methodName, args);
	    }
	    catch (Exception ex) { Debug.LogException(ex); }
    }
}