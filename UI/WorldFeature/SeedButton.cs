using ImproveGame.Packets.WorldFeatures;
using ImproveGame.UIFramework.BaseViews;
using Steamworks;
using Terraria.ModLoader.UI;

namespace ImproveGame.UI.WorldFeature;

public class SeedButton : TimerView
{
    private readonly Dictionary<SeedType, string> SeedPageNameLookup = new()
    {
        {SeedType.Drunk, "Drunk_world"},
        {SeedType.Bees, "Not_the_bees"},
        {SeedType.Ftw, "For_the_worthy"},
        {SeedType.Anniversary, "Celebrationmk10"},
        {SeedType.DontStarve, "The_Constant"},
        {SeedType.Traps, "No_traps"},
        {SeedType.Remix, "Don%27t_dig_up"},
        {SeedType.Zenith, "Get_fixed_boi"}
    };

    private readonly SeedType _seedType;
    private readonly Asset<Texture2D> _icon;

    private int _glitchFrameCounter;
    private int _glitchFrame;
    private int _glitchVariation;

    public SeedButton(SeedType seedType, int left, int top)
    {
        _seedType = seedType;
        _icon = GetIcon();
        if (seedType is SeedType.Zenith)
            _icon = Main.Assets.Request<Texture2D>("Images/UI/IconEverythingAnimated");

        Left.Set(left, 0f);
        Top.Set(top, 0f);
        Width.Set(60f, 0f);
        Height.Set(60f, 0f);

        OnUpdate += UpdateGlitchAnimation;

        OnMouseOver += (_, _) => SoundEngine.PlaySound(SoundID.MenuTick);

        OnLeftClick += (_, _) => SeedTypePacket.ToggleSeedFlag(_seedType);

        OnRightClick += (_, _) =>
        {
            string url = $"https://terraria.wiki.gg/wiki/{SeedPageNameLookup[_seedType]}";
            if (Language.ActiveCulture.Name is "zh-Hans")
                url = $"https://terraria.wiki.gg/zh/wiki/{SeedPageNameLookup[_seedType]}";

            try
            {
                SteamFriends.ActivateGameOverlayToWebPage(url);
            }
            catch
            {
                Utils.OpenToURL(url);
            }
        };
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        var dimensions = GetDimensions();
        var position = dimensions.Position();

        float opacity = MathHelper.Lerp(0.6f, 1f, HoverTimer.Schedule);
        var color = Color.White * opacity;

        if (_seedType is SeedType.Zenith)
            spriteBatch.Draw(_icon.Value, position, _icon.Frame(7, 16, _glitchVariation, _glitchFrame), color);
        else
            spriteBatch.Draw(_icon.Value, position, color);

        if (MyUtils.GetSeedFeatureFlag(_seedType))
            spriteBatch.Draw(ModAsset.WorldFeatureEnabled.Value, position - new Vector2(2f), color);

        if (IsMouseHovering)
        {
            string mouseText = MyUtils.GetText($"UI.WorldFeature.{_seedType.ToString()}");
            mouseText += "\n" + MyUtils.GetText("UI.WorldFeature.OpenWiki");
            UICommon.TooltipMouseText(mouseText);
        }
    }

    private void UpdateGlitchAnimation(UIElement affectedElement)
    {
        // 敌不动我不动（
        // if (!IsMouseHovering && !Main.zenithWorld) return;

        _ = _glitchFrame;
        int minValue = 3;
        int num = 3;
        if (_glitchFrame == 0)
        {
            minValue = 15;
            num = 120;
        }

        if (++_glitchFrameCounter >= Main.rand.Next(minValue, num + 1))
        {
            _glitchFrameCounter = 0;
            _glitchFrame = (_glitchFrame + 1) % 16;
            if (_glitchFrame is 4 or 8 or 12 && Main.rand.NextBool(3))
                _glitchVariation = Main.rand.Next(7);
        }
    }

    private Asset<Texture2D> GetIcon() =>
        _seedType switch
        {
            SeedType.Drunk => GetSpecialSeedIcon("CorruptionCrimson"),
            SeedType.Bees => GetSeedIcon("NotTheBees"),
            SeedType.Ftw => GetSeedIcon("FTW"),
            SeedType.Anniversary => GetSeedIcon("Anniversary"),
            SeedType.DontStarve => GetSeedIcon("DontStarve"),
            SeedType.Traps => GetSeedIcon("Traps"),
            SeedType.Remix => GetSeedIcon("Remix"),
            SeedType.Zenith => GetSpecialSeedIcon("Everything"),
            _ => GetSeedIcon("")
        };

    private static Asset<Texture2D> GetSpecialSeedIcon(string seed) => Main.Assets.Request<Texture2D>("Images/UI/Icon" +
        (Main.hardMode ? "Hallow" : "") + seed);

    private static Asset<Texture2D> GetSeedIcon(string seed) => Main.Assets.Request<Texture2D>("Images/UI/Icon" +
        (Main.hardMode ? "Hallow" : "") + (Main.ActiveWorldFileData.HasCorruption ? "Corruption" : "Crimson") + seed);
}