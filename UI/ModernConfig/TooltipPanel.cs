using ImproveGame.UI.ModernConfig.OptionElements;
using ImproveGame.UIFramework.SUIElements;
using System.Collections;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace ImproveGame.UI.ModernConfig;

public delegate void PreviewDrawing(UIElement element, ModConfig currentConfig, PropertyFieldWrapper varibleInfo, object item, IList list, int index);
public class TooltipPanel : SUIPanel
{
    internal static TooltipPanel Instance;
    public TooltipTextElement Text;
    public SUIDrawingImage PreviewDrawer;
    public ModernConfigOption currentOption;
    public static event PreviewDrawing GlobalDrawing;
    public TooltipPanel(Color color) : base(color, color)
    {
        Instance = this;
        EnableBlur = false;

        SetPadding(16, 10, 16, 10);
        Text = new TooltipTextElement();
        Text.JoinParent(this);
        PreviewDrawer = new SUIDrawingImage(view =>
        {
            if (currentOption == null) return;

            if (!CategorySidePanel.ModdedPreviews.TryGetValue(currentOption.VariableInfo, out PreviewDrawing drawingMethod))
                foreach (var pair in CategorySidePanel.ModdedPreviews)
                {
                    if (pair.Key.fieldInfo == currentOption.VariableInfo.fieldInfo && pair.Key.IsField)
                        drawingMethod = pair.Value;
                    else if (pair.Key.propertyInfo == currentOption.VariableInfo.propertyInfo && pair.Key.IsProperty)
                        drawingMethod = pair.Value;
                    if (drawingMethod != null) break;
                }
            drawingMethod?.Invoke(view, currentOption.Config, currentOption.VariableInfo, currentOption.Item, currentOption.List, currentOption.index);

            GlobalDrawing?.Invoke(view, currentOption.Config, currentOption.VariableInfo, currentOption.Item, currentOption.List, currentOption.index);
        })
        {
            Width = new(0, 1),
            Height = new(0, 1),
            RelativeMode = UIFramework.BaseViews.RelativeMode.None
        };
        PreviewDrawer.JoinParent(this);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        var dimension = PreviewDrawer.GetDimensions();
        if (Instance.Text.TextOrKey == "")
            Instance.Text.TextOrKey = MyUtils.GetText("ModernConfig.NoTooltip");
        base.Draw(spriteBatch);
        Instance.Text.TextOrKey = ConfigOptionsPanel.CurrentCategory?.Tooltip ?? "";
    }
    public static void SetText(string text)
    {
        Instance.Text.TextOrKey = text;
    }

    public static void SetOption(ModernConfigOption option) => Instance.currentOption = option;
}