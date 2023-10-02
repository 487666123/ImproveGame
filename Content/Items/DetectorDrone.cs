using ImproveGame.Content.Projectiles;
using Terraria.DataStructures;
using Terraria.ID;

namespace ImproveGame.Content.Items;

public class DetectorDrone : ModItem
{
    private class DroneRadioDrawLayer : PlayerDrawLayer
    {
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return drawInfo.drawPlayer.HeldItem?.type == ModContent.ItemType<DetectorDrone>() &&
                   drawInfo.drawPlayer.itemAnimation is 0;
        }

        public override Transformation Transform => PlayerDrawLayers.TorsoGroup;

        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.JimsDroneRadio);

        public override void Draw(ref PlayerDrawSet drawInfo)
        {
            Rectangle bodyFrame = drawInfo.drawPlayer.bodyFrame;
            Texture2D texture = TextureAssets.Extra[261].Value;
            drawInfo.DrawDataCache.Add(new DrawData(texture,
                new Vector2(
                    (int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.bodyFrame.Width / 2 +
                          drawInfo.drawPlayer.width / 2) + drawInfo.drawPlayer.direction * 2,
                    (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height -
                        drawInfo.drawPlayer.bodyFrame.Height + 4f + 14f)) + drawInfo.drawPlayer.bodyPosition +
                new Vector2(drawInfo.drawPlayer.bodyFrame.Width / 2, drawInfo.drawPlayer.bodyFrame.Height / 2),
                bodyFrame, drawInfo.colorArmorLegs, drawInfo.drawPlayer.legRotation, drawInfo.legVect, 1f,
                drawInfo.playerEffect)
            {
                shader = drawInfo.cWaist
            });
        }
    }

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.JimsDrone);
        Item.shoot = ModContent.ProjectileType<DetectorDroneProj>();
        Item.shootSpeed = 2;
    }

    public override void HoldItem(Player player)
    {
        player.remoteVisionForDrone = true;
    }

    public override bool CanUseItem(Player player)
    {
        if (player.ownedProjectileCounts[Item.shoot] <= 0)
            return true;

        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile projectile = Main.projectile[i];
            if (projectile.owner == player.whoAmI && projectile.type == Item.shoot)
                projectile.Kill();
        }

        return false;
    }
}