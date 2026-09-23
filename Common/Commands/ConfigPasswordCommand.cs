using ImproveGame.Common.Configs;
using ImproveGame.Common.ModSystems;
using ImproveGame.Packets;

namespace ImproveGame.Common.Commands
{
    public class ConfigPasswordCommand : ModCommand
    {
        public override CommandType Type => CommandType.Server | CommandType.Console;
        public override string Usage => "/qotpassword <password>";
        public override string Command => "qotpassword";
        public override string Description => MyUtils.GetText("Configs.ImproveConfigs.OnlyHostByPassword.CommandDescription");

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (!ImproveConfigs.Instance.OnlyHostByPassword)
            {
                caller.Reply(MyUtils.GetText("Configs.ImproveConfigs.OnlyHostByPassword.NotOn"), new(206, 212, 106));
                return;
            }

            if (caller.CommandType == CommandType.Console)
            {
                caller.Reply(MyUtils.GetTextWith("Configs.ImproveConfigs.OnlyHostByPassword.ServerPasswordLog", new { Password = NetPasswordSystem.ConfigPassword }));
                return;
            }

            if (args.Length != 1 || args[0].Length != 4)
            {
                caller.Reply("Usage: /qotpassword <password>", new(240, 40, 40));
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                if (char.IsLetter(args[0][i]))
                    continue;
                caller.Reply(MyUtils.GetText("Configs.ImproveConfigs.OnlyHostByPassword.Format"), new(240, 40, 40));
                return;
            }

            string password = args[0];
            if (password.ToUpper() == NetPasswordSystem.ConfigPassword)
            {
                caller.Reply(MyUtils.GetText("Configs.ImproveConfigs.OnlyHostByPassword.Correct"), new(40, 240, 40));
                NetPasswordSystem.Registered[caller.Player.whoAmI] = true;
                AuthorizedPacket.Send(caller.Player.whoAmI); // 告诉玩家已认证
            }
            else
            {
                caller.Reply(MyUtils.GetText("Configs.ImproveConfigs.OnlyHostByPassword.Incorrect"), new(240, 40, 40));
            }
        }
    }
}
