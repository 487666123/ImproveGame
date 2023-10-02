using ImproveGame.Common.Configs;
using ImproveGame.Common.Packets;
using ImproveGame.Common.ModPlayers;
using ImproveGame.Common.Packets.Items;
using ImproveGame.Content.Items.Coin;
using ImproveGame.Core;
using ImproveGame.Interface.GUI.WorldFeature;
using ReLogic.Graphics;
using System.Collections;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.UI.Chat;
using static Microsoft.Xna.Framework.Vector2;

namespace ImproveGame;

/// <summary>
/// 局长自用工具
/// </summary>
partial class MyUtils
{
    public static bool HasDevMark => Main.LocalPlayer.inventory.Any(i => i.type == ModContent.ItemType<DevMark>()) &&
                                     Main.netMode is NetmodeID.SinglePlayer;

    public static Matrix GetMatrix(bool ui)
    {
        if (ui)
        {
            GraphicsDevice graphicsDevice = Main.graphics.GraphicsDevice;

            float width;
            float height;

            RenderTargetBinding[] renderTargetBinding = graphicsDevice.GetRenderTargets();
            if (renderTargetBinding.Length > 0 && renderTargetBinding[0].RenderTarget is Texture2D texture2D)
            {
                width = texture2D.Width;
                height = texture2D.Height;
            }
            else
            {
                width = graphicsDevice.PresentationParameters.BackBufferWidth;
                height = graphicsDevice.PresentationParameters.BackBufferHeight;
            }

            return Matrix.CreateOrthographicOffCenter(0, width / Main.UIScale, height / Main.UIScale, 0, 0, 1);
        }
        else
        {
            Vector2 screenSize = new Vector2(Main.screenWidth, Main.screenHeight);
            Vector2 offset = screenSize * (Vector2.One - Vector2.One / Main.GameViewMatrix.Zoom) / 2;
            return Matrix.CreateOrthographicOffCenter(offset.X, Main.screenWidth - offset.X,
                Main.screenHeight - offset.Y, offset.Y, 0, 1);
        }
    }

    public static void UseItemByType(Player player, int itemType)
    {
        if (!player.ItemAnimationEndingOrEnded) return;

        const int fakeSlot = 58;
        var originalItem = player.inventory[fakeSlot];
        player.inventory[fakeSlot] = new Item(itemType);
        QuickUseItem(player, fakeSlot);
        player.inventory[fakeSlot] = originalItem;
    }

    public static void QuickUseItem(Player player, int index)
    {
        player.selectedItem = index;
        player.controlUseItem = true;
        if (CombinedHooks.CanUseItem(player, player.inventory[player.selectedItem]))
        {
            player.ItemCheck();
        }
    }

    public static Vector2 GetFontSize(int text, bool large = false)
    {
        return GetFontSize(text.ToString(), large);
    }

    public static Vector2 GetFontSize(string text, bool large = false)
    {
        if (large)
        {
            return FontAssets.DeathText.Value.MeasureString(text);
        }

        return FontAssets.MouseText.Value.MeasureString(text);
    }

    public static Vector2 GetChatFontSize(string text, float textScale, bool large = false)
    {
        if (large)
        {
            return ChatManager.GetStringSize(FontAssets.DeathText.Value, text, new Vector2(1f)) * textScale;
        }

        return ChatManager.GetStringSize(FontAssets.MouseText.Value, text, new Vector2(1f)) * textScale;
    }

    public static void DrawString(Vector2 position, string text, Color textColor, Color borderColor, float scale = 1f,
        bool large = false)
    {
        DrawString(position, text, textColor, borderColor, Zero, scale, large);
    }

    public static void DrawString(Vector2 pos, string text, Color textColor, Color borderColor, Vector2 origin,
        float textScale, bool large)
    {
        DynamicSpriteFont spriteFont = (large ? FontAssets.DeathText : FontAssets.MouseText).Value;

        float x = pos.X;
        float y = pos.Y;
        Color color = borderColor;
        float border = 2f * textScale;
        for (int i = 0; i < 5; i++)
        {
            switch (i)
            {
                case 0:
                    pos.X = x - border;
                    pos.Y = y;
                    break;
                case 1:
                    pos.X = x + border;
                    pos.Y = y;
                    break;
                case 2:
                    pos.X = x;
                    pos.Y = y - border;
                    break;
                case 3:
                    pos.X = x;
                    pos.Y = y + border;
                    break;
                default:
                    pos.X = x;
                    pos.Y = y;
                    color = textColor;
                    break;
            }

            Main.spriteBatch.DrawString(spriteFont, text, pos, color, 0f, origin, textScale, 0, 0f);
        }
    }

    public static Color[] GetColors(Texture2D texture)
    {
        var w = texture.Width;
        var h = texture.Height;
        var cs = new Color[w * h]; // 创建一个能容下整个贴图颜色信息的 Color[]
        texture.GetData(cs); // 获取颜色信息
        return cs;
    }

    public static bool IntArrayContains(int[] array, int value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == value)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 设置物品旋转（带上玩家方向改变）
    /// </summary>
    /// <param name="player">被操作的玩家实例</param>
    /// <param name="shouldSync">是否应该进行网络同步</param>
    public static void ItemRotation(Player player, bool shouldSync = true)
    {
        // 旋转物品
        Vector2 rotation = (Main.MouseWorld - player.Center).SafeNormalize(Zero);
        player.direction = Main.MouseWorld.X < player.Center.X ? -1 : 1;
        player.itemRotation = MathF.Atan2(rotation.Y * player.direction, rotation.X * player.direction);
        if (shouldSync && Main.netMode != NetmodeID.SinglePlayer && player.whoAmI == Main.myPlayer)
        {
            ItemUsePacket.Get(player).Send();
        }
    }


    /// <summary>
    /// 可以用来实现间隔为10帧的rotation同步
    /// </summary>
    public static IEnumerator ItemRotationCoroutines(Player player, bool shouldSync = true)
    {
        ItemRotation(player, shouldSync);
        yield return null;
    }

    /// <summary>
    /// 为 <seealso cref="ImproveConfigs.ShareRange"/> 范围内的同队玩家执行Action
    /// </summary>
    public static void CheckTeamPlayers(int myselfIndex, Action<Player> whatToDo, bool checkDead = true)
    {
        if (myselfIndex < 0 || myselfIndex >= 255 || Main.netMode is NetmodeID.SinglePlayer)
        {
            return;
        }

        for (int i = 0; i < 255; i++)
        {
            var player = Main.player[i];
            if (i != myselfIndex && player.active && (!player.DeadOrGhost || !checkDead) && player.team is not 0 &&
                player.team == Main.player[myselfIndex].team)
            {
                // 范围检测
                if (Config.ShareRange != -1 &&
                    player.Distance(Main.player[myselfIndex].Center) / 16f > Config.ShareRange)
                {
                    continue;
                }

                whatToDo(player);
            }
        }
    }

    /// <summary>
    /// 获取相应热键的当前绑定名称
    /// </summary>
    public static bool TryGetKeybindString(ModKeybind keybind, out string bindName)
    {
        if (Main.dedServ || keybind == null)
        {
            bindName = "ERROR";
            return false;
        }

        List<string> keys = keybind.GetAssignedKeys();
        if (keys.Count == 0)
        {
            bindName = Language.GetTextValue("LegacyMenu.195"); // <未绑定>
            return false;
        }

        var keybindListItem = new UIKeybindingListItem(keys[0], InputMode.Keyboard, Color.White);
        bindName = keybindListItem.GenInput(keys);
        // StringBuilder sb = new(16);
        // sb.Append(keys[0]);
        // for (int i = 1; i < keys.Count; i++)
        // {
        //     sb.Append(" / ").Append(keys[i]);
        // }
        // bindName = sb.ToString();
        return true;
    }

    /// <summary>
    /// 修改 Reangle 的大小，起点不会变，终点改变。（大于限制，小于不变，这个名字比 Limit 好看而已）
    /// </summary>
    /// <param name="start">起点</param>
    /// <param name="end">终点</param>
    /// <param name="width">限制后的宽度</param>
    /// <param name="height">限制的高度</param>
    /// <returns>终点的位置</returns>
    public static Point ModifySize(Point start, Point end, int width, int height)
    {
        width--;
        height--;
        if (end.X - start.X < -width)
            end.X = start.X - width;
        else if (end.X - start.X > width)
            end.X = start.X + width;

        if (end.Y - start.Y < -height)
            end.Y = start.Y - height;
        else if (end.Y - start.Y > height)
            end.Y = start.Y + height;
        return end;
    }

    public static void DrawBorderRect(Rectangle tileRectangle, Color backgroundColor, Color borderColor)
    {
        var position = tileRectangle.TopLeft() * 16f - Main.screenPosition;
        DrawBorder(position, tileRectangle.Width * 16, tileRectangle.Height * 16, backgroundColor, borderColor);
    }

    public static void DrawBorderRectangle(Rectangle tileRectangleInScreen, Color backgroundColor, Color borderColor)
    {
        Texture2D texture = TextureAssets.MagicPixel.Value;
        Vector2 position = tileRectangleInScreen.TopLeft() * 16f;
        Vector2 scale = new(tileRectangleInScreen.Width, tileRectangleInScreen.Height);
        Main.spriteBatch.Draw(
            texture,
            position,
            new(0, 0, 1, 1),
            backgroundColor,
            0f,
            Zero,
            16f * scale,
            SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(
            texture,
            position + UnitX * -2f + UnitY * -2f,
            new(0, 0, 1, 1),
            borderColor, 0f, Zero,
            new Vector2(2f, 16f * scale.Y + 4),
            SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(texture,
            position + UnitX * 16f * scale.X + UnitY * -2f,
            new(0, 0, 1, 1),
            borderColor, 0f, Zero,
            new Vector2(2f, 16f * scale.Y + 4), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(texture,
            position + UnitY * -2f,
            new(0, 0, 1, 1),
            borderColor, 0f, Zero,
            new Vector2(16f * scale.X, 2f), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(texture,
            position + UnitY * 16f * scale.Y,
            new(0, 0, 1, 1),
            borderColor, 0f, Zero,
            new Vector2(16f * scale.X, 2f), SpriteEffects.None, 0f);
    }

    public static void DrawBorder(Vector2 position, float width, float height, Color backgroundColor, Color borderColor)
    {
        Texture2D texture = TextureAssets.MagicPixel.Value;
        Vector2 scale = new(width, height);
        Main.spriteBatch.Draw(
            texture,
            position,
            new(0, 0, 1, 1),
            backgroundColor,
            0f,
            Zero,
            scale,
            SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(
            texture,
            position + UnitX * -2f + UnitY * -2f,
            new(0, 0, 1, 1),
            borderColor, 0f, Zero,
            new Vector2(2f, scale.Y + 4),
            SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(texture,
            position + UnitX * scale.X + UnitY * -2f,
            new(0, 0, 1, 1),
            borderColor, 0f, Zero,
            new Vector2(2f, scale.Y + 4), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(texture,
            position + UnitY * -2f,
            new(0, 0, 1, 1),
            borderColor, 0f, Zero,
            new Vector2(scale.X, 2f), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(texture,
            position + UnitY * scale.Y,
            new(0, 0, 1, 1),
            borderColor, 0f, Zero,
            new Vector2(scale.X, 2f), SpriteEffects.None, 0f);
    }

    /// <summary>
    /// 获取 HJson 文字
    /// </summary>
    public static string GetText(string str, params object[] arg)
    {
        string text = Language.GetTextValue($"Mods.ImproveGame.{str}", arg);
        return ConvertLeftRight(text);
    }

    public static string GetTextWith(string str, object arg)
    {
        string text = Language.GetTextValueWith($"Mods.ImproveGame.{str}", arg);
        return ConvertLeftRight(text);
    }

    public static string ConvertLeftRight(string text)
    {
        // 支持输入<left>和<right>，就和ItemTooltip一样（原版只有Tooltip支持）
        if (text.Contains("<right>"))
        {
            InputMode inputMode = InputMode.XBoxGamepad;
            if (PlayerInput.UsingGamepad)
                inputMode = InputMode.XBoxGamepadUI;

            if (inputMode == InputMode.XBoxGamepadUI)
            {
                KeyConfiguration keyConfiguration = PlayerInput.CurrentProfile.InputModes[inputMode];
                string input = PlayerInput.BuildCommand("", true, keyConfiguration.KeyStatus["MouseRight"]);
                input = input.Replace(": ", "");
                text = text.Replace("<right>", input);
            }
            else
            {
                text = text.Replace("<right>", Language.GetTextValue("Controls.RightClick"));
            }
        }

        if (text.Contains("<left>"))
        {
            InputMode inputMode2 = InputMode.XBoxGamepad;
            if (PlayerInput.UsingGamepad)
                inputMode2 = InputMode.XBoxGamepadUI;

            if (inputMode2 == InputMode.XBoxGamepadUI)
            {
                KeyConfiguration keyConfiguration2 = PlayerInput.CurrentProfile.InputModes[inputMode2];
                string input = PlayerInput.BuildCommand("", true, keyConfiguration2.KeyStatus["MouseLeft"]);
                input = input.Replace(": ", "");
                text = text.Replace("<left>", input);
            }
            else
            {
                text = text.Replace("<left>", Language.GetTextValue("Controls.LeftClick"));
            }
        }

        return text;
    }

    public static Asset<Texture2D> GetTexture(string fileName)
    {
        return ModContent.Request<Texture2D>($"ImproveGame/Assets/Images/{fileName}", AssetRequestMode.ImmediateLoad);
    }

    public static bool TryConsumeMana(Player player, int cost)
    {
        player.statMana -= cost;
        if (player.statMana < cost)
        {
            player.QuickMana();
            if (player.statMana < cost)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 将存储float液体量转换为原版int整数液体量
    /// <br>0.25%(0.005f) -> 255</br>
    /// </summary>
    /// <param name="amount">float液体量</param>
    /// <returns>整数液体量</returns>
    public static int LiquidAmountToInt(float amount) => (int)Math.Round(amount / 0.0025f * 255);

    /// <summary>
    /// 将原版int整数液体量转换为存储float液体量
    /// <br>255 -> 0.025%(0.0025f)</br>
    /// </summary>
    /// <param name="amount">整数液体量</param>
    /// <returns>float液体量</returns>
    public static float LiquidAmountToFloat(int amount) => amount / 255f * 0.0025f;

    public static int ItemToBanner(Item item)
    {
        if (!ItemID.Sets.BannerStrength.IndexInRange(item.type) || !ItemID.Sets.BannerStrength[item.type].Enabled)
            return -1;

        if (item.createTile == TileID.Banners)
        {
            int style = item.placeStyle;
            int frameX = style * 18;
            int frameY = 0;
            if (style >= 90)
            {
                frameX -= 1620;
                frameY += 54;
            }

            if (frameX >= 396 || frameY >= 54)
            {
                int styleX = frameX / 18 - 21;
                for (int num4 = frameY; num4 >= 54; num4 -= 54)
                {
                    styleX += 90;
                }

                return styleX;
            }
        }

        // AssemblyPublicizer使其可以直接访问
        var bannerToItem = NPCLoader.bannerToItem;
        // 应用
        foreach (var dict in bannerToItem)
        {
            if (dict.Value == item.type && Main.SceneMetrics.NPCBannerBuff.IndexInRange(dict.Key))
            {
                return dict.Key;
            }
        }

        return -1;
    }

    /// <summary>
    /// 绘制一个方框
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="dimensions"></param>
    /// <param name="texture"></param>
    /// <param name="color"></param>
    public static void DrawPanel(SpriteBatch sb, CalculatedStyle dimensions, Texture2D texture, Color color)
    {
        Point point = new Point((int)dimensions.X, (int)dimensions.Y);
        Point point2 = new Point(point.X + (int)dimensions.Width - 12, point.Y + (int)dimensions.Height - 12);
        int width = point2.X - point.X - 12;
        int height = point2.Y - point.Y - 12;
        sb.Draw(texture, new Rectangle(point.X, point.Y, 12, 12), new Rectangle(0, 0, 12, 12), color);
        sb.Draw(texture, new Rectangle(point2.X, point.Y, 12, 12), new Rectangle(12 + 4, 0, 12, 12), color);
        sb.Draw(texture, new Rectangle(point.X, point2.Y, 12, 12), new Rectangle(0, 12 + 4, 12, 12), color);
        sb.Draw(texture, new Rectangle(point2.X, point2.Y, 12, 12), new Rectangle(12 + 4, 12 + 4, 12, 12), color);
        sb.Draw(texture, new Rectangle(point.X + 12, point.Y, width, 12), new Rectangle(12, 0, 4, 12), color);
        sb.Draw(texture, new Rectangle(point.X + 12, point2.Y, width, 12), new Rectangle(12, 12 + 4, 4, 12), color);
        sb.Draw(texture, new Rectangle(point.X, point.Y + 12, 12, height), new Rectangle(0, 12, 12, 4), color);
        sb.Draw(texture, new Rectangle(point2.X, point.Y + 12, 12, height), new Rectangle(12 + 4, 12, 12, 4), color);
        sb.Draw(texture, new Rectangle(point.X + 12, point.Y + 12, width, height), new Rectangle(12, 12, 4, 4), color);
    }

    /// <summary>
    /// 可否使用当前物块魔杖，并返回物块魔杖目标在背包的索引
    /// </summary>
    /// <param name="item">物品</param>
    /// <param name="index">对应物品在背包的索引</param>
    /// <returns>是否可以使用</returns>
    public static bool CheckWandUsability(Item item, Player player, out int index)
    {
        bool canUse = true;
        index = -1;

        if (item.tileWand > 0)
        {
            canUse = false;
            for (int i = 0; i < 58; i++)
            {
                if (item.tileWand == player.inventory[i].type && player.inventory[i].stack > 0)
                {
                    canUse = true;
                    index = i;
                    break;
                }
            }
        }

        return canUse;
    }

    /// <summary>
    /// 堆叠物品到仓库
    /// </summary>
    /// <param name="inventory"></param>
    /// <param name="item"></param>
    /// <returns>堆叠后剩余的物品</returns>
    public static Item ItemStackToInventory(Item[] inventory, Item item, bool hint = true, int end = -1, int begin = 0)
    {
        if (end < begin)
        {
            end = inventory.Length;
        }

        // 先填充和物品相同的
        for (int i = begin; i < end; i++)
        {
            item = ItemStackToInventoryItem(inventory, i, item, hint);
            if (item.IsAir)
            {
                return item;
            }
        }

        // 后填充空位
        for (int i = begin; i < end; i++)
        {
            if (inventory[i] is null || inventory[i].IsAir)
            {
                if (hint)
                {
                    PopupText.NewText(PopupTextContext.ItemPickupToVoidContainer, item, item.stack);
                    SoundEngine.PlaySound(SoundID.Grab);
                }

                inventory[i] = item;
                return new Item();
            }
        }

        return item;
    }

    /// <summary>
    /// 获取所有物品栏的物品
    /// </summary>
    /// <param name="player">玩家实例</param>
    /// <returns>包含全部物品数组的Dictionary，Key为物品所属的物品空间</returns>
    public static Dictionary<string, Item[]> GetAllInventoryItems(Player player)
    {
        var items = new Dictionary<string, Item[]>()
        {
            {"inv", player.inventory},
            {"piggy", player.bank.item},
            {"safe", player.bank2.item},
            {"forge", player.bank3.item},
            {"void", player.bank4.item}
        };
        if (Config.SuperVault && player.TryGetModPlayer<DataPlayer>(out var modPlayer))
        {
            items["mod"] = modPlayer.SuperVault;
        }

        return items;
    }

    /// <summary>
    /// 获取所有物品栏的物品
    /// </summary>
    /// <param name="player">玩家实例</param>
    /// <param name="ignores">
    /// 不获取哪些物品空间的物品，可同时包含多个<br/>
    /// 可用字串有 inv, piggy, safe, forge, void, mod, portable(即piggy+safe+forge+void)</param>
    /// <returns>包含全部物品的List</returns>
    public static List<Item> GetAllInventoryItemsList(Player player, string ignores = "")
    {
        ignores = ignores.Replace("portable", "piggy safe forge void", StringComparison.Ordinal);
        var itemList = new List<Item>();
        var items = GetAllInventoryItems(player);
        foreach ((string name, Item[] itemArray) in items)
        {
            if (ignores.Contains(name, StringComparison.Ordinal))
                continue;
            foreach (var item in itemArray)
            {
                itemList.Add(item);
            }
        }

        return itemList;
    }

    /// <summary>
    /// 堆叠物品到仓库某位置
    /// </summary>
    /// <param name="inventory">仓库</param>
    /// <param name="slot">槽位</param>
    /// <param name="item">来自外来物品</param>
    /// <returns>堆叠后剩余的物品, 如果没有剩余物品就会 new Item</returns>
    public static Item ItemStackToInventoryItem(Item[] inventory, int slot, Item item, bool hint)
    {
        if (inventory[slot].IsAir || inventory[slot].type != item.type)
            return item;

        ItemLoader.TryStackItems(inventory[slot], item, out int numTransferred);
        if (!hint)
            return item;

        PopupText.NewText(PopupTextContext.ItemPickupToVoidContainer, item, numTransferred, noStack: false);
        SoundEngine.PlaySound(SoundID.Grab);

        return item;
    }

    public static bool IsBankItem(int type) => Lookups.Bank2Items.Contains(type) || Lookups.Bank3Items.Contains(type) ||
                                               Lookups.Bank4Items.Contains(type) || Lookups.Bank5Items.Contains(type);

    /// <summary>
    /// 判断指定 Item[] 中是否有 item
    /// </summary>
    public static bool HasItem(Item[] inv, int indexMax = -1, params int[] itemTypes)
    {
        for (int i = 0; i < (indexMax > 0 ? indexMax : inv.Length); i++)
        {
            if (IntArrayContains(itemTypes, inv[i].type) && inv[i].stack > 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 判断指定 IEnumerable.Item 中是否有 item
    /// </summary>
    /// <param name="items">The list of items</param>
    /// <param name="types">The types to check for</param>
    /// <returns>True if yes, false if no</returns>
    public static bool HasItem(IEnumerable<Item> items, params int[] types)
    {
        return items.Any(item => types.Contains(item.type));
    }

    // 获取配置
    public static ImproveConfigs Config;

    /// <summary>
    /// 获取瓷砖数量
    /// </summary>
    /// <returns>是否存在无限瓷砖</returns>
    public static bool ItemCount(Item[] inv, Func<Item, bool> func, out int count)
    {
        bool infinite = false;
        count = 0;
        foreach (var t in inv)
        {
            if (t.IsAir || !func(t))
            {
                continue;
            }

            count += t.stack;
            if (!t.consumable)
            {
                infinite = true;
            }
        }

        return infinite;
    }

    // 获取背包第一个平台
    public static Item GetFirstPlatform(Player player)
    {
        for (int i = 0; i < 50; i++)
        {
            Item item = player.inventory[i];
            if (item.stack > 0 && item.createTile > -1 && TileID.Sets.Platforms[item.createTile])
            {
                return item;
            }
        }

        return new Item();
    }

    // 获取背包第一个平台
    public static Item FirstWall(Player player)
    {
        for (int i = 0; i < 50; i++)
        {
            Item item = player.inventory[i];
            if (item.stack > 0 && item.createWall > 0)
            {
                return item;
            }
        }

        return null;
    }

    /// <summary>
    /// 判断有没有足量的此类物品
    /// </summary>
    public static int EnoughItem(Player player, Func<Item, bool> condition, int amount = 1)
    {
        int oneIndex = -1;
        int num = 0;
        for (int i = 0; i < 50; i++)
        {
            Item item = player.inventory[i];
            if (item.type != ItemID.None && item.stack > 0 && condition(item))
            {
                if (oneIndex == -1)
                    oneIndex = i;
                if (!item.consumable)
                    return oneIndex;
                num += item.stack;
            }
        }

        if (num >= amount)
        {
            return oneIndex;
        }

        return -1;
    }

    /// <summary>
    /// 从物品栏里找到对应物品，返回值为在 <see cref="Player.inventory"/> 中的索引
    /// </summary>
    /// <param name="player">对应玩家</param>
    /// <param name="shouldPick">选取物品的依据</param>
    /// <param name="tryConsume">是否尝试消耗</param>
    /// <returns></returns>
    public static Item PickItemInInventory(Player player, Func<Item, bool> shouldPick, bool tryConsume, out int index)
    {
        for (int i = 54; i < 58; i++)
        {
            ref Item item = ref player.inventory[i];
            if (!item.IsAir && shouldPick.Invoke(item))
            {
                var returnItem = item.Clone();
                if (tryConsume)
                {
                    TryConsumeItem(ref item, player);
                }

                index = i;
                return returnItem; // 要是consume完了就没了，所以clone一下
            }
        }

        for (int i = 0; i < 50; i++)
        {
            ref Item item = ref player.inventory[i];
            if (!item.IsAir && shouldPick.Invoke(item))
            {
                var returnItem = item.Clone();
                if (tryConsume)
                {
                    TryConsumeItem(ref item, player);
                }

                index = i;
                return returnItem; // 要是consume完了就没了，所以clone一下
            }
        }

        index = -1;
        return new();
    }

    /// <summary>
    /// 从物品数组里找到对应物品，返回值为在数组中的索引
    /// </summary>
    /// <returns>物品实例</returns>
    public static Item PickItemFromArray(Player player, Item[] itemArray, Func<Item, bool> shouldPick, bool tryConsume)
    {
        for (int i = 0; i < itemArray.Length; i++)
        {
            ref Item item = ref itemArray[i];
            if (!item.IsAir && shouldPick.Invoke(item))
            {
                if (tryConsume)
                {
                    TryConsumeItem(ref item, player);
                }

                return item;
            }
        }

        return new();
    }

    /// <summary>
    /// 尝试消耗某个物品
    /// </summary>
    /// <param name="item">物品实例</param>
    /// <param name="player">玩家实例</param>
    /// <param name="ignoreConsumable">是否无视<see cref="Item.consumable"/>判定，即无论如何都消耗</param>
    /// <returns>是否成功消耗</returns>
    public static bool TryConsumeItem(ref Item item, Player player, bool ignoreConsumable = false)
    {
        if (!item.IsAir && (item.consumable || ignoreConsumable) && ItemLoader.ConsumeItem(item, player))
        {
            item.stack--;
            if (item.stack < 1)
            {
                item.TurnToAir();
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 强制性的可否放置判断，只有满足条件才能放置物品，没有例外
    /// </summary>
    /// <param name="slotItem">槽内物品</param>
    /// <param name="mouseItem">手持物品</param>
    /// <returns>
    /// 强制判断返回值，判断放物类型
    /// 0: 不可放物<br/>
    /// 1: 两物品不同，应该切换<br/>
    /// 2: 两物品相同，应该堆叠<br/>
    /// 3: 槽内物品为空，应该切换<br/>
    /// </returns>
    public static byte CanPlaceInSlot(Item slotItem, Item mouseItem)
    {
        if (slotItem.IsAir)
            return 3;
        if (mouseItem.type != slotItem.type || mouseItem.prefix != slotItem.prefix)
            return 1;
        if (!slotItem.IsAir && slotItem.stack < slotItem.maxStack && ItemLoader.CanStack(slotItem, mouseItem))
            return 2;
        return 0;
    }

    /// <summary>
    /// 普遍性的可否放置判断
    /// </summary>
    /// <param name="slotItem">槽内物品</param>
    /// <param name="mouseItem">手持物品</param>
    /// <returns>一般判断返回值</returns>
    public static bool SlotPlace(Item slotItem, Item mouseItem) => slotItem.type == mouseItem.type || mouseItem.IsAir;

    /// <summary>
    /// 将未缩放的屏幕坐标转换为缩放后的屏幕坐标
    /// </summary>
    /// <param name="originalPosition">原坐标</param>
    /// <returns>缩放后坐标</returns>
    public static Vector2 GetZoomedPosition(Vector2 originalPosition)
    {
        float oppositeX = (originalPosition.X - Main.screenWidth / 2) / Main.GameZoomTarget;
        float oppositeY = (originalPosition.Y - Main.screenHeight / 2) / Main.GameZoomTarget;
        originalPosition.X -= oppositeX * (Main.GameZoomTarget - 1f);
        originalPosition.Y -= oppositeY * (Main.GameZoomTarget - 1f);
        return originalPosition;
    }

    public static ref bool GetSeedFeatureFlag(byte seedType) => ref GetSeedFeatureFlag((SeedType)seedType);

    public static ref bool GetSeedFeatureFlag(SeedType seedType)
    {
        switch (seedType)
        {
            case SeedType.Drunk:
                return ref Main.drunkWorld;
            case SeedType.Bees:
                return ref Main.notTheBeesWorld;
            case SeedType.Ftw:
                return ref Main.getGoodWorld;
            case SeedType.Anniversary:
                return ref Main.tenthAnniversaryWorld;
            case SeedType.DontStarve:
                return ref Main.dontStarveWorld;
            case SeedType.Traps:
                return ref Main.noTrapsWorld;
            case SeedType.Remix:
                return ref Main.remixWorld;
            case SeedType.Zenith:
                return ref Main.zenithWorld;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}