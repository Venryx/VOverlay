using UnityEngine;
using System;
using System.Collections.Generic;

namespace SPStudios.Examples.Inspectionary {
    public enum CubeControllerSetting {
        Horizontal,
        Vertical,
        Triangle,
    }
    //Dictionaries created to map CubeControllerSetting to different settings like the color or position
    [Serializable]
    public class CubeSettingColor : SerializableDictionary<CubeControllerSetting, Color> { }
    [Serializable]
    public class CubeSettingVec3 : SerializableDictionary<CubeControllerSetting, Vector3> { }
    [Serializable]
    public class CubeSettingQuat : SerializableDictionary<CubeControllerSetting, Quaternion> { }

    public class CubeController : MonoBehaviour {
        //A mapping of cube setting to the color this cube should display when that setting is selected
        [Inspectionary("Setting", "CubeColor")]
        public CubeSettingColor Colors;

        //A mapping of cube setting to positions this cube should be set to when a given setting is selected
        [Inspectionary("Setting", "Position")]
        public CubeSettingVec3 Positions;

        //A mapping of cube setting to rotations this cube should be set to when a given setting is selected
        [Inspectionary("Setting", "Rotation")]
        public CubeSettingQuat Rotations;

        //A mapping of cube setting to scale this cube should be set to when a given setting is selected
        [Inspectionary("Setting", "Scale")]
        public CubeSettingVec3 Scales;

        /// <summary>
        /// Sets the cube to the requested setting
        /// </summary>
        public void AdjustCube(CubeControllerSetting setting) {
            GetComponent<Renderer>().material.color = Colors[setting];
            transform.position = Positions[setting];
            transform.rotation = Rotations[setting];
            transform.localScale = Scales[setting];
        }
    }
}