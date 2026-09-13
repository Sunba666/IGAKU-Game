using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class SimpleSceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("SimpleSceneLoader: Scene Name 没有填写。", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"SimpleSceneLoader: 场景 '{sceneName}' 不在 Build Settings / Build Profiles 中，或名称不一致。",
                this);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
