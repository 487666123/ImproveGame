using ImproveGame.Common.Animations;
using ImproveGame.Interface.Common;
using ReLogic.Graphics;
using Terraria.GameContent.UI;
using Terraria.UI.Chat;

namespace ImproveGame.Common
{
    /// <summary>
    /// 修改自原版 <see cref="Terraria.GameContent.UI.Chat.ItemTagHandler"/> 用于绘制一个有背景框的物品
    /// </summary>
    public class BgItemTagHandler : ITagHandler
    {
        public class BgItemSnippet : TextSnippet
        {
            public Color BorderColor = UIColor.ItemSlotBorder;
            public Color Background = UIColor.ItemSlotBg;
            private Item _item;

            public BgItemSnippet(Item item)
            {
                _item = item;
                Color = ItemRarity.GetColor(item.rare);
            }

            public override void OnHover()
            {
                Main.HoverItem = _item.Clone();
                Main.instance.MouseText(_item.Name, _item.rare, 0);
            }

            public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch,
                Vector2 position = default, Color color = default, float scale = 1f)
            {
                if (Main.netMode is not NetmodeID.Server && !Main.dedServ)
                {
                    Main.instance.LoadItem(_item.type);
                    Texture2D value = TextureAssets.Item[_item.type].Value;
                    if (Main.itemAnimations[_item.type] != null)
                        Main.itemAnimations[_item.type].GetFrame(value);
                    else
                        value.Frame();
                }

                float invScale = scale;
                if (invScale > 0.75f)
                    invScale = 0.75f;

                size = new Vector2(52f) * scale * invScale;

                if (!justCheckingString && color != Color.Black)
                {
                    float inventoryScale = Main.inventoryScale;
                    Main.inventoryScale = scale * invScale;
                    SDFRectangle.HasBorder(position, size, new Vector4(12f), Background, 2, BorderColor);
                    ItemSlot.Draw(spriteBatch, ref _item, ItemSlot.Context.ChatItem, position, Color.White);
                    Main.inventoryScale = inventoryScale;
                }

                size += new Vector2(3f); // 这里拿来作间隔的，应当大一点了，GetStringLength不知道拿来干啥的反正绘制没用
                return true;
            }

            public override float GetStringLength(DynamicSpriteFont font) => 54f * Scale * 0.65f;
        }

        TextSnippet ITagHandler.Parse(string text, Color baseColor, string options)
        {
            Item item = new();
            if (int.TryParse(text, out int result) && result < ItemLoader.ItemCount)
                item.netDefaults(result);

            if (item.type < ItemID.None)
                return new TextSnippet(text);

            var snippet = new BgItemSnippet(item)
            {
                CheckForHover = true,
                DeleteWhole = true
            };

            item.stack = 1;
            if (options != null)
            {
                string[] array = options.Split(',');
                foreach (var arg in array)
                {
                    if (arg.Length == 0)
                        continue;

                    switch (arg[0])
                    {
                        case 's':
                            {
                                if (int.TryParse(arg.AsSpan(1), out var result3))
                                    item.stack = TrUtils.Clamp(result3, 1, item.maxStack);

                                break;
                            }
                        case 'f':
                            {
                                snippet.BorderColor = UIColor.ItemSlotBorderFav;
                                snippet.Background = UIColor.ItemSlotBgFav;
                                break;
                            }
                    }
                }
            }

            string str = "";
            if (item.stack > 1)
                str = " (" + item.stack + ")";

            snippet.Text = "[" + item.AffixName() + str + "]";

            return snippet;
        }

        public static string GenerateTag(Item item)
        {
            string str = "[bgitem";
            if (item.favorited || item.stack != 1)
                str += "/";
            if (item.favorited)
                str += "f,";
            if (item.stack != 1)
                str += "s" + item.stack + ",";
            return str + ":" + item.netID + "]";
        }
    }
}