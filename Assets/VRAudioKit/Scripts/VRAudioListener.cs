//==============================================================================
// Copyright (C) 2016 3D Sound Labs - All rights reserved
//==============================================================================

/**
 * \file  VRAudioListener.cs
 * \brief The VRAudioListener.cs file contains the definition of
 *        the VRAudioListener class.
 */

using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

//==============================================================================
/**
 * \class VRAudioListener
 * \brief The VRAudioListener class represents the listener.
 *
 * It must be placed next to Unity's AudioListener Component to inject the output
 * of the VRAudioKit renderer in Unity's audio pipeline.
 */
[RequireComponent(typeof(AudioListener))]
[AddComponentMenu("VRAudioKit/VRAudioListener")]
public class VRAudioListener : MonoBehaviour
{
  /**
   * The output gain of the engine.
   *
   * Can be used to adjust the overall output volume.
   */
  public float outputGain
  {
    get { return _outputGain; }
    set {
      _outputGain = Mathf.Clamp(value, 0, 10);
      if (_enabled)
        vraudio_unity_listener_set_output_gain(_outputGain);
    }
  }
  [SerializeField]
  private float _outputGain = 1.0f;

  /**
   * The HOA order of the engine.
   *
   * Can be used to trade CPU usage for quality and vice versa.
   */
  public uint hoaOrder
  {
    get { return _hoaOrder; }
    set {
      _hoaOrder = Math.Min(Math.Max(value, 1), 12);
      if (_enabled)
        vraudio_unity_listener_set_hoa_order(_hoaOrder);
    }
  }
  [SerializeField]
  private uint _hoaOrder = 4;

  /**
   * The HRTF used by the engine.
   *
   * \note
   * Relative paths will be looked for in StreamingAssets.
   */
  public string hrtf
  {
    get { return _hrtf; }
    set {
      _hrtf = value;

      if (_updateHrtf != null)
        StopCoroutine(_updateHrtf);

      _updateHrtf = UpdateHrtf();
      StartCoroutine(_updateHrtf);
    }
  }
  [SerializeField]
  private string _hrtf = "default.hrtf";

  /**
   * The optional license file.
   *
   * \note
   * Relative paths will be looked for in StreamingAssets.
   */
  public string license
  {
    get { return _license; }
    set {
      _license = value;

      if (_updateLicense != null)
        StopCoroutine(_updateLicense);

      _updateLicense = UpdateLicense();
      StartCoroutine(_updateLicense);
    }
  }
  [SerializeField]
  private string _license = null;

  void OnEnable()
  {
    VRAudioContext.Init();

    _enabled = true;
    vraudio_unity_listener_set_enabled(_enabled);

    UpdateProperties();
  }

  void OnDisable()
  {
    _enabled = false;
    vraudio_unity_listener_set_enabled(_enabled);

    VRAudioContext.Cleanup();
  }

  void Update()
  {
    vraudio_unity_listener_set_position(
      transform.position.x,
      transform.position.y,
      transform.position.z);

    vraudio_unity_listener_set_orientation(
      transform.rotation.x,
      transform.rotation.y,
      transform.rotation.z,
      transform.rotation.w);
  }

  void OnValidate()
  {
    UpdateProperties();
  }

  void UpdateProperties()
  {
    outputGain = _outputGain;
    hoaOrder = _hoaOrder;
    hrtf = _hrtf;
    license = _license;
  }

  IEnumerator UpdateHrtf()
  {
    if (string.IsNullOrEmpty(_hrtf)) {
      vraudio_unity_listener_set_hrtf(null, IntPtr.Zero);
      yield break;
    }

    string url = null;
    if (Regex.IsMatch (_hrtf, "[a-z]+://.*"))
      url = _hrtf;
    else
      url = "file://" + Application.streamingAssetsPath + "/" + _hrtf;

    WWW www = new WWW(url);
    yield return www;

    if (!_enabled)
      yield break;

    if (string.IsNullOrEmpty(www.error))
      vraudio_unity_listener_set_hrtf(www.bytes, new IntPtr(www.bytes.Length));
    else
      vraudio_unity_listener_set_hrtf(null, IntPtr.Zero);

    _updateHrtf = null;
  }

  IEnumerator UpdateLicense()
  {
    if (string.IsNullOrEmpty(_license))
      yield break;

    string url = null;
    if (Regex.IsMatch (_license, "[a-z]+://.*"))
      url = _license;
    else
      url = "file://" + Application.streamingAssetsPath + "/" + _license;

    WWW www = new WWW(url);
    yield return www;

    if (!_enabled)
      yield break;

    if (string.IsNullOrEmpty(www.error))
      vraudio_unity_listener_set_license(www.bytes, new IntPtr(www.bytes.Length));

    _updateLicense = null;
  }

  private bool _enabled = false;
  private IEnumerator _updateHrtf = null;
  private IEnumerator _updateLicense = null;

#if UNITY_IPHONE
  private const string _pluginName = "__Internal";
#elif VR_AUDIO_DEBUG
  private const string _pluginName = "audioplugin-vraudiokit-unityd";
#else
  private const string _pluginName = "audioplugin-vraudiokit-unity";
#endif

  [DllImport (_pluginName)]
  private static extern void vraudio_unity_listener_set_enabled(
    bool enabled);

  [DllImport (_pluginName)]
  private static extern void vraudio_unity_listener_set_position(
    float x,
    float y,
    float z);

  [DllImport (_pluginName)]
  private static extern void vraudio_unity_listener_set_orientation(
    float x,
    float y,
    float z,
    float w);

  [DllImport (_pluginName)]
  private static extern void vraudio_unity_listener_set_output_gain(
    float gain);

  [DllImport (_pluginName)]
  private static extern void vraudio_unity_listener_set_hoa_order(
    uint hoaOrder);

  [DllImport (_pluginName)]
  private static extern void vraudio_unity_listener_set_hrtf(
    byte[] data,
    IntPtr size);

  [DllImport (_pluginName)]
  private static extern void vraudio_unity_listener_set_license(
    byte[] data,
    IntPtr size);
}
