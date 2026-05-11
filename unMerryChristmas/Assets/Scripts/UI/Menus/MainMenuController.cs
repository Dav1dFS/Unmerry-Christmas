using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string _gameSceneName = "BackYardScenario";

    public void OnPlayPressed()  => SceneManager.LoadScene(_gameSceneName);
    public void OnQuitPressed()  => Application.Quit();
}
