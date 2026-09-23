using ImproveGame.Common.ModPlayers;
using ImproveGame.Packets.NetAutofisher;
using ImproveGame.UI.Autofisher;
using ImproveGame.UIFramework;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ObjectData;

namespace ImproveGame.Content.Tiles
{
    public class Autofisher : TETileBase
    {
        public enum TipType : byte
        {
            FishingWarning,
            NotEnoughWater,
            FishingPower,
            FullFishingPower,
            Unavailable,
            InShimmer
        }

        public override ModTileEntity GetTileEntity() => ModContent.GetInstance<TEAutofisher>();

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

        public virtual int ItemType() => ModContent.ItemType<Items.Placeable.Autofisher>();

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            Tile tile = Main.tile[i, j];
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ItemType();
            player.noThrow = 2;
        }

        public bool ServerOpenRequest = false;

        public override bool OnRightClick(int i, int j)
        {
            var origin = MyUtils.GetTileOrigin(i, j);
            if (!MyUtils.TryGetTileEntityAs<TEAutofisher>(origin, out var fisher)) return false;

            if (AutofishPlayer.LocalPlayer.Autofisher != fisher)
            {
                if (Main.netMode == NetmodeID.MultiplayerClient && !ServerOpenRequest)
                {
                    OpenRequestPacket.Get(fisher).Send(runLocally: false);
                    return false;
                }

                ServerOpenRequest = false;

                AutofishPlayer.LocalPlayer.SetAutofisher(fisher);

                // 多人客户端下此时已经同步好了物品，则将 Refresh 标记为 true
                if (Main.netMode is NetmodeID.MultiplayerClient)
                {
                    AutofisherGUI.RequireRefresh = true;
                }

                if (!AutofisherGUI.Visible)
                {
                    SidedEventTrigger.ToggleViewBody(UISystem.Instance.AutofisherGUI);
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                }
                else
                {
                    // 刷新物品列表
                    UISystem.Instance.AutofisherGUI.RefreshItems();
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }

            }
            else
            {
                AutofishPlayer.LocalPlayer.SetAutofisher(null);
                if (AutofisherGUI.Visible)
                    SidedEventTrigger.ToggleViewBody(UISystem.Instance.AutofisherGUI);
            }

            return true;
        }

        public override bool CanKillTile(int i, int j, ref bool blockDamaged) =>
            !MyUtils.TryGetTileEntityAs<TEAutofisher>(i, j, out var autofisher) || autofisher.IsEmpty;

        public override void ModifyObjectData()
        {
            base.ModifyObjectData();
            TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.StyleHorizontal = true;
        }

        public override bool ModifyObjectDataAlternate(ref int alternateStyle)
        {
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
            alternateStyle = 1;
            return true;
        }

        public override IEnumerable<Item> GetItemDrops(int i, int j) => new[] { new Item(ItemType()) };

        //public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        //    if (!MyUtils.TryGetTileEntityAs<TEAutofisher>(i, j, out var fisher))
        //        return;
        //    if (Main.tile[i, j].TileFrameX != 0 || Main.tile[i, j].TileFrameY != 0)
        //        return;

        //    Vector2 offScreen = new(Main.offScreenRange);
        //    if (Main.drawToScreen) {
        //        offScreen = Vector2.Zero;
        //    }

        //    var worldPos = new Point(i, j).ToWorldCoordinates(-4, -20);
        //    var screenPos = worldPos + offScreen - Main.screenPosition;
        //    string text = fisher.Opened ? "On" : "Off";
        //    Color textColor = fisher.Opened ? new(20, 240, 20) : new(240, 20, 20);
        //    Utils.DrawBorderString(spriteBatch, text, screenPos, textColor);
        //}
    }
}