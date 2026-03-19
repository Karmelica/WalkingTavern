using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class GameLoader : MonoBehaviour
    {
        private void Awake()
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
