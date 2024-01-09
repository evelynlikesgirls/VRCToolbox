using UnityEditor.Animations;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace BlendtreeLib
{
    public static class Common
    {
        public static BlendTree CreateBlendTree(string name, BlendTreeType type)
        {   // Creates a blendtree and returns it
            var tree = new BlendTree()
            {
                hideFlags = HideFlags.None,
                name = name,
                blendType = type,
                blendParameter = name,
                useAutomaticThresholds = false
            };
            return tree;
        }
        public static void AddChildToTree(BlendTree parent, Motion child, bool sort)
        {
            // Get current children as list
            List<ChildMotion> currentChildren = parent.children.ToList();

            // add new tree to list
            currentChildren.Add(new ChildMotion
            {
                motion = child,
                directBlendParameter = "Weight",
                timeScale = 1
            });
            if (sort)
            {
                // Sort list, convert to array, and apply to targetTree
                parent.children = currentChildren.OrderBy(childMotion => childMotion.motion.name).ToArray();
            }
            else
            {
                parent.children = currentChildren.ToArray();
            }

        }
        public static void RefreshList(BlendTree blendTree, List<AnimationClip> animationClipsList)
        {   // Refresh the display list
            foreach (var childMotion in blendTree.children)
            {
                AnimationClip animationClip = childMotion.motion as AnimationClip;
                if (animationClip != null && animationClip.name != "EmptyAnimation")
                {
                    animationClipsList.Add(animationClip);
                }
            }
        }
        public static void CreateParameter(BlendtreeMono blendMono, string name)
        {
            // Check if parameter exists, and if it does not, create it.
            AnimatorControllerParameter parameter = blendMono.controller.parameters.FirstOrDefault(p => p.name == name && p.type == AnimatorControllerParameterType.Float);
            if (parameter == null)
            {
                blendMono.controller.AddParameter(name, AnimatorControllerParameterType.Float);
            }
        }

        public static AnimationClip EmptyAnimation()
        {
            AnimationClip emptyClip = (AnimationClip)AssetDatabase.LoadAssetAtPath("Assets/EmptyAnimation.anim", typeof(AnimationClip));

            if (emptyClip == null)
            {
                emptyClip = new AnimationClip();
                AssetDatabase.CreateAsset(emptyClip, "Assets/EmptyAnimation.anim");
            }
            return emptyClip;
        }
    }
    public static class Boolean
    {
        public static void UpdateBlendtree(BlendtreeMono blendMono, BlendTree blendTree, List<AnimationClip> animationClipsList)
        {   // Apply changes to the blendtree

            // Remove null animations
            animationClipsList = animationClipsList.Where(clip => clip != null).ToList();
            // Sort alphabetically
            animationClipsList = animationClipsList.OrderBy(clip => clip.name).ToList();

            List<ChildMotion> motionList = new List<ChildMotion>();

            foreach (var animationClip in animationClipsList)
            {   // Add animations to empty tree
                if (animationClip == null)
                    continue;

                Common.CreateParameter(blendMono, animationClip.name);
                // Create new ChildMotion with the animation and add it to motionlist
                motionList.Add(new ChildMotion
                {
                    motion = animationClip,
                    directBlendParameter = animationClip.name,
                    timeScale = 1
                });
            }
            // Convert motionList to an array and set the children of boolTree to motionList.
            blendTree.children = motionList.ToArray();
        }
    }
    public static class Integer
    {
        public static void UpdateBlendtree(BlendtreeMono blendMono, BlendTree blendTree, List<AnimationClip> animationClipsList)
        {   // Apply changes to the blendtree

            // Remove null animations
            animationClipsList = animationClipsList.Where(clip => clip != null).ToList();

            List<ChildMotion> motionList = new List<ChildMotion>();

            motionList.Add(new ChildMotion
            {
                motion = Common.EmptyAnimation(),
                timeScale = 1,
                threshold = 0
            });


            int i = 1;
            foreach (var animationClip in animationClipsList)
            {   // Add animations to empty tree
                if (animationClip == null || animationClip.name == "EmptyAnimation")
                    continue;
                // Create new ChildMotion with the animation and add it to motionlist
                motionList.Add(new ChildMotion
                {
                    motion = animationClip,
                    timeScale = 1,
                    threshold = i
                });
                i++;
            }
            // Convert motionList to an array and set the children of boolTree to motionList.
            blendTree.children = motionList.ToArray();
        }
    }
}