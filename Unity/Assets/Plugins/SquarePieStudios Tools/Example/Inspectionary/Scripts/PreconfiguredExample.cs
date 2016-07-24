using SPStudios.SerializableDictionary;
using System.Collections.Generic;
using UnityEngine;

namespace SPStudios.Examples.Inspectionary {
    /// <summary>
    /// Example of how to use the predefined inspectionaries
    /// </summary>
    public class PreconfiguredExample : MonoBehaviour {
        //Determines if a cube will be displayed or not
        //Does not need the [Inspectionary] tag because it has been preconfigured
        public GameObjectBoolDictionary DisplayCubes = new GameObjectBoolDictionary();
        //Use [Inspectionary] attribute to apply custom key/value labels
        [Inspectionary("Custom Key", "Custom Value")]
        public GameObjectVector3Dictionary CubePositions = new GameObjectVector3Dictionary();
        private void OnEnable() {
            //Use it just like a normal dictionary
            IEnumerator<GameObject> e = DisplayCubes.Keys.GetEnumerator();
            while(e.MoveNext()) {
                GameObject key = e.Current;
                key.SetActive(DisplayCubes[key]);
                key.transform.position = CubePositions[key];
            }
        }
    }
}