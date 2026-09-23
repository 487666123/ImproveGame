using ImproveGame.Common.ModPlayers;
using ImproveGame.Content.Tiles;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.SUIElements;

namespace ImproveGame.UI.Autofisher;

internal abstract class AutofisherFilterButton : UIElement
{
    internal int ItemType;
    private readonly SUIPanel _panel;
    private readonly AnimationTimer _timer; // 这是一个计时器哦~

    internal virtual bool IsActivated(TEAutofisher autofisher) => true;

    internal virtual void Clicked(TEAutofisher autofisher) { }

    internal AutofisherFilterButton(int itemType, SUIPanel panel)
    {
        ItemType = itemType;
        _panel = panel;
        _timer = new(timerMax: 60f)
        {
            State = AnimationState.Closing
        };
        Main.instance.LoadItem(itemType);
        this.SetSize(TextureAssets.Item[itemType].Size());
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        if (AutofishPlayer.LocalPlayer.Autofisher is null)
            return;
        Clicked(AutofishPlayer.LocalPlayer.Autofisher);
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    public override void Update(GameTime gameTime)
    {
        _timer.Update();
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        float filtersX = _panel.Left() + _panel.Width() + 10f;
        if (Left.Pixels != filtersX)
        {
            Left.Set(filtersX, 0f);
            Recalculate();
        }

        CalculatedStyle dimensions = GetDimensions();
        var tex = TextureAssets.Item[ItemType];
        if (IsMouseHovering)
        {
            _timer.Open();

            Main.LocalPlayer.mouseInterface = true;

            Main.instance.MouseText(MyUtils.GetText($"UI.Autofisher.{GetType().Name}"));
        }
        else
        {
            _timer.Close();
        }

        if (_timer.Timer > 10)
        {
            Main.spriteBatch.End(); // End后Begin来使用shader绘制描边
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, Main.UIScaleMatrix);

            Main.pixelShader.CurrentTechnique.Passes["ColorOnly"].Apply(); // 全白Shader
            for (int k = -1; k <= 1; k++)
            {
                for (int l = -1; l <= 1; l++)
                {
                    if (Math.Abs(k) + Math.Abs(l) == 1)
                    {
                        var offset = new Vector2(k * 2f, l * 2f);
                        spriteBatch.Draw(tex.Value, dimensions.Position() + offset, Main.OurFavoriteColor * _timer.Schedule);
                    }
                }
            }

            Main.spriteBatch.End(); // End之后Begin恢复原状
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.SamplerStateForCursor, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
        }

        if (AutofishPlayer.LocalPlayer.Autofisher is null)
            return;

        var color = Color.White;
        if (!IsActivated(AutofishPlayer.LocalPlayer.Autofisher))
            color = color.MultiplyRGB(Color.White * 0.4f);
        spriteBatch.Draw(tex.Value, dimensions.Position(), color);
    }

    public override void MouseOver(UIMouseEvent evt)
    {
        base.MouseOver(evt);
        SoundEngine.PlaySound(SoundID.MenuTick);
    }
}