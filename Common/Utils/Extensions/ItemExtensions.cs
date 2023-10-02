using PinyinNet;

namespace ImproveGame.Common.Utils.Extensions;

/// <summary>
/// <see cref="Item"/> 拓展
/// </summary>
public static class ItemExtensions
{
    /// <summary>
    /// 搜索匹配，支持拼音
    /// </summary>
    public static bool AnyMatchWithString(this IEnumerable<Item> items, string searchString)
    {
        if (string.IsNullOrEmpty(searchString))
            return false;

        string searchContent = RemoveSpaces(searchString.ToLower());
        return items.Any(item => MatchWithString(item, searchContent));
    }
    
    /// <summary>
    /// 搜索匹配，支持拼音
    /// </summary>
    public static bool MatchWithString(this Item item, string searchString, bool stringLowered = true)
    {
        if (string.IsNullOrEmpty(searchString))
            return false;

        string searchContent = stringLowered ? searchString : RemoveSpaces(searchString.ToLower());
        string currentLanguageName = RemoveSpaces(Lang.GetItemNameValue(item.type).ToLower());
        if (currentLanguageName.Contains(searchContent))
            return true;

        if (Language.ActiveCulture.Name is not "zh-Hans") return false;

        string pinyin = RemoveSpaces(PinyinConvert.GetPinyinForAutoComplete(currentLanguageName));
        return pinyin.Contains(searchContent);
    }
    
    private static string RemoveSpaces(string s) => s.Replace(" ", "", StringComparison.Ordinal);
    
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
        {
            return false;
        }

        foreach (var target in from i in items where i is not null && i.stack > 0 select i)
        {
            if (target.type == self.type)
            {
                return true;
            }
        }

        return false;
    }

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
            if ((target.IsAir || (target.type == source.type && target.stack < target.maxStack)) &&
                ItemLoader.CanStack(source, target))
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
}
