using Terraria.ModLoader.Config;

namespace ImproveGame.UI.ModernConfig.OptionElements;

public class OptionLabelElement(ModConfig config, string optionName, int reservedWidth = 60, string forcedName = null)
    : SlideText("", reservedWidth)
{
    public string OriginLabel()
    {
        if (forcedName != null)
            return MyUtils.ConvertLeftRight(forcedName);
        string key = $"Mods.{config.Mod.Name}.Configs.{config.GetType().Name}.{optionName}.Label";
        if (Language.Exists(key))
            return MyUtils.ConvertLeftRight(Language.GetTextValue(key));
        return MyUtils.ConvertLeftRight(optionName);

    }

}
