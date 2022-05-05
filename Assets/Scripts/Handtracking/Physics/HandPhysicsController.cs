using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Rhinox.Grappler
{
    public class HandPhysicsController : MonoBehaviour
    {
        private List<HandPhysics.IPhysicsService> _physicsServices = new List<HandPhysics.IPhysicsService>();
        [SerializeField] private Recognition.BaseRecognitionService _recognitionService = null;
        private BoneManagement.BoneManager _boneManager = null;

        [SerializeField] private LayerMask _handLayer = 0;

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
            _boneManager.SetBoneConvertorService(new BoneManagement.UnityXRBoneService());

        }

        private void SetupPhysicServices()
        {
            _physicsServices.Add(new HandPhysics.ContactPointBasedPhysics());

            foreach (var physicsService in _physicsServices)
            {
                physicsService.SetHandLayer(_handLayer);
                physicsService.Initialise(_boneManager);
            }

            _physicsServices[0].SetEnabled(true);

            IsInitialised = true;
        }

        private void SetupRecognitionService()
        {
            _recognitionService = new Recognition.OculusRecognitionService();
            _recognitionService.Initialise(_boneManager);
            _recognitionService.SetEnabled(true);
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

            foreach (var physicsService in _physicsServices)
            {
                if(physicsService.GetIsInitialised())
                    physicsService.Update();
            }
        }

    }

}
