using ImproveGame.Common;
using ImproveGame.Common.Configs;
using ImproveGame.Common.ModSystems;
using System.Reflection;
using Terraria.UI.Chat;

namespace ImproveGame;

public class ImproveGame : Mod
{
    private class DisplayNameUpdater : ModSystem
    {
        public override void OnLocalizationsLoaded() => Instance.DisplayName = MyUtils.GetText("ModName");
    }

    public static ImproveGame Instance => ModContent.GetInstance<ImproveGame>();

    // 额外BUFF槽
    public override uint ExtraPlayerBuffSlots => (uint)ImproveConfigs.Instance.ExtraPlayerBuffSlots;

    public override void Load()
    {
        NetModuleLoader.CurrentMod = this;
        NetModuleLoader.LoadAutoSyncsFrom(typeof(NetModuleLoader).Assembly);
        NetModuleLoader.LoadAutoSyncsFrom(Assembly.GetExecutingAssembly());
        NetModuleLoader.LoadNetModules();
        AddContent<NetModuleLoader>();
        ChatManager.Register<BgItemTagHandler>("bgitem");
        ChatManager.Register<CenteredItemTagHandler>("centeritem");
        ChatManager.Register<QotGlyphTagHandler>("qotglyph");
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI) => NetModule.ReceiveModule(reader, whoAmI);

    public override object Call(params object[] args) => ModIntegrationsSystem.Call(args);
}