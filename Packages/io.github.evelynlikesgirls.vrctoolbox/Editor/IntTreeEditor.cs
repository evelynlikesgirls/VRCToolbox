using BlendtreeLib;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using VRC.SDK3.Avatars.Components;
using UnityEditor.Animations;
using System.Linq;

public class IntTreeEditor : EditorWindow
{
    private GameObject avatarRoot;
    private VRCAvatarDescriptor avatar;
    private BlendtreeMono blendMono;

    string integerName = "Outfits";
    BlendTree targetTree;

    bool updatedList = false;
    [SerializeField]
    private List<AnimationClip> animationClipsList = new List<AnimationClip>();
    private Vector2 scrollPosition;

    [MenuItem("Test/Integer Tree Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<IntTreeEditor>("Integer Tree Editor");
    }
    public static void ShowWindow(VRCAvatarDescriptor _avatar)
    {   // Get avatar from Editor
        IntTreeEditor window = EditorWindow.GetWindow<IntTreeEditor>("Integer Tree Editor");
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
            integerName = EditorGUILayout.TextField("Integer Name", integerName);

            // select existing tree
            var targetChildMotion = blendMono.intTree.children
                .OfType<ChildMotion>()
                .FirstOrDefault(childMotion => childMotion.motion != null && childMotion.motion.name == integerName);
            targetTree = (BlendTree)targetChildMotion.motion;


            if (targetTree == null)
            {   // If not found, ask if to create new tree
                GUILayout.Label("No blend tree found with name \"" + integerName + "\"");
                if (GUILayout.Button("Create new blendtree"))
                {
                    targetTree = Common.CreateBlendTree(integerName, BlendTreeType.Simple1D);
                    Common.AddChildToTree(blendMono.intTree, targetTree, false);
                    Common.CreateParameter(blendMono, integerName);
                    EditorUtility.SetDirty(blendMono.controller);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            else
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
                    Integer.UpdateBlendtree(blendMono, targetTree, animationClipsList);

                    // Save assets
                    EditorUtility.SetDirty(blendMono.controller);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

            }
        }
    }
}