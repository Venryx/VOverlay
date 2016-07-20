using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Assertions.Must;
using VDFN;
using VectorStructExtensions;
using VTree;
using VTree.BiomeDefenseN.ObjectsN;
using VTree.BiomeDefenseN.ObjectsN.ObjectN;
using VTree.BiomeDefenseN.ObjectsN.ObjectN.ComponentsN;
using VTree_Structures;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public enum ObjectType {
	None,
	Plant,
	Structure,
	Unit,
	Projectile
}
public enum Age {
	None,
	Rodent,
	Grazer,
	Hunter,
	Avian,
	Machine
}
public enum UnitClass {
	None,
	Insects,
	Fish_and_Amphibians,
	Shoreline_Creatures,
	Large_Creatures,
	Hoofed_Creatures,
	Climbing_Creatures,
	Birds,
	Agile_Creatures,
	Pack_Hunters,
	Burrow_Creatures
}

public enum Stance {
	NoAction,
	StandGround
}

public class VObjectBiome {
	[P] public double perSquareKM;

	// needed for V.Equals to realize props holding these are equal, when comparing type and instance, and therefore not to serialize them
	public override bool Equals(object other) {
		var obj = other as VObjectBiome;
		if (obj == null)
			return false;
		return perSquareKM == obj.perSquareKM;
	}
}

public enum ModelSource {
	None,
	Unity, // (only model-source that needs vdf entry, rather than just data files in folder)
	/*VModel,
	CacheL1,
	CacheL2,
	CacheL3*/
}
public enum ShaderType {
	None,
	AFS,
	AFS_SingleSided,
	TreeCreator
}
public class VObject_Model {
	[P] public ModelSource source;
	[P] public string name;
	[P] public VObject_Model_Wind wind;
	[P] public ShaderType shaderOverride;
}
public class VObject_Model_Wind {
	[VDFPreDeserialize] VObject_Model_Wind() {}
	[P, D(1)] public double hStart;
	[P, D(1)] public double hEnd = 1;
	[P, D(1)] public double vStart;
	[P, D(1)] public double vEnd = 1;
	[P, D(1)] public double strength = 1;
}

public class VLOD : Node {
	[P, D] public double? minScreenHeight;
	[P] public double vertexKeepPercent;
}

public class ObjectScript : MonoBehaviour {
	public VObject obj;
}