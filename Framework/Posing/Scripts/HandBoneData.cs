using UnityEngine;

public class HandBoneData : MonoBehaviour
{
    public enum HandModelType { Left, Right }

    public HandModelType handType;
    public Transform root;
    public Animator animator;
    public Transform[] fingerBones;

}