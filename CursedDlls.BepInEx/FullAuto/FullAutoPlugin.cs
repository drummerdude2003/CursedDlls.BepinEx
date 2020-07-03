using BepInEx;

namespace Cursed.FullAuto
{
    [BepInPlugin("com.drummerdude2003.curseddll", "Full Auto", "1.0")]
    public class FullAutoPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Hooks.InstallHooks();
        }
    }
}