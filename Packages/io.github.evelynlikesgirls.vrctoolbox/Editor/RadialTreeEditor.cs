using BlendtreeLib;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using System.Collections.Generic;

public class RadialTreeEditor : EditorWindow
{
    private VRCAvatarDescriptor avatar;
    private GameObject avatarRoot;

    bool updatedList = false;
    private BlendtreeMono blendMono;

    [SerializeField]
    private List<AnimationClip> animationClipsList = new List<AnimationClip>();
    private Vector2 scrollPosition;

    [MenuItem("Test/Radial Tree Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<RadialTreeEditor>("Radial Tree Editor");
    }
    public static void ShowWindow(VRCAvatarDescriptor _avatar)
    {   // Get avatar from Editor
        RadialTreeEditor window = EditorWindow.GetWindow<RadialTreeEditor>("Radial Tree Editor");
        window.avatar = _avatar;
    }

    private SerializedObject serializedObject;
    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
    }

    public void OnGUI()
    {
        avatar = EditorGUILayout.ObjectField("Avatar", avatar, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
        avatarRoot = avatar != null ? avatarRoot = avatar.gameObject : null;
        blendMono = avatarRoot?.GetComponent<BlendtreeMono>();
        if (blendMono == null)
        {
            EditorGUILayout.LabelField("Select your avatar or create the base tree");
            if (GUILayout.Button("Open Blendtree Creator"))
            {
                BlendtreeCreator.ShowWindow(avatar);
            }
        }
        else
        {   // Display a list of animations already in the blendtree, then allow the user to update the list
            if (GUILayout.Button("Revert Changes / Refresh"))
            {
                updatedList = false;
                animationClipsList.Clear();
            }

            if (updatedList == false)
            {
                Common.RefreshList(blendMono.radialTree, animationClipsList);
                updatedList = true;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("animationClipsList"), true);
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Update"))
            {   // Create parameters, and update the blendtree.
                Boolean.UpdateBlendtree(blendMono, blendMono.radialTree, animationClipsList);

                // Save assets
                EditorUtility.SetDirty(blendMono.controller);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}