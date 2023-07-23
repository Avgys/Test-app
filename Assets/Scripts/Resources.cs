using UnityEngine;

namespace SimUnity.Management
{
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance;

        public Sprite AddSprite;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance ??= this;
        }
    }
}
