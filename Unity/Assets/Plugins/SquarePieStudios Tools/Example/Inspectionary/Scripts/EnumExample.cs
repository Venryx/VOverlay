using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPStudios.Examples.Inspectionary {
    /// <summary>
    /// Example of how to use the inspectionaries with enums
    /// </summary>
    public class EnumExample : MonoBehaviour {
        //Always include the inspectionary attribute for custom inspectionaries to enable the custom inspector drawer display
        [Inspectionary("Cube Type", "Cube Object")] //First entry is a custom label for keys, second entry for values
        public CubeEnumExampleInspectionary CubeDefinitions;
        [Inspectionary("The Cube", "Cube Settings")] //First entry is a custom label for keys, second entry for values
        public EnumComplexExampleInspectionary CubeSettings;
        private void OnEnable() {
            //Use it just like a normal dictionary
            foreach(KeyValuePair<GameObject, ExampleCubeEnum> kvp in CubeDefinitions) {
                kvp.Key.GetComponent<Renderer>().material.color = CubeSettings[kvp.Value].CubeColor;
                kvp.Key.transform.position = CubeSettings[kvp.Value].CubeSettings.CubePosition;
                kvp.Key.transform.rotation = CubeSettings[kvp.Value].CubeSettings.CubeRotation;
            }
        }
    }
    //Example enum
    public enum ExampleCubeEnum {
        Left,
        Center,
        Right,
    }

    //Simple enum dictionary example
    [Serializable]
    public class CubeEnumExampleInspectionary : SerializableDictionary<GameObject, ExampleCubeEnum> { }

    //Complex enum dictionary example
    [Serializable]
    public class EnumComplexExampleInspectionary : SerializableDictionary<ExampleCubeEnum, ComplexExampleClass> { }
}