using System.Reflection;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;

namespace ImproveGame.UI.ModernConfig;

public class ModernConfigDetours : ILoadable
{
    public void Load(Mod mod)
    {
        var populateConfigsMethod = typeof(UIModConfigList).GetMethod(nameof(UIModConfigList.PopulateConfigs),
            BindingFlags.NonPublic | BindingFlags.Instance);
        MonoModHooks.Add(populateConfigsMethod, PopulateConfigsDetour);

        var drawMenuMethod = typeof(MenuLoader).GetMethod(nameof(MenuLoader.UpdateAndDrawModMenu),
            BindingFlags.NonPublic | BindingFlags.Static);
        MonoModHooks.Add(drawMenuMethod, DrawMenuDetour);

        On_Main.CanPauseGame += orig =>
            orig.Invoke() || (Main.netMode is NetmodeID.SinglePlayer && Main.InGameUI.CurrentState is ModernConfigUI);



        On_IngameFancyUI.Close += (orig, quiet) =>
        {
            // 现记录，因为执行完原版之后CurrentState就是null了
            bool isInGameModernConfig = !Main.gameMenu && Main.InGameUI.CurrentState is ModernConfigUI;

            orig.Invoke(quiet);

            if (!isInGameModernConfig)
                return;

            MyUtils.OperateInventory(false);
            ModernConfigUI.Instance.Enabled = false;

            // 如果是从tModLoader配置选择界面打开的，就重新打开主界面
            if (!ModernConfigUI.Instance.OpenFromMasterControl)
            {
                Interface.modConfigList.ModToSelectOnOpen = ModernConfigUI.Instance.currentMod ?? ImproveGame.Instance;
                IngameFancyUI.OpenUIState(Interface.modConfigList);
            }
        };


        //IL_Main.DoDraw += AddRenderOn;

    }
    //改自LogSpiralLibrary，不知道为什么会经常导致崩溃，于是先移除了
    /*private void AddRenderOn(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        for (int n = 0; n < 5; n++)
            if (!cursor.TryGotoNext(i => i.MatchLdstr("Sepia")))
                return;
        cursor.Index += 14;
        cursor.EmitDelegate(() =>
        {
            return !RenderUsing;
        });
        cursor.EmitAnd();
        for (int n = 0; n < 2; n++)
            if (!cursor.TryGotoNext(i => i.MatchCallOrCallvirt(typeof(FilterManager).GetMethod(nameof(FilterManager.EndCapture), BindingFlags.Public | BindingFlags.Instance))))
                return;
        cursor.Index -= 6;
        cursor.EmitDelegate<Func<bool, bool>>(flag =>
        {
            return flag && (!RenderUsing || Main.hideUI);
        });

        for (int n = 0; n < 2; n++)
            if (!cursor.TryGotoNext(i => i.MatchCallOrCallvirt(typeof(Main).GetMethod(nameof(Main.DrawInterface), BindingFlags.NonPublic | BindingFlags.Instance))))
                return;

        cursor.Index += 3;
        cursor.EmitDelegate(() =>
        {
            if (Lighting.NotRetro && RenderUsing)
                Filters.Scene.EndCapture(null, Main.screenTarget, Main.screenTargetSwap, Color.Black);
        });
    }*/
    /*public static List<Func<bool>> RenderOnConditions = [];
    public static bool RenderUsing
    {
        get
        {
            bool result = false && ModernConfigUI.Instance != null && ModernConfigUI.Instance.Enabled;
            foreach (var condition in RenderOnConditions)
                result |= condition.Invoke();
            return result;
        }
    }*/
    private static void PopulateConfigsDetour(Action<UIModConfigList> orig, UIModConfigList self)
    {
        orig.Invoke(self);


        // 确保里面有东西了
        if (self.configList.Count == 0)
            return;
        bool flag = self.selectedMod.Name == "ImproveGame";
        if (!(flag || true)) return;
        string text = flag ? "Mods.ImproveGame.ModernConfig.Name" : "Mods.ImproveGame.ModernConfig.Name_ModConfig";
        LocalizedText localizedText;
        if (!CategorySidePanel.ModdedTitle.TryGetValue(self.selectedMod, out localizedText))
            localizedText = Language.GetText(text);
        var configPanel = new UIButton<LocalizedText>(localizedText)
        {
            MaxWidth = { Percent = 0.95f },
            HAlign = 0.5f,
            ScalePanel = true,
            UseInnerDimensions = true,
            ClickSound = SoundID.MenuOpen,
        };
        configPanel.OnUpdate += delegate
        {
            configPanel.TextColor = Main.DiscoColor;
        };
        configPanel.OnLeftClick += delegate
        {
            ModernConfigUI.Instance.OpenFromMasterControl = false;
            ModernConfigUI.Instance.Open(self.selectedMod);
        };

        self.configList.Add(configPanel);
    }

    private static void DrawMenuDetour(Action<SpriteBatch, GameTime, Color, float, float> orig, SpriteBatch spriteBatch,
        GameTime gameTime, Color color, float logoRotation, float logoScale)
    {
        if (ModernConfigUI.Instance?.Enabled is true)
            logoScale = 0f;
        orig.Invoke(spriteBatch, gameTime, color, logoRotation, logoScale);
    }

    public void Unload()
    {
    }
}