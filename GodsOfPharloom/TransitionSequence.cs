using System.Resources;
using GlobalEnums;
using Gods_Of_Pharloom;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UniverseLib.Utility;

namespace Gods_Of_Pharloom;

public class TransitionSequence
{
    public static ParticleSystem transitionParticles;
    public static AudioSource transitionStartAudio;
    public static AudioSource transitionEndAudio;
    public static bool audioStarted = false;

    public static void Init()
    {
        GodsOfPharloomMod.instance.StartCoroutine(IInit());
    }
    static IEnumerator IInit()
    {
        if(!transitionParticles.IsNullOrDestroyed()) GameObject.Destroy(transitionParticles);

        GameCameras instance = null;
        while (true){
            instance = GameCameras.instance;
            if(!instance.IsNullOrDestroyed()) break;
            else yield return null;
        }

        var mainCamera = instance.mainCamera.transform;
        var camPos = mainCamera.position;
        var particlesObj = GameObject.Instantiate((GameObject)Preload.bundleResources["Transition Animation"], parent: mainCamera);
        particlesObj.transform.position = new Vector3(camPos.x, camPos.y, -2f);

        transitionParticles = particlesObj.GetComponent<ParticleSystem>();

        var go = new GameObject();
        GameObject.DontDestroyOnLoad(go);

        transitionStartAudio = go.AddComponent<AudioSource>();
        transitionEndAudio = go.AddComponent<AudioSource>();

        transitionStartAudio.priority = 90;
        transitionEndAudio.priority = 90;
        transitionStartAudio.maxDistance = 9999;
        transitionEndAudio.maxDistance = 9999;

        transitionStartAudio.clip = (AudioClip)Preload.bundleResources["gg_room_transition"];
        transitionEndAudio.clip = (AudioClip)Preload.bundleResources["gg_transition_out"];
    }

    public static void FadeAudio(AudioSource audio, float time)
    {
        GodsOfPharloomMod.instance.StartCoroutine(IFadeAudio(audio, time));
    }
    public static IEnumerator IFadeAudio(AudioSource audio, float time)
    {
        float startVolume = audio.volume;

        while (audio.volume > 0f)
        {
            audio.volume -= startVolume * Time.unscaledDeltaTime / time;

            yield return null;
        }

        audio.volume = startVolume;
        audio.Stop();
    }

    public static void Play()
    {
        if (transitionParticles.IsNullOrDestroyed())
        {
            Init();
            return;
        }

        transitionParticles.Play();
    }
    public static void Pause()
    {
        if (transitionParticles.IsNullOrDestroyed())
        {
            Init();
            return;
        }

        transitionParticles.Pause(true);
    }
    public static void Stop(bool stopWithClear = false)
    {
        if (transitionParticles.IsNullOrDestroyed())
        {
            Init();
            return;
        }

        if(stopWithClear) transitionParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        else transitionParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
    public static void SetVisible(bool val)
    {
        if (transitionParticles.IsNullOrDestroyed())
        {
            Init();
            return;
        }

        transitionParticles.gameObject.GetComponent<ParticleSystemRenderer>().forceRenderingOff = !val;
    }
}