using Rhinox.Grappler;
using Rhinox.Grappler.BoneManagement;
using Rhinox.Grappler.Recognition;


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GestureBasedTeleporting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoneManager _boneManager = null;
    [SerializeField] private BaseRecognitionService _recognitionService = null;
    [SerializeField] private Transform _objectToTeleport = null;
    private LineRenderer _lr = null;


    [Header("Settings")]
    [SerializeField] private Hand _handedness = Hand.Left;
    [SerializeField] private int _originBoneIdx = -1;

    private bool _isInitialised = false;
    private bool _isTeleporting = false;

    private RhinoxGesture _confirmTeleportGesture = new RhinoxGesture();

    private Transform _raycastOrigin = null;
    private Vector3 _teleportLocation = Vector3.zero;

    private void Start()
    {
        _boneManager.onIsInitialised.AddListener(Initialiase);
    }

    private void Initialiase()
    {
        // finding components
        _lr = this.GetComponent<LineRenderer>();

        // coupling events
        switch (_handedness)
        {
            case Hand.Left:
                _recognitionService.OnLeftHandGestureRecognised.AddListener(OnGestureRecognised);
                break;
            case Hand.Right:
                _recognitionService.OnRightHandGestureRecognised.AddListener(OnGestureRecognised);
                break;
        }

        // finding origin
        _raycastOrigin = _boneManager.GetRhinoxBones(_handedness)[_originBoneIdx].BoneTransform;

        _isInitialised = true;
    }

    private void Update()
    {
        if (!_isInitialised || !_isTeleporting)
            return;

        Debug.Log("Teleporting");
        RaycastHit hit;
        Physics.Raycast(_raycastOrigin.position, _raycastOrigin.forward, out hit, float.MaxValue);
        if (hit.collider != null)
        {
            _teleportLocation = hit.point;

            _lr.enabled = true; 
            Vector3[] linePositions = new Vector3[2];
            linePositions[0] = _raycastOrigin.position;
            linePositions[1] = _teleportLocation;
            _lr.SetPositions(linePositions);
        }
        else
            _lr.enabled = false;

    }
    private void OnGestureRecognised()
    {
        switch (_handedness)
        {
            case Hand.Left:
                if (_recognitionService._currentGestureLeftHand != _confirmTeleportGesture)
                    AbortTeleport();
                break;
            case Hand.Right:
                if (_recognitionService._currentGestureRightHand != _confirmTeleportGesture)
                    AbortTeleport();
                break;
        }
    }

    public void StartTeleport()
    {
        if (!_isInitialised)
            return;
        _isTeleporting = true;
    }
    public void ConfirmTeleport()
    {
        if (!_isInitialised)
            return;
        if (_confirmTeleportGesture.name == null)
        {
            switch (_handedness)
            {
                case Hand.Left:
                    _confirmTeleportGesture = _recognitionService._currentGestureLeftHand;
                    break;
                case Hand.Right:
                    _confirmTeleportGesture = _recognitionService._currentGestureRightHand;
                    break;
            }
        }

        if (!_isTeleporting)
            return;

        _objectToTeleport.transform.position = _teleportLocation;
    }

    private void AbortTeleport()
    {
        if (!_isInitialised || !_isTeleporting)
            return;

        _isTeleporting = false;
    }

}
