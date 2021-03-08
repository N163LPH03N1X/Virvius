using UnityEngine;
using System.Collections;
public class AudioSystem : MonoBehaviour
{ 
    public static AudioSystem audioSystem;
    public AudioSource[] altAudioSources;
    public AudioSource M_audioSrc;
    public Transform soundFxPool;
    public GameObject soundPrefab;
    public bool expandPool = true;
    private static float volPercentage = 0;
    private static bool isFading = false;
    private static bool fadeVolIn = true;
    private static float fadeTimeInSeconds = 1;
    public void Awake()
    {
        audioSystem = this;
    }
    public static void PlayGameMusic(AudioClip clip, float pitch, float volume, bool loop)
    {

        audioSystem.M_audioSrc.clip = clip;
        audioSystem.M_audioSrc.pitch = pitch;
        audioSystem.M_audioSrc.volume = volume;
        audioSystem.M_audioSrc.loop = loop;
        audioSystem.M_audioSrc.Play();
    }
    public void Start()
    {
        FadeMusic(true, 5);
    }
    private void Update()
    {
        AudioUpdate(M_audioSrc, fadeVolIn, fadeTimeInSeconds);
    }
    public static void PlayAltAudioSource(int sourceNum, AudioClip clip, float pitch, float volume, bool active)
    {
        if (active)
        {
            audioSystem.altAudioSources[sourceNum].clip = clip;
            audioSystem.altAudioSources[sourceNum].pitch = pitch;
            audioSystem.altAudioSources[sourceNum].volume = volume;
            if (!audioSystem.altAudioSources[sourceNum].isPlaying)
            {
                //audioSystem.altAudioSources[sourceNum].loop = true;
                audioSystem.altAudioSources[sourceNum].Play();
            }
            //else if (sourceNum == 1)
            //{
            //    FadeSource(true, audioSystem.altAudioSources[sourceNum]);
            //    audioSystem.altAudioSources[sourceNum].loop = true;
            //    audioSystem.altAudioSources[sourceNum].PlayOneShot(clip);
            //}
        }
        else
        {
            if (audioSystem.altAudioSources[sourceNum].loop)
                audioSystem.altAudioSources[sourceNum].loop = false;
            audioSystem.altAudioSources[sourceNum].Stop();
        }
    }
    public static void PlayAudioSource(AudioClip clip, float pitch, float volume)
    {
        AudioSource source = audioSystem.GetPooledAudioSource();
        if (source != null)
        {
            source.pitch = pitch;
            source.volume = volume;
            source.clip = clip;
            source.Play();
        }
    }
    public AudioSource GetPooledAudioSource()
    {
        for (int a = 0; a < soundFxPool.childCount; a++)
        {
            AudioSource selectedAudio = soundFxPool.GetChild(a).GetComponent<AudioSource>();
            if (!selectedAudio.isPlaying)
                return selectedAudio;
        }
        if (expandPool)
        {
            GameObject newAudio = Instantiate(soundPrefab, soundFxPool);
            AudioSource selectedNewAudio = newAudio.GetComponent<AudioSource>();
            return selectedNewAudio;
        }
        else
            return null;
    }
    public static void FadeMusic(bool fadeIn, float seconds)
    {
        isFading = true;
        fadeVolIn = fadeIn;
        fadeTimeInSeconds = seconds;
    }
    public static void MusicPlayStop(bool play)
    {
        if(play) audioSystem.M_audioSrc.Play();
        else audioSystem.M_audioSrc.Stop();
    }
    public static void MusicPauseUnPause(bool pause)
    {
        if (pause) { if (audioSystem.M_audioSrc.isPlaying) audioSystem.M_audioSrc.Pause(); }
        else { if (!audioSystem.M_audioSrc.isPlaying) audioSystem.M_audioSrc.Play(); }

    }
    public void OnSelectable(AudioClip clip)
    {
        PlayAudioSource(clip, 1, 1);
    }
    void AudioUpdate(AudioSource audio, bool fadeIn, float time)
    {
        if (isFading == false) return;
        float speedAbsolute = 1.0f / time;  // speed desired by user
        float speedDirection = speedAbsolute * (fadeIn ? +1 : -1);  // + or -
        float deltaVolume = Time.deltaTime * speedDirection;  // how much volume changes in 1 frame
        volPercentage += deltaVolume;  // implement change
        volPercentage = Mathf.Clamp(volPercentage, 0.0f, 1.0f);  // make sure you're in 0..100% 
        audio.volume = volPercentage;
        if (volPercentage == 0.0f || volPercentage == 1.0f) isFading = false;
    }
}
