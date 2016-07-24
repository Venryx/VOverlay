using SPStudios; //Don't forget to include this line
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPStudios.Examples.Inspectionary {
    //Different example types
    public enum InspectionaryExampleType {
        Complex,
        Enum,
        Preconfigured,
    }
    //Custom made dictionary for ExampleType to corresponding object
    [Serializable]
    public class ExampleToRunInspectionary : SerializableDictionary<InspectionaryExampleType, GameObject> { }

    //Default settings for individual cubes
    [Serializable]
    public class DefaultCubeState {
        public Vector3 Pos;
        public Quaternion Rot;
        public Color Col;
    }
    //Used to reset the cubes between examples
    [Serializable]
    public class DefaultCubeStateInspectionary : SerializableDictionary<GameObject, DefaultCubeState> { }

    /// <summary>
    /// Scene handler for demonstrating 
    /// </summary>
    public class ExampleSceneHandler : MonoBehaviour {
        [Inspectionary]
        public DefaultCubeStateInspectionary DefaultStates;

        //A dictionary to decide which test to run based on the selected example type
        [Inspectionary("Example Type", "Test to run")]
        public ExampleToRunInspectionary ExampleDictionary;
        //Determines what example is run
        public InspectionaryExampleType StartType = InspectionaryExampleType.Complex;
        public List<CubeController> Cubes = new List<CubeController>();

        private void Start() {
            SetGameObjectStates(StartType);
        }
        
        private Rect DrawRect = new Rect(0, 0, 70, 30);
        private void OnGUI() {
            Rect buttonPos = new Rect(DrawRect);
            if (GUI.Button(buttonPos, "Complex")) {
                SetGameObjectStates(InspectionaryExampleType.Complex);
            }
            buttonPos.x += buttonPos.width;
            if (GUI.Button(buttonPos, "Enum")) {
                SetGameObjectStates(InspectionaryExampleType.Enum);
            }
            buttonPos.x += buttonPos.width;
            if (GUI.Button(buttonPos, "PreCon")) {
                SetGameObjectStates(InspectionaryExampleType.Preconfigured);
            }
            buttonPos.y += buttonPos.height;
            buttonPos.x = DrawRect.x;
            if (GUI.Button(buttonPos, "Horizontal")) {
                SetGameObjectStates(CubeControllerSetting.Horizontal);
            }
            buttonPos.x += buttonPos.width;
            if (GUI.Button(buttonPos, "Vertical")) {
                SetGameObjectStates(CubeControllerSetting.Vertical);
            }
            buttonPos.x += buttonPos.width;
            if (GUI.Button(buttonPos, "Triangle")) {
                SetGameObjectStates(CubeControllerSetting.Triangle);
            }
            buttonPos.x += buttonPos.width;
        }

        private void SetGameObjectStates(CubeControllerSetting type) {
            ResetCubeStates();
            for (int i = 0; i < Cubes.Count; i++) {
                Cubes[i].AdjustCube(type);
            }
        }

        private void SetGameObjectStates(InspectionaryExampleType exampleType) {
            ResetCubeStates();
            //Deactivates all unselected gameobjects, leaving only the selected object active
            foreach (KeyValuePair<InspectionaryExampleType, GameObject> kvp in ExampleDictionary) {
                kvp.Value.SetActive(false);
                kvp.Value.SetActive(kvp.Key == exampleType);
            }
        }

        private void ResetCubeStates() {
            foreach (KeyValuePair<GameObject, DefaultCubeState> kvp in DefaultStates) {
                kvp.Key.SetActive(true);
                kvp.Key.transform.position = kvp.Value.Pos;
                kvp.Key.transform.rotation = kvp.Value.Rot;
                kvp.Key.GetComponent<Renderer>().material.color = kvp.Value.Col;
            }
        }
    }
}