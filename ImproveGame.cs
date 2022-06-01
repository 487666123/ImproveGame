using ImproveGame.Common.GlobalItems;
using ImproveGame.Common.Players;
using ImproveGame.Common.Players;
using ImproveGame.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ImproveGame
{
    // ��������
    // �����أ��ְ汾�Ҽ�ʹ�û����ģ�����¶�棬��Ů�ٻ����°汾���¼��벻���� ��
    // Tile ���ߣ��Զ����㣬�Զ��ɼ����Զ��ڿ�
    // Buff Tile �ڱ���Ҳ���Ի�� Buff ������ɣ�
    // ˢ���� UI
    public class ImproveGame : Mod
    {
        public static Effect itemEffect;
        public static Effect tileEffect;
        public static Texture2D XingKong;
        // ����BUFF��
        public override uint ExtraPlayerBuffSlots => 22;

        public override void Load()
        {
            itemEffect = Assets.Request<Effect>("npc", AssetRequestMode.ImmediateLoad).Value;
            tileEffect = Assets.Request<Effect>("tileEffect", AssetRequestMode.ImmediateLoad).Value;
            XingKong = Assets.Request<Texture2D>("Assets/Images/XingKong", AssetRequestMode.ImmediateLoad).Value;
            // ����ǰ׺��Ϣ
            MyUtils.LoadPrefixInfo();
            // ��ԭ�粼������������Ʒ����������
            On.Terraria.Player.dropItemCheck += Player_dropItemCheck;
            // �����Ƿ����Ĺ��
            On.Terraria.Player.DropTombstone += Player_DropTombstone;
            // ץȡ�����޸�
            On.Terraria.Player.PullItem_Common += Player_PullItem_Common;
            // ����ˢ�� NPC
            IL.Terraria.Main.UpdateTime += Main_UpdateTime;
            // ����NPC��ס�ٶ��޸�
            IL.Terraria.Main.UpdateTime_SpawnTownNPCs += Main_UpdateTime_SpawnTownNPCs;
            // �޸Ŀռ䷨����ʾƽ̨ʣ������
            IL.Terraria.UI.ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color;
            // �˺�����
            On.Terraria.Main.DamageVar += Main_DamageVar;
            // ʹ��Ǯ������Ʒ��Ч����ͬ���뱳��һ��
            On.Terraria.Player.VanillaPreUpdateInventory += Player_VanillaPreUpdateInventory;
            // ���ĸ���
            On.Terraria.SceneMetrics.ScanAndExportToMain += SceneMetrics_ScanAndExportToMain;
            // ʰȡ��Ʒ������
            On.Terraria.Player.PickupItem += Player_PickupItem;
        }

        /// <summary>
        /// ʰȡ��Ʒ��ʱ��
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="player"></param>
        /// <param name="playerIndex"></param>
        /// <param name="worldItemArrayIndex"></param>
        /// <param name="itemToPickUp"></param>
        /// <returns></returns>
        private Item Player_PickupItem(On.Terraria.Player.orig_PickupItem orig, Player player, int playerIndex, int worldItemArrayIndex, Item itemToPickUp)
        {
            ImprovePlayer improvePlayer = ImprovePlayer.G(player);
            // ������ձ��տ�
            if (MyUtils.Config().SmartVoidVault)
            {
                if (itemToPickUp.type != ItemID.None && itemToPickUp.stack > 0 && !itemToPickUp.IsACoin)
                {
                    if (MyUtils.Config().SuperVault && MyUtils.HasItem(player.GetModPlayer<DataPlayer>().SuperVault, itemToPickUp))
                    {
                        itemToPickUp = MyUtils.StackItemToInv(player.whoAmI, player.GetModPlayer<DataPlayer>().SuperVault,
                            itemToPickUp, GetItemSettings.PickupItemFromWorld);
                    }
                    if (player.IsVoidVaultEnabled && MyUtils.HasItem(player.bank4.item, itemToPickUp))
                    {
                        itemToPickUp = MyUtils.StackItemToInv(player.whoAmI, player.bank4.item, itemToPickUp, GetItemSettings.PickupItemFromWorld);
                    }
                    // ������ձ��տ�
                    if (MyUtils.Config().SuperVoidVault)
                    {
                        if (improvePlayer.PiggyBank && MyUtils.HasItem(player.bank.item, itemToPickUp))
                        {
                            itemToPickUp = MyUtils.StackItemToInv(player.whoAmI, player.bank.item, itemToPickUp, GetItemSettings.PickupItemFromWorld);
                        }
                        if (improvePlayer.Safe && MyUtils.HasItem(player.bank2.item, itemToPickUp))
                        {
                            itemToPickUp = MyUtils.StackItemToInv(player.whoAmI, player.bank2.item, itemToPickUp, GetItemSettings.PickupItemFromWorld);
                        }
                        if (improvePlayer.DefendersForge && MyUtils.HasItem(player.bank3.item, itemToPickUp))
                        {
                            itemToPickUp = MyUtils.StackItemToInv(player.whoAmI, player.bank3.item, itemToPickUp, GetItemSettings.PickupItemFromWorld);
                        }
                    }
                }
            }
            Item item = orig(player, playerIndex, worldItemArrayIndex, itemToPickUp);
            if (MyUtils.Config().SuperVault && item.type != ItemID.None && item.stack > 0 && !item.IsACoin)
            {
                item = MyUtils.StackItemToInv(player.whoAmI, player.GetModPlayer<DataPlayer>().SuperVault, item, GetItemSettings.PickupItemFromWorld);
            }
            // ������ձ��տ�
            if (MyUtils.Config().SuperVoidVault)
            {
                if (item.type != ItemID.None && item.stack > 0 && !item.IsACoin)
                {
                    if (improvePlayer.PiggyBank)
                    {
                        item = MyUtils.StackItemToInv(player.whoAmI, player.bank.item, item, GetItemSettings.PickupItemFromWorld);
                    }
                    if (improvePlayer.Safe && item.type != ItemID.None && item.stack > 0)
                    {
                        item = MyUtils.StackItemToInv(player.whoAmI, player.bank2.item, item, GetItemSettings.PickupItemFromWorld);
                    }
                    if (improvePlayer.DefendersForge && item.type != ItemID.None && item.stack > 0)
                    {
                        item = MyUtils.StackItemToInv(player.whoAmI, player.bank3.item, item, GetItemSettings.PickupItemFromWorld);
                    }
                }
            }
            Main.item[worldItemArrayIndex] = item;
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, worldItemArrayIndex);
            }
            return item;
        }

        /// <summary>
        /// ����BUFF�ڱ�����Ч
        /// </summary>
        /// <param name="self"></param>
        /// <param name="player"></param>
        /// <param name="item"></param>
        private static void AddBannerBuff(SceneMetrics self, Player player, Item item)
        {
            if (item.createTile == TileID.Banners)
            {
                int style = item.placeStyle;
                int frameX = style * 18;
                int frameY = 0;
                if (style >= 90)
                {
                    frameX -= 1620;
                    frameY += 54;
                }
                if (frameX >= 396 || frameY >= 54)
                {
                    int styleX = frameX / 18 - 21;
                    for (int num4 = frameY; num4 >= 54; num4 -= 54)
                    {
                        styleX += 90;
                    }
                    self.NPCBannerBuff[styleX] = true;
                    self.hasBanner = true;
                }
            }
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <param name="settings"></param>
        private void SceneMetrics_ScanAndExportToMain(On.Terraria.SceneMetrics.orig_ScanAndExportToMain orig, SceneMetrics self, SceneMetricsScanSettings settings)
        {
            orig(self, settings);
            // �������ģ�����վ��
            if (MyUtils.Config().NoPlace_BUFFTile_Banner)
            {
                Player player = Main.LocalPlayer;
                for (int i = 0; i < player.inventory.Length; i++)
                {
                    Item item = player.inventory[i];
                    if (item.type == ItemID.None)
                        continue;
                    AddBannerBuff(self, player, item);
                }
                for (int i = 0; i < player.bank.item.Length; i++)
                {
                    Item item = player.bank.item[i];
                    if (item.type == ItemID.None)
                        continue;
                    AddBannerBuff(self, player, item);
                }
                for (int i = 0; i < player.bank2.item.Length; i++)
                {
                    Item item = player.bank2.item[i];
                    if (item.type == ItemID.None)
                        continue;
                    AddBannerBuff(self, player, item);
                }
                for (int i = 0; i < player.bank3.item.Length; i++)
                {
                    Item item = player.bank3.item[i];
                    if (item.type == ItemID.None)
                        continue;
                    AddBannerBuff(self, player, item);
                }
                for (int i = 0; i < player.bank4.item.Length; i++)
                {
                    Item item = player.bank4.item[i];
                    if (item.type == ItemID.None)
                        continue;
                    AddBannerBuff(self, player, item);
                }
                if (MyUtils.Config().SuperVault)
                {
                    for (int i = 0; i < player.GetModPlayer<DataPlayer>().SuperVault.Length; i++)
                    {
                        Item item = player.GetModPlayer<DataPlayer>().SuperVault[i];
                        if (item.type == ItemID.None)
                            continue;
                        AddBannerBuff(self, player, item);
                    }
                }
            }
        }

        /// <summary>
        /// ʹ��Ǯ������Ʒ��ͬ���ڱ���
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        private void Player_VanillaPreUpdateInventory(On.Terraria.Player.orig_VanillaPreUpdateInventory orig, Player self)
        {
            orig(self);
            for (int i = 0; i < self.bank.item.Length; i++)
            {
                self.VanillaUpdateInventory(self.bank.item[i]);
            }
            for (int i = 0; i < self.bank2.item.Length; i++)
            {
                self.VanillaUpdateInventory(self.bank2.item[i]);
            }
            for (int i = 0; i < self.bank3.item.Length; i++)
            {
                self.VanillaUpdateInventory(self.bank3.item[i]);
            }
            for (int i = 0; i < self.bank4.item.Length; i++)
            {
                self.VanillaUpdateInventory(self.bank4.item[i]);
            }
            if (MyUtils.Config().SuperVault)
            {
                DataPlayer dataPlayer = self.GetModPlayer<DataPlayer>();
                for (int i = 0; i < dataPlayer.SuperVault.Length; i++)
                {
                    self.VanillaUpdateInventory(dataPlayer.SuperVault[i]);
                }
            }
        }

        /// <summary>
        /// �˺�����
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="dmg"></param>
        /// <param name="luck"></param>
        /// <returns></returns>
        private int Main_DamageVar(On.Terraria.Main.orig_DamageVar orig, float dmg, float luck)
        {
            if (MyUtils.Config().BanDamageVar)
                return (int)Math.Round(dmg);
            else
                return orig(dmg, luck);
        }

        // NPC ����ˢ��
        private void Main_UpdateTime(ILContext il)
        {
            var c = new ILCursor(il);
            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchCall(typeof(Main), "UpdateTime_StartDay"),
                i => i.MatchCall(typeof(Main), "HandleMeteorFall")))
                return;
            c.EmitDelegate(() =>
            {
                if (MyUtils.Config().TownNPCSpawnInNight)
                {
                    MethodInfo methodInfo = typeof(Main).GetMethod("UpdateTime_SpawnTownNPCs", BindingFlags.Static | BindingFlags.NonPublic);
                    methodInfo.Invoke(null, null);
                }
            });
        }

        // NPC ˢ���ٶ�
        private void Main_UpdateTime_SpawnTownNPCs(ILContext il)
        {
            var c = new ILCursor(il);
            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdsfld(typeof(Main), nameof(Main.checkForSpawns)),
                i => i.Match(OpCodes.Ldc_I4_1)))
                return;
            c.EmitDelegate<Func<int, int>>((JiaJi) =>
            {
                return (int)Math.Pow(2, MyUtils.Config().TownNPCSpawnSpeed);
            });
        }

        // �ռ䷨�ȼ���ʣ��ƽ̨��
        private void ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color(ILContext il)
        {
            // ����ʣ��ƽ̨
            var c = new ILCursor(il);
            if (!c.TryGotoNext(MoveType.After,
                i => i.Match(OpCodes.Pop),
                i => i.Match(OpCodes.Ldc_I4_M1)))
                return;
            c.Emit(OpCodes.Ldarg_1); // �����Ʒ��
            c.Emit(OpCodes.Ldarg_2); // content
            c.Emit(OpCodes.Ldarg_3); // ��Ʒ����Ʒ�۵�λ��
            c.EmitDelegate<Func<int, Item[], int, int, int>>((num11, inv, content, slot) =>
                {
                    if (content == 13)
                    {
                        if (inv[slot].type == ModContent.ItemType<Content.Items.SpaceWand>())
                        {
                            int count = 0;
                            MyUtils.GetPlatformCount(inv, ref count);
                            return count;
                        }
                        else if (inv[slot].type == ModContent.ItemType<Content.Items.WallPlace>())
                        {
                            int count = 0;
                            MyUtils.GetWallCount(inv, ref count);
                            return count;
                        }
                        return -1;
                    }
                    else
                    {
                        return -1;
                    }
                });
        }

        /// <summary>
        /// ��Ʒ��ȡ�ٶ�
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="player"></param>
        /// <param name="item"></param>
        /// <param name="xPullSpeed"></param>
        private void Player_PullItem_Common(On.Terraria.Player.orig_PullItem_Common orig, Player player, Item item, float xPullSpeed)
        {
            if (MyUtils.Config().GrabDistance > 0)
            {
                Vector2 velocity = (player.Center - item.Center).SafeNormalize(Vector2.Zero);
                if (item.velocity.Length() + velocity.Length() > 15f)
                {
                    item.velocity = velocity * 15f;
                }
                else
                {
                    item.velocity = velocity * (item.velocity.Length() + 1);
                }
            }
            else
            {
                orig(player, item, xPullSpeed);
            }
        }

        /// <summary>
        /// Ĺ������
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <param name="coinsOwned"></param>
        /// <param name="deathText"></param>
        /// <param name="hitDirection"></param>
        private void Player_DropTombstone(On.Terraria.Player.orig_DropTombstone orig, Player self, int coinsOwned, Terraria.Localization.NetworkText deathText, int hitDirection)
        {
            if (!MyUtils.Config().BanTombstone)
            {
                orig(self, coinsOwned, deathText, hitDirection);
            }
        }

        /// <summary>
        /// ǰ׺����
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        private void Player_dropItemCheck(On.Terraria.Player.orig_dropItemCheck orig, Player self)
        {
            if (Main.reforgeItem.type > ItemID.None && self.GetModPlayer<DataPlayer>().ReforgeItemPrefix > 0)
            {
                Main.reforgeItem.GetGlobalItem<ItemVar>().recastCount =
                    self.GetModPlayer<DataPlayer>().ReforgeItemPrefix;
                self.GetModPlayer<DataPlayer>().ReforgeItemPrefix = 0;
            }
            orig(self);
        }
    }
}