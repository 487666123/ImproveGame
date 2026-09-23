using ImproveGame.Common.Configs;

namespace ImproveGame.Common.ModSystems;

public class NetPasswordSystem : ModSystem
{
    /// <summary>
    /// 允许调节设置的密码，只会在服务器有值
    /// </summary>
    internal static string ConfigPassword;

    /// <summary>
    /// 服务器端记录所有玩家是否已经认证的数组
    /// </summary>
    internal static bool[] Registered = new bool[Main.maxPlayers];

    /// <summary>
    /// 客户端记录自己是否已经认证
    /// </summary>
    internal static bool LocalPlayerRegistered;

    public override void Load()
    {
        On_Netplay.StartServer += SendPassword;
    }

    // 关掉掉线玩家的认证
    public override void PreUpdateWorld()
    {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            if (Registered[i] && !Netplay.Clients[i].Socket.IsConnected())
            {
                Registered[i] = false;
            }
        }
    }

    // 退出时取消认证
    public override void PreSaveAndQuit()
    {
        LocalPlayerRegistered = false;
    }

    private void SendPassword(On_Netplay.orig_StartServer orig)
    {
        orig.Invoke();

        if (!ImproveConfigs.Instance.OnlyHostByPassword)
            return;

        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Registered[i] = false;
        }

        for (int i = 0; i < 4; i++)
            ConfigPassword += (char)Main.rand.Next('A', 'Z' + 1);

        ImproveGame.Instance.Logger.Info(MyUtils.GetTextWith("Configs.ImproveConfigs.OnlyHostByPassword.ServerPasswordLog", new
        {
            Password = ConfigPassword
        }));

        Console.WriteLine(MyUtils.GetTextWith("Configs.ImproveConfigs.OnlyHostByPassword.ServerPassword", new
        {
            Password = ConfigPassword
        }));
    }

    public static bool Permitted(int whoAmI)
    {
        if (Main.netMode is not NetmodeID.Server)
            return true;
        if (ImproveConfigs.Instance.OnlyHostByPassword && !Registered[whoAmI])
            return false;
        if (ImproveConfigs.Instance.OnlyHost && !NetMessage.DoesPlayerSlotCountAsAHost(whoAmI))
            return false;
        return true;
    }
}