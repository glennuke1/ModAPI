using MSCLoader;
using UnityEngine;

namespace eightyseven.ModApi
{
    public class ModAPI : Mod
    {
        public override string ID => "eightyseven.modapi";

        public override string Version => "1.0";

        public override string Name => "Mod API";

        public override string Author => "0387";

        public override string Description => "Mod API for MSC";

        public override void ModSetup()
        {
            SetupFunction(Setup.OnMenuLoad, Mod_OnMenuLoad);
        }

        void Mod_OnMenuLoad()
        {
            ModApiLoader.injectModApi();
        }
    }
}