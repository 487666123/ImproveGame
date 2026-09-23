using Terraria.UI.Chat;

namespace ImproveGame.Content.NPCs.Dummy;

public class DummyDPS
{
    public static int ResetInterval { get; set; } = 120;

    public DummyNPC Parent;
    public bool IsStarted;
    public int SurvivalTimer;
    public int FirstHitTime;
    public int LastHitTime;
    public int FinalBlow;
    public int TotalDamage;
    public int HitDuration => LastHitTime - FirstHitTime;

    public void Update()
    {
        if (IsStarted)
        {
            SurvivalTimer++;
        }

        if (IsStarted && SurvivalTimer - LastHitTime > ResetInterval)
        {
            IsStarted = false;
            Reset();
        }
    }

    /// <summary>
    /// 对假人照成伤害时调用，计入累计伤害
    /// </summary>
    /// <param name="damage">本次伤害</param>
    public void Hurt(int damage)
    {
        IsStarted = true;

        TotalDamage += damage;
        FinalBlow = damage;
        LastHitTime = SurvivalTimer;
    }

    /// <summary>
    /// 重置假人 DPS 数据
    /// </summary>
    public void Reset()
    {
        IsStarted = false;
        FirstHitTime = SurvivalTimer;
        LastHitTime = SurvivalTimer;
        TotalDamage = 0;
        FinalBlow = 0;
        Parent?.ClearBuffs();
    }

    public override string ToString()
    {
        float hitFrame = IsStarted ? Math.Max(1f, HitDuration) : 0f;
        float hitTick = MathF.Max(1f / 60f, hitFrame / 60f);

        return $"{MyUtils.GetTextWith("NPC.Dummy_Damage",
            new
            {
                S = IsStarted ? MathF.Round(TotalDamage / hitTick) : 0,
                F = IsStarted ? MathF.Round(TotalDamage / hitFrame, 1) : 0
            })} ({TotalDamage})\n" +
               $"{MyUtils.GetTextWith("NPC.Dummy_Time", new { S = Math.Round(hitTick, 1), F = hitFrame })}\n" +
               $"{MyUtils.GetTextWith("NPC.Dummy_FinalBlow", new { FinalBlow })}";
    }

    public void DrawString(Vector2 position, Vector2 percentOrign)
    {
        string @string = ToString();
        Vector2 size = ChatManager.GetStringSize(FontAssets.ItemStack.Value, @string, new(1f));

        MyUtils.DrawString(position, @string, Color.White, Color.Black, size * percentOrign, 1f, false, 2,
            Main.LocalPlayer.gravDir < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None);
    }
}