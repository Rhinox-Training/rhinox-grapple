using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Rhinox.Grappler.BoneManagement;

public class ProgramManager : MonoBehaviour
{
    private void Start()
    {
        var temp = BoneManager.Instance;
    }

    private void Update()
    {
        var temp = BoneManager.Instance.GetRhinoxBones(Hand.Left);
    }
}
