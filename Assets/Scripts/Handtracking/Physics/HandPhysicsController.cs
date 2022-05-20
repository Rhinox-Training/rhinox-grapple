using Rhinox.Grappler.BoneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Rhinox.Grappler
{
    public class HandPhysicsController : MonoBehaviour
    {
        [Header("Physics Resolvers")]
        public List<HandPhysics.BasePhysicsService> PhysicsServices;

        [Header("Mesh baking")]
        public MeshBaking.BaseMeshBakingService MeshBakingService;

        [Header("Material Management")]
        public MaterialManagement.BaseMaterialService MaterialService;

        [Header("Recognition system")]
        public Recognition.BaseRecognitionService RecognitionService;


        private BoneManagement.BoneManager _boneManager = null;

        [Header("Settings")]
        [SerializeField] private LayerMask _handLayer = 0;
        [SerializeField] public Material OpaqueHandMaterial = null;
        [SerializeField] public Material SeethroughHandMaterial = null;
        [SerializeField] public Material BakedHandMaterial = null;

        public bool IsInitialised { get; private set; } = false;        
        private void Awake()
        {
            SetupLayerCollisions();
            SetupBoneManager();
        }

        private void SetupBoneManager()
        {
            _boneManager = this.GetComponent<BoneManagement.BoneManager>();
           
            _boneManager.onIsInitialised.AddListener(SetupPhysicServices);
            _boneManager.onIsInitialised.AddListener(SetupRecognitionService);
            _boneManager.onIsInitialised.AddListener(SetupMeshBaking);
            _boneManager.onIsInitialised.AddListener(SetupMaterialManagement);

            _boneManager.SetBoneConvertorService(new BoneManagement.UnityXRBoneService());

        }

        private void SetupPhysicServices()
        {
            foreach (var physicsService in PhysicsServices)
            {
                physicsService.SetHandLayer(_handLayer);
                physicsService.Initialise(_boneManager, this);
                physicsService.SetEnabled(false, BoneManagement.Hand.Both);

            }
            PhysicsServices[1].SetEnabled(true, BoneManagement.Hand.Both);
            IsInitialised = true;
        }

        private void SetupRecognitionService()
        {
            RecognitionService = this.gameObject.GetComponent<Recognition.BaseRecognitionService>();
            RecognitionService.Initialise(_boneManager);
            RecognitionService.SetEnabled(true);
        }

        private void SetupMeshBaking()
        {
            MeshBakingService.Initialise(_boneManager,this);
        }

        private void SetupMaterialManagement()
        {
            MaterialService = new MaterialManagement.OculusMaterialService();
            MaterialService.Initialise(_boneManager,this);
        }

        private void SetupLayerCollisions()
        {
            int layerNumber = 0;
            int layer = _handLayer.value;
            while (layer > 0)
            {
                layer = layer >> 1;
                layerNumber++;
            }

            Physics.IgnoreLayerCollision(layerNumber - 1, layerNumber - 1);

            SetLayerRecursive(this.gameObject, layerNumber - 1);

        }

        /// <summary>
        /// Sets the layer of the gameobject and all its children to the layer number given
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="layer"></param>
        private void SetLayerRecursive(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.layer = layer;

                Transform _HasChildren = child.GetComponentInChildren<Transform>();
                if (_HasChildren != null)
                    SetLayerRecursive(child.gameObject, layer);
            }
        }

        private void Update()
        {
            if (!IsInitialised)
                return;

            foreach (var physicsService in PhysicsServices)
            {
                if(physicsService.GetIsInitialised())
                    physicsService.ManualUpdate();
            }
        }

        public void EnableLeftHandService(int serviceIdx)
        {
            if (serviceIdx + 1 > PhysicsServices.Count)
            {
                Debug.LogError("Rhinox.Grappler.HandPhysicsController.EnableService() : Given service index does not exist");
                return;
            }

            Debug.Log("Rhinox.Grappler.HandPhysicsController.EnableService() : Enabling physics service: " + serviceIdx);

            foreach (var physicsService in PhysicsServices)
            {
                physicsService.SetEnabled(false, BoneManagement.Hand.Left);
            }
            PhysicsServices[serviceIdx].SetEnabled(true, BoneManagement.Hand.Left);

        }

        public void EnableRightHandService(int serviceIdx)
        {
            if (serviceIdx + 1 > PhysicsServices.Count)
            {
                Debug.LogError("Rhinox.Grappler.HandPhysicsController.EnableService() : Given service index does not exist");
                return;
            }

            Debug.Log("Rhinox.Grappler.HandPhysicsController.EnableService() : Enabling physics service: " + serviceIdx);

            foreach (var physicsService in PhysicsServices)
            {
                physicsService.SetEnabled(false, BoneManagement.Hand.Right);
            }
            PhysicsServices[serviceIdx].SetEnabled(true, BoneManagement.Hand.Right);

        }
    
        public void SetLeftHandMaterial(int materialIdx)
        {
            switch (materialIdx)
            {
                case 0:
                    MaterialService.SetHandMaterial(Hand.Left, OpaqueHandMaterial);
                    break;
                case 1:
                    MaterialService.SetHandMaterial(Hand.Left, SeethroughHandMaterial);
                    break;
                case 2:
                    MaterialService.SetHandMaterial(Hand.Left, BakedHandMaterial);
                    break;
                default:
                    MaterialService.SetHandMaterial(Hand.Left, OpaqueHandMaterial);
                    break;
            }
        }

        public void SetRightHandMaterial(int materialIdx)
        {
            switch (materialIdx)
            {
                case 0:
                    MaterialService.SetHandMaterial(Hand.Right, OpaqueHandMaterial);
                    break;
                case 1:
                    MaterialService.SetHandMaterial(Hand.Right, SeethroughHandMaterial);
                    break;
                case 2:
                    MaterialService.SetHandMaterial(Hand.Right, BakedHandMaterial);
                    break;
                default:
                    MaterialService.SetHandMaterial(Hand.Right, OpaqueHandMaterial);
                    break;
            }
        }

    }
}
