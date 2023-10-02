using ImproveGame.Interface.SUIElements;
using Terraria.ModLoader.UI;

namespace ImproveGame.Interface.GUI.OpenBag;

public class SellAllButton : SUIButton
{
    private bool _showingWarning;
    private Action _sellAllCallback;
    
    public SellAllButton(string text, Action sellAllCallback) : base(text) {
        Width.Set(0f, 1f);
        TextAlign = new Vector2(0.5f);
        _sellAllCallback = sellAllCallback;
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);
        
        if (IsMouseHovering)
            UICommon.TooltipMouseText(GetText($"UI.OpenBag.{(_showingWarning ? "AreYouSure" : "SellAll")}.Tooltip"));
    }

    public override void LeftClick(UIMouseEvent evt)
    {
        base.LeftClick(evt);

        // 没物品，出售个啥？
        if (Main.LocalPlayer is null || !Main.LocalPlayer.TryGetModPlayer(out LootKeeper keeper) ||
            keeper.Loots.Count is 0)
        {
            _showingWarning = false;
            Text = GetText("UI.OpenBag.SellAll.Name");
            TextColor = Color.White;
            return;
        }

        if (!_showingWarning)
        {
            _showingWarning = true;
            Text = GetText("UI.OpenBag.AreYouSure.Name");
            TextColor = Color.Red;
            return;
        }
        
        _sellAllCallback?.Invoke();
        _showingWarning = false;
        Text = GetText("UI.OpenBag.SellAll.Name");
        TextColor = Color.White;
    }

    public override void RightClick(UIMouseEvent evt)
    {
        base.RightClick(evt);

        if (!_showingWarning)
            return;

        _showingWarning = false;
        Text = GetText("UI.OpenBag.SellAll.Name");
        TextColor = Color.White;
    }
}