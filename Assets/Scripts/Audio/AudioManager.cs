using System;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Linq;

public class AudioManager : Singleton<AudioManager>
{
    //TODO: Add randomized pitch, global value change for Effects vs Music

    public Sound[] music;
    public Sound[] effects;
    public SoundGroup[] effectGroups;

    protected List<Sound> allSounds;

    // TODO: Load these values from PlayerPrefs in Start().
    [HideInInspector] public float masterMusicVolume = 1.0f;
    [HideInInspector] public float masterEffectVolume = 1.0f;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this.transform.root.gameObject);

        // Add all sounds to the internal sounds list for easier iteration.
        allSounds = new List<Sound>();
        allSounds.AddRange(music);
        allSounds.AddRange(effects);
        foreach (SoundGroup group in effectGroups)
            allSounds.AddRange(group.sounds);

        InitializeAudioSources();
    }

#region Management
    private void InitializeAudioSources() {
        foreach (Sound sound in allSounds) {
            sound.source = this.gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            // sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
        }
    }

    private void UpdateSourceVolumes() {
        foreach (Sound s in allSounds)
            s.source.volume = s.volume * (s.isEffect ? masterEffectVolume : masterMusicVolume);
    }

    public void SetEffectsVolume(float value) {
        masterEffectVolume = value;
        UpdateSourceVolumes();
    }

    public void SetMusicVolume(float value) {
        masterMusicVolume = value;
        UpdateSourceVolumes();
    }
#endregion

#region Play Audio
    public static void PlaySound(string name) {
        Sound sound = AudioManager.instance.allSounds.Where(sound => sound.name == name).First();
        if (sound == null) {
            Debug.LogWarning("Sound of name " + name + " not found!");
            return;
        }

        PlaySound(sound);
    }

    public static void PlaySound(Sound sound) {
        sound.source.volume = sound.volume * (sound.isEffect ? AudioManager.instance.masterEffectVolume : AudioManager.instance.masterMusicVolume);
        sound.source.Play();
    }

    public static void PlayRandomGroupSound(SoundGroup group) {
        Sound sound = group.sounds[UnityEngine.Random.Range(0, group.sounds.Length - 1)];
        PlaySound(sound);
    }

    public static void PlayRandomGroupSound(string groupName) {
        SoundGroup soundGroup = AudioManager.instance.effectGroups.Where(group => group.name == groupName).First();
        PlayRandomGroupSound(soundGroup);
    }
#endregion

    // public void PlayRandomEffect() {
    //     Sound[] soundGroup = Array.FindAll(sounds, sound => sound.isEffect == true);
    //     string soundName = sounds[UnityEngine.Random.Range(0, soundGroup.Length - 1)].name;
    //     PlaySound(soundName);
    // }
}
