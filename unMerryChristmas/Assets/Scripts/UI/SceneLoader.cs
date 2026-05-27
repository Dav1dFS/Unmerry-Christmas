using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string _mainMenuScene = "MainMenu";

    public void LoadMainMenu()
    {
        PlayerFreezeManager.Instance?.SetMenuFrozen(false);

        UIManager.Instance?.CloseBook();

        SceneManager.LoadScene(_mainMenuScene);
    }
}