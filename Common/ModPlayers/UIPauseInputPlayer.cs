using ImproveGame.Content.Functions.HomeTeleporting;

namespace ImproveGame.Common.ModPlayers;

/// <summary>
/// Processes selected keybinds while the vanilla game is paused.
/// </summary>
public class UIPauseInputPlayer : ModPlayer
{
    public override void UpdateAutopause()
    {
        // These current ProcessTriggers implementations only inspect ModKeybind state
        // and do not read the TriggersSet argument.
        Player.GetModPlayer<ImprovePlayer>().ProcessTriggers(null);
        Player.GetModPlayer<HomeTeleportingPlayer>().ProcessTriggers(null);
    }
}
