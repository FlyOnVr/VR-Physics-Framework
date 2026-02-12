using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Animations.Rigging;

public class VRPhysicsAndPosingWindow : EditorWindow
{
    // WINDOW / TABS

    private enum Tab
    {
        Setup,
        CustomHands,
        GrabbableCreator,
        FullBodyIK
    }

    private Tab currentTab;

    [MenuItem("Tools/VRPF/Setup")]
    public static void OpenSetup()
    {
        Open(Tab.Setup, "Setup");
    }

    [MenuItem("Tools/VRPF/Custom Hands")]
    public static void OpenHands()
    {
        Open(Tab.CustomHands, "Custom Hands");
    }

    [MenuItem("Tools/VRPF/Grabbable Creator")]
    public static void OpenGrabbable()
    {
        Open(Tab.GrabbableCreator, "Grabbable Creator");
    }

    [MenuItem("Tools/VRPF/Full Body")]
    public static void OpenFullBody()
    {
        Open(Tab.FullBodyIK, "Full Body IK");
    }

    [MenuItem("Tools/VRPF/Mirror Selected Pose")]
    public static void MirrorRightPose()
    {
        HandPosePlayer handPose = Selection.activeGameObject.GetComponent<HandPosePlayer>();
        handPose.MirrorPosing(handPose.LeftPose, handPose.HandPose);
    }

    private static void Open(Tab tab, string title)
    {
        var window = GetWindow<VRPhysicsAndPosingWindow>(title);
        window.currentTab = tab;
        window.minSize = new Vector2(600, 400);
        window.Show();
    }

    private void OnGUI()
    {
        switch (currentTab)
        {
            case Tab.Setup:
                DrawPhysicsTab();
                break;
            case Tab.CustomHands:
                DrawPosingTab();
                break;
            case Tab.GrabbableCreator:
                DrawGrabbableCreator();
                break;
            case Tab.FullBodyIK:
                DrawFullBodyIK();
                break;
        }
    }

    private void DrawPhysicsTab()
    {
        GUILayout.Label("VR Physics Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Creates layers, configures the physics collision matrix, physics and time settings.",
            MessageType.Info
        );

        if (GUILayout.Button("Run Full Setup", GUILayout.Height(45)))
        {
            CreateLayer(6, "Grabbable");
            CreateLayer(7, "Left Hand Physics");
            CreateLayer(8, "Right Hand Physics");
            CreateLayer(9, "Body");

            SetupCollisionMatrix();
            ApplyPhysicsSettings();
            ApplyTimeSettings();

            EditorUtility.DisplayDialog(
                "Setup Complete",
                "VR Physics Framework settings applied successfully.",
                "OK"
            );
        }
    }

    private void CreateLayer(int index, string name)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

        SerializedProperty layersProp = tagManager.FindProperty("layers");
        SerializedProperty slot = layersProp.GetArrayElementAtIndex(index);

        if (slot == null)
            return;

        if (!string.IsNullOrEmpty(slot.stringValue) && slot.stringValue != name)
        {
            Debug.LogWarning(
                $"[VRPF] Overwriting layer {index}: '{slot.stringValue}' → '{name}'");
        }

        slot.stringValue = name;
        tagManager.ApplyModifiedProperties();
    }

    private void SetupCollisionMatrix()
    {
        int def = LayerMask.NameToLayer("Default");
        int grab = LayerMask.NameToLayer("Grabbable");
        int lh = LayerMask.NameToLayer("Left Hand Physics");
        int rh = LayerMask.NameToLayer("Right Hand Physics");
        int body = LayerMask.NameToLayer("Body");

        int[] layers = { def, grab, lh, rh, body };

        // Reset
        foreach (int a in layers)
            foreach (int b in layers)
                Physics.IgnoreLayerCollision(a, b, false);

        // Hands
        Physics.IgnoreLayerCollision(lh, lh, true);
        Physics.IgnoreLayerCollision(rh, rh, true);
        Physics.IgnoreLayerCollision(lh, rh, true);

        // Body
        Physics.IgnoreLayerCollision(body, body, true);
        Physics.IgnoreLayerCollision(body, grab, true);
        Physics.IgnoreLayerCollision(body, lh, true);
        Physics.IgnoreLayerCollision(body, rh, true);
    }

    private void ApplyPhysicsSettings()
    {
        Physics.defaultSolverIterations = 25;
        Physics.defaultSolverVelocityIterations = 15;
        Physics.defaultContactOffset = 0.01f;
        Physics.sleepThreshold = 0.005f;
        Physics.defaultMaxDepenetrationVelocity = 10f;
    }

    private void ApplyTimeSettings()
    {
        Time.fixedDeltaTime = 0.01f;
        Time.maximumDeltaTime = 0.3333333f;
    }

    private Transform selectedRoot;
    private Vector2 scrollPos;
    private List<Transform> boneList = new();

    private HandBoneData.HandModelType handType =
        HandBoneData.HandModelType.Left;

    private readonly string[] fingerNames =
        { "thumb", "index", "middle", "ring", "pinky" };

    private readonly Dictionary<string, int> fingerBoneCounts = new()
    {
        { "thumb", 3 },
        { "index", 3 },
        { "middle", 3 },
        { "ring", 3 },
        { "pinky", 4 }
    };

    private void DrawPosingTab()
    {
        GUILayout.Label("Hand Posing Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        handType = (HandBoneData.HandModelType)EditorGUILayout.EnumPopup(
            "Hand Type", handType);

        selectedRoot = (Transform)EditorGUILayout.ObjectField(
            "Root Bone", selectedRoot, typeof(Transform), true);

        if (GUILayout.Button("Auto-Detect Hand Bones", GUILayout.Height(40)))
            AutoDetectBones();

        GUILayout.Space(10);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        foreach (var bone in boneList)
            EditorGUILayout.ObjectField(bone.name, bone, typeof(Transform), true);
        EditorGUILayout.EndScrollView();

        if (boneList.Count > 0 &&
            GUILayout.Button("Save Bone References", GUILayout.Height(40)))
        {
            SaveHandBoneData();
        }
    }

    private void AutoDetectBones()
    {
        boneList.Clear();
        if (!selectedRoot) return;

        foreach (var finger in fingerNames)
        {
            Transform root = FindFingerRoot(selectedRoot, finger);
            if (!root) continue;
            CollectFingerChain(root, fingerBoneCounts[finger]);
        }
    }

    private Transform FindFingerRoot(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>())
        {
            string n = t.name.ToLower();
            if (n.Contains(name) && n.EndsWith("1"))
                return t;
        }
        return null;
    }

    private void CollectFingerChain(Transform start, int count)
    {
        Transform current = start;
        for (int i = 0; i < count && current; i++)
        {
            boneList.Add(current);
            current = current.childCount > 0 ? current.GetChild(0) : null;
        }
    }

    private void SaveHandBoneData()
    {
        var data = selectedRoot.GetComponent<HandBoneData>() ??
                   selectedRoot.gameObject.AddComponent<HandBoneData>();

        data.handType = handType;
        data.root = selectedRoot;
        data.fingerBones = boneList.ToArray();

        EditorUtility.SetDirty(data);
    }

    private GameObject targetObject;
    private bool hasColliders = true;

    private void DrawGrabbableCreator()
    {
        GUILayout.Label("Grabbable Creator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        targetObject = (GameObject)EditorGUILayout.ObjectField(
            "Target", targetObject, typeof(GameObject), true);

        hasColliders = EditorGUILayout.Toggle("Has Colliders", hasColliders);

        if (GUILayout.Button("Make Grabbable", GUILayout.Height(35)))
        {
            if (!hasColliders && !targetObject.GetComponent<Collider>())
            {
                var col = targetObject.AddComponent<MeshCollider>();
                col.convex = true;
            }

            if (!targetObject.GetComponent<XRGrabInteractable>())
                targetObject.AddComponent<XRGrabInteractable>();
        }
    }

    private Animator animator;
    private Transform leftController;
    private Transform rightController;

    private void DrawFullBodyIK()
    {
        GUILayout.Label("Full Body IK Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        animator = (Animator)EditorGUILayout.ObjectField(
            "Humanoid Animator", animator, typeof(Animator), true);

        leftController = (Transform)EditorGUILayout.ObjectField(
            "Left Controller", leftController, typeof(Transform), true);

        rightController = (Transform)EditorGUILayout.ObjectField(
            "Right Controller", rightController, typeof(Transform), true);

        GUILayout.Space(10);

        if (GUILayout.Button("Setup Arm IK", GUILayout.Height(45)))
        {
            SetupArmIK(HumanBodyBones.LeftUpperArm,
                       HumanBodyBones.LeftLowerArm,
                       HumanBodyBones.LeftHand,
                       leftController,
                       "Left Arm IK");

            SetupArmIK(HumanBodyBones.RightUpperArm,
                       HumanBodyBones.RightLowerArm,
                       HumanBodyBones.RightHand,
                       rightController,
                       "Right Arm IK");
        }
    }

    private void SetupArmIK(
        HumanBodyBones upper,
        HumanBodyBones lower,
        HumanBodyBones hand,
        Transform target,
        string name)
    {
        if (!animator || !target) return;

        var rigBuilder = animator.GetComponent<RigBuilder>() ??
                         animator.gameObject.AddComponent<RigBuilder>();

        GameObject rigGO = new GameObject(name);
        rigGO.transform.SetParent(animator.transform, false);

        Rig rig = rigGO.AddComponent<Rig>();
        rigBuilder.layers.Add(new RigLayer(rig));

        var ikGO = new GameObject("TwoBoneIK");
        ikGO.transform.SetParent(rigGO.transform, false);

        var ik = ikGO.AddComponent<TwoBoneIKConstraint>();
        ik.data.root = animator.GetBoneTransform(upper);
        ik.data.mid = animator.GetBoneTransform(lower);
        ik.data.tip = animator.GetBoneTransform(hand);
        ik.data.target = target;

        var hintGO = new GameObject("Elbow Hint");
        hintGO.transform.SetParent(target, false);
        hintGO.transform.localPosition = new Vector3(0, 0, -0.2f);
        ik.data.hint = hintGO.transform;

        ik.weight = 1f;
    }
}