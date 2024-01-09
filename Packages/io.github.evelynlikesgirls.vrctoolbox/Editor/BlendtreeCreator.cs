using BlendtreeLib;
using UnityEditor.Animations;
using UnityEditor;
using UnityEngine;
using System.Linq;
using VRC.SDK3.Avatars.Components;

public class BlendtreeCreator : EditorWindow
{
    private VRCAvatarDescriptor avatar;
    private GameObject avatarRoot;
    private BlendtreeMono blendMono;

    [MenuItem("Test/Blendtree Creator")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<BlendtreeCreator>("Blendtree Creator");
    }
    public static void ShowWindow(VRCAvatarDescriptor _avatar)
    {   // Get avatar from Editor
        BlendtreeCreator window = EditorWindow.GetWindow<BlendtreeCreator>("Blendtree Creator");
        window.avatar = _avatar;
    }

    private void OnGUI()
    {
        avatar = EditorGUILayout.ObjectField("Avatar", avatar, typeof(VRCAvatarDescriptor), true) as VRCAvatarDescriptor;
        avatarRoot = avatar != null ? avatarRoot = avatar.gameObject : null;
        blendMono = avatarRoot?.GetComponent<BlendtreeMono>();

        if (blendMono == null || blendMono.controller == null)
        {
            GUI.enabled = avatar != null;
            if (GUILayout.Button("Create Blendtree"))
            {
                CreateController();
            }
        }
        else
        {
            if (GUILayout.Button("Open Boolean Tree Editor"))
            {
                BoolTreeEditor.ShowWindow(avatar);
            }
            if (GUILayout.Button("Open Integer Tree Editor"))
            {
                IntTreeEditor.ShowWindow(avatar);
            }
            if (GUILayout.Button("Open Radial Tree Editor"))
            {
                RadialTreeEditor.ShowWindow(avatar);
            }
            if (GUILayout.Button("Open Outfits Tree Editor"))
            {
                PartsTreeEditor.ShowWindow(avatar);
            } /* tbd
            GUILayout.Space(20);

            if (GUILayout.Button("Re-create Parameters"))
            {

            } */
        }
    }

    private void CreateController()
    {
        if (blendMono == null)
        {
            blendMono = (BlendtreeMono)avatarRoot.AddComponent(typeof(BlendtreeMono));
        }

        AnimatorController M_controller = blendMono.controller;
        if (M_controller == null)
        {
            M_controller = new AnimatorController
            {
                name = "Blendtree Controller"
            };

            M_controller.AddParameter(new AnimatorControllerParameter
            {
                name = "Weight",
                type = AnimatorControllerParameterType.Float,
                defaultFloat = 1f
            });

            if (!AssetDatabase.IsValidFolder("Assets/!SDK3"))
            {
                AssetDatabase.CreateFolder("Assets", "!SDK3");
            }

            AssetDatabase.CreateAsset(M_controller, $"Assets/!SDK3/{M_controller.name}.controller");

            blendMono.boolTree = Common.CreateBlendTree("Boolean Tree", BlendTreeType.Direct);
            blendMono.intTree = Common.CreateBlendTree("Integer Tree", BlendTreeType.Direct);
            blendMono.partsTree = Common.CreateBlendTree("Parts Tree", BlendTreeType.Simple1D);
            blendMono.radialTree = Common.CreateBlendTree("Radial Tree", BlendTreeType.Direct);
            blendMono.rootTree = CreateRootBlendTree("Root Tree");

            CreateLayer(M_controller);

            EditorUtility.SetDirty(M_controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            blendMono.controller = M_controller;
        }
    }

    private BlendTree CreateRootBlendTree(string name)
    {
        var tree = new BlendTree()
        {
            name = name,
            blendType = BlendTreeType.Direct,
            children = new ChildMotion[4]
            {
                new ChildMotion { motion = blendMono.boolTree, directBlendParameter = "Weight", timeScale = 1f },
                new ChildMotion { motion = blendMono.intTree, directBlendParameter = "Weight", timeScale = 1f },
                new ChildMotion { motion = blendMono.partsTree, directBlendParameter = "Weight", timeScale = 1f},
                new ChildMotion { motion = blendMono.radialTree, directBlendParameter = "Weight", timeScale = 1f}
            }
        };
        return tree;
    }

    private void CreateLayer(AnimatorController controller)
    {
        // Check if layer exists, if not, add new layer.
        AnimatorControllerLayer newLayer = controller.layers.FirstOrDefault(layer => layer.name == "Blendtree");
        if (newLayer == null)
        {
            newLayer = new AnimatorControllerLayer
            {
                name = "Blendtree",
                stateMachine = new AnimatorStateMachine
                {
                    name = "Blendtree",
                    hideFlags = HideFlags.HideInHierarchy
                }
            };
            if (AssetDatabase.GetAssetPath(controller) != "")
            {
                AssetDatabase.AddObjectToAsset(newLayer.stateMachine, AssetDatabase.GetAssetPath(controller));
            }
            newLayer.defaultWeight = 1f;

            newLayer.stateMachine.entryPosition = new Vector3(490, 0);
            newLayer.stateMachine.exitPosition = new Vector3(490, 50);
            newLayer.stateMachine.anyStatePosition = new Vector3(50, 0);

            AnimatorState newState = newLayer.stateMachine.AddState("Blendtree", new Vector3(250, 0));
            newState.motion = blendMono.rootTree;

            controller.AddLayer(newLayer);
        }
    }
}