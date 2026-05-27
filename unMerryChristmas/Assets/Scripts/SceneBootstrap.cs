using UnityEngine;
public class SceneBootstrap : MonoBehaviour
{
    private void Start()
    {
        GameObject playerGo = GameObject.FindWithTag("Player");
        if (playerGo != null)
            SceneEntryPoint.PositionPlayer(playerGo.transform);
    }
}
