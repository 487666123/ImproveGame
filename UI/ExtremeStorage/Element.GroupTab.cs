using ImproveGame.Common.Configs;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;

namespace ImproveGame.UI.ExtremeStorage;

public class GroupTab : View
{
    private readonly Asset<Texture2D> _tabTexture;
    private readonly ItemGroup _group;
    private readonly Rectangle _groupFrame;
    private Rectangle _tabFrame;

    public GroupTab(ItemGroup group)
    {
        // 作为定位和frame的x值
        int x = group switch
        {
            ItemGroup.Everything => 0,
            ItemGroup.Weapon => 1,
            ItemGroup.Tool => 2,
            ItemGroup.Ammo => 3,
            ItemGroup.Armor => 4,
            ItemGroup.Accessory => 5,
            ItemGroup.Block => 6,
            ItemGroup.Misc => 7,
            ItemGroup.Furniture => 8,
            ItemGroup.Alchemy => 9,
            ItemGroup.Setting => 10,
            _ => 0 // 不可能的情况
        };

        this.SetSize(42f, 48f);
        this.SetPos(x * 40 + 8, 0f);

        _group = group;
        _tabTexture = Main.Assets.Request<Texture2D>("Images/UI/Creative/Infinite_Tabs_B");
        _tabFrame = new Rectangle(0, 0, 42, 48);
        _groupFrame = new Rectangle(x * 30, 0, 28, 28);
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        var dimensions = GetDimensions();
        var pos = dimensions.Position();
        var center = dimensions.Center() - new Vector2(0f, 4f);
        var groupTexture = UIConfigs.Instance.ThemeType is ThemeType.Stormdark
            ? ModAsset.Icons_Stormdark
            : ModAsset.Icons_Regular;
        bool groupSelected = ExtremeStorageGUI.CurrentGroup == _group;
        var iconColor = Color.White * (groupSelected ? 1f : 0.5f);
        spriteBatch.Draw(position: pos, texture: _tabTexture.Value, sourceRectangle: _tabFrame, color: Color.White);
        spriteBatch.Draw(groupTexture.Value, center, _groupFrame, iconColor, 0f, _groupFrame.Size() / 2f, 1f,
            SpriteEffects.None, 0f);

        if (IsMouseHovering)
            Main.instance.MouseText(MyUtils.GetText($"UI.ExtremeStorage.ItemGroup.{_group}"));
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        bool mouseHover = IsMouseHovering;
        bool groupSelected = ExtremeStorageGUI.CurrentGroup == _group;

        if (!mouseHover && !groupSelected)
            _tabFrame = _tabTexture.Frame(2, 4, 0, 0).OffsetSize(-2, -2);
        if (mouseHover && !groupSelected)
            _tabFrame = _tabTexture.Frame(2, 4, 0, 1).OffsetSize(-2, -2);
        if (!mouseHover && groupSelected)
            _tabFrame = _tabTexture.Frame(2, 4, 1, 2).OffsetSize(-2, -2);
        if (mouseHover && groupSelected)
            _tabFrame = _tabTexture.Frame(2, 4, 1, 3).OffsetSize(-2, -2);
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);
        ExtremeStorageGUI.SetGroup(_group);
    }

    public override void MouseOver(UIMouseEvent evt)
    {
        base.MouseOver(evt);
        SoundEngine.PlaySound(SoundID.MenuTick);
    }
}