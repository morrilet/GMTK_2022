using System;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AudioManager : Singleton<AudioManager>
{
    //TODO: Add randomized pitch, global value change for Effects vs Music

    public Sound[] music;
    public Sound[] effects;
    public SoundGroup[] effectGroups;

    protected bool isPlayingMenuMusic;
    protected bool isPlayingInGameMusic;
    protected bool isTransitioningSoundtrack;

    protected List<Sound> allSounds;

    public delegate void TransitionCallback();

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

    protected Sound GetSoundByName(string name) {
        Sound sound = AudioManager.instance.allSounds.Where(sound => sound.name == name).First();
        if (sound == null) {
            Debug.LogWarning("Sound of name " + name + " not found!");
            return null;
        }
        return sound;
    }

#endregion

#region Play Audio
    public static void PlaySound(string name) {
        Sound sound = AudioManager.instance.GetSoundByName(name);
        PlaySound(sound);
    }

    public static void PlaySound(Sound sound) {
        sound.source.volume = sound.volume * (sound.isEffect ? AudioManager.instance.masterEffectVolume : AudioManager.instance.masterMusicVolume);
        sound.source.Play();
    }

    public static void Crossfade(string currentSound, string nextSound, float duration, TransitionCallback callback = null) {
        Sound current = AudioManager.instance.GetSoundByName(currentSound);
        Sound next = AudioManager.instance.GetSoundByName(nextSound);
        AudioManager.Crossfade(current, next, duration, callback);
    }

    public static void Crossfade(Sound currentSound, Sound nextSound, float duration, TransitionCallback callback = null) {
        AudioManager.instance.StartCoroutine(CrossfadeCoroutine(currentSound, nextSound, duration, callback));
    }

    protected static IEnumerator CrossfadeCoroutine(Sound currentSound, Sound nextSound, float duration, TransitionCallback callback = null) {
        float timer = 0.0f;

        // Kick off the next sound to set the volume before we go fiddling with it.
        PlaySound(nextSound);

        float currentSoundVolumeStart = currentSound.source.volume;
        float nextSoundVolumeStart = nextSound.source.volume;

        // Crossfade the source volumes over time.
        while (timer < duration) {
            currentSound.source.volume = currentSoundVolumeStart * (1.0f - timer / (duration / 2.0f));  // Fade out for the first half
            nextSound.source.volume = nextSoundVolumeStart * Mathf.Clamp01(((timer - duration / 2.0f) / duration));  // Fade in for the second half.

            timer += Time.deltaTime;
            yield return null;
        }

        // Set the volumes back to normal and hard-stop the old audio.
        nextSound.source.volume = nextSoundVolumeStart;
        currentSound.source.volume = currentSoundVolumeStart;
        currentSound.source.Stop();

        if (callback != null)
            callback();
    }

    public static void SwitchAfterLoops(string currentSound, string nextSound, int loopCount, TransitionCallback callback = null) {
        Sound current = AudioManager.instance.GetSoundByName(currentSound);
        Sound next = AudioManager.instance.GetSoundByName(nextSound);
        AudioManager.SwitchAfterLoops(current, next, loopCount, callback);
    }

    public static void SwitchAfterLoops(Sound currentSound, Sound nextSound, int loopCount, TransitionCallback callback = null) {
        AudioManager.instance.StartCoroutine(SwitchAfterLoopsCoroutine(currentSound, nextSound, loopCount, callback));
    }

    protected static IEnumerator SwitchAfterLoopsCoroutine(Sound currentSound, Sound nextSound, int loopCount, TransitionCallback callback = null) {

        // Get the wait duration based on the length of a loop, how far we are in the current loop (that counts as one loop), and how many loops we need to wait for.
        float waitDuration = (currentSound.clip.length - currentSound.source.time) + (currentSound.clip.length * Mathf.Clamp(loopCount - 1, 0, int.MaxValue));
        yield return new WaitForSeconds(waitDuration);

        currentSound.source.Stop();
        PlaySound(nextSound);

        if (callback != null)
            callback();
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

#region Soundtrack
    // This is a bunch of complicated state management code for transitioning between menu music and in-game music.
    // Ideally this would be refactored and moved somewhere less global but it is what it is with the deadline looming.

    public static void TryStartMenuMusic() {
        if (AudioManager.instance.isTransitioningSoundtrack) {
            AudioManager.instance.isTransitioningSoundtrack = false;
            AudioManager.instance.StopAllCoroutines();
        }
        
        if (AudioManager.instance.isPlayingInGameMusic) {
            AudioManager.Crossfade(
                GlobalVariables.MAIN_SOUNDTRACK_EFFECT,
                GlobalVariables.MAIN_MENU_SOUNDTRACK_EFFECT,
                GlobalVariables.SOUNDTRACK_CROSSFADE_DURATION,
                () => {
                    AudioManager.instance.isTransitioningSoundtrack = false;
                    AudioManager.instance.isPlayingInGameMusic = false;
                    AudioManager.instance.isPlayingMenuMusic = true;
                }
            );
            AudioManager.instance.isTransitioningSoundtrack = true;
        }
        else if (!AudioManager.instance.isPlayingMenuMusic) {
            AudioManager.PlaySound(GlobalVariables.MAIN_MENU_SOUNDTRACK_EFFECT);
            AudioManager.instance.isPlayingInGameMusic = false;
            AudioManager.instance.isPlayingMenuMusic = true;
        }
    }

    public static void TryStartInGameMusic() {
        if (AudioManager.instance.isTransitioningSoundtrack) {
            AudioManager.instance.isTransitioningSoundtrack = false;
            AudioManager.instance.StopAllCoroutines();
        }
        
        if (AudioManager.instance.isPlayingMenuMusic) {
            AudioManager.SwitchAfterLoops(
                GlobalVariables.MAIN_MENU_SOUNDTRACK_EFFECT,
                GlobalVariables.MAIN_SOUNDTRACK_EFFECT,
                1,
                () => {
                    AudioManager.instance.isTransitioningSoundtrack = false;
                    AudioManager.instance.isPlayingInGameMusic = true;
                    AudioManager.instance.isPlayingMenuMusic = false;
                }
            );
            AudioManager.instance.isTransitioningSoundtrack = true;
        }
        else if (!AudioManager.instance.isPlayingInGameMusic) {
            AudioManager.PlaySound(GlobalVariables.MAIN_SOUNDTRACK_EFFECT);
            AudioManager.instance.isPlayingInGameMusic = true;
            AudioManager.instance.isPlayingMenuMusic = false;
        }
    }
#endregion

    // public void PlayRandomEffect() {
    //     Sound[] soundGroup = Array.FindAll(sounds, sound => sound.isEffect == true);
    //     string soundName = sounds[UnityEngine.Random.Range(0, soundGroup.Length - 1)].name;
    //     PlaySound(soundName);
    // }
}
