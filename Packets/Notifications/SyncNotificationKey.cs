namespace ImproveGame.Packets.Notifications;

[AutoSync]
public class SyncNotificationKey : NetModule
{
    private string _key;
    [ColorSync(syncAlpha: false)]
    private Color _textColor;

    public static void Send(string key, Color color = default, int to = -1, int ignore = -1)
    {
        var module = NetModuleLoader.Get<SyncNotificationKey>();
        module._key = key;
        module._textColor = color;
        module.Send(to, ignore, runLocally: true);
    }

    public override void Receive()
    {
        MyUtils.AddNotification(MyUtils.GetText(_key), _textColor);
    }
}