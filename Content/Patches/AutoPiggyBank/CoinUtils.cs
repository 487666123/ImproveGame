namespace ImproveGame.Content.Patches.AutoPiggyBank;

public static class CoinUtils
{
    public static int PlatinumMaxStack = 9999;
    private const ulong CopperValue = 1;
    private const ulong SilverValue = 100;
    private const ulong GoldValue = 100 * 100;
    private const ulong PlatinumValue = 100 * 100 * 100;

    /// <summary>
    /// Checks if an item is a coin.
    /// </summary>
    /// <param name="type">The item's type id</param>
    /// <returns>True if yes, false if no</returns>
    public static bool IsCoin(int type)
    {
        return type is >= 71 and <= 74;
    }

    /// <summary>
    /// Calculates
    /// </summary>
    /// <param name="type">The type of coin. Must be between 71-74.</param>
    /// <param name="stack">The amount of coins</param>
    /// <returns>The value in copper coins, if type is valid, otherwise 0.</returns>
    public static ulong CalculateCoinValue(int type, uint stack)
    {
        switch (type)
        {
            case ItemID.CopperCoin:
                return stack * CopperValue;
            case ItemID.SilverCoin:
                return stack * SilverValue;
            case ItemID.GoldCoin:
                return stack * GoldValue;
            case ItemID.PlatinumCoin:
                return stack * PlatinumValue;
            default:
                return 0;
        }
    }

    /// <summary>
    /// Converts a copper value to the 4 types of coins like in-game.
    /// </summary>
    /// <param name="value">The copper value</param>
    /// <returns>An Item list of coin stacks in the following format: [copper, silver, gold, platinum]</returns>
    public static List<Item> ConvertCopperValueToCoins(ulong value)
    {
        (ulong plat, ulong plat_rem) = Math.DivRem(value, PlatinumValue);
        (ulong gold, ulong gold_rem) = Math.DivRem(plat_rem, GoldValue);
        (ulong silver, ulong copper) = Math.DivRem(gold_rem, SilverValue);

        var toReturn = new List<Item>();

        while (plat > 0)
        {
            toReturn.Add(new Item(ItemID.PlatinumCoin, Math.Min((int)plat, PlatinumMaxStack)));
            plat -= Math.Min(plat, (ulong)PlatinumMaxStack);
        }

        toReturn.Add(new Item(ItemID.GoldCoin, (int)gold));
        toReturn.Add(new Item(ItemID.SilverCoin, (int)silver));
        toReturn.Add(new Item(ItemID.CopperCoin, (int)copper));

        return toReturn;
    }

    /// <summary>
    /// Places items into a chest in a specific way. If there is already a stack of the current item in the chest, then it replaces that stack, otherwise it places the stack into the first empty slot.
    /// </summary>
    /// <param name="chest">The chest object to put the items into</param>
    /// <param name="items">The items to put into the chest</param>
    public static void ReplaceOrPlaceIntoChest(Chest chest, List<Item> items)
    {
        var toIgnore = new List<int>();

        foreach (Item item in items)
        {
            for (int i = 0; i < chest.item.Length; i++)
            {
                if (toIgnore.Contains(i)) continue;

                if (chest.item[i].type == item.type)
                {
                    chest.item[i] = item.Clone();
                    item.TurnToAir();
                    toIgnore.Add(i);
                    goto outer_end;
                }
            }

            for (int i = 0; i < chest.item.Length; i++)
            {
                if (toIgnore.Contains(i)) continue;
                 
                if (chest.item[i].stack == 0)
                {
                    chest.item[i] = item.Clone();
                    item.TurnToAir();
                    toIgnore.Add(i);
                    goto outer_end;
                }
            }

            outer_end:;
        }
    }
}