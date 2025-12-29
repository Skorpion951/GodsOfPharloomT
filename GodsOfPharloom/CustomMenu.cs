using BepInEx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Reflection;
using HarmonyLib;
using HutongGames.PlayMaker;
using UnityEngine.UI;

namespace Gods_Of_Pharloom;

public class CustomButton
{
    public string buttonName;
    public Action buttonAction;
    public GameObject GO;
}
public class CustomMenu : MonoBehaviour
{
    public static List<CustomMenu> menus = new List<CustomMenu>();
    public bool isActivated = false;
    public string menuName;
    public Action<CustomMenu> OnActivate;
    public Action<CustomMenu> OnDeactivate;
    public CustomButton[][] buttons;
    public GameObject pointer;
    public int vertIndex = 0;
    public int horizIndex = 0;
    public CustomButton currentButton;

    void Awake()
    {
        var menuCanvas = this.gameObject.transform.GetChild(0);
        var buttons = menuCanvas.transform.Find("Buttons");
        foreach(Transform button in buttons)
        {
            
        }
    }
    void Start()
    {
        menus.Add(this);
    }
    public static void Reset()
    {
        menus = new List<CustomMenu>();
    }
}