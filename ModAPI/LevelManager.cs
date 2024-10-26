using UnityEngine;

namespace TommoJProductions.ModApi
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance;

        void Start()
        {
            Instance = this;
        }

        private void OnLevelWasLoaded(int level)
        {
            Debug.Log("[ModAPI.Level] A level was loaded: " + Application.loadedLevelName);

            switch (level)
            {
                case 3: //GAME
                    ModClient.refreshCache();

                    ES2.Init();

                    Debug.Log("GAME Loaded");

                    StartCoroutine(ModApiLoader.loadModApi());
                    break;
                case 1: // MAIN MENU
                    break;
            }
        }
    }
}
