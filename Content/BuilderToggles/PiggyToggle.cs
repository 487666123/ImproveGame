using ImproveGame.Content.Patches.AutoPiggyBank;

namespace ImproveGame.Content.BuilderToggles;

public class PiggyToggle : BuilderToggle
{
    public override string Texture
    {
        get
        {
            if (Main.gameMenu || !Main.player.IndexInRange(Main.myPlayer) ||
                !Main.player[Main.myPlayer].builderAccStatus.IndexInRange(Type))
                return (GetType().Namespace + "." + "Piggy_On").Replace('.', '/');

            string name = "Piggy_";
            name += CurrentState is 0 ? "On" : "Off";
            return (GetType().Namespace + "." + name).Replace('.', '/');
        }
    }

    public override string HoverTexture => Texture + "_Hover";

    public static LocalizedText OnText { get; private set; }
    public static LocalizedText OffText { get; private set; }

    public override bool Active() => Main.LocalPlayer.TryGetModPlayer<AutoMoneyPlayerListener>(out var listener) &&
                                     listener.AutoSaveUnlocked;

    public override int NumberOfStates => 2;

    public static bool AutoSaveEnabled
    {
        get
        {
            var instance = ModContent.GetInstance<PiggyToggle>();
            return instance.Active() && instance.CurrentState is 0;
        }
    }

    public override void SetStaticDefaults()
    {
        // 预加载
        Mod.Assets.Request<Texture2D>(ModAsset.Piggy_OnPath);
        Mod.Assets.Request<Texture2D>(ModAsset.Piggy_OffPath);
        Mod.Assets.Request<Texture2D>(ModAsset.Piggy_On_HoverPath);
        Mod.Assets.Request<Texture2D>(ModAsset.Piggy_Off_HoverPath);

        OnText = this.GetLocalization(nameof(OnText));
        OffText = this.GetLocalization(nameof(OffText));
    }

    public override string DisplayValue()
    {
        return CurrentState == 0 ? OnText.Value : OffText.Value;
    }
}