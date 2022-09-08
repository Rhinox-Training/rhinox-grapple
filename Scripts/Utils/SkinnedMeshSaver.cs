using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


/// <summary>
/// This is a tool to create baked meshes from skinned meshes to create them into prefabs.
/// This script is mainly used to create a baked mesh from a hand position to use later and is not intended for actual runtime use
/// </summary>
/// 
#if UNITY_EDITOR
public class SkinnedMeshSaver : MonoBehaviour
{
    [SerializeField] private bool _bakeMesh = false;
    private void Update()
    {
        if (_bakeMesh)
        {
            var skinnedMeshes = this.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (var skin in skinnedMeshes)
            {
                Mesh meshToSave = new Mesh();
                skin.BakeMesh(meshToSave);
                AssetDatabase.CreateAsset(meshToSave, "Assets/" + skin.gameObject.name + "_BAKED.mesh");
                Debug.Log("SkinnedMeshSaver::Update() -> Mesh asset saved as: " + "Assets/" + skin.gameObject.name + "_BAKED.mesh");
            }
            _bakeMesh = false;
        }
    }

}

#endif
