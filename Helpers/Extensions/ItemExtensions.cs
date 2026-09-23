using FuzzySearchNet;
using ImproveGame.Common.Configs;
using ImproveGame.Core;
using ImproveGame.UIFramework.Common;
using PinyinNet;
using Terraria.GameContent.UI;
using Terraria.ModLoader.Default;

namespace ImproveGame.Helpers.Extensions;

/// <summary>
/// <see cref="Item"/> 拓展
/// </summary>
public static class ItemExtensions
{
    extension(Item item)
    {
        /// <summary>
        /// 物品非空
        /// </summary>
        public bool IsNotAir => !item.IsAir;

        /// <summary>
        /// 物品是否未收藏
        /// </summary>
        public bool NotFavorited => !item.favorited;

        /// <summary>
        /// 物品非硬币
        /// </summary>
        public bool IsNotACoin => !item.IsACoin;
    }

    #region 搜索 - Searching

    /// <summary>
    /// 搜索匹配，支持拼音
    /// </summary>
    public static bool AnyMatchWithString(this IEnumerable<Item> items, string searchString)
    {
        if (string.IsNullOrEmpty(searchString))
            return false;

        string searchContent = MyUtils.RemoveSpaces(searchString.ToLower());
        return items.Any(item => MatchWithString(item, searchContent));
    }

    /// <summary>
    /// 搜索匹配，支持拼音
    /// </summary>
    public static bool MatchWithString(this Item item, string searchString, bool stringLowered = true)
    {
        if (string.IsNullOrEmpty(searchString))
            return false;

        string searchContent = stringLowered ? searchString : MyUtils.RemoveSpaces(searchString.ToLower());

        UIPlayerSetting setting = Main.LocalPlayer.GetModPlayer<UIPlayerSetting>();
        bool fuzzySearch = setting.FuzzySearch;
        bool tooltipSearch = setting.SearchTooltip;

        string currentLanguageName = MyUtils.RemoveSpaces(
                tooltipSearch
                    ? string.Concat(GetItemTooltipLines(item))
                    : Lang.GetItemNameValue(item.type))
            .ToLower();

        if (fuzzySearch)
        {
            if (FuzzySearch.Find(searchContent, currentLanguageName, 1).Any())
                return true;
        }

        if (currentLanguageName.Contains(searchContent))
            return true;

        if (Language.ActiveCulture.Name is not "zh-Hans") return false;

        string pinyin = MyUtils.RemoveSpaces(PinyinConvert.GetPinyinForAutoComplete(currentLanguageName));
        return fuzzySearch ? FuzzySearch.Find(searchContent, pinyin, 1).Any() : pinyin.Contains(searchContent);
    }

    #endregion

    #region 集合操作 - Collection Operations

    /// <summary>
    /// 有其中一个
    /// </summary>
    public static bool HasOne(this Item[] array, params int[] types)
    {
        if (array is null)
        {
            return false;
        }

        foreach (Item item in array)
        {
            if (item is not null && !item.IsAir && types.Contains(item.type))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 有全部
    /// </summary>
    public static bool HasAll(this Item[] array, params int[] types)
    {
        if (array is null)
        {
            return false;
        }

        int count = 0;

        foreach (var type in types)
        {
            foreach (Item item in array)
            {
                if (item is not null && !item.IsAir && item.type == type)
                {
                    count++;
                    break;
                }
            }
        }

        return count == types.Length;
    }

    /// <summary>
    /// 数组里面有这个物品
    /// </summary>
    public static bool TheArrayHas(this Item self, Item[] items)
    {
        if (items is null)
            return false;

        return (from i in items where i is not null && i.stack > 0 select i).Any(target => target.type == self.type);
    }

    #endregion

    #region 堆叠 - Stacking

    /// <summary>
    /// 无需考虑集合内有没有此物品，能堆叠进去即可
    /// </summary>
    /// <returns></returns>
    public static bool CanStackToArray(this Item source, Item[] items)
    {
        if (items is null)
        {
            return false;
        }

        foreach (Item target in items)
        {
            if (target.IsAir)
                return true;

            if (ItemLoader.CanStack(source, target) && target.type == source.type && target.stack < target.maxStack)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 堆叠到一个物品集合中
    /// </summary>
    public static void StackToArray(this Item source, Item[] items)
    {
        if (items is null)
        {
            return;
        }

        // source 来源
        // destination 目的地
        // 填补
        foreach (var destination in items)
        {
            if (destination is not null && !destination.IsAir && destination.type == source.type)
            {
                ItemLoader.TryStackItems(destination, source, out var _);

                if (source.IsAir)
                {
                    return;
                }
            }
        }

        // 创建
        for (int i = 0; i < items.Length; i++)
        {
            ref Item destination = ref items[i];

            if (destination is null || destination.IsAir)
            {
                destination = source.Clone();
                source.TurnToAir();
                return;
            }
        }
    }

    #endregion

    #region 设置物品默认参数 - Base Values Setting

    /// <summary>
    /// 设置物品默认参数
    /// </summary>
    /// <param name="item"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="rare">推荐使用 <see cref="ItemRarityID"/> 设置稀有度</param>
    /// <param name="value">可以使用 <see cref="Item.buyPrice(int, int, int, int)"/> 或 <see cref="Item.sellPrice(int, int, int, int)"/> 设置价格</param>
    /// <param name="maxStack"></param>
    /// <param name="consumable"></param>
    public static void SetBaseValues(this Item item, int width, int height, int rare = 0, int value = 0,
        int maxStack = 1, bool consumable = false)
    {
        item.width = width;
        item.height = height;
        item.rare = rare;
        item.value = value;
        item.maxStack = maxStack;
        item.consumable = consumable;
    }

    /// <summary>
    /// 设置挥舞物品的默认参数
    /// </summary>
    /// <param name="item"></param>
    /// <param name="useStyle">推荐使用 <see cref="ItemUseStyleID"/> 设置使用类型</param>
    /// <param name="soundStyle">推荐使用 <see cref="SoundStyle"/> 设置使用音效</param>
    /// <param name="useAnimation"></param>
    /// <param name="useTime"></param>
    /// <param name="autoReuse"></param>
    /// <param name="mana"></param>
    public static void SetUseValues(this Item item, int useStyle, SoundStyle soundStyle, int useAnimation, int useTime,
        bool autoReuse = false, int mana = 0)
    {
        item.useStyle = useStyle;
        item.UseSound = soundStyle;
        item.useAnimation = useAnimation;
        item.useTime = useTime;
        item.autoReuse = autoReuse;
        item.mana = mana;
    }

    /// <summary>
    /// 设置武器物品的默认参数
    /// </summary>
    /// <param name="item"></param>
    /// <param name="damage"></param>
    /// <param name="damageType">推荐使用 <see cref="DamageClass"/> 设置伤害类型</param>
    /// <param name="knockBack"></param>
    /// <param name="noMelee"></param>
    public static void SetWeaponValues(this Item item, int damage, DamageClass damageType, float knockBack = 0f,
        bool noMelee = false)
    {
        item.damage = damage;
        item.DamageType = damageType;
        item.knockBack = knockBack;
        item.noMelee = noMelee;
    }

    /// <summary>
    /// 弹幕相关属性设置
    /// </summary>
    /// <param name="item"></param>
    /// <param name="shoot"></param>
    /// <param name="shootSpeed"></param>
    /// <param name="useAmmo">推荐使用 <see cref="AmmoID"/> 设置使用的弹药类型</param>
    /// <param name="ammo">推荐使用 <see cref="AmmoID"/> 设置弹药归属</param>
    public static void SetShootValues(this Item item, int shoot, float shootSpeed, int useAmmo = 0, int ammo = 0)
    {
        item.shoot = shoot;
        item.shootSpeed = shootSpeed;
        item.useAmmo = useAmmo;
        item.ammo = ammo;
    }

    #endregion

    #region 筛选 - Filtering

    public static bool IsHook(this Item item) => Main.projHook.IndexInRange(item.shoot) && Main.projHook[item.shoot];

    public static bool IsTool(this Item item) => IsOrdinaryTool(item) || item.IsHook() || item.fishingPole > 0 ||
                                                 IsWiringTool(item) || IsOtherTool(item);

    public static bool IsAccessory(this Item item) => item.accessory;

    public static bool IsAmmo(this Item item) => item.ammo != AmmoID.None && !item.notAmmo;

    /// <summary>
    /// 可被用作弹药，与 IsAmmo 不同的是钱币（钱币枪弹药）、沙子（沙枪弹药）什么的也会算进来
    /// </summary>
    public static bool CanBeUsedAsAmmo(this Item item) => item.ammo != AmmoID.None;

    public static bool IsArmor(this Item item) => item.headSlot >= 0 || item.bodySlot >= 0 || item.legSlot >= 0;

    public static bool IsPlaceable(this Item item) => item.createTile >= TileID.Dirt || item.createWall > WallID.None;

    public static bool IsWeapon(this Item item) =>
        item.damage > 0 && item.axe is 0 && item.hammer is 0 && item.pick is 0;

    public static ContentSamples.CreativeHelper.ItemGroup GetCreativeItemGroup(this Item item) =>
        ContentSamples.CreativeHelper.GetItemGroup(item, out _);

    public static bool IsOrdinaryTool(this Item item) => item.axe != 0 || item.hammer != 0 || item.pick != 0;

    public static bool IsOtherTool(this Item item) => ItemID.Sets.DuplicationMenuToolsFilter[item.type];

    public static bool IsWiringTool(this Item item) =>
        GetCreativeItemGroup(item) is ContentSamples.CreativeHelper.ItemGroup.Wiring;

    public static bool IsHerb(this Item item) => item.GetCreativeItemGroup()
        is ContentSamples.CreativeHelper.ItemGroup.AlchemyPlants
        or ContentSamples.CreativeHelper.ItemGroup.AlchemySeeds;

    public static bool IsSummonItem(this Item item) => item.GetCreativeItemGroup()
        is ContentSamples.CreativeHelper.ItemGroup.EventItem
        or ContentSamples.CreativeHelper.ItemGroup.BossItem;

    public static bool IsPet(this Item item) => item.GetCreativeItemGroup()
        is ContentSamples.CreativeHelper.ItemGroup.VanityPet
        or ContentSamples.CreativeHelper.ItemGroup.LightPet;

    public static bool IsMount(this Item item) => item.GetCreativeItemGroup()
        is ContentSamples.CreativeHelper.ItemGroup.Mount;

    public static bool IsMaterial(this Item item) => item.material;

    public static bool IsBankItem(this Item item) => IsBankItem(item.type);

    public static bool IsBankItem(int type) => Lookups.Bank2Items.Contains(type) || Lookups.Bank3Items.Contains(type) ||
                                               Lookups.Bank4Items.Contains(type) || Lookups.Bank5Items.Contains(type);

    #endregion

    #region 其他 - Misc

    public static bool IsSameItem(Item item1, Item item2)
    {
        // 卸载物品是特殊的
        if (item1.ModItem is UnloadedItem u1 && item2.ModItem is UnloadedItem u2)
        {
            if (u1.ItemName == u2.ItemName && u1.ModName == u2.ModName)
                return true;
            return false;
        }

        // 正常物品
        return item1.type == item2.type;
    }

    public static bool IsCustomCurrency(int type) =>
        CustomCurrencyManager._currencies.Any(currencies => currencies.Value._valuePerUnit.ContainsKey(type));

    private static IEnumerable<string> GetItemTooltipLines(Item item)
    {
        Item hoverItem = item;
        int yoyoLogo = -1;
        int rare = ItemRarityID.White;

        float knockBack = hoverItem.knockBack;
        float num = 1f;
        if (hoverItem.melee && Main.LocalPlayer.kbGlove)
            num += 1f;

        if (Main.LocalPlayer.kbBuff)
            num += 0.5f;

        if (num != 1f)
            hoverItem.knockBack *= num;

        if (hoverItem.ranged && Main.LocalPlayer.shroomiteStealth)
            hoverItem.knockBack *= 1f + (1f - Main.LocalPlayer.stealth) * 0.5f;

        int numLines = 1;
        string[] mouseTextTooltipLine_Text = Main._mouseTextTooltipLine_Text;
        Color[] mouseTextTooltipLine_Color = Main._mouseTextTooltipLine_Color;

        int expectedLineCount = Main._mouseTextTooltip_MaxLines + hoverItem.ToolTip?.Lines ?? 0; // Easy fix to #4772. A full fix would require rewrites of hooks.
        if (mouseTextTooltipLine_Text.Length < expectedLineCount)
        {
            Array.Resize(ref mouseTextTooltipLine_Text, expectedLineCount);
            Array.Resize(ref mouseTextTooltipLine_Color, expectedLineCount);
        }

        float num2 = Main.mouseTextColor / 255f;
        for (int i = 0; i < mouseTextTooltipLine_Text.Length; i++)
        {
            mouseTextTooltipLine_Color[i] = new Color(255, 255, 255);
        }

        mouseTextTooltipLine_Color[0] = Main.MouseText_DrawItemTooltip_GetItemNameColor(rare, 0);

        // This array will be filled with internal names assigned to vanilla tooltips.
        string[] tooltipNames = new string[mouseTextTooltipLine_Text.Length];

        Main.MouseText_DrawItemTooltip_GetLinesInfo(hoverItem, ref yoyoLogo, knockBack, ref numLines, mouseTextTooltipLine_Text, mouseTextTooltipLine_Color, tooltipNames, out int prefixlineIndex);
        Main.MouseText_DrawItemTooltip_AddShopLines(hoverItem, ref numLines, mouseTextTooltipLine_Text, mouseTextTooltipLine_Color, tooltipNames);
        if (NewCraftingUI.Visible)
            NewCraftingUI.AddTooltipLines(hoverItem, ref numLines, mouseTextTooltipLine_Text, mouseTextTooltipLine_Color, tooltipNames);

        Vector2 zero = Vector2.Zero;

        // TML's abstractions over tooltip arrays.
        List<TooltipLine> lines = ItemLoader.ModifyTooltips(item, ref numLines, tooltipNames, ref mouseTextTooltipLine_Text, ref mouseTextTooltipLine_Color, ref yoyoLogo, prefixlineIndex);
        return lines.Select(line => line.Text);
    }

    public static bool CanActivateRedPotionExtension(this Item item)
    {
        bool isFtw = ImproveConfigs.Instance.RedPotionEverywhere || Main.getGoodWorld;
        return item.type is ItemID.RedPotion && ImproveConfigs.Instance.InfiniteRedPotion && isFtw &&
               item.stack >= ImproveConfigs.Instance.RedPotionRequirement && ImproveConfigs.Instance.NoConsume_Potion;
    }

    public static void DrawIcon(this Item item, SpriteBatch sb, Color lightColor, Vector2 center,
        float maxSize = 32f, float itemScale = 1f)
    {
        Main.instance.LoadItem(item.type);
        Texture2D texture2D = TextureAssets.Item[item.type].Value;
        Rectangle frame = Main.itemAnimations[item.type] is null
            ? texture2D.Frame()
            : Main.itemAnimations[item.type].GetFrame(texture2D);
        itemScale *= frame.Width > maxSize || frame.Height > maxSize
            ? frame.Width > frame.Height ? maxSize / frame.Width : maxSize / frame.Height
            : 1f;
        Vector2 origin = frame.Size() / 2f;
        if (ItemLoader.PreDrawInInventory(item, sb, center, frame, item.GetAlpha(lightColor),
                item.GetColor(lightColor), origin, itemScale))
        {
            sb.Draw(texture2D, center, frame, item.GetAlpha(lightColor), 0f, origin, itemScale,
                SpriteEffects.None, 0f);
            if (item.color != Color.Transparent)
                sb.Draw(texture2D, center, frame, item.GetColor(lightColor), 0f, origin, itemScale,
                    SpriteEffects.None, 0f);
        }

        ItemLoader.PostDrawInInventory(item, sb, center, frame, item.GetAlpha(lightColor),
            item.GetColor(lightColor), origin, itemScale);

        if (ItemID.Sets.TrapSigned[item.type])
            Main.spriteBatch.Draw(TextureAssets.Wire.Value, center + new Vector2(14f) * itemScale,
                new Rectangle(4, 58, 8, 8), lightColor, 0f, new Vector2(4f), 1f, SpriteEffects.None, 0f);

        if (ItemID.Sets.DrawUnsafeIndicator[item.type])
        {
            Vector2 vector2 = new Vector2(-4f, -4f) * itemScale;
            Texture2D value7 = TextureAssets.Extra[258].Value;
            Rectangle rectangle2 = value7.Frame();
            Main.spriteBatch.Draw(value7, center + vector2 + new Vector2(14f) * itemScale, rectangle2, lightColor, 0f,
                rectangle2.Size() / 2f, 1f, SpriteEffects.None, 0f);
        }

        if (item.type is ItemID.RubblemakerSmall or ItemID.RubblemakerMedium or ItemID.RubblemakerLarge)
        {
            Vector2 vector3 = new Vector2(2f, -6f) * itemScale;
            switch (item.type)
            {
                case 5324:
                {
                    Texture2D value10 = TextureAssets.Extra[257].Value;
                    Rectangle rectangle5 = value10.Frame(3, 1, 2);
                    Main.spriteBatch.Draw(value10, center + vector3 + new Vector2(16f) * itemScale, rectangle5,
                        lightColor, 0f, rectangle5.Size() / 2f, 1f, SpriteEffects.None, 0f);
                    break;
                }
                case 5329:
                {
                    Texture2D value9 = TextureAssets.Extra[257].Value;
                    Rectangle rectangle4 = value9.Frame(3, 1, 1);
                    Main.spriteBatch.Draw(value9, center + vector3 + new Vector2(16f) * itemScale, rectangle4,
                        lightColor, 0f, rectangle4.Size() / 2f, 1f, SpriteEffects.None, 0f);
                    break;
                }
                case 5330:
                {
                    Texture2D value8 = TextureAssets.Extra[257].Value;
                    Rectangle rectangle3 = value8.Frame(3);
                    Main.spriteBatch.Draw(value8, center + vector3 + new Vector2(16f) * itemScale, rectangle3,
                        lightColor, 0f, rectangle3.Size() / 2f, 1f, SpriteEffects.None, 0f);
                    break;
                }
            }
        }
    }

    #endregion
}