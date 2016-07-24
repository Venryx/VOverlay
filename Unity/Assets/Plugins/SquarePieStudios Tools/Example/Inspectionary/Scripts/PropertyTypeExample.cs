using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPStudios.Examples.Inspectionary {
    /// <summary>
    /// A behaviour to demonstrate the look and feel of most major proprety types supported by Unity
    /// </summary>
    public class PropertyTypeExample : MonoBehaviour {

        #region Public Inspectionary Declarations
        [Inspectionary]
        public AnimationCurveExampleInspectionary AnimationCurveExample;
        [Inspectionary]
        public BooleanExampleInspectionary BooleanExample;
        [Inspectionary]
        public BoundsExampleInspectionary BoundsExample;
        [Inspectionary]
        public CharacterExampleInspectionary CharacterExample;
        [Inspectionary]
        public ColorExampleInspectionary ColorExample;
        [Inspectionary]
        public EnumExampleInspectionary EnumExample;
        [Inspectionary]
        public FloatExampleInspectionary FloatExample;
        [Inspectionary]
        public IntegerExampleInspectionary IntegerExample;
        [Inspectionary]
        public ObjectReferenceExampleInspectionary ObjectReferenceExample;
        [Inspectionary]
        public RectExampleInspectionary RectExample;
        [Inspectionary]
        public StringExampleInspectionary StringExample;
        [Inspectionary]
        public Vector2ExampleInspectionary Vector2Example;
        [Inspectionary]
        public Vector3ExampleInspectionary Vector3Example;
        [Inspectionary]
        public QuaternionExampleInspectionary QuaternionExample;
        [Inspectionary]
        public Vector4ExampleInspectionary Vector4Example;
        #endregion
    }

    //An example enum for display purposes
    public enum PropertyTypeExampleEnum {
        ExampleValue1,
        ExampleValue2,
        ExampleValue3,
        ExampleValue4,
        ExampleValue5,
    }

    //Example Inspectionary declarations used to provide example Inspector presentation for most supported property types
    #region Inspectionary class declarations
    [Serializable]
    public class AnimationCurveExampleInspectionary : SerializableDictionary<AnimationCurve, AnimationCurve> { }
    [Serializable]
    public class BooleanExampleInspectionary : SerializableDictionary<bool, bool> { }
    [Serializable]
    public class BoundsExampleInspectionary : SerializableDictionary<Bounds, Bounds> { }
    [Serializable]
    public class CharacterExampleInspectionary : SerializableDictionary<char, char> { }
    [Serializable]
    public class ColorExampleInspectionary : SerializableDictionary<Color, Color> { }
    [Serializable]
    public class EnumExampleInspectionary : SerializableDictionary<PropertyTypeExampleEnum, PropertyTypeExampleEnum> { }
    [Serializable]
    public class FloatExampleInspectionary : SerializableDictionary<float, float> { }
    [Serializable]
    public class IntegerExampleInspectionary : SerializableDictionary<int, int> { }
    [Serializable]
    public class ObjectReferenceExampleInspectionary : SerializableDictionary<GameObject, GameObject> { }
    [Serializable]
    public class RectExampleInspectionary : SerializableDictionary<Rect, Rect> { }
    [Serializable]
    public class StringExampleInspectionary : SerializableDictionary<string, string> { }
    [Serializable]
    public class Vector2ExampleInspectionary : SerializableDictionary<Vector2, Vector2> { }
    [Serializable]
    public class Vector3ExampleInspectionary : SerializableDictionary<Vector3, Vector3> { }
    [Serializable]
    public class QuaternionExampleInspectionary : SerializableDictionary<Quaternion, Quaternion> { }
    [Serializable]
    public class Vector4ExampleInspectionary : SerializableDictionary<Vector4, Vector4> { }
    #endregion
}