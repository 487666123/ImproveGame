using ImproveGame.Common.Configs;
using ImproveGame.Common.ModSystems;
using ImproveGame.Modules.InfiniteBuff.UserInterface;
using MonoMod.Cil;
using SilkyUIFramework;
using Terraria.DataStructures;
using Terraria.UI.Gamepad;

namespace ImproveGame.Modules.InfiniteBuff;

class HideGlobalBuff : GlobalBuff
{
    //static readonly bool IsDrawingBuffTracker;
    //static bool UseRegularMethod_NoInventory = false;
    //static bool UseRegularMethod_Inventory = false;

    public override bool PreDraw(SpriteBatch spriteBatch, int type, int buffIndex, ref BuffDrawParams drawParams)
    {
        var player = Main.LocalPlayer;
        if (player.TryGetModPlayer<InfiniteBuffModPlayer>(out var infinitePlayer) && infinitePlayer.ActivationFlags[type])
        {
            if (UIConfigs.Instance.HideNoConsumeBuffs)
            {
                // 关掉鼠标悬浮信息
                drawParams.MouseRectangle = Rectangle.Empty;
                return false;
            }
        }

        return true;
    }

    public override void ModifyBuffText(int type, ref string buffName, ref string tip, ref int rare)
    {
        if (TryGetKeybindString(KeybindSystem.BuffTrackerKeybind, out _)) return;

        if (!UISceneManager.Instance.TryGetInstance<InfiniteBuffController>(out var controller)) return;

        tip += $"\n{GetText($"Tips.BuffTracker{(controller.Enabled ? "Off" : "On")}")}";

        if (!UIConfigs.Instance.HideNoConsumeBuffs) tip += $"\n{GetText("Tips.HideMyBuffs")}";
    }

    // 先用 IL，如果 IL 出错了才在本次加载中启用备用方案
    public override void Load()
    {
        //IL_Main.DrawInventory += TweakDrawInventoryBuffs;
        //IL_Main.DrawInterface_Resources_Buffs += TweakDrawInterfaceBuffs;

        // 无限 Buff 始终不显示时长
        On_Main.TryGetBuffTime += CompatibleWithInfiniteBuff;

        // 这两个新的实现是直接把计算交给了原方法，只影响输入。

        // 影响 Buff 绘制位置
        IL_Main.DrawInventory += IL_Main_DrawInventory;

        // 物品栏下方 Buff 绘制的位置
        IL_Main.DrawInterface_Resources_Buffs += IL_Main_DrawInterface_Resources_Buffs;

        // 右键点击 Buff 图标打开控制面板
        IL_Main.DrawInventory += ClickBuff;
        IL_Main.DrawInterface_Resources_Buffs += ClickBuff;
    }

    private static void ClickBuff(ILContext il)
    {
        var c = new ILCursor(il);
        if (!c.TryGotoNext(MoveType.After,
            i => i.MatchCall(typeof(BuffLoader), "ModifyBuffText")
            )) 
        {
            ILMatchLog(nameof(ClickBuff), il);
            return;
        }

        c.EmitDelegate(() =>
        {
            if (!Main.mouseLeft || !Main.mouseLeftRelease) return;
            if (!UISceneManager.Instance.TryGetInstance<InfiniteBuffController>(out var controller)) return;

            controller.Enabled = !controller.Enabled;
        });
    }

    private static int _count;
    private static void IL_Main_DrawInterface_Resources_Buffs(ILContext il)
    {
        var c = new ILCursor(il);

        c.EmitDelegate<Action>(() => _count = 0);

        // loc.3 是 i

        // int x = 32 + i * 38;
        if (!c.TryGotoNext(MoveType.After,
            i => i.MatchLdcI4(32),
            i => i.MatchLdloc(2)
            ))
        {
            ILMatchLog(nameof(IL_Main_DrawInterface_Resources_Buffs), il);
            return;
        }

        //for (int i = 0; i < Player.maxBuffs; i++)
        //{
        //    if (player[myPlayer].buffType[i] > 0)
        //    {
        //        _ = player[myPlayer].buffType[i];
        //        int x = 32 + i * 38; << 这里的 i
        //        int num3 = 76;
        //        int num4 = i; << 这里的 i，同时根据是否是无限 Buff 决定是否 count++
        //        while (num4 >= num2)
        //        {
        //            num4 -= num2;
        //            x = 32 + num4 * 38;
        //            num3 += 50;
        //        }
        //        num = DrawBuffIcon(num, i, x, num3);
        //    }
        //    else
        //    {
        //        buffAlpha[i] = 0.4f;
        //    }
        //}

        c.EmitDelegate<Func<int, int>>(static (i) => _count);

        // int num4 = i;
        if (!c.TryGotoNext(MoveType.After,
            i => i.MatchLdloc(2)
            ))
        {
            ILMatchLog(nameof(IL_Main_DrawInterface_Resources_Buffs), il);
            return;
        }

        c.EmitDelegate<Func<int, int>>(static (i) =>
        {
            var player = Main.LocalPlayer;
            if (UIConfigs.Instance.HideNoConsumeBuffs &&
                player.TryGetModPlayer<InfiniteBuffModPlayer>(out var infinitePlayer) && infinitePlayer.ActivationFlags[player.buffType[i]])
            {
                return _count;
            }

            return _count++;
        });
    }

    private static void IL_Main_DrawInventory(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.After,
            i => i.MatchCall<Main>("DrawBuffIcon")
            ))
        {
            ILMatchLog(nameof(IL_Main_DrawInventory), il);
            return;
        }

        // 这个循环结束，进入下个循环前，会 +1，现在让无限 Buff 不 +1
        if (!c.TryGotoNext(MoveType.After,
            i => i.MatchCall<UILinkPointNavigator>("SetPosition"),
            i => i.MatchLdloc(49),
            i => i.MatchLdcI4(1),
            i => i.MatchAdd(),
            i => i.MatchStloc(49)
            ))
        {
            ILMatchLog(nameof(IL_Main_DrawInventory), il);
            return;
        }

        //for (int n = 0; n < Player.maxBuffs; n++)
        //{
        //    if (player[myPlayer].buffType[n] != 0)
        //    {
        //        int num33 = num29 / num30;
        //        int num34 = num29 % num30;
        //        Point point = new Point(num24 + num33 * -num32, num25 + num34 * num32);
        //        num28 = DrawBuffIcon(num28, n, point.X, point.Y);
        //        UILinkPointNavigator.SetPosition(9000 + num29, new Vector2(point.X + 30, point.Y + 30));
        //        num29++; << 这里
        //        if (buffAlpha[n] < 0.65f)
        //        {
        //            buffAlpha[n] = 0.65f;
        //        }
        //    }
        //}

        c.EmitLdloc(62);
        c.EmitLdloc(49);
        c.EmitDelegate<Func<int, int, int>>(static (index, count) =>
        {
            var player = Main.LocalPlayer;
            var type = player.buffType[index];

            if (player.TryGetModPlayer<InfiniteBuffModPlayer>(out var infinitePlayer) && infinitePlayer.ActivationFlags[type])
            {
                return count - 1;
            }

            return count;
        });
        c.EmitStloc(49);
    }

    private static bool CompatibleWithInfiniteBuff(On_Main.orig_TryGetBuffTime orig, int buffSlotOnPlayer, out int buffTimeValue)
    {
        var player = Main.LocalPlayer;
        if (player.TryGetModPlayer<InfiniteBuffModPlayer>(out var infinitePlayer) &&
            infinitePlayer.ActivationFlags[player.buffType[buffSlotOnPlayer]])
        {
            buffTimeValue = 0;
            // 返回 false 就不会绘制了
            return false;
        }

        return orig.Invoke(buffSlotOnPlayer, out buffTimeValue);
    }

    // 源码片段
    /*for (int n = 0; n < Player.MaxBuffs; n++) {
	        if (player[myPlayer].buffType[n] != 0) {
		        int num32 = num28 / num29;
		        int num33 = num28 % num29;
		        Point point = new Point(num23 + num32 * -num31, num24 + num33 * num31);
		        num27 = DrawBuffIcon(num27, n, point.X, point.Y);
		        UILinkPointNavigator.SetPosition(9000 + num28, new Vector2(point.X + 30, point.Y + 30));
		        num28++;
		        if (buffAlpha[n] < 0.65f)
			        buffAlpha[n] = 0.65f;
	        }
    }*/

    // 这里num28是拿来定位的，和索引n分开了，虽然不知道为啥，很显然更容易改了
    //private void TweakDrawInventoryBuffs(ILContext il)
    //{
    //    try
    //    {
    //        ILCursor c = new(il);

    //        #region 获取索引
    //        //IL_0B55: ldsfld    class Terraria.Player[] Terraria.Main::player
    //        //IL_0B5A: ldsfld    int32 Terraria.Main::myPlayer
    //        //IL_0B5F: ldelem.ref
    //        //IL_0B60: ldfld     int32[] Terraria.Player::buffType
    //        //IL_0B65: ldloc.s   n
    //        // 先获取到索引
    //        //int index = -1;
    //        if (!c.TryGotoNext(MoveType.After,
    //                           i => i.MatchLdsfld(typeof(Main), nameof(Main.player)),
    //                           i => i.MatchLdsfld(typeof(Main), nameof(Main.myPlayer)),
    //                           i => i.MatchLdelemRef(),
    //                           i => i.MatchLdfld(typeof(Player), nameof(Player.buffType)),
    //                           i => i.Match(OpCodes.Ldloc_S)))
    //        {
    //            ErrorHappenedInventory();
    //            return;
    //        }

    //        // 开一个EmitDelegate来获取索引
    //        c.EmitDelegate<Func<int, int>>(returnValue =>
    //        {
    //            _index = returnValue;
    //            return returnValue;
    //        });
    //        #endregion

    //        #region 修改绘制坐标
    //        if (!c.TryGotoNext(MoveType.Before,
    //                           i => i.Match(OpCodes.Ldloc_S),
    //                           i => i.Match(OpCodes.Ldloc_S),
    //                           i => i.Match(OpCodes.Div),
    //                           i => i.Match(OpCodes.Stloc_S)))
    //        {
    //            ErrorHappenedInventory();
    //            return;
    //        }
    //        c.Index++;
    //        c.EmitDelegate<Func<int, int>>(x =>
    //        {
    //            var player = Main.LocalPlayer;
    //            if (!player.TryGetModPlayer<InfiniteBuffModPlayer>(out var infinitePlayer)) return x;
    //            var type = player.buffType[_index];

    //            if (!UseRegularMethod_Inventory && UIConfigs.Instance.HideNoConsumeBuffs && infinitePlayer.ActivationFlags[type])
    //            {
    //                // x设置成-100000
    //                return -100000;
    //            }
    //            return x;
    //        });

    //        if (!c.TryGotoNext(MoveType.Before,
    //                           i => i.Match(OpCodes.Ldloc_S),
    //                           i => i.Match(OpCodes.Ldloc_S),
    //                           i => i.Match(OpCodes.Rem),
    //                           i => i.Match(OpCodes.Stloc_S)))
    //        {
    //            ErrorHappenedInventory();
    //            return;
    //        }
    //        c.Index++;
    //        c.EmitDelegate<Func<int, int>>(y =>
    //        {
    //            var player = Main.LocalPlayer;
    //            if (!player.TryGetModPlayer<InfiniteBuffModPlayer>(out var infinitePlayer)) return y;

    //            if (!UseRegularMethod_Inventory && UIConfigs.Instance.HideNoConsumeBuffs && infinitePlayer.ActivationFlags[Main.LocalPlayer.buffType[_index]])
    //            {
    //                // y设置成-100000
    //                return -100000;
    //            }
    //            return y;
    //        });
    //        #endregion

    //        #region 修改位置索引添加

    //        //add
    //        //conv.r4
    //        //newobj instance void [FNA]Microsoft.Xna.Framework.Vector2::.ctor(float32, float32)
    //        //call      void Terraria.UI.Gamepad.UILinkPointNavigator::SetPosition(int32, valuetype[FNA]Microsoft.Xna.Framework.Vector2)
    //        //ldloc.s   num28
    //        //ldc.i4.1
    //        // 修改

    //        if (!c.TryGotoNext(MoveType.After,
    //                               i => i.Match(OpCodes.Add),
    //                               i => i.Match(OpCodes.Conv_R4),
    //                               i => i.Match(OpCodes.Newobj),
    //                               i => i.MatchCall<UILinkPointNavigator>(nameof(UILinkPointNavigator.SetPosition)),
    //                               i => i.Match(OpCodes.Ldloc_S),
    //                               i => i.Match(OpCodes.Ldc_I4_1)))
    //        {
    //            ErrorHappenedInventory();
    //            return;
    //        }

    //        c.EmitDelegate<Func<int, int>>(add =>
    //        {
    //            var player = Main.LocalPlayer;
    //            if (!player.TryGetModPlayer<InfiniteBuffModPlayer>(out var infinitePlayer)) return add;

    //            if (!UseRegularMethod_Inventory && UIConfigs.Instance.HideNoConsumeBuffs && infinitePlayer.ActivationFlags[Main.LocalPlayer.buffType[_index]])
    //            {
    //                // 不让他 +1，让他 +0
    //                return 0;
    //            }
    //            return add;
    //        });
    //        #endregion

    //    }
    //    catch (Exception e)
    //    {
    //        ImproveGame.Instance.Logger.Error(e.Message);
    //        ErrorHappenedInventory();
    //    }
    //}

    //private static void ErrorHappenedInventory()
    //{
    //    Console.WriteLine("Main.DrawInventory IL editing error! Alternative solutions enabled.");
    //    UseRegularMethod_Inventory = true;
    //}


    // 源码片段
    /*
     * for (int i = 0; i < Player.maxBuffs; i++) {
     * 	if (player[myPlayer].buffType[i] > 0) {
     * 		_ = player[myPlayer].buffType[i];
     * 		int x = 32 + i * 38;
     * 		int num3 = 76;
     * 		int num4 = i;
     * 		while (num4 >= num2) {
     * 			num4 -= num2;
     * 			x = 32 + num4 * 38;
     * 			num3 += 50;
     * 		}
     * 		num = DrawBuffIcon(num, i, x, num3);
     * 	}
     * 	else {
     * 		buffAlpha[i] = 0.4f;
     *  }
     * }
     */
    // 在原版代码中，"i"以"ldloc.3"读取，处于一个for循环中，既作为buffType的索引，也用于定位
    // 此处应只修改作为定位的部分，作为索引的部分不修改，不然就乱套了
    //private static void TweakDrawInterfaceBuffs(ILContext il)
    //{
    //    try
    //    {
    //        ILCursor c = new(il);

    //        // (sbyte)必须强转，不然游戏会以为你传了int，然后崩了

    //        // 修改第一个
    //        if (!c.TryGotoNext(MoveType.After,
    //                           i => i.Match(OpCodes.Pop),
    //                           i => i.Match(OpCodes.Ldc_I4_S, (sbyte)32),
    //                           i => i.Match(OpCodes.Ldloc_3)))
    //        {
    //            ErrorHappenedInterface();
    //            return;
    //        }
    //        c.EmitDelegate<Func<int, int>>(i => ModifyDrawingIndex(i, Main.LocalPlayer.buffType[i], true));
    //        // 修改第二个
    //        if (!c.TryGotoNext(MoveType.After,
    //                           i => i.Match(OpCodes.Ldc_I4_S, (sbyte)76),
    //                           i => i.Match(OpCodes.Stloc_S),
    //                           i => i.Match(OpCodes.Ldloc_3)))
    //        {
    //            ErrorHappenedInterface();
    //            return;
    //        }
    //        c.EmitDelegate<Func<int, int>>(i => ModifyDrawingIndex(i, Main.LocalPlayer.buffType[i]));
    //    }
    //    catch (Exception e)
    //    {
    //        ImproveGame.Instance.Logger.Error(e.Message);
    //        ErrorHappenedInterface();
    //    }
    //}
    //static int ModifyDrawingIndex(int i, int buffType, bool addCount = false)
    //{
    //    var player = Main.LocalPlayer;
    //    if (!player.TryGetModPlayer<InfiniteBuffModPlayer>(out var infinitePlayer)) return i - HidedBuffCountThisFrame;

    //    if (!UseRegularMethod_NoInventory && UIConfigs.Instance.HideNoConsumeBuffs && infinitePlayer.ActivationFlags[buffType])
    //    {
    //        // 作为-10000传入
    //        if (addCount)
    //            HidedBuffCountThisFrame++;
    //        return -10000;
    //    }
    //    return i - HidedBuffCountThisFrame; // 当前的减去需要隐藏的
    //}
    //static void ErrorHappenedInterface()
    //{
    //    Console.WriteLine("Main.DrawInterface_Resources_Buffs IL editing error! Alternative solutions enabled.");
    //    UseRegularMethod_NoInventory = true;
    //}
    // 本帧被隐藏的Buff数量，便于后面的Buff重设绘制坐标
    //internal static int HidedBuffCountThisFrame { get; set; }

    //public override bool PreDraw(SpriteBatch spriteBatch, int type, int buffIndex, ref BuffDrawParams drawParams)
    //{
    //    var player = Main.LocalPlayer;
    //    if (player.TryGetModPlayer<InfiniteBuffModPlayer>(out var infinitePlayer) && infinitePlayer.ActivationFlags[type])
    //    {
    //        if (UIConfigs.Instance.HideNoConsumeBuffs)
    //        {
    //            // 关掉鼠标悬浮信息
    //            drawParams.MouseRectangle = Rectangle.Empty;
    //            if (UseRegularMethod_NoInventory || Main.playerInventory)
    //            {
    //                HidedBuffCountThisFrame++;
    //            }
    //            return false;
    //        }
    //    }

    //    return true;

    //    if (HidedBuffCountThisFrame <= 0) return true;

    //    int i = buffIndex - HidedBuffCountThisFrame;
    //    if (UseRegularMethod_NoInventory)
    //    {
    //        int x = 32 + i * 38;
    //        int y = 76;
    //        if (i >= 11)
    //        { // 一行
    //            x = 32 + Math.Abs(i % 11) * 38;
    //            y += 50 * (i / 11);
    //        }
    //        // 重设各种参数
    //        drawParams.Position = new Vector2(x, y);
    //        int width = drawParams.Texture.Width;
    //        int height = drawParams.Texture.Height;
    //        drawParams.TextPosition = new Vector2(x, y + height);
    //        drawParams.MouseRectangle = new Rectangle(x, y, width, height);
    //    }

    //    // 装备栏下方绘制
    //    if (Main.playerInventory && UseRegularMethod_Inventory)
    //    {
    //        int mH = 0;
    //        if (Main.mapEnabled && !Main.mapFullscreen && Main.mapStyle == 1)
    //        {
    //            mH = 256;
    //        }
    //        if (mH + Main.instance.RecommendedEquipmentAreaPushUp > Main.screenHeight)
    //            mH = Main.screenHeight - Main.instance.RecommendedEquipmentAreaPushUp;
    //        int num23 = Main.screenWidth - 92;
    //        int num24 = mH + 174;
    //        num24 += 247;
    //        num23 += 8;
    //        int num29 = 3;
    //        int num30 = 260;
    //        if (Main.screenHeight > 630 + num30 * (Main.mapStyle == 1).ToInt())
    //            num29++;

    //        if (Main.screenHeight > 680 + num30 * (Main.mapStyle == 1).ToInt())
    //            num29++;

    //        if (Main.screenHeight > 730 + num30 * (Main.mapStyle == 1).ToInt())
    //            num29++;

    //        int num31 = 46;

    //        int num32 = i / num29;
    //        int num33 = i % num29;
    //        int x = num23 + num32 * -num31;
    //        int y = num24 + num33 * num31;
    //        // 重设各种参数
    //        drawParams.Position = new Vector2(x, y);
    //        int width = drawParams.Texture.Width;
    //        int height = drawParams.Texture.Height;
    //        drawParams.TextPosition = new Vector2(x, y + height);
    //        drawParams.MouseRectangle = new Rectangle(x, y, width, height);
    //    }

    //    return true;
    //}
}
