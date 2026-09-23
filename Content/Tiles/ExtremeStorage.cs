using ImproveGame.Content.Items;
using ImproveGame.Packets.NetStorager;
using ImproveGame.UI.ExtremeStorage;
using ImproveGame.UIFramework;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ObjectData;

namespace ImproveGame.Content.Tiles;

public class ExtremeStorage : TETileBase
{
    public bool ServerOpenRequest;

    public static float GlowFactor
    {
        get
        {
            // 这个计时是宝藏袋源码，实现渐变
            float glowFactor = Main.GlobalTimeWrappedHourly;

            glowFactor %= 4f;
            glowFactor /= 2f;

            if (glowFactor >= 1f)
            {
                glowFactor = 2f - glowFactor;
            }

            glowFactor = glowFactor * 0.6f + 0.6f;
            return glowFactor;
        }
    }

    public override ModTileEntity GetTileEntity() => ModContent.GetInstance<TEExtremeStorage>();

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

    public virtual int ItemType() => ModContent.ItemType<Items.Placeable.ExtremeStorage>();

    #region 基本物块信息

    public override int Hook_AfterPlacement_NoEntity(int i, int j, int type, int style, int direction,
        int alternate)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendTileSquare(Main.myPlayer, i - 1, j - 2, 3, 3);
        }

        return 0;
    }

    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;
        Tile tile = Main.tile[i, j];
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ItemType();
        player.noThrow = 2;
    }

    public override bool OnRightClick(int i, int j)
    {
        var origin = MyUtils.GetTileOrigin(i, j);
        if (!TEExtremeStorage.TryGet(out var storage, origin))
        {
            Mod.Logger.Error("Failed to get TEExtremeStorage");
            return true;
        }

        Player player = Main.LocalPlayer;
        Item item = player.HeldItem;
        if (!ServerOpenRequest && !item.IsAir && item.ModItem is StorageCommunicator locator)
        {
            locator.Location = origin;
            if (player.selectedItem == 58)
                Main.mouseItem = item.Clone();

            MyUtils.GetMeterCoords(origin.ToPoint(), out string compassText, out string depthText);

            Main.NewText(MyUtils.GetText("Items.StorageCommunicator.SetTo", compassText, depthText));
            return true;
        }

        if (Main.netMode is NetmodeID.MultiplayerClient && !ServerOpenRequest &&
            SidedEventTrigger.IsClosed(UISystem.Instance.ExtremeStorageGUI))
        {
            OpenStoragePacket.Get(storage.ID).Send();
            return false;
        }

        ServerOpenRequest = false;

        ExtremeStorageGUI.Storage = storage;
        SidedEventTrigger.ToggleViewBody(UISystem.Instance.ExtremeStorageGUI);

        return true;
    }

    public override void ModifyObjectData()
    {
        Main.tileLighted[Type] = true;
        TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
        TileObjectData.newTile.Origin = new Point16(1, 2);
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 18 };
    }

    #endregion

    #region 绘制与光照

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        float lightFactor = GlowFactor * 0.7f + 0.3f;
        r = 0.3f * lightFactor;
        g = 0.9f * lightFactor;
        b = 1f * lightFactor;
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Tile tile = Main.tile[i, j];
        Color lightColor = Lighting.GetColor(i, j);
        if (tile.IsTileFullbright) lightColor = Color.White;
        Texture2D texture = Main.instance.TilesRenderer.GetTileDrawTexture(tile, i, j);
        Texture2D glowTexture = ModAsset.ExtremeStorage_Glow.Value;
        Texture2D highlightTexture = ModAsset.ExtremeStorage_Highlight.Value;

        if (!Main.ShouldShowInvisibleBlocksAndWalls() && tile.IsTileInvisible) return false;

        if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
        {
            Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
        }

        // If you are using ModTile.SpecialDraw or PostDraw or PreDraw, use this snippet and add zero to all calls to spriteBatch.Draw
        // The reason for this is to accommodate the shift in drawing coordinates that occurs when using the different Lighting mode
        // Press Shift+F9 to change lighting modes quickly to verify your code works for all lighting modes
        Vector2 offscreenVector = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

        // Because height of third tile is different we change it
        int height = tile.TileFrameY == 36 ? 18 : 16;

        // Firstly we draw the original texture and then glow mask texture
        spriteBatch.Draw(
            texture,
            new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + offscreenVector,
            new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, height),
            lightColor, 0f, default, 1f, SpriteEffects.None, 0f);

        // Make sure to draw with Color.White or at least a color that is fully opaque
        // Achieve opaqueness by increasing the alpha channel closer to 255. (lowering closer to 0 will achieve transparency)
        spriteBatch.Draw(
            glowTexture,
            new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + offscreenVector,
            new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, height),
            Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        // Interpret smart cursor outline color & draw it
        int selectionLevel = 0;
        if (Main.InSmartCursorHighlightArea(i, j, out bool actuallySelected))
        {
            selectionLevel = 1;
            if (actuallySelected)
            {
                selectionLevel = 2;
            }
        }

        if (selectionLevel == 0)
        {
            return false;
        }

        int averageBrightness = (lightColor.R + lightColor.G + lightColor.B) / 3;

        if (averageBrightness <= 10)
        {
            return false;
        }

        Color selectionGlowColor = Colors.GetSelectionGlowColor(selectionLevel == 2, averageBrightness);
        spriteBatch.Draw(
            highlightTexture,
            new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + offscreenVector,
            new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, height),
            selectionGlowColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        // Return false to stop vanilla draw
        return false;
    }

    #endregion

    #region Bloom 效果

    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Texture2D bloomTexture = ModAsset.ExtremeStorage_Bloom.Value;
        var offscreenVector = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        var offset = new Vector2(18f, 14f);

        float tileBrightness = Lighting.Brightness(i + 1, j + 1);
        float bloomFactor = GlowFactor - tileBrightness * 0.5f; // 物块本体越亮，光晕越暗

        var color = Color.White * bloomFactor;
        color.A = 0;
        spriteBatch.Draw(
            bloomTexture,
            new Vector2(i * 16, j * 16) - Main.screenPosition.Floor() + offscreenVector - offset,
            color);
    }

    #endregion
}