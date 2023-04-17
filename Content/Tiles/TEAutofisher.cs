using ImproveGame.Common.Packets.NetAutofisher;
using ImproveGame.Common.Players;
using ImproveGame.Common.ModSystems;
using ImproveGame.Content.Items;
using ImproveGame.Content.Items.Placeable;
using ImproveGame.Interface.Common;
using System;
using System.Reflection;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace ImproveGame.Content.Tiles
{
    public class TEAutofisher : ModTileEntity
    {
        internal Point16 locatePoint = Point16.NegativeOne;
        internal Item fishingPole = new();
        internal Item bait = new();
        internal Item accessory = new();
        internal Item[] fish = new Item[15];
        internal const int checkWidth = 50;
        internal const int checkHeight = 30;

        internal string FishingTip { get; private set; } = "Error";
        internal double FishingTipTimer { get; private set; } = 0;
        internal bool Opened = false;
        internal int OpenAnimationTimer = 0;

        public bool CatchCrates = true;
        public bool CatchAccessories = true;
        public bool CatchTools = true;
        public bool CatchWhiteRarityCatches = true;
        public bool CatchNormalCatches = true;

        public bool IsEmpty => accessory.IsAir && bait.IsAir && fishingPole.IsAir && (fish is null || fish.All(item => item.IsAir));

        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<Autofisher>();
        }

        public void SetFishingTip(Autofisher.TipType tipType, int fishingLevel = 0, float waterQuality = 0f)
        {
            FishingTip = tipType switch
            {
                Autofisher.TipType.FishingWarning => Language.GetTextValue("GameUI.FishingWarning"),
                Autofisher.TipType.NotEnoughWater => Language.GetTextValue("GameUI.NotEnoughWater"),
                Autofisher.TipType.FishingPower => Language.GetTextValue("GameUI.FishingPower", fishingLevel),
                Autofisher.TipType.FullFishingPower => Language.GetTextValue("GameUI.FullFishingPower", fishingLevel, 0.0 - Math.Round(waterQuality * 100f)),
                Autofisher.TipType.Unavailable => GetText("Autofisher.Unavailable"),
                _ => ""
            };
            FishingTipTimer = 0;

            if (Main.netMode is not NetmodeID.Server)
            {
                return;
            }

            // ���������� 1000 �����ڵ���ҷ���
            const int distance = 1000 * 1000;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                var player = Main.player[i];
                // ������ DistanceSQ �жϣ�û�п����������и���
                if (player.active && !player.DeadOrGhost && player.Center.DistanceSQ(Position.ToWorldCoordinates()) <= distance)
                    FishingTipPacket.Get(ID, tipType, fishingLevel, waterQuality).Send(i);
            }
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            for (int k = 0; k < 15; k++)
            {
                fish[k] = new();
            }
            fishingPole = new();
            bait = new();
            accessory = new();

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i - 1, j - 1, 2, 2);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i - 1, j - 1, Type);
                return -1;
            }

            int placedEntity = Place(i - 1, j - 1);
            return placedEntity;
        }

        public static Player GetClosestPlayer(Point16 Position) => Main.player[Player.FindClosest(new Vector2(Position.X * 16, Position.Y * 16), 1, 1)];

        #region ����

        public int FishingTimer;
        public override void Update()
        {
            for (int i = 0; i < fish.Length; i++)
            {
                fish[i] ??= new();
            }

            FishingTipTimer += 1.0 / 60.0;
            if (Main.netMode != NetmodeID.Server && Main.netMode != NetmodeID.SinglePlayer)
                return;
            if (locatePoint.X < 0 || locatePoint.Y < 0)
                return;
            if (Framing.GetTileSafely(locatePoint).LiquidAmount == 0)
            {
                locatePoint = Point16.NegativeOne;
                return;
            }

            int finalFishingLevel = GetFishingConditions().FinalFishingLevel;

            if (Main.rand.Next(300) < finalFishingLevel)
                FishingTimer += Main.rand.Next(1, 3);

            FishingTimer += finalFishingLevel / 30;
            FishingTimer += Main.rand.Next(1, 3);
            if (Main.rand.NextBool(60))
                FishingTimer += 60;

            //����ΪAnglerEarring��ʹ�����ٶ�*200%
            //����ΪAnglerTackleBag��ʹ�����ٶ�*300%
            //����ΪLavaproofTackleBag��ʹ�����ٶ�*500%
            float fishingSpeedBonus = accessory.type switch
            {
                ItemID.AnglerEarring => 2f,
                ItemID.AnglerTackleBag => 3f,
                ItemID.LavaproofTackleBag => 5f,
                _ => 1f
            };

            // �洢�� Bass ���� 20:1 �ı���ת��Ϊ�����ٶȼӳɣ���߿ɴ� 500% �ӳ�
            int bassCount = 0;
            for (int i = 0; i < 15; i++)
            {
                if (fish[i].type == ItemID.Bass)
                {
                    bassCount += fish[i].stack;
                }
            }
            fishingSpeedBonus += Math.Min(bassCount / 20f, 5f);

            const float fishingCooldown = 3300f; // �����������ȴ������ģ�ԭ������ٶ���660
            if (FishingTimer > fishingCooldown / fishingSpeedBonus)
            {
                FishingTimer = 0;
                ApplyAccessories();
                FishingCheck();
            }
        }

        private bool lavaFishing;
        private bool tackleBox;
        private int fishingSkill;

        private void ApplyAccessories()
        {
            lavaFishing = false;
            tackleBox = false;
            fishingSkill = 0;
            switch (accessory.type)
            {
                case ItemID.TackleBox:
                    tackleBox = true;
                    break;
                case ItemID.AnglerEarring:
                    fishingSkill += 10;
                    break;
                case ItemID.AnglerTackleBag:
                    tackleBox = true;
                    fishingSkill += 10;
                    break;
                case ItemID.LavaFishingHook:
                    lavaFishing = true;
                    break;
                case ItemID.LavaproofTackleBag:
                    tackleBox = true;
                    fishingSkill += 10;
                    lavaFishing = true;
                    break;
            }
        }

        public void FishingCheck()
        {
            var player = GetClosestPlayer(Position);

            FishingAttempt fisher = default;
            fisher.X = locatePoint.X;
            fisher.Y = locatePoint.Y;
            fisher.bobberType = fishingPole.shoot;
            GetFishingPondState(fisher.X, fisher.Y, out fisher.inLava, out fisher.inHoney, out fisher.waterTilesCount, out fisher.chumsInWater);
            if (fisher.waterTilesCount < 75)
            {
                SetFishingTip(Autofisher.TipType.NotEnoughWater);
                return;
            }

            fisher.playerFishingConditions = GetFishingConditions();
            if (fisher.playerFishingConditions.BaitItemType == ItemID.TruffleWorm)
            {
                SetFishingTip(Autofisher.TipType.FishingWarning);
                if (Main.rand.NextBool(5) && (fisher.X < 380 || fisher.X > Main.maxTilesX - 380) && fisher.waterTilesCount > 1000 && player.active && !player.dead && player.Distance(new(fisher.X * 16, fisher.Y * 16)) <= 2000 && NPC.CountNPCS(NPCID.DukeFishron) < 3)
                {
                    // �ٻ����� ��������   ������3��
                    int npc = NPC.NewNPC(NPC.GetBossSpawnSource(player.whoAmI), fisher.X * 16, fisher.Y * 16, NPCID.DukeFishron, 1);
                    if (npc == 200)
                        return;

                    Main.npc[npc].alpha = 255;
                    Main.npc[npc].target = player.whoAmI;
                    Main.npc[npc].timeLeft *= 20;
                    string typeName = Main.npc[npc].TypeName;
                    if (Main.netMode == NetmodeID.Server && npc < 200)
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc);

                    switch (Main.netMode)
                    {
                        case NetmodeID.SinglePlayer:
                            Main.NewText(GetText("Autofisher.CarefulNextTime"), 175, 75);
                            Main.NewText(Language.GetTextValue("Announcement.HasAwoken", typeName), 175, 75);
                            break;
                        case NetmodeID.Server:
                            ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Mods.ImproveGame.Autofisher.CarefulNextTime"), new(175, 75, 255));
                            ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasAwoken", Main.npc[npc].GetTypeNetName()), new Color(175, 75, 255));
                            break;
                    }

                    bait.stack--;
                    if (bait.stack <= 0)
                        bait = new();

                    UISystem.Instance.AutofisherGUI.RefreshItems(16);
                }
                return;
            }

            fisher.fishingLevel = fisher.playerFishingConditions.FinalFishingLevel;
            if (fisher.fishingLevel == 0)
                return;

            fisher.CanFishInLava = ItemID.Sets.CanFishInLava[fisher.playerFishingConditions.PoleItemType] || ItemID.Sets.IsLavaBait[fisher.playerFishingConditions.BaitItemType] || lavaFishing;
            if (fisher.chumsInWater > 0)
                fisher.fishingLevel += 11;

            if (fisher.chumsInWater > 1)
                fisher.fishingLevel += 6;

            if (fisher.chumsInWater > 2)
                fisher.fishingLevel += 3;

            SetFishingTip(Autofisher.TipType.FishingPower, fisher.fishingLevel);
            fisher.waterNeededToFish = 300;
            float num = Main.maxTilesX / 4200;
            num *= num;
            fisher.atmo = (float)((Position.Y - (60f + 10f * num)) / (Main.worldSurface / 6.0));
            if (fisher.atmo < 0.25)
                fisher.atmo = 0.25f;

            if (fisher.atmo > 1f)
                fisher.atmo = 1f;

            fisher.waterNeededToFish = (int)(fisher.waterNeededToFish * fisher.atmo);
            fisher.waterQuality = fisher.waterTilesCount / (float)fisher.waterNeededToFish;
            if (fisher.waterQuality < 1f)
                fisher.fishingLevel = (int)(fisher.fishingLevel * fisher.waterQuality);

            fisher.waterQuality = 1f - fisher.waterQuality;
            if (fisher.waterTilesCount < fisher.waterNeededToFish)
                SetFishingTip(Autofisher.TipType.FullFishingPower, fisher.fishingLevel, fisher.waterQuality);

            if (player.active && !player.dead)
            {
                if (player.luck < 0f)
                {
                    if (Main.rand.NextFloat() < 0f - player.luck)
                        fisher.fishingLevel = (int)(fisher.fishingLevel * (0.9 - Main.rand.NextFloat() * 0.3));
                }
                else if (Main.rand.NextFloat() < player.luck)
                {
                    fisher.fishingLevel = (int)(fisher.fishingLevel * (1.1 + Main.rand.NextFloat() * 0.3));
                }
            }

            int fishChance = (fisher.fishingLevel + 75) / 2;
            if (Main.rand.Next(100) > fishChance)
                return;

            fisher.heightLevel = 0;
            if (fisher.Y < Main.worldSurface * 0.5)
                fisher.heightLevel = 0;
            else if (fisher.Y < Main.worldSurface)
                fisher.heightLevel = 1;
            else if (fisher.Y < Main.rockLayer)
                fisher.heightLevel = 2;
            else if (fisher.Y < Main.maxTilesY - 300)
                fisher.heightLevel = 3;
            else
                fisher.heightLevel = 4;

            FishingCheck_RollDropLevels(player, fisher.fishingLevel, out fisher.common, out fisher.uncommon, out fisher.rare, out fisher.veryrare, out fisher.legendary, out fisher.crate);
            //FishingCheck_ProbeForQuestFish(ref fisher);
            //FishingCheck_RollEnemySpawns(ref fisher);

            // αװһ��proj���÷������Projectile.FishingCheck_RollItemDrop
            var fakeProj = new Projectile
            {
                owner = 255
            };

            Main.player[255].Center = Position.ToWorldCoordinates();
            TileCounter tileCounter = new();
            tileCounter.ScanAndExportToMain(Position);
            tileCounter.Simulate(Main.player[255]);
            tileCounter.FargosFountainSupport(Main.player[255]);

            // AssemblyPublicizer ʹ�� FishingCheck_RollItemDrop ����ֱ�ӷ���
            fakeProj.FishingCheck_RollItemDrop(ref fisher);

            AdvancedPopupRequest sonar = new();
            Vector2 sonarPosition = new(-1145141f, -919810f); // ֱ��fake����������
            PlayerLoader.CatchFish(Main.player[255], fisher, ref fisher.rolledItemDrop, ref fisher.rolledEnemySpawn, ref sonar, ref sonarPosition);

            if (fisher.rolledItemDrop != 0)
            {
                GiveItemToStorage(player, fisher.rolledItemDrop);
                //Main.NewText($"[i:{fisher.rolledItemDrop}]");
            }

            // ����ģʽ���⻹��Ϊ��Ч���ж�����˵�ǿ�Ƹ���
            if (Main.netMode == NetmodeID.SinglePlayer)
                Main.LocalPlayer.ForceUpdateBiomes();
        }

        private void GiveItemToStorage(Player player, int itemType)
        {
            Item item = new(itemType);

            int fishType = 0; // 0 ��ͨ�� (ϡ�жȴ��ڰ�)
            if (ItemID.Sets.IsFishingCrate[itemType]) fishType = 1; // 1 ��ϻ
            else if (item.accessory) fishType = 2; // 2 ��Ʒ
            else if (item.damage > 0) fishType = 3; // 3 ��������
            else if (item.OriginalRarity <= ItemRarityID.White) fishType = 4; // 4 ��ɫϡ�ж�

            switch (fishType)
            {
                case 1:
                    if (!CatchCrates) return;
                    break;
                case 2:
                    if (!CatchAccessories) return;
                    break;
                case 3:
                    if (!CatchTools) return;
                    break;
                case 4:
                    if (!CatchWhiteRarityCatches) return;
                    break;
                default:
                    if (!CatchNormalCatches) return;
                    break;
            }

            int finalFishingLevel = player.GetFishingConditions().FinalFishingLevel;

            if (itemType == ItemID.BombFish)
            {
                int minStack = (finalFishingLevel / 20 + 3) / 2;
                int maxStack = (finalFishingLevel / 10 + 6) / 2;
                if (Main.rand.Next(50) < finalFishingLevel)
                    maxStack++;

                if (Main.rand.Next(100) < finalFishingLevel)
                    maxStack++;

                if (Main.rand.Next(150) < finalFishingLevel)
                    maxStack++;

                if (Main.rand.Next(200) < finalFishingLevel)
                    maxStack++;

                item.stack = Main.rand.Next(minStack, maxStack + 1);
            }

            if (itemType == ItemID.FrostDaggerfish)
            {
                int minStack = (finalFishingLevel / 4 + 15) / 2;
                int maxStack = (finalFishingLevel / 2 + 30) / 2;
                if (Main.rand.Next(50) < finalFishingLevel)
                    maxStack += 4;

                if (Main.rand.Next(100) < finalFishingLevel)
                    maxStack += 4;

                if (Main.rand.Next(150) < finalFishingLevel)
                    maxStack += 4;

                if (Main.rand.Next(200) < finalFishingLevel)
                    maxStack += 4;

                item.stack = Main.rand.Next(minStack, maxStack + 1);
            }

            PlayerLoader.ModifyCaughtFish(player, item);
            ItemLoader.CaughtFishStack(item);
            item.newAndShiny = true;
            int oldStack = item.stack;

            // ��������Ʒ��ͬ��
            for (int i = 0; i < 15; i++)
            {
                int oldStackSlot = fish[i].stack;
                item = ItemStackToInventoryItem(fish, i, item, false);
                if (fish[i].stack != oldStackSlot && Main.netMode is NetmodeID.Server)
                {
                    // ����Ǹ����ŵ����������õģ�ֻ�����ŵķ���������
                    for (int p = 0; p < Main.maxPlayers; p++)
                    {
                        var client = Main.player[p];
                        if (client.active && !client.DeadOrGhost && client.GetModPlayer<AutofishPlayer>().IsAutofisherOpened)
                            ItemsStackChangePacket.Get(ID, (byte)i, fish[i].stack - oldStackSlot).Send(p);
                    }
                }
                if (item.IsAir)
                    goto FilledEnd;
            }
            // ������λ
            for (int i = 0; i < 15; i++)
            {
                if (fish[i].IsAir)
                {
                    fish[i] = item.Clone();
                    if (Main.netMode is NetmodeID.Server)
                    {
                        ItemSyncPacket.Get(ID, (byte)i).Send(runLocally: false);
                    }
                    item = new();
                    goto FilledEnd;
                }
            }

            FilledEnd:;

            // �����������ˣ�Ҳ��������ܴ� | TryConsumeBait����true��ʾ���������
            if (item.stack != oldStack && TryConsumeBait(player))
            {
                if (Main.netMode is NetmodeID.Server)
                {
                    // û��
                    if (bait.IsAir)
                    {
                        ItemSyncPacket.Get(ID, 16).Send(runLocally: false);
                    }
                    else // ���ڣ�ͬ��stack
                    {
                        ItemsStackChangePacket.Get(ID, 16, -1).Send(runLocally: false);
                    }
                }
            }

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                UISystem.Instance.AutofisherGUI.RefreshItems();
            }
        }

        private bool TryConsumeBait(Player player)
        {
            bool canCunsume = false;
            float chanceDenominator = 1f + bait.bait / 6f;
            if (chanceDenominator < 1f)
                chanceDenominator = 1f;

            if (tackleBox)
                chanceDenominator += 1f;

            if (Main.rand.NextFloat() * chanceDenominator < 1f)
                canCunsume = true;

            if (bait.type == ItemID.TruffleWorm)
                canCunsume = true;

            if (CombinedHooks.CanConsumeBait(player, bait) ?? canCunsume)
            {
                if (bait.type == ItemID.LadyBug || bait.type == ItemID.GoldLadyBug)
                    NPC.LadyBugKilled(Position.ToWorldCoordinates(), bait.type == ItemID.GoldLadyBug);

                bait.stack--;
                if (bait.stack <= 0)
                {
                    bait.SetDefaults();
                    if (Config.EmptyAutofisher)
                    {
                        var center = new Point(Position.X + 1, Position.Y + 2);

                        // ԭ����룬��������
                        int compass = center.X * 2 - Main.maxTilesX;
                        string compassText = (compass > 0) ? Language.GetTextValue("GameUI.CompassEast", compass) : ((compass >= 0) ? Language.GetTextValue("GameUI.CompassCenter") : Language.GetTextValue("GameUI.CompassWest", -compass));

                        int depthToSurface = (int)(center.Y - Main.worldSurface) * 2;
                        float num23 = Main.maxTilesX / 4200;
                        num23 *= num23;
                        int num24 = 1200;
                        float num25 = (float)((center.Y - (65f + 10f * num23)) / (Main.worldSurface / 5.0));
                        var layer = ((center.Y > (float)((Main.maxTilesY - 204))) ? Language.GetTextValue("GameUI.LayerUnderworld") : ((center.Y > Main.rockLayer + num24 / 2 + 16.0) ? Language.GetTextValue("GameUI.LayerCaverns") : ((depthToSurface > 0) ? Language.GetTextValue("GameUI.LayerUnderground") : ((!(num25 >= 1f)) ? Language.GetTextValue("GameUI.LayerSpace") : Language.GetTextValue("GameUI.LayerSurface")))));
                        depthToSurface = Math.Abs(depthToSurface);
                        string depth = ((depthToSurface != 0) ? Language.GetTextValue("GameUI.Depth", depthToSurface) : Language.GetTextValue("GameUI.DepthLevel"));
                        string depthText = depth + " " + layer;

                        string finalText = GetTextWith("Config.EmptyAutofisher.Tip", new
                        {
                            Compass = compassText,
                            Depth = depthText
                        });
                        WorldGen.BroadcastText(NetworkText.FromLiteral(finalText), Color.OrangeRed);
                    }
                }
                return true;
            }
            return false;
        }

        private static void FishingCheck_RollDropLevels(Player closetPlayer, int fishingLevel, out bool common, out bool uncommon, out bool rare, out bool veryrare, out bool legendary, out bool crate)
        {
            int commonChance = 150 / fishingLevel;
            int uncommonChance = 150 * 2 / fishingLevel;
            int rareChance = 150 * 7 / fishingLevel;
            int veryRareChance = 150 * 15 / fishingLevel;
            int legendaryChance = 150 * 30 / fishingLevel;
            int crateChance = 10;
            if (closetPlayer.cratePotion)
                crateChance += 10;

            if (commonChance < 2)
                commonChance = 2;

            if (uncommonChance < 3)
                uncommonChance = 3;

            if (rareChance < 4)
                rareChance = 4;

            if (veryRareChance < 5)
                veryRareChance = 5;

            if (legendaryChance < 6)
                legendaryChance = 6;

            common = false;
            uncommon = false;
            rare = false;
            veryrare = false;
            legendary = false;
            crate = false;
            if (Main.rand.NextBool(commonChance))
                common = true;

            if (Main.rand.NextBool(uncommonChance))
                uncommon = true;

            if (Main.rand.NextBool(rareChance))
                rare = true;

            if (Main.rand.NextBool(veryRareChance))
                veryrare = true;

            if (Main.rand.NextBool(legendaryChance))
                legendary = true;

            if (Main.rand.Next(100) < crateChance)
                crate = true;
        }

        private void GetFishingPondState(int x, int y, out bool lava, out bool honey, out int numWaters, out int chumCount)
        {
            chumCount = 0;
            lava = false;
            honey = false;
            for (int i = 0; i < tileChecked.GetLength(0); i++)
            {
                for (int j = 0; j < tileChecked.GetLength(1); j++)
                {
                    tileChecked[i, j] = false;
                }
            }

            numWaters = GetFishingPondSize(x, y, ref lava, ref honey, ref chumCount);
            if (ModIntegrationsSystem.NoLakeSizePenaltyLoaded) // ����if else��Ϊ���ж��Ƿ�������/����
                numWaters = 10000;

            if (honey)
                numWaters = (int)(numWaters * 1.5);
        }

        private bool[,] tileChecked = new bool[checkWidth * 2 + 1, checkHeight * 2 + 1];

        public TEAutofisher()
        {
            tackleBox = false;
            fishingSkill = 0;
        }

        private int GetFishingPondSize(int x, int y, ref bool lava, ref bool honey, ref int chumCount)
        {
            Point16 arrayLeftTop = new(Position.X + 1 - checkWidth, Position.Y + 1 - checkHeight);
            if (x - arrayLeftTop.X < 0 || x - arrayLeftTop.X > checkWidth * 2 || y - arrayLeftTop.Y < 0 || y - arrayLeftTop.Y > checkHeight * 2)
                return 0;
            if (tileChecked[x - arrayLeftTop.X, y - arrayLeftTop.Y])
                return 0;

            tileChecked[x - arrayLeftTop.X, y - arrayLeftTop.Y] = true;
            var tile = Framing.GetTileSafely(x, y);
            if (tile.LiquidAmount > 0 && !WorldGen.SolidTile(x, y))
            {
                if (tile.LiquidType == LiquidID.Lava)
                    lava = true;
                if (tile.LiquidType == LiquidID.Honey)
                    honey = true;
                chumCount += Main.instance.ChumBucketProjectileHelper.GetChumsInLocation(new Point(x, y));
                // �ݹ��ٽ����ĸ����
                int left = GetFishingPondSize(x - 1, y, ref lava, ref honey, ref chumCount);
                int right = GetFishingPondSize(x + 1, y, ref lava, ref honey, ref chumCount);
                int up = GetFishingPondSize(x, y - 1, ref lava, ref honey, ref chumCount);
                int bottom = GetFishingPondSize(x, y + 1, ref lava, ref honey, ref chumCount);
                return left + right + up + bottom + 1;
            }
            return 0;
        }

        public PlayerFishingConditions GetFishingConditions()
        {
            PlayerFishingConditions result = default;
            result.Pole = fishingPole;
            result.Bait = bait;
            if (result.BaitItemType == ItemID.TruffleWorm)
                return result;

            if (result.BaitPower == 0 || result.PolePower < 5)
                return result;

            var player = GetClosestPlayer(Position);
            int num = result.BaitPower + result.PolePower + fishingSkill;
            result.LevelMultipliers = Fishing_GetPowerMultiplier(result.Pole, result.Bait, player);
            result.FinalFishingLevel = (int)(num * result.LevelMultipliers);
            return result;
        }

        private float Fishing_GetPowerMultiplier(Item pole, Item bait, Player player)
        {
            float num = 1f;
            if (Main.raining)
                num *= 1.2f;

            if (Main.cloudBGAlpha > 0f)
                num *= 1.1f;

            switch (Main.dayTime)
            {
                case true when Main.time is < 5400.0 or > 48600.0: // ����
                    num *= 1.3f;
                    break;
                case true when Main.time is > 16200.0 and < 37800.0: // ����
                case false when Main.time is > 6480.0 and < 25920.0: // ����
                    num *= 0.8f;
                    break;
            }

            switch (Main.moonPhase)
            {
                case 0:
                    num *= 1.1f;
                    break;
                case 1:
                case 7:
                    num *= 1.05f;
                    break;
                case 3:
                case 5:
                    num *= 0.95f;
                    break;
                case 4:
                    num *= 0.9f;
                    break;
            }

            if (Main.bloodMoon)
                num *= 1.1f;

            PlayerLoader.GetFishingLevel(player, pole, bait, ref num);
            return num;
        }

        #endregion

        public override void OnKill()
        {
            if (!fishingPole.IsAir)
                SpawnDropItem(ref fishingPole);
            if (!bait.IsAir)
                SpawnDropItem(ref bait);
            if (!accessory.IsAir)
                SpawnDropItem(ref accessory);
            for (int k = 0; k < 15; k++)
                if (!fish[k].IsAir)
                    SpawnDropItem(ref fish[k]);
        }

        private void SpawnDropItem(ref Item item)
        {
            var position = Position.ToWorldCoordinates();
            int i = Item.NewItem(new EntitySource_Misc("FishingMachine"), (int)position.X, (int)position.Y, 32, 32, item.type);
            item.position = Main.item[i].position;
            Main.item[i] = item;
            var drop = Main.item[i];
            item = new Item();
            drop.velocity.Y = -2f;
            drop.velocity.X = Main.rand.NextFloat(-4f, 4f);
            drop.favorited = false;
            drop.newAndShiny = false;
        }

        public override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
            }
        }

        public override void LoadData(TagCompound tag)
        {
            locatePoint = tag.Get<Point16>("locatePoint");
            fishingPole = tag.Get<Item>("fishingPole");
            bait = tag.Get<Item>("bait");
            accessory = tag.Get<Item>("accessory");

            if (tag.ContainsKey("fishes"))
                fish = tag.Get<Item[]>("fishes");
            for (int i = 0; i < 15; i++)
                if (tag.ContainsKey($"fish{i}"))
                    fish[i] = tag.Get<Item>($"fish{i}");

            if (!tag.TryGet("CatchCrates", out CatchCrates))
                CatchCrates = true;
            if (!tag.TryGet("CatchAccessories", out CatchAccessories))
                CatchAccessories = true;
            if (!tag.TryGet("CatchTools", out CatchTools))
                CatchTools = true;
            if (!tag.TryGet("CatchWhiteRarityCatches", out CatchWhiteRarityCatches))
                CatchWhiteRarityCatches = true;
            if (!tag.TryGet("CatchNormalCatches", out CatchNormalCatches))
                CatchNormalCatches = true;
        }

        public override void SaveData(TagCompound tag)
        {
            tag["locatePoint"] = locatePoint;
            tag["fishingPole"] = fishingPole;
            tag["bait"] = bait;
            tag["accessory"] = accessory;
            tag["fishes"] = fish;
            tag["CatchCrates"] = CatchCrates;
            tag["CatchAccessories"] = CatchAccessories;
            tag["CatchTools"] = CatchTools;
            tag["CatchWhiteRarityCatches"] = CatchWhiteRarityCatches;
            tag["CatchNormalCatches"] = CatchNormalCatches;
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(locatePoint.X);
            writer.Write(locatePoint.Y);
            ItemIO.Send(fishingPole, writer, true);
            ItemIO.Send(bait, writer, true);
            ItemIO.Send(accessory, writer, true);
            writer.Write(fish);
        }

        public override void NetReceive(BinaryReader reader)
        {
            locatePoint = new(reader.ReadInt16(), reader.ReadInt16());
            fishingPole = ItemIO.Receive(reader, true);
            bait = ItemIO.Receive(reader, true);
            accessory = ItemIO.Receive(reader, true);
            fish = reader.ReadItemArray();
        }
    }
}
