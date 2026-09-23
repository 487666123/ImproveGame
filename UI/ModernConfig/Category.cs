using Terraria.ModLoader.Config;

namespace ImproveGame.UI.ModernConfig;

public abstract class Category
{
    public virtual int ItemIconId => 0;

    public virtual Texture2D GetIcon()
    {
        try
        {
            if (ItemIconId < ItemID.Count)
                Main.instance.LoadItem(ItemIconId);
            return TextureAssets.Item[ItemIconId].Value;
        }
        catch
        {
            return TextureAssets.MagicPixel.Value;
        }
    }

    public virtual string LocalizationKey => GetType().Name;

    public abstract void AddOptions(ConfigOptionsPanel panel);

    public virtual string Label => MyUtils.GetText($"ModernConfig.{LocalizationKey}.Label");
    public virtual string Tooltip => MyUtils.GetText($"ModernConfig.{LocalizationKey}.Tooltip");

    public virtual Func<ModConfig, string, bool> CanOptionBeAdded => (config, optionName) => true;
}