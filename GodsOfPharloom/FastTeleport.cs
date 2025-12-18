namespace Gods_Of_Pharloom;
public class FastTeleport
{
    public static string sceneName = "GG_Pharloom_Hall_Of_Gods";
    public static string entryGate = "door1";
    public static void Start()
    {
        var sceneLoadInfo = new GameManager.SceneLoadInfo
        {
            SceneName = sceneName,
            EntryGateName = entryGate,
            EntrySkip = true,
            Visualization = GameManager.SceneLoadVisualizations.Default
        };

        GameManager.instance.BeginSceneTransition(sceneLoadInfo);
    }
}