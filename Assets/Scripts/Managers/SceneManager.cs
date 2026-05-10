using System.Xml.Serialization;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance;
    private string lastScene;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void LoadScene(string sceneName)
   {
       lastScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().ToString();
       UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
    public void CloseScene()
    {
        if (lastScene != null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(lastScene);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

}
