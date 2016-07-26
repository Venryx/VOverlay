using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine.UI;
using VDFN;
using VTree.OverlayN;
using VTree.VOverlayN;
using VTree_Structures;
using Object = UnityEngine.Object;

namespace VTree {
	public static class VO {
		// exception to "_..." naming rule for-this-v-tree-only properties (though this is static, so it might not count/be part of that)
		// exception to VO children being only aliases for VOverlay children
		public static VOverlay main;
		public static void SubmitChange(Change change, ContextGroup? group = null) { VOverlay.SubmitChange(change, group); }

		// project constants
		public const double pixelsPerMeter = 30;
		public static VVector3 GetSize_WorldPixels() { return new VVector3(1920, 0, 1080); }
		public static VVector3 GetSize_WorldMeter() { return GetSize_WorldPixels() / pixelsPerMeter; } // 64, 0, 36

		public const float unitSize = 3;
	}

	[VDFType(popOutL1: true)] public class VOverlay : Node {
		// root static constructor
		static VOverlay() { VDFExtensions.Init(); }

		VOverlay s; // yes, exception to not-in-other-v-trees naming rule

		public VOverlay(VOverlayScript script) {
			VO.main = this; // so VO.main can be used even before it's done being constructed
			rootObjects = new List<GameObject>();
			foreach (GameObject obj in Object.FindObjectsOfType<GameObject>())
				if (obj.transform.parent == null)
					rootObjects.Add(obj);
			gameObject = script.gameObject;
			this.script = script;

			transformHelper = gameObject.GetChild("@General/@TransformHelper").transform;
			emojiAdderScript = gameObject.GetChild("@General/EmojiAdder").GetComponent<EmojiAdderScript>();
			chatInputBox = gameObject.GetChild("@General/UI/Canvas_L2/ChatInputBox").GetComponent<InputField>();
		}

		// maybe make-so: for props with NotToUI tag, default group is Local_CS (and therefore doesn't cause error)
		public static void SubmitChange(Change change, ContextGroup? group = null) {
			group = group ?? ContextGroup.Local_CSAndUI;

			if (group == ContextGroup.Local_CSAndUI || group == ContextGroup.Local_UI) // if going to send to UI
				if (change is Change_Set && change.propInfo.notTo && change.propInfo.notTo.js) // if data is marked as not-to-[js/ui], remove ui from send-to group
					if (group == ContextGroup.Local_CSAndUI)
						group = ContextGroup.Local_CS;
					else if (group == ContextGroup.Local_UI)
						return;

			//if (change.gameTime == -1) // if there aren't any conflicts possible (i.e. not a multiplayer-game change)
			change.PreApply();
			// if change originated in this context, and ui is part of target
			//if (change.obj.IsConnectedToMainTree() && change.sourceContext == "cs_" + Environment.UserName && (group == ContextGroup.Local_CSAndUI || group == ContextGroup.Local_UI))
			/*if (change.obj.IsChangeOrMessageSubmissionToUIAllowed() && change.sourceContext == "cs_" + Environment.UserName && (group == ContextGroup.Local_CSAndUI || group == ContextGroup.Local_UI))
				JSBridge.CallJS("BD.SubmitChange", change, ContextGroup.Local_UI); // send change submission to UI v-tree*/
			if (group == ContextGroup.Local_CSAndUI || group == ContextGroup.Local_CS)
				change.Apply();
		}

		public static Node GetNodeByNodePath(NodePath path, VDFNodePath vdfPath = null, VDFLoadOptions options = null) {
			var currentNodes = new List<object>(); // for debugging

			object currentNode = null; // (can also be a list or map)
			var currentNode_indexInVDFPath = -1;
			for (var i = 0; i < path.nodes.Count; i++) {
				var pathNode = path.nodes[i];
				if (pathNode.voRoot)
					currentNode = VO.main;
				else if (pathNode.vdfRoot)
					currentNode = vdfPath.rootNode.obj;
				else if (pathNode.nodeRoot)
					//currentNode = options.messages.OfType<DisconnectedRootReference>().First().node;
					currentNode = vdfPath.nodes.First(a=>a.obj is Node).obj;
				else if (pathNode.currentParent) {
					currentNode_indexInVDFPath = vdfPath.nodes.Count - 2; // skip the last node (the last node is the prop/reference, so can't also be the reference value)
					currentNode = vdfPath.nodes[currentNode_indexInVDFPath].obj;
				}
				else if (pathNode.moveUp) {
					if (currentNode_indexInVDFPath == -1) // if no current-parent path-node before this, pretend there was one
						currentNode_indexInVDFPath = vdfPath.nodes.Count - 2; // skip the last node (the last node is the prop/reference, so can't also be the reference value)
					//currentNode = vdfPath.nodes[currentNode_indexInVDFPath].obj;

					currentNode_indexInVDFPath--;
					currentNode = vdfPath.nodes[currentNode_indexInVDFPath].obj;
				}
				else
					/*if (currentNode is IList)
						currentNode = (currentNode as IList)[pathNode.listIndex];
					else if (currentNode is IDictionary)
						currentNode = (currentNode as IDictionary)[VConvert.FromVDF('"' + pathNode.mapKey_str + '"', currentNode.GetType().GetGenericArguments()[0])];
					else
						currentNode = currentNode.GetVTypeInfo().props[pathNode.prop_str].GetValue(currentNode);*/
					if (pathNode.listIndex != -1)
						currentNode = (currentNode as IList).HasIndex(pathNode.listIndex) ? (currentNode as IList)[pathNode.listIndex] : V.Break();
					else if (pathNode.mapKeyIndex != -1)
						currentNode = (currentNode as IDictionary).Keys.ToList_Object()[pathNode.mapKeyIndex];
					else if (pathNode.mapKey_str != null)
						if (pathNode.mapKey_str.Contains(">")) // if has type metadata, from NodePathNode constructor
							currentNode = (currentNode as IDictionary)[VConvert.FromVDF<object>(pathNode.mapKey_str)];
						else if (pathNode.mapKey_str.Contains("[embedded path]")) { // if embedded-path, from NodePathNode constructor
							var nodePath = NodePath.Deserialize(new VDFNode(pathNode.mapKey_str.Replace("[embedded path]", "").Replace("[fs]", "/")));
							var key = GetNodeByNodePath(nodePath, vdfPath);
							currentNode = (currentNode as IDictionary)[key];
						}
						else
							currentNode = (currentNode as IDictionary)[VConvert.FromVDF('"' + pathNode.mapKey_str + '"', currentNode.GetType().GetGenericArguments()[0])];
					else
						currentNode = currentNode.GetVTypeInfo().props[pathNode.prop_str].GetValue(currentNode);

				currentNodes.Add(currentNode);
				if (currentNode == null) // for debugging
					V.Break();
			}
			return (Node)currentNode;
		}
		static void Node_BroadcastMessage(NodePath path, ContextGroup group, string methodName, params object[] args) { GetNodeByNodePath(path).BroadcastMessage(group, methodName, args); }
		static object Node_SendMessage(NodePath path, ContextGroup group, string methodName, params object[] args) { return GetNodeByNodePath(path).SendMessage(group, methodName, args); }

		// subtrees
		// ==========

		[VDFProp(popOutL2: true)] public Maps maps;
		[VDFProp(popOutL2: true)] public Objects objects;
		[VDFProp(popOutL2: true)] public Race race;
		[VDFProp(popOutL2: true)] public Tower tower;

		// unity-linked properties
		// ==========

		[P(false)]
		public List<GameObject> rootObjects;
		[P(false)] public GameObject gameObject;
		[P(false)] public VOverlayScript script;
		[P(false)] public Transform transformHelper;
		[P(false)] public EmojiAdderScript emojiAdderScript;
		[P(false)] public InputField chatInputBox;

		// other properties
		// ==========

		[P(false)] public string rootFolderPath;

		public bool _initialFileSystemDataLoadDone;
		public bool _launched;
		public void Launch() {
			_launched = true;

			// this is the only page atm, so always start it out visible
			//race.a(a=>a.visible).set = true;
		}

		/*void MakePageVisible(string pageName) {
			/*var pageProp = this.GetVTypeInfo().props[propName];
			var pageValue = pageProp.GetValue(this);
			pageValue.GetVTypeInfo().*#/
			if (pageName == "race")
				s.race.a(a=>a.visible).set = true;
			else if (pageName == "tower")
				s.tower.a(a=>a.visible).set = true;
		}*/

		public void LoadDataAndLaunch() {
			var S = M.GetCurrentMethod().Profile_AllFrames();

			Action action = ()=> {
				S._____("load data");
				try {
					BroadcastMessage(ContextGroup.Local_CS, "LoadFileSystemData");
					BroadcastMessage(ContextGroup.Local_CSAndUI, "PostLoadFileSystemData");
					_initialFileSystemDataLoadDone = true;
				}
				catch (Exception ex) {
					var exCopy = ex;
					Debug.Log("Error loading file-system-data. Saving of file-system-data (when the program exits) has been disabled.");
					throw;
				}

				S._____("launch");
				VO.main.Launch();

				S._____(null);
			};

			if (FileManager.GetFile("DelayLoad").Exists || FileManager.GetFile("DelayLoad_Temp").Exists) // if mirror-settings-data flag-file, or custom-override flag-file
				V.WaitXSecondsThenCall(5, action);
			else
				action();
		}

		//bool _mainDataLoadSuccessful;
		public void SaveFileSystemData() {
			var vdf = VConvert.ToVDF(this, false, options: new VDFSaveOptionsV(toFile: true));
			File.WriteAllText(FileManager.GetFile("MainData.vdf").FullName, vdf);

			/*var vdfNode = VConvert.ToVDFNode(this, false, options: new VDFSaveOptionsV(toFile: true));
			vdfNode["objects"].move-to-before-maps();
			File.WriteAllText(FileManager.GetFile("MainData.vdf").FullName, vdf);*/

			/*if (settings.delayFileSystemDataLoad)
				File.WriteAllText(FileManager.GetFile("DelayLoad").FullName, "");
			else
				if (FileManager.GetFile("DelayLoad").Exists)
					FileManager.GetFile("DelayLoad").Delete();*/
		}
		void LoadFileSystemData() {
			var S = M.GetCurrentMethod().Profile_AllFrames();

			// finalize attachment for Node props not in VDF
			/*foreach (string propName in new[] {"scriptContext"}) {
				var prop = VTypeInfo.Get(GetType()).props[propName];
				SubmitChange(new Change_Set(this, prop, prop.GetValue(this)), ContextGroup.Local_CS);
			}*/

			S._____("parse into VDFNode");
			string vdf;
			if (FileManager.GetFile("MainData.vdf").Exists)
				vdf = File.ReadAllText(FileManager.GetFile("MainData.vdf").FullName);
			else
				vdf = "{}"; // probably todo: add default vdf, or default initialization code
			var options = VConvert.FinalizeFromVDFOptions(new VDFLoadOptions(new List<object> {"from file"}));
			var node = VDFLoader.ToVDFNode<VOverlay>(vdf, options);

			// reorder subnodes, so that lazy-loading doesn't need to be used as much
			/*var keys_newOrder = node.mapChildren.Keys.ToList();
			//keys_newOrder.Move(keys_newOrder.FirstOrDefault(a=>a == "modules"), keys_newOrder.Count); // move 'modules' to the end (so it's loaded second-last)
			keys_newOrder.Move(keys_newOrder.FirstOrDefault(a=>a == "maps"), keys_newOrder.Count); // move 'maps' to the end (so it's loaded last)
			node.mapChildren = node.mapChildren.OrderBy(a=>keys_newOrder.IndexOf(a.Key)).ToDictionary(a=>a.Key, a=>a.Value);*/

			S._____("apply data to C# main-tree");
			node.IntoObject(this, options);
			foreach (string propName in node.mapChildren.Keys) {
				var prop = VTypeInfo.Get(GetType()).props[propName];
				SubmitChange(new Change_Set(this, prop, prop.GetValue(this)), ContextGroup.Local_CS);
			}
			/*PreAdd(_parent, null, _pathNode);
			PostAdd(_parent, null, _pathNode);*/

			rootFolderPath = FileManager.root.VFullName();

			S._____(null);
		}

		public bool _finalizeShutdown;
		public static int _shutdownActionsLeft = 0;
		public void Shutdown(bool lastFrameOfShutdown = false) { // note; lastFrameOfShutdown will only be true if we are in the Editor (we need specific handling of it, as it leads to CoherentUI C++ errors, otherwise)
			//V.BroadcastGlobalMessage("PreFrameClose");
			VO.main.BroadcastMessage(ContextGroup.Local_CS, "PreFrameClose");

			if (lastFrameOfShutdown) { // if we are in Editor
				// we have to call this now, before this last frame ends
				//CoherentUISystem.Instance.PreFrameClose_Late(); //V.BroadcastGlobalMessage("PreFrameClose_Late");
				VO.main.BroadcastMessage(ContextGroup.Local_CS, "PreFrameClose_Late");

				// for editor (e.g. BiomeDefense_EditorHelper)
				//forEditor_beforeQuit();
			}
			else // otherwise, use the standard shutdown path
				script.StartCoroutine(Shutdown_Internal());
		}
		IEnumerator Shutdown_Internal() {
			float startTime = Time.realtimeSinceStartup;
			while (_shutdownActionsLeft > 0 && Time.realtimeSinceStartup - startTime < 1) // if we have shutdown actions left, and we haven't gone over the 1 second timeout
				yield return null;
			//CoherentUISystem.Instance.PreFrameClose_Late(); //V.BroadcastGlobalMessage("PreFrameClose_Late");
			VO.main.BroadcastMessage(ContextGroup.Local_CS, "PreFrameClose_Late");

			_finalizeShutdown = true;
			if (Application.isEditor)
				Application.Quit();
			else
				System.Diagnostics.Process.GetCurrentProcess().Kill();
		}

		// methods: events
		// ==========

		void PreFrameClose() {
			//if (Application.isEditor) // if in editor, program will close at end of frame anyway (before any UI-to-CS responses), so don't bother telling UI that program is closing
			//	return;
			_shutdownActionsLeft++;
			SendMessage(ContextGroup.Local_UI, "PreViewClose", (Action)(()=> { _shutdownActionsLeft--; }));
		}
		void PreFrameClose_Late() {
			//if (_mainDataLoadSuccessful)
			if (_initialFileSystemDataLoadDone)
				SaveFileSystemData();
		}

		// methods: helpers
		// ==========

		public void WaitXSecondsThenCall(float seconds, Action call) { script.StartCoroutine(WaitXSecondsThenCall_CR(seconds, call)); }
		IEnumerator WaitXSecondsThenCall_CR(float seconds, Action call) {
			yield return new WaitForSeconds(seconds);
			call();
		}
		public void WaitXFramesThenCall(int frames, Action call) { script.StartCoroutine(WaitXFramesThenCall_CR(frames, call)); }
		IEnumerator WaitXFramesThenCall_CR(int frames, Action call) {
			for (int i = 0; i < frames; i++)
				yield return null; // wait one frame
			call();
		}

		public void Update() {
			if (!_initialFileSystemDataLoadDone) // delay-file-system-data-load must be enabled
				return;

			// log VDebug.Log messages from last frame
			foreach (var message in VDebug.frameLogMessages)
				Debug.Log(message);
			VDebug.frameLogMessages.Clear();

			// calls hook-in functions from lots of objects (not just own PreViewFrameTick() method)
			//CallMethod("PreViewFrameTick", viewFrame + 1);
			Profiler_LastDataFrame.PreViewFrameTick_Outer();
			CallMethod(null, "PreViewFrameTick", viewFrame + 1); // use this full/final overload, so extra call-stack-entry doesn't get made

			if (VInput.GetKeyDown(KeyCode.Return)) {
				/*if (!chatInputBox.gameObject.activeSelf) {
					chatInputBox.gameObject.SetActive(true);
					chatInputBox.ActivateInputField();
				}
				else {
					var message = GameObject.Find("ChatInputBox").GetComponent<InputField>().text;
					GameObject.Find("ChatInputBox").GetComponent<InputField>().text = "";
					VBotBridge.OnLocalChatMessageAdded(message);
					chatInputBox.gameObject.SetActive(false);
				}*/
				var message = GameObject.Find("ChatInputBox").GetComponent<InputField>().text;
				GameObject.Find("ChatInputBox").GetComponent<InputField>().text = "";
				VBotBridge.OnLocalChatMessageAdded(message);
			}
			chatInputBox.gameObject.GetComponent<Image>().enabled = chatInputBox.gameObject.GetComponent<InputField>().text.Length > 0;
			chatInputBox.ActivateInputField();
		}

		public float _timeAtEndOfLastFrame;
		public void LateUpdate() {
			s.a(a=>a.viewFrame).set_self = viewFrame + 1; // only send to CS side, for now

			V.WaitForEndOfFrameThenCall(()=>_timeAtEndOfLastFrame = Time.realtimeSinceStartup);
		}

		[NotTo("file")] public int viewFrame;
		Vector3 _camPosLastFrame;
		Vector3 _camRotLastFrame;
		Rect _camRectLastFrame;
		//[P(false)] public bool camViewChangedThisFrame;
		[P(false)] public bool camViewOutflowsNeedUpdate;
		[IgnoreStartData] void viewFrame_PostSet() {
			var cam = Camera.main;
			var camPos = cam.transform.position;
			var camRot = cam.transform.eulerAngles;
			var camRect = cam.pixelRect;
			if (camPos != _camPosLastFrame || camRot != _camRotLastFrame || camRect != _camRectLastFrame) {
				_camPosLastFrame = camPos;
				_camRotLastFrame = camRot;
				_camRectLastFrame = camRect;
				camViewOutflowsNeedUpdate = true;
			}
			else
				camViewOutflowsNeedUpdate = false;

			//BroadcastMessage(ContextGroup.Local_CS, "PostViewFrameTick", viewFrame);
			CallMethod("PostViewFrameTick", viewFrame); // calls hook-in functions from lots of objects (not just own PostViewFrameTick() method)
			//Profiler_LastViewFrame.PostViewFrameTick(); // now turn off profiler, so it doesn't include later data
			Profiler_LastDataFrame.PostViewFrameTick_Outer(); // now turn off data-frame profiler, so it doesn't include later data
		}
		//public double GetViewTime() { return (double)viewFrame / Time.fp; } // data-frame-based version of Time.time // minor: maybe todo: implement well, and do it the same way for match.GetDataTime()
		
		public float GetTimeTakenThisFrame() { return Time.realtimeSinceStartup - _timeAtEndOfLastFrame; }
	}
}