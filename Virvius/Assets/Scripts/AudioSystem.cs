using UnityEngine;
using System.Collections;
public class AudioSystem : MonoBehaviour
{
    OptSystem optSystem = new OptSystem();
    public static AudioSystem audioSystem;
    public AudioSource[] altAudioSources;
    public AudioSource M_audioSrc;
    public Transform soundFxPool;
    public GameObject soundPrefab;
    IEnumerator sourceRoutine = null;
    //public int amountToPool;
    public bool expandPool = true;
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
    public static void FadeMusic(bool fadeIn)
    {
        FadeSource(fadeIn, audioSystem.M_audioSrc);
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
    public static void FadeSource(bool fadeIn, AudioSource audioSrc)
    {
        if (audioSystem.sourceRoutine != null)
            audioSystem.StopCoroutine(audioSystem.sourceRoutine);
        audioSystem.sourceRoutine = FadeAudioSource(fadeIn, audioSrc);
        audioSystem.StartCoroutine(audioSystem.sourceRoutine);
    }
    static IEnumerator FadeAudioSource(bool fadeIn, AudioSource audioSrc)
    {
        float audioMax = audioSrc.volume;
        float inFactor = fadeIn ? 0 : audioMax;
        float outFactor = fadeIn ? audioMax : 0;
        audioSrc.volume = inFactor;
        for (float f = 0; f < 1; f += 0.01f)
        {
            audioSrc.volume = Mathf.Lerp(inFactor, outFactor, f);
            if (f > 0.99)
                audioSrc.volume = outFactor;
            yield return audioSystem.optSystem.EndOfFrame;
        }
        yield break;
    }
}
