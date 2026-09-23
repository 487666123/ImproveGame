using ImproveGame.Attributes;
using ImproveGame.Content.NPCs.Dummy;
using System.Reflection;

namespace ImproveGame.Common.Commands;

public class DummyCommand : ModCommand
{
    public override CommandType Type => CommandType.Chat;
    public override string Command => "dummy";
    public override string Usage => MyUtils.GetText("NPC.DummyCommand_Usage");
    public override string Description => MyUtils.GetText("NPC.DummyCommand_Description");

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        Type type = typeof(DummyConfig);

        switch (args.Length)
        {
            // args[0] equals "info" or "help" ignoring case
            case 1 when args[0].Equals("info", StringComparison.OrdinalIgnoreCase) ||
                        args[0].Equals("help", StringComparison.OrdinalIgnoreCase):
            {
                FieldInfo[] fields = type.GetFields();

                caller.Reply(MyUtils.GetText("NPC.DummyCommand_DummyAttributes"), MyColor.Normal);

                foreach (var field in fields)
                {
                    if (field.GetCustomAttribute<AnnotateAttribute>() is AnnotateAttribute annotateAttribute)
                    {
                        ref string annotate = ref annotateAttribute.Annotate;

                        if (annotate.Equals(string.Empty))
                        {
                            annotate = MyUtils.GetText($"NPC.{field.Name}");
                        }
                        else if (annotate.Length > 1 && annotate.StartsWith('$'))
                        {
                            annotate = MyUtils.GetText($"NPC.{annotate.TrimStart('$')}");
                        }

                        caller.Reply(
                            $"[{field.FieldType.Name}] {field.Name}: {field.GetValue(DummyNPC.LocalConfig)} ({annotate})",
                            MyColor.Normal);
                    }
                }

                return;
            }
            case 2:
            {
                string name = args[0];
                FieldInfo[] fields = type.GetFields();

                foreach (var field in fields)
                {
                    if (field.FieldType.IsPrimitive && field.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            field.SetValueDirect(__makeref(DummyNPC.LocalConfig),
                                Convert.ChangeType(args[1], field.FieldType));
                            caller.Reply(MyUtils.GetTextWith("NPC.DummyCommand_Success", new { name, args = args[1] }),
                                MyColor.Success);

                            SyncDummyModule.Get(null, Main.myPlayer, DummyNPC.LocalConfig).Send(runLocally: true);
                            return;
                        }
                        catch
                        {
                            caller.Reply(MyUtils.GetTextWith("NPC.DummyCommand_Fail", new { input }), MyColor.Fail);
                            return;
                        }
                    }
                }

                break;
            }
        }

        caller.Reply(MyUtils.GetTextWith("NPC.DummyCommand_Invalid", new { input }), MyColor.Fail);
    }

    public record CommandColor(Color Normal, Color Success, Color Fail);

    public CommandColor MyColor = new(new(0, 255, 127), new(0, 200, 0), new(200, 0, 0));
}