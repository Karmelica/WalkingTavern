using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] worldSpacePrefabs;
    
        private void Start()
        {
            InstantiateWorldSpacePrefabs();
        }

        private void InstantiateWorldSpacePrefabs()
        {
            foreach (var prefab in worldSpacePrefabs)
            {
                Instantiate(prefab, transform);
            }
        }
    }
}
