using ImproveGame.Common.Configs;
using ImproveGame.Modules.InfiniteBuff;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.Graphics2D;
using System.Collections.ObjectModel;
using Terraria.UI.Chat;

namespace ImproveGame.Common.GlobalItems;

public class TagItem : GlobalItem
{
    /// <summary>
    /// 物品增益组合 (应改为 buff 识别，根据物品的 buff 判断，非兼容类药水可不显示提示提示)
    /// </summary>
    public static Dictionary<string, HashSet<int>> CombinationSets { get; } = new()
    {
        ["Battle"] = [ItemID.PeaceCandle, ItemID.WaterCandle, ItemID.BattlePotion, ItemID.Sunflower, ItemID.CalmingPotion]
    };

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        if (line.Mod == Mod.Name && line.Name.StartsWith("CombinedBuff") && Main.SettingsEnabled_OpaqueBoxBehindTooltips)
        {
            // 这样绘制，没有黑框，就有一种特殊的效果，挺好看的
            var font = FontAssets.MouseText.Value;
            var position = new Vector2(line.X, line.Y);
            var color = line.Color;
            var snippets = ChatManager.ParseMessage(line.Text, color);
            ChatManager.ConvertNormalSnippets(snippets);
            ChatManager.DrawColorCodedString(Main.spriteBatch, font, snippets, position, Color.White, 0f, Vector2.Zero, Vector2.One, out _, -1);
            return false;
        }
        return base.PreDrawTooltipLine(item, line, ref yOffset);
    }

    /// <summary>
    /// 为药水物品简介添加Tag标题文本
    /// </summary>
    public static void ModifyBuffTooltips(Mod mod, int itemType, int buffType, List<TooltipLine> tooltips)
    {
        bool buffEnabled = !Main.LocalPlayer.GetModPlayer<InfiniteBuffModPlayer>().Blacklist.ContainsByType(buffType);

        if (itemType is ItemID.GardenGnome)
        {
            tooltips.Add(new(mod, "TagDetailed.GardenGnome", MyUtils.GetText("Tips.TagDetailed.GardenGnome"))
            {
                Color = Color.SkyBlue
            });
            AddShiftForMoreTooltip(tooltips);
            return;
        }

        // 是否被增益追踪器禁用
        if (!buffEnabled)
        {
            tooltips.Add(new(mod, "TagDetailed.Disabled", MyUtils.GetText("Tips.TagDetailed.Disabled"))
            {
                Color = Color.SkyBlue
            });
        }
        else
        {
            tooltips.Add(new(mod, "TagDetailed.Enabled", MyUtils.GetText("Tips.TagDetailed.Enabled"))
            {
                Color = Color.SkyBlue
            });

            AddIconHiddenTooltips(mod, tooltips);
        }

        // 组合增益
        foreach (var (name, combination) in CombinationSets)
        {
            if (combination.Contains(itemType))
            {
                tooltips.Add(new(mod, $"CombinedBuff.{name}", MyUtils.GetText("Tips.TagDetailed.CombinedBuff"))
                {
                    Color = Color.Turquoise
                });
            }
        }

        AddShiftForMoreTooltip(tooltips);
    }

    public static void AddIconHiddenTooltips(Mod mod, List<TooltipLine> tooltips)
    {
        // 图标是否被隐藏
        if (UIConfigs.Instance.HideNoConsumeBuffs)
        {
            tooltips.Add(new TooltipLine(mod, "TagDetailed.Hided", MyUtils.GetText("Tips.TagDetailed.Hided"))
            {
                Color = Color.LightGreen
            });
        }
        else
        {
            tooltips.Add(new TooltipLine(mod, "TagDetailed.NonHided", MyUtils.GetText("Tips.TagDetailed.NonHided"))
            {
                Color = Color.LightGreen
            });
        }
    }

    public static void AddShiftForMoreTooltip(List<TooltipLine> tooltips)
    {
        // Shift显示更多信息
        if (!ItemSlot.ShiftInUse)
            tooltips.Add(new(ImproveGame.Instance, "Tag.ShiftEnable", MyUtils.GetText("Tips.Tag.ShiftEnable"))
            {
                Color = Color.Orange
            });
    }

    /// <summary>
    /// 根据物品原Tooltip中的Tag标题文本生成详细文本列表
    /// </summary>=
    public static List<TooltipLine> GenerateDetailedTags(Mod Mod, ReadOnlyCollection<TooltipLine> tooltips, object arg = null)
    {
        List<TooltipLine> list = new();
        foreach (TooltipLine line in tooltips)
        {
            if (line.Mod == Mod.Name)
            {
                // 一般的Tag提示
                if (line.Name.StartsWith("TagDetailed"))
                {
                    RegularTagSetup();
                    // Tag详细信息
                    string[] parts = line.Name.Split('.');
                    AddToList($"Tips.{parts[0]}Tip.{parts[1]}");
                }
                // 增益组合提示
                if (line.Name.StartsWith("CombinedBuff"))
                {
                    RegularTagSetup();
                    // Tag详细信息
                    AddToList($"Tips.TagDetailedTip.{line.Name}");
                }

                // 设置基本的Tag信息
                void RegularTagSetup()
                {
                    // 一行空位隔开
                    if (list.Count > 0)
                    {
                        list.Add(new(Mod, "Empty", ""));
                    }
                    // Tag名称
                    list.Add(new(Mod, line.Name, line.Text)
                    {
                        Color = line.Color
                    });
                }

                // 添加到list里面
                void AddToList(string key)
                {
                    if (arg is not null)
                    {
                        list.Add(new(Mod, key, MyUtils.GetTextWith(key, arg)));
                    }
                    else
                    {
                        list.Add(new(Mod, key, MyUtils.GetText(key)));
                    }
                }
            }
        }
        return list;
    }

    /// <summary>
    /// 绘制生成好的Tag详细文本
    /// <br/> 名称为 Empty 的行会生成一行空行
    /// </summary>
    /// <param name="tooltips">原物品Tooltip</param>
    /// <param name="tagTooltips">Tag Tooltip文本</param>
    /// <param name="x">原Tooltip文本绘制起始点 X 坐标</param>
    /// <param name="y">原Tooltip文本绘制起始点 Y 坐标</param>
    public static void DrawTagTooltips(ReadOnlyCollection<TooltipLine> tooltips, List<TooltipLine> tagTooltips, int x, int y)
    {
        var font = FontAssets.MouseText.Value;
        int widthOffset = 14;
        int heightOffset = 9;

        float length = 0f;
        foreach (TooltipLine line in tooltips)
        {
            length = Math.Max(length, ChatManager.GetStringSize(font, line.Text, Vector2.One).X);
        }
        x += (int)length + widthOffset * 2 + 6;

        float lengthY = 0f;
        length = 0f;
        foreach (TooltipLine line in tagTooltips)
        {
            if (line.Name == "Empty")
            {
                lengthY += 24;
                continue;
            }
            var stringSize = ChatManager.GetStringSize(font, line.Text, Vector2.One);
            length = Math.Max(length, stringSize.X + 8);
            lengthY += stringSize.Y;
        }
        if (Main.SettingsEnabled_OpaqueBoxBehindTooltips)
        {
            TrUtils.DrawInvBG(Main.spriteBatch, new Rectangle(x - widthOffset, y - heightOffset, (int)length + widthOffset * 2, (int)lengthY + heightOffset + heightOffset / 2), new Color(23, 25, 81, 255) * 0.925f);
        }

        foreach (TooltipLine line in tagTooltips)
        {
            if (line.Name == "Empty")
            {
                y += 24;
                continue;
            }
            int drawX = x;
            if (!line.Name.StartsWith("TagDetailed") && !line.Name.StartsWith("CombinedBuff"))
            {
                drawX += 8;
            }
            Color color = line.Color; // ?? new(0.7f, 0.7f, 0.7f);
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, line.Text, new(drawX, y), color, 0f, Vector2.Zero, Vector2.One, spread: 1.6f);
            y += (int)ChatManager.GetStringSize(font, line.Text, Vector2.One).Y;
        }
    }

    /// <summary>
    /// 纯粹的绘制另一个tooltips，没有 DrawTagTooltips 那些特判
    /// <br/> 名称为 Empty 的行会生成一行空行
    /// </summary>
    /// <param name="tooltips">原物品Tooltip</param>
    /// <param name="tagTooltips">Tooltip文本</param>
    /// <param name="x">原Tooltip文本绘制起始点 X 坐标</param>
    /// <param name="y">原Tooltip文本绘制起始点 Y 坐标</param>
    /// <param name="useBox">是否使用box, true为强制使用, false为强制不使用, null为随原版</param>
    /// <param name="shaderBorder">是否使用局长特制的丝滑Shader边框</param>
    public static void DrawTooltips(ReadOnlyCollection<TooltipLine> tooltips, List<TooltipLine> tagTooltips, int x, int y, bool? useBox = null, bool shaderBorder = true)
    {
        var font = FontAssets.MouseText.Value;
        int widthOffset = 9;
        int heightOffset = 9;

        float length = 0f;
        foreach (TooltipLine line in tooltips)
        {
            length = Math.Max(length, ChatManager.GetStringSize(font, line.Text, Vector2.One).X);
        }
        x += (int)length + 34;

        float lengthY = 0f;
        length = 0f;
        foreach (TooltipLine line in tagTooltips)
        {
            if (line.Name == "Empty")
            {
                lengthY += 24;
                continue;
            }
            var stringSize = ChatManager.GetStringSize(font, line.Text, Vector2.One);
            length = Math.Max(length, stringSize.X);
            lengthY += stringSize.Y;
        }
        if ((useBox is null && Main.SettingsEnabled_OpaqueBoxBehindTooltips) || useBox is true)
        {
            if (shaderBorder)
                SDFRectangle.HasBorder(new Vector2(x - widthOffset, y - heightOffset), new Vector2(length + widthOffset * 2, lengthY + heightOffset + heightOffset / 2), new Vector4(12f), UIStyle.PanelBg, 2, UIStyle.PanelBorder, Main.UIScaleMatrix);
            else
                TrUtils.DrawInvBG(Main.spriteBatch, new Rectangle(x - widthOffset, y - heightOffset, (int)length + widthOffset * 2, (int)lengthY + heightOffset + heightOffset / 2), new Color(23, 25, 81, 255) * 0.925f);
        }

        foreach (TooltipLine line in tagTooltips)
        {
            if (line.Name == "Empty")
            {
                y += 24;
                continue;
            }
            Color color = line.Color;// ?? new(0.7f, 0.7f, 0.7f);
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, line.Text, new(x, y), color, 0f, Vector2.Zero, Vector2.One, spread: 1.6f);
            y += (int)ChatManager.GetStringSize(font, line.Text, Vector2.One).Y;
        }
    }
}
