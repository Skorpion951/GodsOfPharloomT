using BepInEx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Reflection;
using UnityEngine.AddressableAssets;
using HarmonyLib;

namespace Gods_Of_Pharloom
{
    public partial class GodsOfPharloomMod : BaseUnityPlugin
    {
        public static Action<Scene, Scene> afterSceneLoadedGetScenes;
        public static Action afterSceneLoaded;
        public static Scene? lastLoadedScene = null;
        public static MethodInfo RecordBeginTime = AccessTools.Method(typeof(SceneLoad), "RecordBeginTime");
        public static MethodInfo RecordEndTime = AccessTools.Method(typeof(SceneLoad), "RecordEndTime");
        public static MethodInfo LocalTryClearMemory = AccessTools.Method(typeof(SceneLoad), "LocalTryClearMemory");

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SceneLoad), "Begin")]
        private static bool Prefix(SceneLoad __instance)
        {
            GodsOfPharloomMod.currentSceneName = __instance.TargetSceneName;

            var runner = __instance.GetType().GetField("runner", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            ((MonoBehaviour)runner.GetValue(__instance)).StartCoroutine(BeginRoutine_Patched(__instance));
            return false;
        }

        private static System.Collections.IEnumerator BeginRoutine_Patched(SceneLoad __instance)
        {
            Log.LogInfo("PAAAAAAAAATTTTTCCCCHHHEEEDDD");
            FieldInfo operationHandle = __instance.GetType().GetField("operationHandle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo _tempOps = typeof(SceneLoad).GetField("_tempOps", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            FieldInfo runner = __instance.GetType().GetField("runner", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Log.LogInfo("PAAAAAAAAATTTTTCCCCHHHEEEDDD2");

            Func<MethodInfo, object[], object> InvokeMethod = (method, obj) => method.Invoke(__instance, obj);

            string address = "Scenes/" + __instance.SceneLoadInfo.SceneName;

            int sceneTypeTo = 0;

            var scene = customScenes.Find((item) => item.sceneName == __instance.TargetSceneName);
            if(scene == null) sceneTypeTo = 0;
            else if(scene.isSkongScene) sceneTypeTo = 1;
            else sceneTypeTo = 2;

            bool wasPreloaded = false;

            UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle
                        <UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>? 
                            preLoadOperation = null;

            SceneAdditiveLoadConditional.LoadInSequence = true;

            AsyncOperation op = null; //custom scene handler

            lastLoadedScene = null;

            if(sceneTypeTo == 0 || sceneTypeTo == 1)
            {
                preLoadOperation = ScenePreloader.TakeSceneLoadOperation(address, LoadSceneMode.Additive);
                
                wasPreloaded = preLoadOperation.HasValue;
            }
            else
            {
                op = SceneManager.LoadSceneAsync(scene.sceneName, LoadSceneMode.Additive);
                // op.allowSceneActivation = false;
            }

            InvokeMethod(RecordBeginTime, new object[] {SceneLoad.Phases.FetchBlocked});
            while (!__instance.IsFetchAllowed)
            {
                yield return null;
            }
            Log.LogInfo("PAAAAAAAAATTTTTCCCCHHHEEEDDD3");
            InvokeMethod(RecordEndTime, new object[] {SceneLoad.Phases.FetchBlocked});
            bool hasClearedMemory = false;
            if (/*sceneTypeTo == 0 || sceneTypeTo == 1 && */SceneLoad.IsClearMemoryRequired())
            {
                GameManager.IsCollectingGarbage = true;
                InvokeMethod(RecordBeginTime, new object[] {SceneLoad.Phases.ClearMemPreFetch});
                yield return InvokeMethod(LocalTryClearMemory, new object[] {true, false});
                hasClearedMemory = true;
                InvokeMethod(RecordEndTime, new object[] {SceneLoad.Phases.ClearMemPreFetch});//RecordEndTime(SceneLoad.Phases.ClearMemPreFetch);
            }
            InvokeMethod(RecordBeginTime, new object[] {SceneLoad.Phases.Fetch});//RecordBeginTime(SceneLoad.Phases.Fetch);
            int priority = __instance.SceneLoadInfo.AsyncPriority;
            
            if(sceneTypeTo == 0 || sceneTypeTo == 1)
            {
                if (CheatManager.OverrideSceneLoadPriority)
                {
                    priority = CheatManager.SceneLoadPriority;
                }
                if (wasPreloaded)
                {
                    operationHandle.SetValue(__instance, preLoadOperation.Value);//operationHandle = preLoadOperation.Value;
                }
                else if (__instance.SceneLoadInfo.SceneResourceLocation != null)
                {
                    operationHandle.SetValue(__instance, Addressables.LoadSceneAsync(__instance.SceneLoadInfo.SceneResourceLocation, LoadSceneMode.Additive, activateOnLoad: false, priority));//operationHandle = Addressables.LoadSceneAsync(__instance.SceneLoadInfo.SceneResourceLocation, LoadSceneMode.Additive, activateOnLoad: false, priority);
                }
                else
                {
                    operationHandle.SetValue(__instance, Addressables.LoadSceneAsync(address, LoadSceneMode.Additive, activateOnLoad: false, priority));//operationHandle = Addressables.LoadSceneAsync(address, LoadSceneMode.Additive, activateOnLoad: false, priority);
                }
                yield return (UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>)
                    (operationHandle.GetValue(__instance));
            }

            ////////////////////////////////////////////////////////////
            while(!Preload.isInitialized) yield return null; // wait for preloads

            if(sceneTypeTo == 1 || sceneTypeTo == 2)
            {
                if(sceneTypeTo == 2)
                {
                    yield return op;
                    currentScene = SceneManager.GetSceneAt(SceneManager.sceneCount-1);
                    scene.Activate(currentScene);
                }
                else{
                    currentScene = ((UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>)
                        (operationHandle.GetValue(__instance))).Result.Scene;
                    
                    scene.Activate(currentScene);
                }

                while(!scene.isSceneActive)
                {
                    yield return null;
                }
                scene.isSceneActive = false;
                scene.isPreloading = false;
                Log.LogInfo("Activated2");
            }

            afterSceneLoaded?.Invoke();
            Log.LogInfo("Activated3");
            /////////////////////////////////////////////////////////////

            InvokeMethod(RecordEndTime, new object[] {SceneLoad.Phases.Fetch});//RecordEndTime(SceneLoad.Phases.Fetch);
            Log.LogInfo("PAAAAAAAAATTTTTCCCCHHHEEEDD4");

            var FetchCompleteField = AccessTools.Field(typeof(SceneLoad), "FetchComplete");
            var FetchCompleteDel = (SceneLoad.FetchCompleteDelegate)FetchCompleteField.GetValue(__instance);
            if (FetchCompleteDel != null)//if (this.FetchComplete != null)
            {
                try
                {
                    FetchCompleteDel();//this.FetchComplete();
                }
                catch (Exception ex)
                {
                    Debug.LogError("Exception in responders to SceneLoad.FetchComplete. Attempting to continue load regardless.");
                    CheatManager.LastErrorText = ex.ToString();
                    Debug.LogException(ex);
                }
            }
            Log.LogInfo("PAAAAAAAAATTTTTCCCCHHHEEEDDD4.1");
            InvokeMethod(RecordBeginTime, new object[] {SceneLoad.Phases.ActivationBlocked});//RecordBeginTime(SceneLoad.Phases.ActivationBlocked);
            if (!wasPreloaded && ScenePreloader.HasPendingOperations)
            {
                yield return ((MonoBehaviour)runner.GetValue(__instance)).StartCoroutine(ScenePreloader.ForceEndPendingOperations());//runner.StartCoroutine(ScenePreloader.ForceEndPendingOperations());
            }
            while (!__instance.IsActivationAllowed)
            {
                yield return null;
            }
            Log.LogInfo("PAAAAAAAAATTTTTCCCCHHHEEEDDD4.1.1");

            Scene from = SceneManager.GetActiveScene();
            Scene to = SceneManager.GetSceneByName(__instance.TargetSceneName);

            SceneAdditiveLoadConditional.Unload(SceneManager.GetActiveScene(), ((List<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>)
                (_tempOps.GetValue(null)))/*_tempOps*/);

            Log.LogInfo("PAAAAAAAAATTTTTCCCCHHHEEEDDD4.1.2");
            InvokeMethod(RecordEndTime, new object[] {SceneLoad.Phases.ActivationBlocked});//RecordEndTime(SceneLoad.Phases.ActivationBlocked);
            InvokeMethod(RecordBeginTime, new object[] {SceneLoad.Phases.Activation});//RecordBeginTime(SceneLoad.Phases.Activation);
            Log.LogInfo("PAAAAAAAAATTTTTCCCCHHHEEEDDD4.2");

            var WillActivateField = AccessTools.Field(typeof(SceneLoad), "WillActivate");
            var WillActivateDel = (SceneLoad.WillActivateDelegate)WillActivateField.GetValue(__instance);
            if (WillActivateDel != null) //if (this.WillActivate != null)
            {
                try
                {
                    WillActivateDel(); //this.WillActivate();
                }
                catch (Exception ex2)
                {
                    Debug.LogError("Exception in responders to SceneLoad.WillActivate. Attempting to continue load regardless.");
                    CheatManager.LastErrorText = ex2.ToString();
                    Debug.LogException(ex2);
                }
            }
            Log.LogInfo("PAAAAAAAAATTTTTCCCCHHHEEEDDD5");
            if(sceneTypeTo == 0 || sceneTypeTo == 1)
            {
                if (((UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>)
                    operationHandle.GetValue(__instance)).OperationException != null) //if (operationHandle.OperationException != null)
                {
                    Debug.LogError("Exception in scene load OperationHandle:");
                    CheatManager.LastErrorText = ((UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>)
                        (operationHandle.GetValue(__instance))).OperationException.ToString(); //operationHandle.OperationException.ToString();
                    Debug.LogException(((UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>)
                        (operationHandle.GetValue(__instance))).OperationException /*operationHandle.OperationException*/);
                }
                yield return ((UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>)
                        (operationHandle.GetValue(__instance))).Result.ActivateAsync(); //operationHandle.Result.ActivateAsync();
            }
            
            InvokeMethod(RecordEndTime, new object[] {SceneLoad.Phases.Activation}); //RecordEndTime(SceneLoad.Phases.Activation);

            var ActivationCompleteField = AccessTools.Field(typeof(SceneLoad), "ActivationComplete");
            var ActivationCompleteDel = (SceneLoad.ActivationCompleteDelegate)ActivationCompleteField.GetValue(__instance);
            if (ActivationCompleteDel != null) //if (this.ActivationComplete != null)
            {
                try
                {
                    ActivationCompleteDel(); //this.ActivationComplete();
                }
                catch (Exception ex3)
                {
                    Debug.LogError("Exception in responders to SceneLoad.ActivationComplete. Attempting to continue load regardless.");
                    CheatManager.LastErrorText = ex3.ToString();
                    Debug.LogException(ex3);
                }
            }
            Log.LogInfo("PAAAAAAAAATTTTTCCCCHHHEEEDDD6");
            foreach (UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>
                tempOp in ((List<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>)
                _tempOps.GetValue(null))) //foreach (AsyncOperationHandle<SceneInstance> tempOp in _tempOps)
            {
                yield return tempOp;
            }
            ((List<UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance>>)
                _tempOps.GetValue(null)).Clear(); //_tempOps.Clear();
            
            var _assetUnloadOps = ((List<AsyncOperation>)
                __instance.GetType().GetField("_assetUnloadOps", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .GetValue(__instance));
                    Log.LogInfo("PAAAAAAAAATTTTTCCCCHHHEEEDDD7");
            while (_assetUnloadOps.Count > 0)
            {
                int index = _assetUnloadOps.Count - 1;
                AsyncOperation assetUnloadOp = _assetUnloadOps[index];
                _assetUnloadOps.RemoveAt(index);
                if (assetUnloadOp != null && !assetUnloadOp.isDone)
                {
                    float t = 5f;
                    while (!assetUnloadOp.isDone && t > 0f)
                    {
                        t -= Time.deltaTime;
                        yield return null;
                    }
                    if (!assetUnloadOp.isDone)
                    {
                        Debug.LogError("Timed out while waiting for asset unload.");
                    }
                }
            }
            Log.LogInfo("PAAAAAAAAATTTTTCCCCHHHEEEDDD7.1");
            if (__instance.IsUnloadAssetsRequired || SceneLoad.IsClearMemoryRequired())
            {
                GameManager.IsCollectingGarbage = true;
                InvokeMethod(RecordBeginTime, new object[] {SceneLoad.Phases.ClearMemPostActivation});//RecordBeginTime(SceneLoad.Phases.ClearMemPostActivation);
                yield return SceneLoad.TryClearMemory(!hasClearedMemory);
                InvokeMethod(RecordEndTime, new object[] {SceneLoad.Phases.ClearMemPostActivation});//RecordEndTime(SceneLoad.Phases.ClearMemPostActivation);
            }
            else if (__instance.IsGarbageCollectRequired)
            {
                GameManager.IsCollectingGarbage = true;
                InvokeMethod(RecordBeginTime, new object[] {SceneLoad.Phases.GarbageCollect});//RecordBeginTime(SceneLoad.Phases.GarbageCollect);
                GCManager.Collect();
                InvokeMethod(RecordEndTime, new object[] {SceneLoad.Phases.GarbageCollect});//RecordEndTime(SceneLoad.Phases.GarbageCollect);
            }
            GameManager.IsCollectingGarbage = false;

            var CompleteField = AccessTools.Field(typeof(SceneLoad), "Complete");
            var CompleteDel = (SceneLoad.CompleteDelegate)CompleteField.GetValue(__instance);
            if (CompleteDel != null) //if (this.Complete != null)
            {
                try
                {
                    CompleteDel(); //this.Complete();
                }
                catch (Exception ex4)
                {
                    Debug.LogError("Exception in responders to SceneLoad.Complete. Attempting to continue load regardless.");
                    CheatManager.LastErrorText = ex4.ToString();
                    Debug.LogException(ex4);
                }
            }
            Log.LogInfo("PAAAAAAAAATTTTTCCCCHHHEEEDDD8");
            InvokeMethod(RecordBeginTime, new object[] {SceneLoad.Phases.StartCall});//RecordBeginTime(SceneLoad.Phases.StartCall);
            yield return null;
            InvokeMethod(RecordEndTime, new object[] {SceneLoad.Phases.StartCall});//RecordEndTime(SceneLoad.Phases.StartCall);

            var StartCalledField = AccessTools.Field(typeof(SceneLoad), "StartCalled");
            var StartCalledDel = (SceneLoad.StartCalledDelegate)StartCalledField.GetValue(__instance);
            if (StartCalledDel != null) //if (this.StartCalled != null)
            {
                try
                {
                    StartCalledDel(); //this.StartCalled();
                }
                catch (Exception ex5)
                {
                    Debug.LogError("Exception in responders to SceneLoad.StartCalled. Attempting to continue load regardless.");
                    CheatManager.LastErrorText = ex5.ToString();
                    Debug.LogException(ex5);
                }
            }
            if (SceneAdditiveLoadConditional.ShouldLoadBoss)
            {
                InvokeMethod(RecordBeginTime, new object[] {SceneLoad.Phases.LoadBoss});//RecordBeginTime(SceneLoad.Phases.LoadBoss);
                yield return ((MonoBehaviour)runner.GetValue(__instance)).StartCoroutine(SceneAdditiveLoadConditional.LoadAll()); //yield return runner.StartCoroutine(SceneAdditiveLoadConditional.LoadAll());
                InvokeMethod(RecordEndTime, new object[] {SceneLoad.Phases.LoadBoss});//RecordEndTime(SceneLoad.Phases.LoadBoss);
                try
                {
                    var BossLoadedField = AccessTools.Field(typeof(SceneLoad), "BossLoaded");
                    var BossLoadedDel = (SceneLoad.BossLoadCompleteDelegate)BossLoadedField.GetValue(__instance);
                    if (BossLoadedDel != null)
                    {
                        BossLoadedDel();
                    }
                    if ((bool)GameManager.instance)
                    {
                        GameManager.instance.LoadedBoss();
                    }
                }
                catch (Exception ex6)
                {
                    Debug.LogError("Exception in responders to SceneLoad.BossLoaded. Attempting to continue load regardless.");
                    CheatManager.LastErrorText = ex6.ToString();
                    Debug.LogException(ex6);
                }
            }
            Log.LogInfo("PAAAAAAAAATTTTTCCCCHHHEEEDDD9");
            try
            {
                ScenePreloader.Cleanup();
            }
            catch (Exception ex7)
            {
                Debug.LogError("Exception in responders to ScenePreloader.Cleanup. Attempting to continue load regardless.");
                CheatManager.LastErrorText = ex7.ToString();
                Debug.LogException(ex7);
            }
            __instance.GetType().GetProperty("IsFinished",BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(__instance, true); //IsFinished = true;
            var FinishField = AccessTools.Field(typeof(SceneLoad), "Finish");
            var FinishDel = (SceneLoad.FinishDelegate)FinishField.GetValue(__instance);
            Log.LogInfo("PAAAAAAAAATTTTTCCCCHHHEEEDDD10");
            if (FinishDel != null) //if (this.Finish != null)
            {
                try
                {
                    FinishDel(); //this.Finish();
                }
                catch (Exception ex8)
                {
                    Debug.LogError("Exception in responders to SceneLoad.Finish. Attempting to continue load regardless.");
                    CheatManager.LastErrorText = ex8.ToString();
                    Debug.LogException(ex8);
                }
            }
        }
    }
}