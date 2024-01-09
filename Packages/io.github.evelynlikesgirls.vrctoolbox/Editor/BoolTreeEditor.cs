using BlendtreeLib;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using VRC.SDK3.Avatars.Components;

public class BoolTreeEditor : EditorWindow
{
    private VRCAvatarDescriptor avatar;
    private GameObject avatarRoot;
    bool updatedList = false;
    private BlendtreeMono blendMono;

    [SerializeField]
    private List<AnimationClip> animationClipsList = new List<AnimationClip>();
    private Vector2 scrollPosition;

    [MenuItem("Test/Boolean Tree Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<BoolTreeEditor>("Boolean Tree Editor");
    }
    public static void ShowWindow(VRCAvatarDescriptor _avatar)
    {   // Get avatar from Editor
        BoolTreeEditor window = EditorWindow.GetWindow<BoolTreeEditor>("Boolean Tree Editor");
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
        {   // make sure the avatar has the mono component
            EditorGUILayout.LabelField("Select your avatar or create the base tree");
            if (GUILayout.Button("Open Blendtree Creator"))
            {
                if (avatar == null)
                    BlendtreeCreator.ShowWindow();
                else if (avatar != null)
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
                Common.RefreshList(blendMono.boolTree, animationClipsList);
                updatedList = true;
            }
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("animationClipsList"), true);
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Apply Changes"))
            {   // Create parameters, and update the blendtree.
                Boolean.UpdateBlendtree(blendMono, blendMono.boolTree, animationClipsList);

                // Save assets
                EditorUtility.SetDirty(blendMono.controller);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}