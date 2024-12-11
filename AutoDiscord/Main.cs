using JALib.Core;
using UnityModManagerNet;

namespace AutoDiscord;

public class Main : JAMod {

    public static Main Instance;

    public Main(UnityModManager.ModEntry modEntry) : base(modEntry, true, gid: 874916546) {
        AddFeature(new AutoHeadset());
    }

    protected override void OnEnable() {
    }

    protected override void OnDisable() {
    }
}