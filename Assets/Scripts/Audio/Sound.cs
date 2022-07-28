using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound {
    public string name;
    public AudioClip clip;
    public bool isEffect;  // Whether the sound is to be treated as an effect or as music.
    public bool loop;

    [Range(0f, 1f)] public float volume = 1.0f;
    // [Range(.1f, 3f)] public float pitch = 1;
    [HideInInspector] public AudioSource source;
}

[System.Serializable]
public class SoundGroup {
    public string name;
    public Sound[] sounds;
}