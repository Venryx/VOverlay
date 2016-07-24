using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPStudios.Examples.Inspectionary {
    /// <summary>
    /// Example of how to use a dictionary with simple data structures
    /// </summary>
    public abstract class SimpleExample : MonoBehaviour {
        //Always include the inspectionary attribute for custom inspectionaries to enable the custom inspector drawer display
        [Inspectionary("The Cube", "The Material")]
        public SimpleExampleInspectionary TheInspectionary;
        private void OnEnable() {
            //Use it just like a normal dictionary
            foreach(KeyValuePair<GameObject, Color> kvp in TheInspectionary) {
                kvp.Key.GetComponent<Renderer>().material.color = kvp.Value;
            }
        }
    }

    //Simple dictionary example
    [Serializable]
    public class SimpleExampleInspectionary : SerializableDictionary<GameObject, Color> { }
}