using MelonLoader;

namespace CartridgeCaseDespawnUnlimiter
{
    public class Main : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MethodPatches.Initialize();
            UI.Initialize();
        }
    }
}
