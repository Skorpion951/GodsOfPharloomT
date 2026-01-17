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
    public Action submitAction;
    public Action OnSelected;
    public Action OnDeselected;
    public CustomButton[] selectables;
    public GameObject GO;
}
public class CustomMenu
{
    public static List<CustomMenu> menus = new List<CustomMenu>();
    public bool isActivated = false;
    public string menuName;
    public Action<CustomMenu> OnActivate;
    public Action<CustomMenu> OnDeactivate;
    public CustomButton[] buttons;
    public GameObject pointer;
    public CustomButton currentButton;

    void Start()
    {
        menus.Add(this);
    }
    public static void Reset()
    {
        menus = new List<CustomMenu>();
    }
}