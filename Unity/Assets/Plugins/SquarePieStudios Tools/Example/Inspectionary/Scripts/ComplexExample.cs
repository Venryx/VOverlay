using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPStudios.Examples.Inspectionary {
    /// <summary>
    /// Example of how to use a dictionary with complex data structures
    /// </summary>
    public class ComplexExample : MonoBehaviour {
        //Always include the inspectionary attribute for custom inspectionaries to enable the custom inspector drawer display
        [Inspectionary("The Cube", "Cube Settings")] //First entry is a custom label for keys, second entry for values
        public ComplexExampleInspectionary TheInspectionary;
        private void OnEnable() {
            //Use it just like a normal dictionary
            foreach(KeyValuePair<GameObject, ComplexExampleClass> kvp in TheInspectionary) {
                kvp.Key.GetComponent<Renderer>().material.color = kvp.Value.CubeColor;
                kvp.Key.transform.position = kvp.Value.CubeSettings.CubePosition;
                kvp.Key.transform.rotation = kvp.Value.CubeSettings.CubeRotation;
            }
        }
    }

    //Complex class that includes simple attributes and complex attributes
    [Serializable]
    public class ComplexExampleClass {
        public Color CubeColor;
        public CubePositionSettings CubeSettings;
    }

    //Small class for cube position settings.
    [Serializable]
    public class CubePositionSettings {
        public Vector3 CubePosition;
        public Quaternion CubeRotation;
    }
    //Complex dictionary example
    [Serializable]
    public class ComplexExampleInspectionary : SerializableDictionary<GameObject, ComplexExampleClass> { }
}