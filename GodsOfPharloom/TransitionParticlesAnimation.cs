using System.Resources;
using GlobalEnums;
using Gods_Of_Pharloom;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;
using UnityEngine.Events;
using HarmonyLib;
using System.Drawing;
using GenericVariableExtension;
using InControl.NativeDeviceProfiles;
using System.Collections;
using UniverseLib.Utility;

namespace Gods_Of_Pharloom;

public class TransitionParticlesAnimation
{
    public static ParticleSystem transitionParticles;

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
}