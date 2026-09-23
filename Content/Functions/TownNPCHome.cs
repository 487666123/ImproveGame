using ImproveGame.Common.Configs;

namespace ImproveGame.Content.Functions;

public class TownNPCHome : ModSystem
{
    /// <summary>
    /// 将所有NPC传送回家
    /// </summary>
    private class TownNPCHomePacket : NetModule
    {
        public static void TeleportToHome() => NetModuleLoader.Get<TownNPCHomePacket>().Send(runLocally: true);

        public override void Receive()
        {
            foreach (var npc in from n in Main.npc where n is not null && n.active && n.townNPC && !n.homeless select n)
            {
                TownEntitiesTeleportToHome(npc, npc.homeTileX, npc.homeTileY);
            }
        }
    }

    private static bool TownNPCHomeLoaded => ModLoader.HasMod("TownNPCHome");
    private static bool _callTeleportAll;
    private static int _indexReadyToTeleport = -1;

    public override void Load()
    {
        On_WorldGen.moveRoom += WorldGen_moveRoom;
    }

    public override void PreUpdateEntities()
    {
        if (_callTeleportAll)
        {
            TownNPCHomePacket.TeleportToHome();
            _callTeleportAll = false;
        }

        ref int n = ref _indexReadyToTeleport;
        if (Main.npc.IndexInRange(n) && Main.npc[n] is not null)
        {
            TownEntitiesTeleportToHome(Main.npc[n], Main.npc[n].homeTileX, Main.npc[n].homeTileY);
            n = -1;
        }
    }

    private void WorldGen_moveRoom(On_WorldGen.orig_moveRoom orig, int x, int y, int n)
    {
        orig.Invoke(x, y, n);
        if (ImproveConfigs.Instance.TownNPCHome && !TownNPCHomeLoaded && Main.npc.IndexInRange(n) && Main.npc[n] is not null)
            _indexReadyToTeleport = n;
        // TownEntitiesTeleportToHome(Main.npc[n], Main.npc[n].homeTileX, Main.npc[n].homeTileY);
    }

    private static void TownEntitiesTeleportToHome(NPC npc, int homeFloorX, int homeFloorY)
    {
        WorldGen.QuickFindHome(npc.whoAmI);

        npc.AI_007_TownEntities_TeleportToHome(homeFloorX, homeFloorY);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        var inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
        if (inventoryIndex != -1)
        {
            layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
                "ImproveGame: Quick Home Feature",
                () =>
                {
                    if (!Main.playerInventory || TownNPCHomeLoaded || !ImproveConfigs.Instance.TownNPCHome)
                    {
                        return true;
                    }

                    // vanilla code
                    int mH = 0;
                    if (Main.mapEnabled)
                    {
                        if (!Main.mapFullscreen && Main.mapStyle == 1)
                            mH = 256;
                        if (mH + Main.instance.RecommendedEquipmentAreaPushUp > Main.screenHeight)
                            mH = Main.screenHeight - Main.instance.RecommendedEquipmentAreaPushUp;
                    }

                    int yPos = mH + 142;
                    int accessorySlots = 8 + Main.LocalPlayer.GetAmountOfExtraAccessorySlotsToShow();
                    if (Main.screenHeight < 950 && accessorySlots >= 10)
                    {
                        yPos -= (int)(56f * 0.85f /*Main.inventoryScale*/ * (accessorySlots - 9));
                    }

                    // the position of the "housing" icon
                    Vector2 iconPosition = new(Main.screenWidth - 128, yPos);
                    Vector2 size = TextureAssets.EquipPage[5].Size();
                    if (Collision.CheckAABBvAABBCollision(iconPosition, size, Main.MouseScreen, Vector2.One) &&
                        Main.mouseItem.stack < 1)
                    {
                        Main.hoverItemName += MyUtils.GetText("Configs.ImproveConfigs.TownNPCHome.HoverText");
                        if (Main.mouseRight && Main.mouseRightRelease)
                        {
                            SoundEngine.PlaySound(SoundID.Chat);
                            _callTeleportAll = true;
                        }
                    }

                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }
}