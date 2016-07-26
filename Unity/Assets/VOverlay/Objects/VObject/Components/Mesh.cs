using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking.Match;
using VDFN;
using VectorStructExtensions;
using VTree.BiomeDefenseN.MapsN;
using VTree.BiomeDefenseN.MapsN.MapN;
using VTree.BiomeDefenseN.ObjectsN.ObjectN;
using VTree_Structures;

namespace VTree.VOverlayN.ObjectsN.ObjectN.ComponentsN {
	public class MeshComp : VComponent {
		MeshComp typeComp;
		[VDFPreDeserialize] public MeshComp() {}
	}
}