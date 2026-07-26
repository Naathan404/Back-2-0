using UnityEditor;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private string _levelToLoad = "Level_1";
    public void LoadGame()
    {
        SceneController.Instance.LoadSceneWithName(_levelToLoad);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
