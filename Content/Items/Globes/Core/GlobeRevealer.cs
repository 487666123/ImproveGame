using ImproveGame.Packets.Notifications;
using ImproveGame.Packets.WorldFeatures;
using Terraria.Chat;
using Terraria.DataStructures;

namespace ImproveGame.Content.Items.Globes.Core;

public static class GlobeRevealer
{
    public static void NoDataNotification(Globe dummyItem, int owner)
    {
        string key = "Mods.ImproveGame.Items.GlobeBase.NotFound";
        string argumentKey = dummyItem.GetLocalizationKey("BiomeName");
        if (Main.dedServ)
        {
            GlobePopupMessagePacket.Get(false, key, argumentKey).Send(owner);
        }
        else
        {
            MyUtils.AddNotification(
                Language.GetText(key)
                .WithFormatArgs(Language.GetTextValue(argumentKey)).Value,
                Globe.hintTextColor);
        }
    }
    public static void NotFoundNotification(Globe dummyItem, int owner)
    {
        string key = dummyItem.GetLocalizationKey("NotFound");
        if (Main.dedServ)
        {
            GlobePopupMessagePacket.Get(false, key).Send(owner);
        }
        else
        {
            MyUtils.AddNotification(Language.GetTextValue(key), Globe.hintTextColor);
        }
    }
    public static void AlreadyRevealedNotification(Globe dummyItem, int owner)
    {
        string key = "Mods.ImproveGame.Items.GlobeBase.AlreadyRevealed";
        string argumentKey = dummyItem.GetLocalizationKey("BiomeName");
        if (Main.dedServ)
        {
            GlobePopupMessagePacket.Get(false, key, argumentKey).Send(owner);
        }
        else
        {
            MyUtils.AddNotification(
                Language.GetText(key)
                .WithFormatArgs(Language.GetTextValue(argumentKey)).Value,
                Globe.hintTextColor);
        }
    }
    public static void RevealBroadcast(Globe dummyItem, string name, int owner)
    {
        string key = "Mods.ImproveGame.Items.GlobeBase.Reveal";
        string argumentKey = dummyItem.GetLocalizationKey("BiomeName");
        if (Main.dedServ)
        {
            // 由于服务器和客户端使用的语言可能不一样，所以用FromKey并专门设了个翻译文本
            ChatHelper.BroadcastChatMessage(
                NetworkText.FromKey(
                    "Mods.ImproveGame.Items.GlobeBase.Reveal",
                    NetworkText.FromKey(argumentKey),
                    name),
                Globe.foundColor,
                owner);
            GlobePopupMessagePacket.Get(true, key, argumentKey, name).Send(owner);
        }
        else
        {
            MyUtils.AddNotification(
                Language.GetText(key)
                .WithFormatArgs(Language.GetTextValue(argumentKey), name).Value,
                Globe.foundColor);
        }
    }


    //这两个都是实时查找，一次解锁一个型
    public static bool RevealPlantera(Projectile projectile, Globe dummyItem, bool onlyJudging)
    {
        if (Main.netMode is NetmodeID.MultiplayerClient) //此时由服务器进行查找
            return true;

        var player = Main.player[projectile.owner];
        var playerPosition = player.position.ToTileCoordinates().ToVector2();
        Point16 position = Point16.Zero;
        float currentDistance = float.MaxValue;
        for (int i = 10; i < Main.maxTilesX - 10; i++)
        {
            for (int j = 10; j < Main.maxTilesY - 10; j++)
            {
                var tile = Framing.GetTileSafely(i, j);
                if (!tile.HasTile || tile.TileType is not TileID.PlanteraBulb ||
                    tile.TileFrameX is not 18 || tile.TileFrameY is not 18)
                    continue;

                var tilePosition = new Vector2(i, j);
                if (StructureDatas.PlanteraPositions.Contains(tilePosition.ToPoint16()))
                    continue;

                var newDistance = tilePosition.Distance(playerPosition);
                if (newDistance < currentDistance)
                {
                    currentDistance = newDistance;
                    position = tilePosition.ToPoint16();
                }
            }
        }

        if (onlyJudging)
            return position != Point16.Zero;

        if (position == Point16.Zero)
        {
            SyncNotificationKey.Send("Items.PlanteraGlobe.NotFound", Globe.hintTextColor, player.whoAmI);
            return false;
        }

        var module = NetModuleLoader.Get<RevealPlanteraPacket>();
        module._position = position;
        module.Send(runLocally: true);
        RevealBroadcast(dummyItem, player.name, player.whoAmI);

        return true;
    }
    public static bool RevealEnchantedSword(Projectile projectile, Globe dummyItem, bool onlyJudging)
    {
        if (Main.netMode is NetmodeID.MultiplayerClient) //此时由服务器进行查找
            return true;
        var player = Main.player[projectile.owner];
        var playerPosition = player.position.ToTileCoordinates().ToVector2();
        Point16 position = Point16.Zero;
        float currentDistance = float.MaxValue;
        for (int i = 10; i < Main.maxTilesX - 10; i++)
        {
            for (int j = 10; j < Main.maxTilesY - 10; j++)
            {
                var tile = Framing.GetTileSafely(i, j);
                if (!tile.HasTile || tile.TileType is not TileID.LargePiles2 ||
                    tile.TileFrameX is not 918 || tile.TileFrameY is not 0)
                    continue;

                var tilePosition = new Vector2(i, j);
                if (StructureDatas.EnchantedSwordPositions.Contains(tilePosition.ToPoint16()))
                    continue;

                var newDistance = tilePosition.Distance(playerPosition);
                if (newDistance < currentDistance)
                {
                    currentDistance = newDistance;
                    position = tilePosition.ToPoint16();
                }
            }
        }
        if (onlyJudging)
            return position != Point16.Zero;
        if (position == Point16.Zero)
        {
            SyncNotificationKey.Send("Items.EnchantedSwordGlobe.NotFound", Globe.hintTextColor, player.whoAmI);
            return false;
        }
        var module = NetModuleLoader.Get<RevealEnchantedSwordPacket>();
        module._position = position;
        module.Send(runLocally: true);
        RevealBroadcast(dummyItem, player.name, player.whoAmI);
        return true;
    }

    //这两个都是生成时查找，一次解锁一个
    public static bool RevealMarble(Projectile projectile, Globe dummyItem, bool onlyJudging)
    {
        if (StructureDatas.AllMarbleCavePositions.Count <= StructureDatas.MarbleCavePositions.Count)//没有判定0的必要，因为如果是0一定满足这个
        {
            if (!onlyJudging)
            {
                if (!StructureDatas.QotEanbledInWorldGeneration)
                    NoDataNotification(dummyItem, projectile.owner);
                else
                    NotFoundNotification(dummyItem, projectile.owner);
            }
            return false;
        }
        if (onlyJudging)
            return true;

        StructureDatas.MarbleCavePositions.Add(StructureDatas.AllMarbleCavePositions
            .Except(StructureDatas.MarbleCavePositions)
            .MinBy(position => projectile.Center.Distance(position.ToVector2() * 16)));
        var playerName = Main.player[projectile.owner].name;
        RevealBroadcast(dummyItem, playerName, projectile.owner);
        return true;
    }
    public static bool RevealGranite(Projectile projectile, Globe dummyItem, bool onlyJudging)
    {
        if (StructureDatas.AllGraniteCavePositions.Count <= StructureDatas.GraniteCavePositions.Count)//没有判定0的必要，因为如果是0一定满足这个
        {
            if (!onlyJudging)
            {
                if (!StructureDatas.QotEanbledInWorldGeneration)
                    NoDataNotification(dummyItem, projectile.owner);
                else
                    NotFoundNotification(dummyItem, projectile.owner);
            }
            return false;
        }
        if (onlyJudging)
            return true;

        StructureDatas.GraniteCavePositions.Add(StructureDatas.AllGraniteCavePositions
            .Except(StructureDatas.GraniteCavePositions)
            .MinBy(position => projectile.Center.Distance(position.ToVector2() * 16)));
        var playerName = Main.player[projectile.owner].name;
        RevealBroadcast(dummyItem, playerName, projectile.owner);
        return true;
    }
    public static bool RevealHive(Projectile projectile, Globe dummyItem, bool onlyJudging)
    {
        if (StructureDatas.AllHivePositions.Count <= StructureDatas.HivePositions.Count)//没有判定0的必要，因为如果是0一定满足这个
        {
            if (!onlyJudging)
            {
                if (!StructureDatas.QotEanbledInWorldGeneration)
                    NoDataNotification(dummyItem, projectile.owner);
                else
                    NotFoundNotification(dummyItem, projectile.owner);
            }
            return false;
        }
        if (onlyJudging)
            return true;

        StructureDatas.HivePositions.Add(StructureDatas.AllHivePositions
            .Except(StructureDatas.HivePositions)
            .MinBy(position => projectile.Center.Distance(position.ToVector2() * 16)));
        var playerName = Main.player[projectile.owner].name;
        RevealBroadcast(dummyItem, playerName, projectile.owner);
        return true;
    }

    //这里都是生成时查找，一次解锁完毕，有对于生成时数据未记录的额外查找处理
    public static bool RevealOnceForAll(Projectile projectile, Globe dummyItem, bool onlyJudging)
    {
        var onceForAll = projectile.ModProjectile as IOnceForAllGlobeProj;
        bool extraChecked = false;
        //这一遍是看看世界数据有没有记录
        if ((!StructureDatas.QotEanbledInWorldGeneration || onceForAll.NotFoundCheck()) && Main.netMode != NetmodeID.MultiplayerClient)
        {
            //没有就当场另作检测
            onceForAll.ExtraCheckWhenNotRecorded();
            extraChecked = true;
        }

        //你要再没我也没办法了
        if (onceForAll.NotFoundCheck())
        {

            if (!onlyJudging)
            {
                if (StructureDatas.QotEanbledInWorldGeneration)
                    NotFoundNotification(dummyItem, projectile.owner);
                else
                    NoDataNotification(dummyItem, projectile.owner);
            }
            return false;
        }
        if (StructureDatas.StructuresUnlocked[(byte)onceForAll.StructureType])
        {
            if (!onlyJudging)
                AlreadyRevealedNotification(dummyItem, projectile.owner);
            return false;
        }
        if (onlyJudging)
            return true;
        StructureDatas.StructuresUnlocked[(byte)onceForAll.StructureType] = true;
        string name = Main.player[projectile.owner].name;

        RevealBroadcast(dummyItem, name, projectile.owner);

        if (extraChecked) //只有额外检测了才有发包的意义
        {
            var packet = NetModuleLoader.Get<RevealOnceForAllPacket>();
            packet._type = onceForAll.StructureType;
            packet._position = onceForAll.Positions;
            packet._positionAnother = onceForAll.PositionsAnother;
            packet.Send(runLocally: true);
        }
        else
            StructureDatas.StructuresUnlocked[(byte)onceForAll.StructureType] = true;
        return true;
    }
}

[AutoSync]
public class GlobePopupMessagePacket : NetModule
{
    public bool IsFoundText;
    public string LocalizationTextKey;
    public string ArgumentKey;
    public string playerName;
    public static GlobePopupMessagePacket Get(bool isFound, string localizationTextKey, string argumentKey = "", string playerName = "")
    {
        var packet = NetModuleLoader.Get<GlobePopupMessagePacket>();
        packet.IsFoundText = isFound;
        packet.LocalizationTextKey = localizationTextKey;
        packet.ArgumentKey = argumentKey;
        packet.playerName = playerName;
        return packet;
    }
    public override void Receive()
    {
        LocalizedText text = Language.GetText(LocalizationTextKey);
        if (!string.IsNullOrEmpty(ArgumentKey))
        {
            if (!string.IsNullOrEmpty(playerName))
                text = text.WithFormatArgs(Language.GetTextValue(ArgumentKey), playerName);
            else
                text = text.WithFormatArgs(Language.GetTextValue(ArgumentKey));

        }
        MyUtils.AddNotification(text.Value, IsFoundText ? Globe.foundColor : Globe.hintTextColor);
    }
}