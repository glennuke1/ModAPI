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

            ModConsole.Log($"<color=white>[ModApiLoader]</color> <color=yellow>modapi v{ModApi.VersionInfo.version} build {ModApi.VersionInfo.build}: Loading</color>");

            while (ModClient.getPOV == null)
            {
                yield return new WaitForSeconds(0.5f);
            }

            ModClient.getPartManager.load();
            ModClient.getBoltManager.load();

            while (ModClient.getBoltManager.assetsLoaded == false)
            {
                yield return new WaitForSeconds(0.5f);
            }

            while (ModClient.getPartManager.loaded == false)
            {
                yield return new WaitForSeconds(0.5f);
            }

            initialized = true;
            ModConsole.Log($"<color=white>[ModApiLoader]</color> <color=green>modapi v{ModApi.VersionInfo.version} build {ModApi.VersionInfo.build}: Loaded Successfully!</color>");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads modapi.
        /// </summary>
        internal static void injectModApi()
        {
            bool loaded = ModClient.loaded;
            if (!loaded)
            {
                initialized = false;
                ModClient.setModApiGo(new GameObject("Mod API Loader"));
                GameObject.DontDestroyOnLoad(ModClient.modapiGo);
                ModClient.levelManager = ModClient.modapiGo.AddComponent<LevelManager>();
            }
        }

        #endregion
    }
}
