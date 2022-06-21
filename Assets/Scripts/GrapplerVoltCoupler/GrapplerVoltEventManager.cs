#if USING_VOLT
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rhinox.VOLT;
using Rhinox.VOLT.Interaction;
using Rhinox.VOLT.XR.UnityXR;
using UnityEngine.Events;
using System;

namespace Rhinox.Grappler.EventManagement.VOLT
{
    public class GrapplerVoltEventManager : MonoBehaviour
    {
        private void Awake()
        {
            GrapplerEventManager.Instance.OnTouch.AddListener(OnTouch);
            GrapplerEventManager.Instance.OnUnTouched.AddListener(OnUnTouched);
            GrapplerEventManager.Instance.OnGrab.AddListener(OnGrab);
            GrapplerEventManager.Instance.OnDrop.AddListener(OnDrop); 
        }


        private void OnTouch(GameObject sender, GameObject receiver, BoneManagement.Hand handedness)
        {
            VoltInteractable interactable = receiver.GetComponent<VoltInteractable>();
            if (interactable == null)
                return;

            if (handedness == BoneManagement.Hand.Left)
                interactable.NotifyTouched(PlayerManager.Instance.ActivePlayer.GetLeftHand());
            else interactable.NotifyTouched(PlayerManager.Instance.ActivePlayer.GetRightHand());
        }

        private void OnUnTouched(GameObject sender, GameObject receiver, BoneManagement.Hand handedness)
        {
            VoltInteractable interactable = receiver.GetComponent<VoltInteractable>();
            if (interactable == null)
                return;

            if (handedness == BoneManagement.Hand.Left)
                interactable.NotifyUntouched(PlayerManager.Instance.ActivePlayer.GetLeftHand());
            else interactable.NotifyUntouched(PlayerManager.Instance.ActivePlayer.GetRightHand());
        }


        private void OnGrab(GameObject sender, GameObject receiver, BoneManagement.Hand handedness)
        {
            VoltInteractable interactable = receiver.GetComponent<VoltInteractable>();
            if (interactable == null)
                return;

            if (handedness == BoneManagement.Hand.Left)
                interactable.NotifyGrabbed(PlayerManager.Instance.ActivePlayer.GetLeftHand());
            else interactable.NotifyGrabbed(PlayerManager.Instance.ActivePlayer.GetRightHand());
        }

        private void OnDrop(GameObject sender, GameObject receiver, BoneManagement.Hand handedness)
        {
            VoltInteractable interactable = receiver.GetComponent<VoltInteractable>();
            if (interactable == null)
                return;

            if (handedness == BoneManagement.Hand.Left)
                interactable.NotifyDropped(PlayerManager.Instance.ActivePlayer.GetLeftHand());
            else interactable.NotifyDropped(PlayerManager.Instance.ActivePlayer.GetRightHand());
        }

    }
}
#endif