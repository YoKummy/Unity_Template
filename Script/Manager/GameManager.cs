using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static GameManager GetInstance()
    {
        if (instance == null)
        {
            Debug.LogError("DialogueManager instance is null! Ensure one exists in the scene.");
        }
        return instance;
    }
    public void loadScene(int sceneIndex)
    {
        SceneManager.LoadSceneAsync(sceneIndex);
    }

}
