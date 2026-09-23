namespace ImproveGame.Packets.Notifications;

[AutoSync]
public class SyncNotificationText : NetModule
{
    private string _text;
    [ColorSync(syncAlpha: false)]
    private Color _textColor;

    public static void Send(string text, Color color = default, int to = -1, int ignore = -1)
    {
        var module = NetModuleLoader.Get<SyncNotificationText>();
        module._text = text;
        module._textColor = color;
        module.Send(to, ignore, runLocally: true);
    }

    public override void Receive()
    {
        MyUtils.AddNotification(_text, _textColor);
    }
}