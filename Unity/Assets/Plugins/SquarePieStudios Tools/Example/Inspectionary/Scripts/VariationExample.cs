using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPStudios.Examples.Inspectionary {
    /// <summary>
    /// Example to show all of the different visual variations of the Inspectionary.
    /// </summary>
    public class VariationExample : MonoBehaviour {
        [Inspectionary]
        public int InvalidDictionaryExample; //Will only display an error
        [Inspectionary]
        public SimpleInspectionary SimpleDictionary; //Will display all entries on a single line each
        [Inspectionary]
        public ComplexKeyInspectionary ComplexKeyDictionary; //Keys will be expandable
        [Inspectionary]
        public ComplexValueInspectionary ComplexValueDictionary; //Values will be expandable
        [Inspectionary]
        public ComplexKeyValuePairInspectionary ComplexKeyValuePairDictionary; //Keys and Values will be expandable
    }

    [Serializable]
    public class SimpleInspectionary : SerializableDictionary<int, int> { }                                  //Simple Key, Simple Value
    [Serializable]
    public class ComplexKeyInspectionary : SerializableDictionary<ComplexClass, int> { }                     //Complex Key, Simple Value
    [Serializable]
    public class ComplexValueInspectionary : SerializableDictionary<int, ComplexClass> { }                   //Simple Key, Complex Value
    [Serializable]
    public class ComplexKeyValuePairInspectionary : SerializableDictionary<ComplexClass, ComplexClass> { }   //Complex Key, Complex Value

    //Example classes
    [Serializable]
    public class ComplexClass {
        public int IntegerValue;
        public Vector3 Vector3Value;
        public SubComplexClass SubClass;
    }
    [Serializable]
    public class SubComplexClass {
        public int SubIntValue;
        public Vector3 SubVec3Value;
    }
}