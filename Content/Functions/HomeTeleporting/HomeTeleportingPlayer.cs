using ImproveGame.Common.Configs;
using ImproveGame.Common.ModSystems;
using ImproveGame.Content.Projectiles;
using ImproveGame.Core;
using Terraria.DataStructures;
using Terraria.GameInput;

namespace ImproveGame.Content.Functions.HomeTeleporting;

public record struct HomeTeleportingItem(int ItemType, bool IsPotion, bool IsComebackItem);

public class HomeTeleportingPlayer : ModPlayer
{
    public static List<HomeTeleportingItem> HomeTeleportingItems =
    [
        // 返回药水优先级最高
        new HomeTeleportingItem(ItemID.PotionOfReturn, true, true),
        // 然后是回忆
        new HomeTeleportingItem(ItemID.RecallPotion, true, false),
        // 最后是魔镜
        new HomeTeleportingItem(ItemID.MagicMirror, false, false),
        new HomeTeleportingItem(ItemID.CellPhone, false, false),
        new HomeTeleportingItem(ItemID.IceMirror, false, false),
        new HomeTeleportingItem(ItemID.Shellphone, false, false),
        new HomeTeleportingItem(ItemID.ShellphoneOcean, false, false),
        new HomeTeleportingItem(ItemID.ShellphoneHell, false, false),
        new HomeTeleportingItem(ItemID.ShellphoneSpawn, false, false),
    ];

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (!Main.LocalPlayer.dead && KeybindSystem.HomeKeybind.JustPressed)
            PressHomeKeybind();
    }

    private void PressHomeKeybind()
    {
        foreach ((int itemType, bool isPotion, bool isComebackItem) in HomeTeleportingItems)
        {
            int totalStack = CurrentFrameProperties.ExistItems.GetTotalStack(itemType);
            if (totalStack is 0)
                continue;
            // 是药水的条件下：必须同时满足“开启了无限药水”与“达到了药水需求量”
            if (isPotion && (totalStack < ImproveConfigs.Instance.NoConsume_PotionRequirement || !ImproveConfigs.Instance.NoConsume_Potion))
                continue;

            Projectile.NewProjectile(new EntitySource_Sync(), Player.position, Vector2.Zero,
                ModContent.ProjectileType<HomeEffect>(), 0, 0, Player.whoAmI);

            var text = MyUtils.GetText("QuickHome.Teleported", Lang.GetItemName(itemType));
            MyUtils.AddNotification(text, Color.Yellow, itemType);

            if (isComebackItem)
            {
                Player.DoPotionOfReturnTeleportationAndSetTheComebackPoint();
            }
            else
            {
                Player.RemoveAllGrapplingHooks();
                bool immune = Player.immune;
                int immuneTime = Player.immuneTime;
                Player.Spawn(PlayerSpawnContext.RecallFromItem);
                Player.immune = immune;
                Player.immuneTime = immuneTime;
            }

            return;
        }

        MyUtils.AddNotificationFromKey("QuickHome.NoItem", Color.Yellow, ItemID.MagicMirror);
    }
}