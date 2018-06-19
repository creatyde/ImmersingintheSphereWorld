//==============================================================================
// Copyright (C) 2016 3D Sound Labs - All rights reserved
//==============================================================================

using UnityEngine;
using UnityEditor;
using System;

//==============================================================================
[CustomEditor(typeof(VRAudioRoom))]
public class VRAudioRoomEditor : Editor
{
  private SerializedProperty size = null;
  private SerializedProperty reverbMix = null;
  private SerializedProperty reflectionFactor = null;

  private GUIContent sizeLabel = new GUIContent(
    "Size",
    "The size of the room.");

  private GUIContent reverbMixLabel = new GUIContent(
    "Reverb Mix",
    "The amount of reverb which will be added in the mix.");

  private GUIContent reflectionFactorLabel = new GUIContent(
    "Reflection Factor",
    "The walls' reflection factor.");

  void OnEnable()
  {
    size = serializedObject.FindProperty("_size");
    reverbMix = serializedObject.FindProperty("_reverbMix");
    reflectionFactor = serializedObject.FindProperty("_reflectionFactor");
  }

  public override void OnInspectorGUI()
  {
    MonoScript script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);

    EditorGUI.BeginDisabledGroup(true);
    EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
    EditorGUI.EndDisabledGroup();

    serializedObject.Update();
    EditorGUILayout.PropertyField(size, sizeLabel);
    EditorGUILayout.Slider(reverbMix, 0.0f, 1.0f, reverbMixLabel);
    EditorGUILayout.Slider(reflectionFactor, 0.0f, 1.0f, reflectionFactorLabel);
    serializedObject.ApplyModifiedProperties();
  }
}
