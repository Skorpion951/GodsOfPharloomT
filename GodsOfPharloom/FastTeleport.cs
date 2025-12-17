namespace Gods_Of_Pharloom;
public class FastTeleport
{
    public static string sceneName = "Weave_03";
    public static string entryGate = "right1";
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