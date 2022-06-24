using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Rhinox.Grappler.EventManagement
{
    public sealed class GrappleEvent : UnityEvent<GameObject, GameObject, BoneManagement.Hand>
    {}

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
        public GrappleEvent OnTouch { get; private set; } = new GrappleEvent();

        /// <summary>
        /// OnUnTouched is called whenever a physics solution has ended a collision with another object
        /// </summary>
        public GrappleEvent OnUnTouched { get; private set; } = new GrappleEvent();

        /// <summary>
        /// onGrab is called whenever a physics solution has initiated a grabbing behaviour with another object
        /// </summary>
        public GrappleEvent OnGrab { get; private set; } = new GrappleEvent();

        /// <summary>
        /// onDrop is called whenever a physics solution has stopped a grabbing behaviour with another object
        /// </summary>
        public GrappleEvent OnDrop { get; private set; } = new GrappleEvent();

    }
}
