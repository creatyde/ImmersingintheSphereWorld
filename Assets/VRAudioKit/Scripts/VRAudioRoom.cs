//==============================================================================
// Copyright (C) 2016 3D Sound Labs - All rights reserved
//==============================================================================

/**
 * \file  VRAudioRoom.cs
 * \brief The VRAudioRoom.cs file contains the definition of
 *        the VRAudioRoom class.
 */

using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Runtime.InteropServices;

//FIXME: fix clamp values

//==============================================================================
/**
 * \class VRAudioRoom
 * \brief The VRAudioRoom class represents a room which will generate
 *        early reflections and late reverberation.
 *
 * \note
 * As of today, you can only create one room.
 */
[AddComponentMenu("VRAudioKit/VRAudioRoom")]
public class VRAudioRoom : MonoBehaviour
{
  /**
   * The size of the room.
   */
  public Vector3 size
  {
    get { return _size; }
    set {
      _size = new Vector3(
        Mathf.Max(value.x, 0.00001f),
        Mathf.Max(value.y, 0.00001f),
        Mathf.Max(value.z, 0.00001f));

      if (_enabled)
        vraudio_unity_room_set_size(
          _size.x,
          _size.y,
          _size.z);
    }
  }
  [SerializeField]
  private Vector3 _size = new Vector3(6.0f, 2.5f, 8.0f);

  /**
   * The amount of reverb which will be added in the mix.
   */
  public float reverbMix
  {
    get { return _reverbMix; }
    set {
      _reverbMix = Mathf.Clamp(value, 0.00001f, 1.0f);
      if (_enabled)
        vraudio_unity_room_set_reverb_mix(_reverbMix);
    }
  }
  [SerializeField]
  private float _reverbMix = 0.04f;

  /**
   * The walls' reflection factor.
   */
  public float reflectionFactor
  {
    get { return _reflectionFactor; }
    set
    {
      _reflectionFactor = Mathf.Clamp(value, 0.00001f, 1.0f);
      if (_enabled)
        vraudio_unity_room_set_reflection_factor(_reflectionFactor);
    }
  }
  [SerializeField]
  private float _reflectionFactor = 0.6f;

  void OnEnable()
  {
    VRAudioContext.Init();

    _enabled = true;
    vraudio_unity_room_set_enabled(_enabled);

    UpdateProperties();
  }

  void OnDisable()
  {
    _enabled = false;
    vraudio_unity_room_set_enabled(_enabled);

    VRAudioContext.Cleanup();
  }

  void Update()
  {
    vraudio_unity_room_set_position(
      transform.position.x,
      transform.position.y,
      transform.position.z);
  }

  void OnValidate()
  {
    UpdateProperties();
  }

  void OnDrawGizmosSelected()
  {
    Gizmos.matrix = Matrix4x4.TRS(
      transform.position,
      Quaternion.identity,
      Vector3.one);

    Gizmos.color = new Color(1.0f, 0.9f, 0.54f, 1.0f);
    Gizmos.DrawWireCube(Vector3.zero, size);
  }

  void UpdateProperties()
  {
    size = _size;
    reverbMix = _reverbMix;
    reflectionFactor = _reflectionFactor;
  }

  private bool _enabled = false;

#if UNITY_IPHONE
  private const string _pluginName = "__Internal";
#elif VR_AUDIO_DEBUG
  private const string _pluginName = "audioplugin-vraudiokit-unityd";
#else
  private const string _pluginName = "audioplugin-vraudiokit-unity";
#endif

  [DllImport (_pluginName)]
  private static extern void vraudio_unity_room_set_enabled(
    bool enabled);

  [DllImport (_pluginName)]
  private static extern void vraudio_unity_room_set_position(
    float x,
    float y,
    float z);

  [DllImport (_pluginName)]
  private static extern void vraudio_unity_room_set_size(
    float x,
    float y,
    float z);

  [DllImport (_pluginName)]
  private static extern void vraudio_unity_room_set_reverb_mix(
    float reverbMix);

  [DllImport (_pluginName)]
  private static extern void vraudio_unity_room_set_reflection_factor(
    float reflectionFactor);
}
