using ImproveGame.Common.AnimationActions;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection;
using Terraria.GameContent.Animations;
using Terraria.GameContent.UI;

namespace ImproveGame.UI.WeatherControl;

public partial class WeatherAmbientElement
{
    internal class FindFrameBlocker : ILoadable
    {
        internal static bool ShouldBlock;

        public void Load(Mod mod)
        {
            IL_NPC.FindFrame += il =>
            {
                var c = new ILCursor(il);
                var label = c.DefineLabel(); // 记录位置
                c.Emit(OpCodes.Ldsfld,
                    typeof(FindFrameBlocker).GetField(nameof(ShouldBlock),
                        BindingFlags.Static | BindingFlags.NonPublic)!);
                c.Emit(OpCodes.Brfalse_S, label); // 为false就跳到下面
                c.Emit(OpCodes.Ret); // 为true直接return
                c.MarkLabel(label);
            };
        }

        public void Unload()
        {
        }
    }

    // 彩蛋动画，用原版的Credits做
    private bool _easterEggActivated;
    private bool _updatedThisTick;
    private int _easterTotalTime;
    private int _easterCurrentTime;
    private readonly float _opacity = 1f;
    private readonly List<IAnimationSegment> _segments = [];

    private void TryActiveEasterEgg()
    {
        if (Main.dayTime)
            return;

        var dimensions = GetDimensions();
        var boundary = new Rectangle((int)dimensions.X, (int)dimensions.Y, 400, 208);
        var mushroomHitbox = new Rectangle(boundary.X + 386, boundary.Y + 112, 16, 16);
        if (_easterEggActivated || !mushroomHitbox.Contains(Main.MouseScreen.ToPoint()))
            return;

        SoundEngine.PlaySound(SoundID.Grass);
        MyUtils.AddNotification(MyUtils.GetText("UI.WeatherGUI.EasterEgg"));
        _easterEggActivated = true;
        _easterCurrentTime = 0;
        FillSegments(out _easterTotalTime);
    }

    private void FillSegments(out int totalTime)
    {
        _segments.Clear();

        totalTime = 0;

        var bunnySegment =
            new Segments.NPCSegment(totalTime, NPCID.Bunny, new Vector2(26, 176), new Vector2(0f, 1f))
                .Then(new Actions.NPCs.Fade(255)).With(new Actions.NPCs.Fade(-5, 50))
                .Then(new Actions.NPCs.Move(new Vector2(0.7f, 0f), 180))
                .Then(new Actions.NPCs.Wait(30));
        totalTime += (int)bunnySegment.DedicatedTimeNeeded;
        int bunnyStopTime = -totalTime; // 这里减，到兔兔准备动的时候再加上totalTime，就是这一段的时间了

        var lovingEmote = new Segments.EmoteSegment(EmoteID.EmotionLove, totalTime, 60, new Vector2(186, 160), SpriteEffects.None, Vector2.Zero);
        totalTime += (int)lovingEmote.DedicatedTimeNeeded;

        var zombieSegment =
            new Segments.NPCSegment(totalTime, NPCID.Zombie, new Vector2(368, 128), new Vector2(0f, 1f))
                .Then(new Actions.NPCs.Fade(255)).With(new Actions.NPCs.Fade(-5, 50))
                .Then(new Actions.NPCs.Move(new Vector2(-0.7f, 0f), 40));
        totalTime += (int)zombieSegment.DedicatedTimeNeeded;

        zombieSegment.Then(new Actions.NPCs.Move(new Vector2(-0.7f, 0f), 30));
        var questionEmote = new Segments.EmoteSegment(EmoteID.EmoteConfused, totalTime, 60, new Vector2(186, 160), SpriteEffects.None, Vector2.Zero);
        totalTime += (int)questionEmote.DedicatedTimeNeeded;

        zombieSegment
            .Then(new Actions.NPCs.Move(new Vector2(-0.7f, 0f), 40))
            .Then(new NPCJump(new Vector2(-1.2f, -6f), 48f, 0.18f, out int dedicatedTime));
        var awareEmote = new Segments.EmoteSegment(EmoteID.EmotionAlert, totalTime + 20, 60, new Vector2(186, 160), SpriteEffects.None, Vector2.Zero);
        totalTime += 40 + dedicatedTime;
        bunnyStopTime += totalTime;
        bunnySegment.Then(new Actions.NPCs.Wait(bunnyStopTime));

        zombieSegment
            .Then(new Actions.NPCs.Move(new Vector2(-0.7f, 0f), 180))
            .Then(new Actions.NPCs.Move(new Vector2(-0.7f, 0f), 50)).With(new Actions.NPCs.Fade(5, 50));
        bunnySegment
            .Then(new Actions.NPCs.Move(new Vector2(-0.74f, 0f), 150))
            .Then(new Actions.NPCs.Move(new Vector2(-0.74f, 0f), 50)).With(new Actions.NPCs.Fade(5, 50));
        totalTime += 230;

        _segments.Add(bunnySegment);
        _segments.Add(zombieSegment);
        _segments.Add(lovingEmote);
        _segments.Add(questionEmote);
        _segments.Add(awareEmote);
    }

    private void UpdateEasterEgg()
    {
        _easterCurrentTime++;

        // 修复和High FPS Support的适配问题，原版在Draw时调用FindFrame，而FindFrame会根据帧数计算，导致高帧数时动画速度加快
        // 希望HFS早日修复这个问题（虽然我觉得不太可能）
        // 这里我的方法是在Update重设这个值，然后在Draw的时候设为true，并且限定只有在false时才调用FindFrame
        _updatedThisTick = false;
    }

    private void DrawEasterEgg(Rectangle boundary, Color color)
    {
        Vector2 anchorPosition = boundary.Location.ToVector2();

        GameAnimationSegment info = default;
        info.SpriteBatch = Main.spriteBatch;
        info.AnchorPositionOnScreen = anchorPosition;
        info.TimeInAnimation = _easterCurrentTime;
        info.DisplayOpacity = _opacity;

        for (int i = 0; i < _segments.Count; i++)
        {
            IAnimationSegment t = _segments[i];
            if (t is Segments.NPCSegment npcSegment)
                npcSegment._npc.color = color * npcSegment._npc.Opacity;

            if (!_updatedThisTick)
            {
                t.Draw(ref info);
                continue;
            }

            FindFrameBlocker.ShouldBlock = true;
            t.Draw(ref info);
            FindFrameBlocker.ShouldBlock = false;
        }

        if (!_updatedThisTick)
            _updatedThisTick = true;
    }
}