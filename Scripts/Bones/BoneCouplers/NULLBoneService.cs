using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.Grappler.BoneManagement
{
	public sealed class NULLBoneService : IBoneService
	{
		public void Initialise(GameObject controllerParent) { return; }
		public bool GetIsInitialised() { return false; }
		public List<RhinoxBone> GetBones(Hand hand) { return null; }

        public bool TryLoadBones() {  return false; }

        public bool GetAreBonesLoaded() {  return false;  }
    }
}