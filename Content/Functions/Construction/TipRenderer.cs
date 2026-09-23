using Terraria.UI.Chat;

namespace ImproveGame.Content.Functions.Construction;

public class TipRenderer : ModSystem
{
    public enum State { Saving, Saved, Placing, Placed, Stopped }

    internal static State CurrentState = State.Stopped;
    internal static int TextDisplayCountdown;
    private static int _dotTimer;

    public override void UpdateUI(GameTime gameTime)
    {
        // High FPS Support 适配
        _dotTimer++;
        TextDisplayCountdown--;
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int rulerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Ruler"));
        if (rulerIndex != -1)
        {
            layers.Insert(rulerIndex, new LegacyGameInterfaceLayer("ImproveGame: Structure Operation Tip", delegate
            {
                if (TextDisplayCountdown <= 0 && CurrentState is not State.Saving and not State.Placing)
                    return true;

                var center = Main.LocalPlayer.Bottom - Main.screenPosition;
                center.Y += 16;
                center = center.Floor();

                string dot = "";
                for (int i = 0; i < _dotTimer % 60 / 15; i++)
                {
                    dot += ".";
                }

                var text = CurrentState switch
                {
                    State.Saving => MyUtils.GetText("ConstructGUI.Saving") + dot,
                    State.Saved => MyUtils.GetText("ConstructGUI.Saved"),
                    State.Placing => MyUtils.GetText("ConstructGUI.Placing") + dot,
                    State.Placed => MyUtils.GetText("ConstructGUI.Placed"),
                    State.Stopped => MyUtils.GetText("ConstructGUI.Stopped"),
                    _ => ""
                };

                var color = CurrentState switch
                {
                    State.Saving => Color.Yellow,
                    State.Saved => Color.LightGreen,
                    State.Placing => Color.Yellow,
                    State.Placed => Color.LightGreen,
                    State.Stopped => Color.Red,
                    _ => Color.White
                };

                var font = FontAssets.ItemStack.Value;
                var textSize = ChatManager.GetStringSize(font, text, Vector2.One);
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, center, color, 0f,
                    textSize * 0.5f, Vector2.One, spread: 1f);
                return true;
            }, InterfaceScaleType.Game));
        }
    }
}