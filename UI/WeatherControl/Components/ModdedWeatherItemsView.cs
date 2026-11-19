using ImproveGame.Content.Functions.WeatherControl;
using ImproveGame.UIFramework.BaseViews;

namespace ImproveGame.UI.WeatherControl.Components;

/// <summary>
/// 模组项默认网格栏
/// 浮在艺术画面左上角的天空区域，让外部模组的控制项跟原版控件一样长在场景里
/// </summary>
public class ModdedWeatherItemsView : View
{
    private int _builtCount = -1;

    public ModdedWeatherItemsView()
    {
        SetPadding(0f);
        IsAdaptiveWidth = true;
        Height.Set(32f, 0f);
        Left.Pixels = 8f;
        Top.Pixels = 8f;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        var registry = WeatherControlRegistry.Instance;
        if (registry is null) return;

        int count = 0;
        foreach (var _ in registry.ModdedGridControls()) count++;

        if (count != _builtCount)
            Rebuild(registry);
    }

    private void Rebuild(WeatherControlRegistry registry)
    {
        RemoveAllChildren();
        _builtCount = 0;

        foreach (var control in registry.ModdedGridControls())
        {
            var slot = new WeatherControlSlot(control)
            {
                RelativeMode = RelativeMode.Horizontal,
                Spacing = new Vector2(4f, 0f),
            };
            slot.JoinParent(this);
            _builtCount++;
        }

        // 通知父级重算，让自适应宽度的 MainPanel 和槽位坐标都更新
        Parent?.Recalculate();
    }
}
