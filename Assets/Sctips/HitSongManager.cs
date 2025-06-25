using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static E7.Native.NativeSource;

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
using E7.Native;
#endif

public class HitSongManager : MonoBehaviour
{
    public int trackIndex;
    public AudioSource aus;
    public AudioClip[] clips;

    public static HitSongManager instance;

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
    public NativeAudioPointer[] pointers;
#endif

    private void Awake()
    {
        instance = this;

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        NativeAudio.Dispose();
        var options = new NativeAudio.InitializationOptions
        {
            androidAudioTrackCount = 5,
            preserveOnMinimize = NativeAudio.InitializationOptions.defaultOptions.preserveOnMinimize,
            androidBufferSize = NativeAudio.InitializationOptions.defaultOptions.androidBufferSize
        };
        NativeAudio.Initialize(options);

        var pointers = new List<NativeAudioPointer>();
        foreach (var clip in clips)
        {
            pointers.Add(NativeAudio.Load(clip));
        }

        this.pointers = pointers.ToArray();
#endif
    }

    public void Play(int idx)
    {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        trackIndex = (trackIndex + 1) % 4;
        var nativeSource = NativeAudio.GetNativeSource(trackIndex);
        nativeSource.Play(pointers[idx], new PlayOptions
        {
            volume = 0.2f 
        });
#else
        aus.PlayOneShot(clips[idx], 0.2f);
#endif
    }
}
