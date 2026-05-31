using UnityEngine;
using UnityEngine.SceneManagement;

public static class HW23AutoLauncher
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Launch()
    {
        if (SceneManager.GetActiveScene().name != "HW23_DataSaveLoad")
        {
            return;
        }

        Debug.Log("[HW23 AutoLauncher] Play mode started in HW23_DataSaveLoad.");

        HW23DemoSceneBootstrap bootstrap = Object.FindObjectOfType<HW23DemoSceneBootstrap>();
        if (bootstrap == null)
        {
            GameObject managerObject = new GameObject("HW23_DataSaveLoad_RuntimeManager");
            managerObject.AddComponent<WorldSaveLoadManager>();
            bootstrap = managerObject.AddComponent<HW23DemoSceneBootstrap>();
        }

        bootstrap.Initialize();
    }
}
