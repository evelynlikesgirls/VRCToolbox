#if UNITY_EDITOR
using UnityEngine;
using UnityEditor.Animations;

public class BlendtreeMono : MonoBehaviour, VRC.SDKBase.IEditorOnly
{
    public AnimatorController controller;

    public BlendTree rootTree;
    public BlendTree boolTree;
    public BlendTree intTree;
    public BlendTree partsTree;
    public BlendTree radialTree;
}
#endif