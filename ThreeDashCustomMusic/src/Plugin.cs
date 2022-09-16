using BepInEx;

using SixDash;

namespace ThreeDashCustomMusic;

[BepInPlugin("mod.cgytrus.plugins.3dashcustommusic", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("mod.cgytrus.plugins.sixdash", "0.2.0")]
public class Plugin : BaseUnityPlugin {
    private void Awake() {
        CustomMusic.Setup(Logger);

        Logger.LogInfo("Applying patches");
        Util.ApplyAllPatches();
    }
}
