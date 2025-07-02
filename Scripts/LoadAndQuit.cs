using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadAndQuit : MonoBehaviour
{
    [Header("Scene To Load")]
    [Tooltip("Name (or build index) of the level scene to load.")]
    public string levelSceneName = "Level";

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void OpenLevel()
    {
        SceneManager.LoadScene(levelSceneName);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}