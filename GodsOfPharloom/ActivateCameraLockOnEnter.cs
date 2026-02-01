using BepInEx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Reflection;
using HarmonyLib;
using Unity.Mathematics;
using Newtonsoft.Json;
using GlobalEnums;
using System.Collections;
using HutongGames.PlayMaker.Actions;
using GenericVariableExtension;
using HutongGames.PlayMaker;

namespace Gods_Of_Pharloom;

public class ActiveCameraLockOnEnter : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collider)
    {
        var hornetCollider = HeroController.instance.gameObject.GetComponent<BoxCollider2D>();
        if(collider == hornetCollider)
        {
            var camController = GameCameras.instance.cameraController;
            var camLock = this.GetComponent<CameraLockArea>();

            camLock.enabled = true;
        }
    }
    void OnTriggerStay2D(Collider2D collider)
    {
        var hornetCollider = HeroController.instance.gameObject.GetComponent<BoxCollider2D>();
        if(collider == hornetCollider)
        {
            var camController = GameCameras.instance.cameraController;
            var camLock = this.GetComponent<CameraLockArea>();

            camLock.enabled = true;
        }
    }
    void OnTriggerExit2D(Collider2D collider)
    {
        var hornetCollider = HeroController.instance.gameObject.GetComponent<BoxCollider2D>();
        if(collider == hornetCollider)
        {
            var camController = GameCameras.instance.cameraController;
            var camLock = this.GetComponent<CameraLockArea>();

            camLock.enabled = false;
        }
    }
}