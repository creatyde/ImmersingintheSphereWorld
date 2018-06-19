//==============================================================================
// Copyright (C) 2016 3D Sound Labs - All rights reserved
//==============================================================================

/**
 * \file  VRAudioContext.cs
 * \brief The VRAudioContext.cs file contains the definition of
 *        the VRAudioContext class.
 */

using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Runtime.InteropServices;

//==============================================================================
/**
 * \class VRAudioContext
 * \brief The VRAudioContext class is used to initialize/cleanup the VRAudioKit
 *        engine.
 *
 * You should not use this class directly.
 */
public class VRAudioContext
{
  public static void Init()
  {
    AudioConfiguration config = AudioSettings.GetConfiguration();
    vraudio_unity_context_init(
      (uint)config.sampleRate,
      new IntPtr(config.dspBufferSize));
  }

  public static void Cleanup()
  {
    vraudio_unity_context_cleanup();
  }

#if UNITY_IPHONE
  private const string _pluginName = "__Internal";
#elif VR_AUDIO_DEBUG
  private const string _pluginName = "audioplugin-vraudiokit-unityd";
#else
  private const string _pluginName = "audioplugin-vraudiokit-unity";
#endif

  [DllImport (_pluginName)]
  private static extern void vraudio_unity_context_init(uint sampleRate, IntPtr bufferSize);

  [DllImport (_pluginName)]
  private static extern void vraudio_unity_context_cleanup();
}
