using BlendtreeLib;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using VRC.SDK3.Avatars.Components;
using UnityEditor.Animations;
using System.Linq;
public class PartsTreeEditor : EditorWindow
{
    private VRCAvatarDescriptor avatar;
    private GameObject avatarRoot;
    private BlendtreeMono blendMono;

    private bool updatedList = false;

    private BlendTree integerTree;
    private BlendTree targetTree;
    private string partsName = "Outfits";
    private string partsSubName = "Default";

    [SerializeField]
    private List<AnimationClip> animationClipsList = new List<AnimationClip>();
    private Vector2 scrollPosition;

    [MenuItem("Test/Outfits Tree Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<PartsTreeEditor>("Outfits Tree Editor");
    }
    public static void ShowWindow(VRCAvatarDescriptor _avatar)
    {   // Get avatar from Editor
        PartsTreeEditor window = EditorWindow.GetWindow<PartsTreeEditor>("Outfits Tree Editor");
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
            EditorGUILayout.LabelField("Select your avatar and/or create the base tree");
            if (GUILayout.Button("Open Blendtree Creator"))
            {
                BlendtreeCreator.ShowWindow(avatar);
            }
        }
        else
        {
            partsName = EditorGUILayout.TextField("Outfit Integer Name", partsName);

            // select existing trees
            var targetChildMotion = blendMono.intTree.children
                .OfType<ChildMotion>()
                .FirstOrDefault(childMotion => childMotion.motion != null && childMotion.motion.name == partsName);
            integerTree = (BlendTree)targetChildMotion.motion;
            targetChildMotion = blendMono.partsTree.children
                .OfType<ChildMotion>()
                .FirstOrDefault(childMotion => childMotion.motion != null && childMotion.motion.name == partsName);
            targetTree = (BlendTree)targetChildMotion.motion;

            if (integerTree == null)
            {   // If not found, ask if to create new tree
                GUILayout.Label("No outfit blendtree found with name \"" + partsName + "\"");
                GUILayout.Label("Please make one with Integer Editor");
            }
            else if (integerTree != null && targetTree != null)
            {
                GUILayout.Label("Build Outfits Layer");
            }

            else
            {
                partsSubName = EditorGUILayout.TextField("Outfit Name", partsSubName);
            }
            if (targetTree == null && targetTree != null)
            {
                GUILayout.Label("No outfit tree found with name \"" + partsSubName + "\"");
                if (GUILayout.Button("Create new outfit blendtree"))
                {
                    targetTree = Common.CreateBlendTree(partsSubName, BlendTreeType.Direct);

                    Common.AddChildToTree(targetTree, targetTree, false);

                    Common.CreateParameter(blendMono, partsName);
                    // save assets
                    EditorUtility.SetDirty(blendMono.controller);
                    AssetDatabase.SaveAssets();
                }
            }
            else if (targetTree != null)
            {
                if (GUILayout.Button("Revert Changes / Refresh"))
                {
                    updatedList = false;
                    animationClipsList.Clear();
                }
                if (updatedList == false)
                {
                    Common.RefreshList(targetTree, animationClipsList);
                    updatedList = true;
                }
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                serializedObject.Update();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("animationClipsList"), true);
                serializedObject.ApplyModifiedProperties();

                EditorGUILayout.EndScrollView();

                if (GUILayout.Button("Apply Changes"))
                {   // Create parameters, and update the blendtree.
                    Boolean.UpdateBlendtree(blendMono, targetTree, animationClipsList);

                    // Save assets
                    EditorUtility.SetDirty(blendMono.controller);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }
    }


}