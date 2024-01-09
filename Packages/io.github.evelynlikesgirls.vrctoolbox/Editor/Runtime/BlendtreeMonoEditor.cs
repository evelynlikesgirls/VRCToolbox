using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.Components;

[CustomEditor(typeof(BlendtreeMono))]
public class BlendtreeMonoEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        BlendtreeMono mono = (BlendtreeMono)target;
        // Create an IMGUIContainer to host the UI elements
        var container = new IMGUIContainer(() =>
        {
            // Start the GUILayout area
            GUILayout.BeginVertical();

            if (mono.controller == null)
            {
                GUILayout.Label("DO NOT MANUALLY ADD THIS TO YOUR AVATAR");
                GUILayout.Label("You might end up breaking your animators!");
                GUILayout.Label("You should only let this script touch the controller it creates!");
                GUILayout.Label("Remove this script and open the Blendtree Creator to begin");
                GUILayout.Space(20);
            }


            // Add your UI elements here
            if (GUILayout.Button("Open Blendtree Creator"))
            {
                OpenBlendtreeCreator();
            }

            // End the GUILayout area
            GUILayout.EndVertical();
        });

        return container;
    }

    private void OpenBlendtreeCreator()
    {
        BlendtreeMono mono = (BlendtreeMono)target;
        VRCAvatarDescriptor avatarDescriptor = mono.gameObject.GetComponent<VRCAvatarDescriptor>();

        BlendtreeCreator.ShowWindow(avatarDescriptor);
    }
}
