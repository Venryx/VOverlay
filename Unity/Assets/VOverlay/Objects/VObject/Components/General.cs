using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking.Match;
using VDFN;
using VTree.BiomeDefenseN.MapsN.MapN;
using VTree_Structures;

namespace VTree.BiomeDefenseN.ObjectsN.ObjectN.ComponentsN {
	// structure
	// ==========

	// unit
	// ==========

	// other
	// ==========

	public class IsProjectile : VComponent {
		IsProjectile typeComp;
		[VDFPreDeserialize] IsProjectile() {}

		// set in type vdf-file
		[NotTo("obj")] public string effect;

		// set by projectile-firer
		[NotTo("js"), ByPath] public VObject firer;
		public double speed;
		[NotTo("js"), ByPath] public VObject target;
		[NotTo("js")] public VVector3 targetPos;

		[NotTo("js")] public GameObject effectObj;
		[NotTo("js")] int reachedTargetPos_dataFrame = -1;

		//void _PostAdd_EM1()
		/*new void _PostAdd() {
			base._PostAdd();*/
		void _PostAdd() {
			if (obj == null || obj.type == null)
				return;
			if (typeComp.effect != null) {
				effectObj = V.Clone(VO.main.script.effects.First(a=>a.name == typeComp.effect), obj.transform.Position.ToVector3(), Quaternion.identity);
				effectObj.transform.parent = obj.map.obj.GetChild("Effects").transform;
				effectObj.transform.LookAt(targetPos.ToVector3());
				effectObj.GetComponent<Rigidbody>().AddForce(effectObj.transform.forward * (float)speed, ForceMode.VelocityChange);
				//typeComp.effect.GetComponent<ProjectileScript>().impactNormal = hit.normal;
				effectObj.GetComponent<ProjectileScript>().impactNormal = -effectObj.transform.forward;
			}
		}
		public void PostDataFrameTick() {
			/*if (target.Parent == null) // if target was destroyed, destroy ourself
				//return;
				DestroyObj();*/

			if (reachedTargetPos_dataFrame == -1) {
				var movementLeft = targetPos - obj.transform.Position;
				var direction = movementLeft.normalized;
				var speedPerFrame = speed / obj.map.match.dataFramesPerSecond;
				VVector3 newPos = targetPos;
				if (movementLeft.magnitudeSquared > speedPerFrame.ToPower(2))
					newPos = obj.transform.Position + (direction * speedPerFrame);
				else
					reachedTargetPos_dataFrame = obj.map.match.dataFrame;
				obj.transform.Position = newPos;
				obj.transform.Rotation = VVector4.LookRotation(movementLeft, VVector3.up);
			}

			if (reachedTargetPos_dataFrame != -1)
				if (reachedTargetPos_dataFrame == obj.map.match.dataFrame) {
					// for now, consider projectile to always hit its target
					//if (target.transform.Intersects(obj)) {
					if (target.attachPoint != null)
						target.ReceiveAttack(obj.owner);
					DestroyObj();
				}
				else if (obj.map.match.dataFrame >= reachedTargetPos_dataFrame + (3 * obj.map.match.dataFramesPerSecond))
					DestroyObj();
		}
		//public void PreViewFrameTick() {}

		void DestroyObj() {
			obj.map.a(a=>a.projectiles).remove = obj;
			if (effectObj)
				effectObj.GetComponent<ProjectileScript>().ShowImpactThenDestroy();
		}
	}
}