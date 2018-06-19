//==============================================================================
// Copyright (C) 2016 3D Sound Labs - All rights reserved
//==============================================================================

/**
 * \file  VRAudioSource.cs
 * \brief The VRAudioSource.cs file contains the definition of
 *        the VRAudioSource class.
 */

using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Runtime.InteropServices;

//==============================================================================
/**
 * \class VRAudioSource
 * \brief The VRAudioSource class represents  a monophonic point source.
 *
 * This class replaces Unity's AudioSource component.
 */
[AddComponentMenu("VRAudioKit/VRAudioSource")]
public class VRAudioSource : MonoBehaviour
{
  /**
   * Returns true if the clip is currently playing (Read Only).
   */
  public bool isPlaying
  {
    get {
      if (_audioSource != null)
        return _audioSource.isPlaying;
      else
        return false;
    }
  }

  /**
   * The current playback position in seconds.
   */
  public float time
  {
    get {
      if (_audioSource != null)
        return _audioSource.time;
      else
        return 0.0f;
    }
    set {
      if (_audioSource != null)
        _audioSource.time = value;
    }
  }

  /**
   * The current playback position in samples.
   */
  public int timeSamples
  {
    get {
      if (_audioSource != null)
        return _audioSource.timeSamples;
      else
        return 0;
    }
    set {
      if (_audioSource != null)
        _audioSource.timeSamples = value;
    }
  }

  /**
   * The audio clip of the source.
   */
  public AudioClip clip
  {
    get { return _clip; }
    set {
      _clip = value;
      if (_audioSource != null)
        _audioSource.clip = _clip;
    }
  }
  [SerializeField]
  private AudioClip _clip = null;

  /**
   * Mutes the sound.
   */
  public bool mute
  {
    get { return _mute; }
    set {
      _mute = value;
      if (_audioSource != null)
        _audioSource.mute = mute;
    }
  }
  [SerializeField]
  private bool _mute = false;

  /**
   * Loops the sound.
   */
  public bool loop
  {
    get { return _loop; }
    set {
      _loop = value;
      if (_audioSource != null)
        _audioSource.loop = _loop;
    }
  }
  [SerializeField]
  private bool _loop = false;

  /**
   * Play the sound when the scene loads.
   */
  public bool playOnAwake = false;

  /**
   * The overall volume of the sound.
   */
  public float volume
  {
    get { return _volume; }
    set {
      _volume = value;
      if (_audioSource != null)
        _audioSource.volume = _volume;
    }
  }
  [SerializeField]
  private float _volume = 1.0f;

  /**
   * The pitch of the sound.
   */
  public float pitch
  {
    get { return _pitch; }
    set {
      _pitch = value;
      if (_audioSource != null)
        _audioSource.pitch = _pitch;
    }
  }
  [SerializeField]
  private float _pitch = 1.0f;

  /**
   * Specifies how much the pitch is changed based on the relative velocity
   * between VRAudioListener and VRAudioSource.
   */
  public float dopplerLevel
  {
    get { return _dopplerLevel; }
    set {
      _dopplerLevel = value;
      if (_audioSource != null)
        _audioSource.dopplerLevel = _dopplerLevel;
    }
  }
  [SerializeField]
  private float _dopplerLevel = 1.0f;

  /**
   * Which type of rolloff curve to use.
   *
   * \note
   * As of today CustomRolloff is not supported.
   */
  public AudioRolloffMode rolloffMode
  {
    get { return _rolloffMode; }
    set {
      _rolloffMode = value;
      if (_audioSource != null) {
        _audioSource.rolloffMode = _rolloffMode;

        if (rolloffMode == AudioRolloffMode.Custom)
          _audioSource.SetCustomCurve(
            AudioSourceCurveType.CustomRolloff,
            AnimationCurve.Linear(_minDistance, 1.0f, _maxDistance, 1.0f));
      }
    }
  }
  [SerializeField]
  private AudioRolloffMode _rolloffMode = AudioRolloffMode.Logarithmic;

  /**
   * The distance within wich, the volume will stay at the loudest possible.
   * Outside of it the sound begins to attenuate.
   */
  public float minDistance
  {
    get { return _minDistance; }
    set {
      _minDistance = Mathf.Max(value, 0.0f);
      if (_audioSource != null)
        _audioSource.minDistance = _minDistance;
    }
  }
  [SerializeField]
  private float _minDistance = 1.0f;

  /**
   * The distance a sound stops attenuating at.
   */
  public float maxDistance
  {
    get { return _maxDistance; }
    set {
      _maxDistance = Mathf.Max(value, 0.0f);
      if (_audioSource != null)
        _audioSource.maxDistance = _maxDistance;
    }
  }
  [SerializeField]
  private float _maxDistance = 500.0f;

  /**
   * The input gain of this source.
   */
  public float inputGain = 1.0f;

  /**
   * The priority of this source.
   *
   * Sources with higher priority will be rendered with highest quality.
   */
  public uint priority = 0;

  /**
   * Provides a block of the currently playing source's output data.
   */
  public void GetOutputData(float[] samples, int channel)
  {
    if (_audioSource != null)
      _audioSource.GetOutputData(samples, channel);
  }

  /**
   * Provides a block of the currently playing audio source's spectrum data.
   */
  public void GetSpectrumData(float[] samples, int channel, FFTWindow window)
  {
    if (_audioSource != null)
      _audioSource.GetSpectrumData(samples, channel, window);
  }

  /**
   * Pauses playing the clip.
   */
  public void Pause()
  {
    if (_audioSource != null)
      _audioSource.Pause();
  }

  /**
   * Unpause the paused playback of this AudioSource.
   */
  public void UnPause()
  {
    if (_audioSource != null)
      _audioSource.UnPause();
  }

  /**
   * Plays the clip.
   */
  public void Play()
  {
    if (_audioSource != null)
      _audioSource.Play();
  }

  /**
   * Plays the clip with a delay specified in seconds.
   */
  public void PlayDelayed(float delay)
  {
    if (_audioSource != null)
      _audioSource.PlayDelayed(delay);
  }

  /**
   * Plays an AudioClip.
   */
  public void PlayOneShot(AudioClip clip)
  {
    if (_audioSource != null)
      _audioSource.PlayOneShot(clip);
  }

  /**
   * Plays an AudioClip, and scales its volume.
   */
  public void PlayOneShot(AudioClip clip, float volume)
  {
    if (_audioSource != null)
      _audioSource.PlayOneShot(clip, volume);
  }

  /**
   * Plays the clip at a specific time on the absolute time-line
   * that AudioSettings.dspTime reads from.
   */
  public void PlayScheduled(double time)
  {
    if (_audioSource != null)
      _audioSource.PlayScheduled(time);
  }

  /**
   * Stops playing the clip.
   */
  public void Stop()
  {
    if (_audioSource != null)
      _audioSource.Stop();
  }

  /**
   * Changes the time at which a sound that has already been scheduled
   * to play will start.
   */
  public void SetScheduledStartTime(double time)
  {
    if (_audioSource != null)
      _audioSource.SetScheduledStartTime(time);
  }

  /**
   * Changes the time at which a sound that has already been scheduled
   * to play will end.
   */
  public void SetScheduledEndTime(double time)
  {
    if (_audioSource != null)
      _audioSource.SetScheduledEndTime(time);
  }

  void Awake()
  {
    _audioMixer = (AudioMixer)Resources.Load("VRAudioMixer");
    if (_audioMixer == null)
      Debug.LogError(
        "VRAudioMixer could not be found in Resources. " +
        "Make sure that the VRAudioKit Unity package has been imported properly.");

    _audioSource = gameObject.AddComponent<AudioSource>();
    _audioSource.enabled = false;
    _audioSource.playOnAwake = false;
    _audioSource.spatialBlend = 1.0f;
    _audioSource.spatialize = true;
    _audioSource.outputAudioMixerGroup = _audioMixer.FindMatchingGroups("Master")[0];

    UpdateProperties();
  }

  void OnDestroy()
  {
    _audioMixer = null;

    _audioSource.spatialize = false;
    Destroy(_audioSource);

    _audioSource = null;
  }

  void OnEnable()
  {
    _audioSource.enabled = true;
    if (playOnAwake)
      this.Play();
  }

  void OnDisable()
  {
    _audioSource.enabled = false;
  }

  void Update()
  {
    _audioSource.SetSpatializerFloat(0, inputGain);
    _audioSource.SetSpatializerFloat(1, priority);
  }

  void OnValidate()
  {
    UpdateProperties();
  }

  void OnDrawGizmosSelected()
  {
    float a = 0.2f;
    float k = (1.0f - a) / a;
    float d = Vector3.Distance(Camera.current.transform.position, transform.position);

    a = 1.0f / (1.0f + k * Mathf.Max(1.0f - d / minDistance, 0.0f));
    Color cMin = new Color(0.38f, 0.55f, 0.65f, a);

    a = 1.0f / (1.0f + k * Mathf.Max(1.0f - d / maxDistance, 0.0f));
    Color cMax = new Color(0.38f, 0.55f, 0.65f, a);

    Gizmos.matrix = Matrix4x4.TRS(
      transform.position,
      Quaternion.identity,
      Vector3.one);

    Gizmos.color = cMin;
    Gizmos.DrawWireSphere(Vector3.zero, minDistance);
    Gizmos.color = cMax;
    Gizmos.DrawWireSphere(Vector3.zero, maxDistance);
  }

  void UpdateProperties()
  {
    clip = _clip;
    mute = _mute;
    loop = _loop;
    volume = _volume;
    pitch = _pitch;
    dopplerLevel = _dopplerLevel;
    rolloffMode = _rolloffMode;
    minDistance = _minDistance;
    maxDistance = _maxDistance;
  }

  private AudioSource _audioSource = null;
  private AudioMixer _audioMixer = null;

#if UNITY_IPHONE
  private const string _pluginName = "__Internal";
#elif VR_AUDIO_DEBUG
  private const string _pluginName = "audioplugin-vraudiokit-unityd";
#else
  private const string _pluginName = "audioplugin-vraudiokit-unity";
#endif

  [DllImport (_pluginName)]
  private static extern void vraudio_source_set_input_gain(
    ulong id,
    float input_gain);

  [DllImport (_pluginName)]
  private static extern void vraudio_source_set_priority(
    ulong id,
    uint priority);
}
