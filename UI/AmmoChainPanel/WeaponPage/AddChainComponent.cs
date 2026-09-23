using ImproveGame.Content.Functions.ChainedAmmo;

namespace ImproveGame.UI.AmmoChainPanel.WeaponPage;

public class AddChainComponent(WeaponPage parent) : ToolComponent(parent)
{
    protected override string Key => "Mods.ImproveGame.UI.AmmoChain.AddChain";

    protected override Texture2D Icon => ModAsset.AddChainIcon.Value;

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);
        Click();
    }

    public override void RightMouseDown(UIMouseEvent evt)
    {
        base.RightMouseDown(evt);
        Click();
    }

    private void Click()
    {
        string name = MyUtils.GetText("UI.AmmoChain.FileName");
        AmmoChainUI.Instance.StartEditingChain(new AmmoChain(), true, name);
        SoundEngine.PlaySound(SoundID.Item37);
        AmmoChainUI.Instance.GenerateParticleAtMouse();
    }
}