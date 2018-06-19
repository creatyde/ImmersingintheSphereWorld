//==============================================================================
// Copyright (C) 2016 3D Sound Labs - All rights reserved
//==============================================================================

using UnityEngine;
using UnityEditor;
using System;

//==============================================================================
[CustomEditor(typeof(VRAudioListener))]
public class VRAudioListenerEditor : Editor
{
  private SerializedProperty outputGain = null;
  private SerializedProperty hoaOrder = null;
  private SerializedProperty hrtf = null;
  private SerializedProperty license = null;

  private GUIContent outputGainLabel = new GUIContent(
    "Output Gain",
    "The output gain of the engine. " +
    "Can be used to adjust the overall output volume.");

  private GUIContent hoaOrderLabel = new GUIContent(
    "Hoa Order",
    "The HOA order of the engine. " +
    "Can be used to trade CPU usage for quality and vice versa.");

  private GUIContent hrtfLabel = new GUIContent(
    "Hrtf",
    "The HRTF used by the engine. " +
    "Relative paths will be looked for in StreamingAssets.");

  private GUIContent licenseLabel = new GUIContent(
    "License",
    "The optional license file." +
    "Relative paths will be looked for in StreamingAssets.");

  void OnEnable()
  {
    outputGain = serializedObject.FindProperty("_outputGain");
    hoaOrder = serializedObject.FindProperty("_hoaOrder");
    hrtf = serializedObject.FindProperty("_hrtf");
    license = serializedObject.FindProperty("_license");
  }

  public override void OnInspectorGUI()
  {
    MonoScript script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);

    EditorGUI.BeginDisabledGroup(true);
    EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
    EditorGUI.EndDisabledGroup();

    serializedObject.Update();
    EditorGUILayout.Slider(outputGain, 0.0f, 10.0f, outputGainLabel);
    EditorGUILayout.IntSlider(hoaOrder, 1, 12, hoaOrderLabel);
    EditorGUILayout.PropertyField(hrtf, hrtfLabel);
    EditorGUILayout.PropertyField(license, licenseLabel);
    serializedObject.ApplyModifiedProperties();
  }
}
