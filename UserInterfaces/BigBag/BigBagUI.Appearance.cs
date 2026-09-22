using SilkyUIFramework;
using SilkyUIFramework.Elements;
using SilkyUIFramework.StyleSystem;
using Terraria.ModLoader.UI;

namespace ImproveGame.UserInterfaces.BigBag;

public partial class BigBagUI
{
    private (UITextView View, LocalizedText Text)[] _localizedLabels = [];
    private (UIView View, LocalizedText Text)[] _buttonTooltips = [];

    private void InitializeAppearance()
    {
        BorderColor = SUIColor.Border;
        BackgroundColor = SUIColor.Background * 0.75f;

        SettingsHeader.ControlTarget = this;

        Title.UseDeathText();
        SettingsTitle.UseDeathText();

        SettingsPanel.BorderColor = SUIColor.Border;
        SettingsPanel.BackgroundColor = SUIColor.Background;

        InitializeLabels();

        StyleButton(QuickButton, QuickIcon, MyUtils.GetTexture("UI/Quick"));
        StyleButton(PutButton, PutIcon, MyUtils.GetTexture("UI/Put"));
        StyleButton(ReplenishButton, ReplenishIcon, Main.Assets.Request<Texture2D>("Images/UI/ChestStack_0"));
        StyleButton(SortButton, SortIcon, Main.Assets.Request<Texture2D>("Images/UI/Sort_0"));
        StyleButton(SettingsButton, SettingsIcon, ModAsset.Setting);
    }

    private void InitializeLabels()
    {
        _localizedLabels =
        [
            (Title, Language.GetText("Mods.ImproveGame.SuperVault.Name")),
            (SettingsTitle, Language.GetText("LegacyMenu.14")),
            (RecipesLabel, Language.GetText("Mods.ImproveGame.SuperVault.Synthesis")),
            (SmartGrabLabel, Language.GetText("Mods.ImproveGame.SuperVault.SmartPickup")),
            (AutoGrabLabel, Language.GetText("Mods.ImproveGame.SuperVault.OverflowPickup"))
        ];

        _buttonTooltips =
        [
            (QuickButton, Lang.inter[29]),
            (PutButton, Lang.inter[30]),
            (ReplenishButton, Lang.inter[31]),
            (SortButton, Language.GetText("Mods.ImproveGame.SuperVault.Sort")),
            (SettingsButton, Language.GetText("LegacyMenu.14"))
        ];

        RefreshLabels();
    }

    private void RefreshLabels()
    {
        foreach (var (view, text) in _localizedLabels)
            view.Text = text.Value;
    }

    private void DrawButtonTooltip()
    {
        foreach (var (view, text) in _buttonTooltips)
        {
            if (view.IsMouseHovering)
            {
                UICommon.TooltipMouseText(text.Value);
                return;
            }
        }
    }

    private static void SetIcon(SUIImage image, Asset<Texture2D> texture, float sizeLimit)
    {
        image.Texture2D = texture;
        image.ImageScale = new Vector2(Math.Min(1f, sizeLimit / Math.Max(texture.Value.Width, texture.Value.Height)));
    }

    private static void StyleButton(UIElementGroup button, SUIImage icon, Asset<Texture2D> texture)
    {
        SetIcon(icon, texture, 24f);

        button.StyleSheet.SetStyle(UIElementState.Normal,
            new StyleDefinition()
                .BorderColor(SUIColor.Border)
                .Background(SUIColor.Background * 0.75f)
        );

        button.StyleSheet.SetStyle(UIElementState.Hover,
            new StyleDefinition()
                .BorderColor(SUIColor.Highlight)
                .Background(SUIColor.Highlight * 0.25f)
        );

        button.MouseEnter += (_, _) => SoundEngine.PlaySound(SoundID.MenuTick);
    }
}
