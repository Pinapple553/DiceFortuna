using UnityEngine;

public class SceneUIActions : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.Instance.LoadScene(sceneName);
    }
    public void CloseScene()
    {
        SceneManager.Instance.CloseScene();
    }
}
