using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using System.ComponentModel.Design;
using UnityEngine.UI;

[RequireComponent(typeof(XRGrabInteractable))]
public class HandPosePlayer : MonoBehaviour
{
    [Header("Target Pose")]
    public HandBoneData HandPose;
    public HandBoneData LeftPose;
    [Header("Mirror Posing (Not working yet)")]
    public bool MirrorPose = false;
    [Header("Pose Settings")]
    public float PoseTransit = 0.2f;

    private Vector3 startHandPose;
    private Vector3 endHandPose;
    private Quaternion startHandRotation;
    private Quaternion endHandRotation;

    private Quaternion[] startFingerRotations;
    private Quaternion[] endFingerRotations;

    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);

        if (HandPose != null)
            HandPose.gameObject.SetActive(false);
        if (LeftPose != null)
            LeftPose.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
        grabInteractable.selectExited.RemoveListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactorObject is XRBaseControllerInteractor controllerInteractor)
        {
            var hand = controllerInteractor.GetComponentInChildren<HandBoneData>();
            if (hand == null)
                return;

            if (hand.animator != null)
                hand.animator.enabled = false;

            if (hand.handType == HandBoneData.HandModelType.Right)
            {
                CacheHandData(hand, HandPose);
            }
            else
            {
                CacheHandData(hand, LeftPose);
            }

                //ApplyHandData(hand, endHandPose, endHandRotation, endFingerRotations);
                StartCoroutine(ApplyHandDataR(hand, endHandPose, endHandRotation, endFingerRotations, startHandPose, startHandRotation, startFingerRotations));
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (args.interactorObject is XRBaseControllerInteractor controllerInteractor)
        {
            var hand = controllerInteractor.GetComponentInChildren<HandBoneData>();
            if (hand == null)
                return;

            if (hand.animator != null)
                hand.animator.enabled = true;

            StartCoroutine(ApplyHandDataR(hand, startHandPose, startHandRotation, startFingerRotations, endHandPose, endHandRotation, endFingerRotations));
            //ApplyHandData(hand, startHandPose, startHandRotation, startFingerRotations);
        }
    }

    private void CacheHandData(HandBoneData fromHand, HandBoneData toHand)
    {
        startHandPose = fromHand.root.localPosition;
        endHandPose = toHand.root.localPosition;

        startHandRotation = fromHand.root.localRotation;
        endHandRotation = toHand.root.localRotation;

        startFingerRotations = new Quaternion[fromHand.fingerBones.Length];
        endFingerRotations = new Quaternion[fromHand.fingerBones.Length];

        for (int i = 0; i < fromHand.fingerBones.Length; i++)
        {
            startFingerRotations[i] = fromHand.fingerBones[i].localRotation;
            endFingerRotations[i] = toHand.fingerBones[i].localRotation;
        }
    }

    private void ApplyHandData(HandBoneData hand, Vector3 position, Quaternion rotation, Quaternion[] fingerRotations)
    {
        hand.root.localPosition = position;
        hand.root.localRotation = rotation;

        for (int i = 0; i < fingerRotations.Length; i++)
        {
            hand.fingerBones[i].localRotation = fingerRotations[i];
        }
    }

    public IEnumerator ApplyHandDataR(HandBoneData hand, Vector3 position, Quaternion rotation, Quaternion[] fingerRotations, Vector3 startPosition, Quaternion startrotation, Quaternion[] startfingerRotations)
    {
        float t = 0;
        while (t < PoseTransit)
        {
            Vector3 pos = Vector3.Lerp(startPosition, position, t / PoseTransit);
            Quaternion rot = Quaternion.Slerp(startrotation, rotation, t / PoseTransit);
            hand.root.localPosition = pos;
            hand.root.localRotation = rot;

            for (int i = 0; i < fingerRotations.Length; i++)
            {
                hand.fingerBones[i].localRotation = Quaternion.Lerp(startfingerRotations[i], fingerRotations[i], t / PoseTransit);
            }
            t += Time.deltaTime;
            yield return null;
        }
    }



    /*public static void MirrorRightPose()
    {
        HandPosePlayer handpose = Object.FindFirstObjectByType<HandPosePlayer>();
        handpose.MirrorPosing(handpose.LeftPose, handpose.HandPose);
    }*/

    public void MirrorPosing(HandBoneData useMirroredHand, HandBoneData handToMirror)
    {
        Vector3 mirroredPosition = useMirroredHand.root.localPosition;
        mirroredPosition.x *= -1;

        Quaternion mirroredRotation = useMirroredHand.root.localRotation;
        mirroredRotation.y *= -1;
        mirroredRotation.z *= -1;

        handToMirror.root.localPosition = mirroredPosition;
        handToMirror.root.localRotation = mirroredRotation;

        for (int i = 0; i < useMirroredHand.fingerBones.Length; i++)
        {
            handToMirror.fingerBones[i].localRotation = useMirroredHand.fingerBones[i].localRotation;
        }
    }
}