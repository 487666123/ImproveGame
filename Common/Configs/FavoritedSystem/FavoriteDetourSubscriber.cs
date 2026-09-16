using ImproveGame.Common.Configs.Elements;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection;
using Terraria.ModLoader.Config.UI;

namespace ImproveGame.Common.Configs.FavoritedSystem;

public class FavoriteDetourSubscriber : ILoadable
{
    public void Load(Mod mod)
    {
        // 提供右键收藏功能
        var wrapItMethodInfo = typeof(UIModConfig).GetMethod("WrapIt", BindingFlags.Public | BindingFlags.Static);
        if (wrapItMethodInfo is not null)
            MonoModHooks.Add(wrapItMethodInfo, WrapItDetour);

        // 被收藏的选项会有提示
        var drawSelfMethodInfo = typeof(ConfigElement).GetMethod("DrawSelf",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (drawSelfMethodInfo is not null)
            MonoModHooks.Modify(drawSelfMethodInfo, DrawSelfILEditing);

        // Hook到OnActivate，给特殊的Config - FavoritedConfigs添加元素用
        var onActivateMethodInfo = typeof(UIModConfig).GetMethod("OnActivate",
            BindingFlags.Public | BindingFlags.Instance);
        if (onActivateMethodInfo is not null)
            MonoModHooks.Add(onActivateMethodInfo, OnActivateDetour);

        // Hook到SaveConfig，给特殊的Config - FavoritedConfigs保存用
        var saveConfigMethodInfo = typeof(UIModConfig).GetMethod("SaveConfig",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (saveConfigMethodInfo is not null)
            MonoModHooks.Add(saveConfigMethodInfo, SaveConfigDetour);
    }

    private delegate Tuple<UIElement, UIElement> WrapItOrig(UIElement parent, ref int top,
        PropertyFieldWrapper memberInfo, object item, int order, object list = null, Type arrayType = null,
        int index = -1);

    private static Tuple<UIElement, UIElement> WrapItDetour(WrapItOrig orig, UIElement parent, ref int top,
        PropertyFieldWrapper memberInfo, object item, int order, object list, Type arrayType, int index)
    {
        var result = orig(parent, ref top, memberInfo, item, order, list, arrayType, index);
        if (item is not ImproveConfigs improveConfigs)
            return result;

        var container = result.Item1;
        var element = result.Item2;
        container.OnRightClick += (_, _) =>
        {
            if (element is LargerPanelElement or OtherFunctionsElement)
                return;

            string key = memberInfo.Name;
            FavoritedOptionDatabase.ToggleFavoriteForOption(improveConfigs, key);
        };
        return result;
    }

    private static void DrawSelfILEditing(ILContext il)
    {
        var c = new ILCursor(il);

        // IL_00ed: ldarg.0      // this
        // IL_00ee: call         instance class [System.Runtime]System.Func`1<string> Terraria.ModLoader.Config.UI.ConfigElement::get_TextDisplayFunction()
        // IL_00f3: callvirt     instance !0/*string*/ class [System.Runtime]System.Func`1<string>::Invoke()
        // (插入到这里)
        // IL_00f8: stloc.s      label
        if (!c.TryGotoNext(
                MoveType.After,
                i => i.Match(OpCodes.Ldarg_0),
                i => i.MatchCallOrCallvirt(typeof(ConfigElement), "get_TextDisplayFunction"),
                i => i.MatchCallOrCallvirt(typeof(Func<string>), "Invoke")
            )) 
        {
            ILMatchLog(nameof(DrawSelfILEditing), il);
            return;
        }

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<string, ConfigElement, string>>((text, self) =>
        {
            if (self.Item is not ImproveConfigs { IsRealImproveConfigs: true } ||
                !FavoritedOptionDatabase.FavoritedOptions.Contains(self.MemberInfo.Name))
                return text;

            return $"[centeritem:FallenStar] {text}";
            // return $"{text} - [c/f6ee8d:Favorited]";
        });
    }

    private static void OnActivateDetour(Action<UIModConfig> orig, UIModConfig self)
    {
        orig.Invoke(self);

        if (self.pendingConfig is FavoritedConfigs favoritedConfigs)
            favoritedConfigs.PopulateElements(self);
    }

    private static void SaveConfigDetour(Action<UIModConfig, UIMouseEvent, UIElement> orig, UIModConfig self,
        UIMouseEvent evt, UIElement listeningElement)
    {
        if (self.pendingConfig is FavoritedConfigs favoritedConfigs)
        {
            favoritedConfigs.SaveConfig(orig, self, evt, listeningElement);
            return;
        }

        orig.Invoke(self, evt, listeningElement);
    }

    public void Unload()
    {
    }
}