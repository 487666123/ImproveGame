namespace ImproveGame.UI.PlayerStats;

/// <summary>
/// 基本属性
/// </summary>
public class BaseStat(BaseStatsCategory parent, string nameKey, Func<string> value, bool isAddedFromCall = false)
{
    /// <summary>
    /// Whether this stat category is added via Mod.Call
    /// </summary>
    public bool IsAddedFromCall { get; set; } = isAddedFromCall;

    public bool Favorite { get; set; } = true;
    public BaseStatsCategory Parent { get; set; } = parent;

    public string NameKey { get; set; } = nameKey;

    public string Name => IsAddedFromCall ? Language.GetTextValue(NameKey) : MyUtils.GetText(NameKey);

    public Func<string> Value { get; set; } = value;
}
