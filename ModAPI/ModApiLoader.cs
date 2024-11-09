using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MSCLoader;

namespace eightyseven.ModApi
{
    /// <summary>
    /// Represents behaviour to load modapi stuff. eg bolt assets, load physics raycaster behaviour
    /// </summary>
    public static class ModApiLoader
    {
        // Written, 11.07.2022

        internal static bool initialized;

        #region IEnumerators

        internal static IEnumerator loadModApi()
        {
            // Written, 11.07.2022

            Debug.Log($"[ModApiLoader] modapi v{ModApi.VersionInfo.version}: Loading");

            while (ModClient.getPOV == null)
            {
                yield return new WaitForSeconds(0.5f);
            }

            ModClient.getPartManager.load();
            ModClient.getBoltManager.load();

            initialized = true;
            Debug.Log($"[ModApiLoader] modapi v{ModApi.VersionInfo.version}: Loaded");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads modapi.
        /// </summary>
        internal static void injectModApi()
        {
            // Written, 11.09.2023
            bool loaded = ModClient.loaded;
            Debug.Log("MODAPI_MAIN: Load" + (loaded ? "ed" : "ing"));
            if (!loaded)
            {
                initialized = false;
                ModClient.setModApiGo(new GameObject("Mod API Loader"));
                GameObject.DontDestroyOnLoad(ModClient.modapiGo);
                ModClient.levelManager = ModClient.modapiGo.AddComponent<LevelManager>();

                ConsoleCommand.Add(new ConsoleCommands());
            }
        }

        internal static void addDevMode()
        {
            // Written, 25.08.2022

            if (ModClient.devModeBehaviour == null)
            {
                ModClient.devModeBehaviour = ModClient.modapiGo.AddComponent<DevMode>();
            }
        }   

        #endregion
    }
}
