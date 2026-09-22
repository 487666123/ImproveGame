using ImproveGame.UI.PlayerStats;
using Terraria.ModLoader.IO;

namespace ImproveGame.UIFramework.Common;

[AutoSync]
public class PlayerBigBagSettingPacket : NetModule
{
    public int WhoAmI;
    public bool SuperVault_PrioritizeGrabbing;
    public bool SuperVault_GrabItemsWhenOverflowing;

    public static void SendMyPlayer()
    {
        if (Main.LocalPlayer.TryGetModPlayer<UIPlayerSetting>(out var setting))
        {
            PlayerBigBagSettingPacket settingPacket = NetModuleLoader.Get<PlayerBigBagSettingPacket>();
            settingPacket.WhoAmI = Main.myPlayer;
            settingPacket.SuperVault_PrioritizeGrabbing = setting.SuperVault_PrioritizeGrabbing;
            settingPacket.SuperVault_GrabItemsWhenOverflowing = setting.SuperVault_GrabItemsWhenOverflowing;
            settingPacket.Send();
        }
    }

    public override void Receive()
    {
        if (Main.player[WhoAmI].TryGetModPlayer<UIPlayerSetting>(out var setting))
        {
            setting.SuperVault_PrioritizeGrabbing = SuperVault_PrioritizeGrabbing;
            setting.SuperVault_GrabItemsWhenOverflowing = SuperVault_GrabItemsWhenOverflowing;
        }
    }
}

public class UIPlayerSetting : ModPlayer
{
    /// <summary>
    /// 大背包：让大背包中的参与物品合成
    /// </summary>
    public bool SuperVault_ParticipateSynthesis;

    /// <summary>
    /// 大背包：即将进入背包的物品在大背包中有同类物品时，优先进入大背包。
    /// </summary>
    public bool SuperVault_PrioritizeGrabbing;

    /// <summary>
    /// 大背包：如果物品无法进入普通背包时，尝试进入大背包
    /// </summary>
    public bool SuperVault_GrabItemsWhenOverflowing;

    public bool FuzzySearch;
    public bool SearchTooltip;

    public TagCompound ProCatsPos = [];
    public TagCompound ProCatsFav = [];
    public TagCompound ProFavs = [];

    public override void LoadData(TagCompound tag)
    {
        tag.TryGet(nameof(SuperVault_ParticipateSynthesis), out SuperVault_ParticipateSynthesis);
        tag.TryGet(nameof(SuperVault_PrioritizeGrabbing), out SuperVault_PrioritizeGrabbing);
        tag.TryGet(nameof(SuperVault_GrabItemsWhenOverflowing), out SuperVault_GrabItemsWhenOverflowing);
        tag.TryGet(nameof(FuzzySearch), out FuzzySearch);
        tag.TryGet(nameof(SearchTooltip), out SearchTooltip);

        if (tag.TryGet("ProCatsPos", out TagCompound proCatsPos))
        {
            ProCatsPos = proCatsPos;
        }

        if (tag.TryGet("ProCatsFav", out TagCompound proCatsFav))
        {
            ProCatsFav = proCatsFav;
        }

        if (tag.TryGet("ProFavs", out TagCompound proFavs))
        {
            ProFavs = proFavs;
        }
    }

    public override void SaveData(TagCompound tag)
    {
        tag.Set(nameof(SuperVault_ParticipateSynthesis), SuperVault_ParticipateSynthesis);
        tag.Set(nameof(SuperVault_PrioritizeGrabbing), SuperVault_PrioritizeGrabbing);
        tag.Set(nameof(SuperVault_GrabItemsWhenOverflowing), SuperVault_GrabItemsWhenOverflowing);
        tag.Set(nameof(FuzzySearch), FuzzySearch);
        tag.Set(nameof(SearchTooltip), SearchTooltip);

        TagCompound proCatsPos = [];
        TagCompound proCatsFav = [];
        TagCompound proFavs = [];

        foreach (var proCat in PlayerStatsSystem.Instance.StatsCategories)
        {
            proCatsPos.Set(proCat.Key, proCat.Value.UIPosition);
            proCatsFav.Set(proCat.Key, proCat.Value.Favorite);

            TagCompound proFav = [];

            foreach (var stat in proCat.Value.BaseProperties)
            {
                proFav.Set(stat.Name, stat.Favorite);
            }

            proFavs.Set(proCat.Key, proFav);
        }

        tag.Set("ProCatsPos", proCatsPos);
        tag.Set("ProCatsFav", proCatsFav);
        tag.Set("ProFavs", proFavs);
    }
}
