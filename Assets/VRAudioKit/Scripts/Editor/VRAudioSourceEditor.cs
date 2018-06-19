//==============================================================================
// Copyright (C) 2016 3D Sound Labs - All rights reserved
//==============================================================================

using UnityEngine;
using UnityEditor;
using System;

//==============================================================================
[CustomEditor(typeof(VRAudioSource))]
public class VRAudioSourceEditor : Editor
{
  private SerializedProperty clip = null;
  private SerializedProperty mute = null;
  private SerializedProperty loop = null;
  private SerializedProperty playOnAwake = null;
  private SerializedProperty volume = null;
  private SerializedProperty pitch = null;
  private SerializedProperty dopplerLevel = null;
  private SerializedProperty rolloffMode = null;
  private SerializedProperty minDistance = null;
  private SerializedProperty maxDistance = null;
  private SerializedProperty inputGain = null;
  private SerializedProperty priority = null;

  private GUIContent clipLabel = new GUIContent(
    "AudioClip",
    "The audio clip played by the VRAudioSource.");

  private GUIContent muteLabel = new GUIContent(
    "Mute",
    "Mutes the sound.");

  private GUIContent loopLabel = new GUIContent(
    "Loop",
    "Loops the sound.");

  private GUIContent playOnAwakeLabel = new GUIContent(
    "Play On Awake",
    "Play the sound when the scene loads.");

  private GUIContent volumeLabel = new GUIContent(
    "Volume",
    "The overall volume of the sound.");

  private GUIContent pitchLabel = new GUIContent(
    "Pitch",
    "The pitch of the sound.");

  private GUIContent dopplerLevelLabel = new GUIContent(
    "Doppler Level",
    "Specifies how much the pitch is changed based on " +
    "the relative velocity between VRAudioListener and VRAudioSource.");

  private GUIContent rolloffModeLabel = new GUIContent(
    "Volume Rolloff",
    "Which type of rolloff curve to use.");

  private GUIContent minDistanceLabel = new GUIContent(
    "Min Distance",
    "The distance within wich, the volume will stay at the loudest possible. " +
    "Outside of this min distance it begins to attenuate.");

  private GUIContent maxDistanceLabel = new GUIContent(
    "Max Distance",
    "The distance a sound stops attenuating at.");

  private GUIContent inputGainLabel = new GUIContent(
    "Input Gain",
    "The input gain of this source. " +
    "Can be used to adjust the volume of this source.");

  private GUIContent priorityLabel = new GUIContent(
    "Priority",
    "The priority of this source. " +
    "Sources with higher priority will be rendered with highest quality.");

  void OnEnable()
  {
    clip = serializedObject.FindProperty("_clip");
    mute = serializedObject.FindProperty("_mute");
    loop = serializedObject.FindProperty("_loop");
    playOnAwake = serializedObject.FindProperty("playOnAwake");
    volume = serializedObject.FindProperty("_volume");
    pitch = serializedObject.FindProperty("_pitch");
    dopplerLevel = serializedObject.FindProperty("_dopplerLevel");
    rolloffMode = serializedObject.FindProperty("_rolloffMode");
    minDistance = serializedObject.FindProperty("_minDistance");
    maxDistance = serializedObject.FindProperty("_maxDistance");
    inputGain = serializedObject.FindProperty("inputGain");
    priority = serializedObject.FindProperty("priority");
  }

  public override void OnInspectorGUI()
  {
    MonoScript script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);

    EditorGUI.BeginDisabledGroup(true);
    EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
    EditorGUI.EndDisabledGroup();

    serializedObject.Update();
    EditorGUILayout.PropertyField(clip, clipLabel);
    EditorGUILayout.PropertyField(mute, muteLabel);
    EditorGUILayout.PropertyField(loop, loopLabel);
    EditorGUILayout.PropertyField(playOnAwake, playOnAwakeLabel);
    EditorGUILayout.Separator();

    EditorGUILayout.Slider(volume, 0.0f, 1.0f, volumeLabel);
    EditorGUILayout.Slider(pitch, -3.0f, 3.0f, pitchLabel);
    EditorGUILayout.Slider(dopplerLevel, 0.0f, 5.0f, dopplerLevelLabel);
    EditorGUILayout.Separator();

    EditorGUILayout.PropertyField(rolloffMode, rolloffModeLabel);
    if (rolloffMode.enumValueIndex == (int)AudioRolloffMode.Custom)
      EditorGUILayout.HelpBox(
        "Custom rolloff mode is not supported.",
        MessageType.Warning);

    EditorGUILayout.PropertyField(minDistance, minDistanceLabel);
    EditorGUILayout.PropertyField(maxDistance, maxDistanceLabel);
    EditorGUILayout.Separator();

    EditorGUILayout.Slider(inputGain, 0.0f, 10.0f, inputGainLabel);
    EditorGUILayout.IntSlider(priority, 0, 10, priorityLabel);
    serializedObject.ApplyModifiedProperties();
  }
}
