using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Rhinox.Grappler.EventManagement
{
    public class GrapplerEventManager : MonoBehaviour
    {
        private static GrapplerEventManager _instance;
        public static GrapplerEventManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[GENERATED]_GrapplerEventManager");
                    _instance = go.AddComponent<GrapplerEventManager>();
                }
                return _instance;
            }
        }
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        /// <summary>
        /// onTouch is called whenever a physics solution has started a collision with another object
        /// </summary>
        public UnityEvent<GameObject, GameObject, BoneManagement.Hand> OnTouch { get; private set; } = new UnityEvent<GameObject, GameObject, BoneManagement.Hand>();

        /// <summary>
        /// OnUnTouched is called whenever a physics solution has ended a collision with another object
        /// </summary>
        public UnityEvent<GameObject, GameObject, BoneManagement.Hand> OnUnTouched { get; private set; } = new UnityEvent<GameObject, GameObject, BoneManagement.Hand>();

        /// <summary>
        /// onGrab is called whenever a physics solution has initiated a grabbing behaviour with another object
        /// </summary>
        public UnityEvent<GameObject, GameObject, BoneManagement.Hand> OnGrab { get; private set; } = new UnityEvent<GameObject, GameObject, BoneManagement.Hand>();

        /// <summary>
        /// onDrop is called whenever a physics solution has stopped a grabbing behaviour with another object
        /// </summary>
        public UnityEvent<GameObject, GameObject, BoneManagement.Hand> OnDrop { get; private set; } = new UnityEvent<GameObject, GameObject, BoneManagement.Hand>();

    }
}
