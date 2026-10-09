using ImproveGame.Content.Functions;
using ImproveGame.Packets.Weather;
using ImproveGame.UIFramework.BaseViews;
using Terraria.GameContent.Events;
using Terraria.GameContent.Skies;
using Terraria.Graphics.Effects;
using Terraria.UI.Chat;
using Terraria.Utilities;

namespace ImproveGame.UI.WeatherControl;

public partial class WeatherAmbientElement : View
{
    private const bool Testing = false;
    private static string _hoverText = "";

    // 环境物品专用
    private static float _pinWheelRotation;
    private static float _cloudFade;
    internal static int StarRandomSeed;

    // 模拟风，根据风的位置判断植物摆动
    private readonly int[] _windSimulator = new int[3];

    // 模拟雨
    private float _rainScroller1;
    private float _rainScroller2;

    // 模拟沙尘
    private List<(Vector3, uint)> _dustScroller1 = [];

    // 会动的云
    private float _cloudScroller1;
    private float _cloudScroller2;

    public WeatherAmbientElement()
    {
        SetSizePixels(400f, 208f);

        _rainScroller1 = 800f / 2f;
        _rainScroller2 = 0f;
        _cloudScroller1 = 800f / 2f;
        _cloudScroller2 = 0f;
        for (int i = 0; i < _windSimulator.Length; i++)
            _windSimulator[i] = 1200 / _windSimulator.Length * i;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        UpdateEasterEgg();

        // 风车和风速调节
        float rotateSpeed = MathHelper.Clamp(Main.WindForVisuals * 0.5f, -0.3f, 0.3f);
        _pinWheelRotation += rotateSpeed;

        // 云的动态
        // 淡入淡出
        float realFade = Main.numClouds / 150f;
        ref float fade = ref _cloudFade;
        if (fade > realFade)
            fade -= 0.002f * (float)Main.dayRate;
        if (fade < realFade)
            fade += 0.002f * (float)Main.dayRate;
        // 移动
        float windSpeed = Main.windSpeedCurrent * 0.3f * (float)Main.dayRate;
        _cloudScroller1 += windSpeed;
        if (_cloudScroller1 >= 400f)
            _cloudScroller1 -= 800f;
        if (_cloudScroller1 <= -400f)
            _cloudScroller1 += 800f;
        _cloudScroller2 += windSpeed;
        if (_cloudScroller2 >= 400f)
            _cloudScroller2 -= 800f;
        if (_cloudScroller2 <= -400f)
            _cloudScroller2 += 800f;

        // 雨的动态
        float rainSpeed = 3.4f;
        _rainScroller1 += rainSpeed;
        if (_rainScroller1 >= 400f)
            _rainScroller1 -= 800f;
        _rainScroller2 += rainSpeed;
        if (_rainScroller2 >= 400f)
            _rainScroller2 -= 800f;

        // 风和植物的动态
        for (int i = 0; i < _windSimulator.Length; i++)
        {
            float windFactor = MathHelper.Lerp(0.6f, 1f, MathHelper.Clamp(Math.Abs(Main.WindForVisuals), 0, 1));
            _windSimulator[i] += (int)(9 * windFactor);
            if (_windSimulator[i] > 800)
                _windSimulator[i] = -400;
        }
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);

        TryActiveEasterEgg();

        var tipColor = Color.Lime;
        var dimensions = GetDimensions();
        var boundary = new Rectangle((int)dimensions.X, (int)dimensions.Y, 400, 208);

        // 风车和风速调节
        var pinWheelHitbox = new Rectangle(boundary.X + 160, boundary.Y + 136, 32, 48);
        if (pinWheelHitbox.Contains(Main.MouseScreen.ToPoint()))
        {
            ProcessWindSetting();
        }

        // 钟和时间调节
        var clock = ModAsset.ClockHighlight.Value;
        var clockHitbox = new Rectangle(boundary.X + 322, boundary.Y + 54, clock.Width, clock.Height);
        if (Main.hardMode && clockHitbox.Contains(Main.MouseScreen.ToPoint()))
        {
            switch (Main.dayTime)
            {
                case true when Main.time < 27000:
                    SetTimePacket.SetTime(27000, setIsDayTime: true);
                    Main.NewText(GetText("UI.WeatherGUI.Noon"), tipColor);
                    break;
                case true:
                    SetTimePacket.SetTime(0, setIsDayTime: false);
                    Main.NewText(GetText("UI.WeatherGUI.Dusk"), tipColor);
                    break;
                case false when Main.time < 16200:
                    SetTimePacket.SetTime(16200, setIsDayTime: false);
                    Main.NewText(GetText("UI.WeatherGUI.Midnight"), tipColor);
                    break;
                case false:
                    SetTimePacket.SetTime(0, setIsDayTime: true);
                    Main.NewText(GetText("UI.WeatherGUI.Dawn"), tipColor);
                    break;
            }
        }

        // 月玻璃球和月相调节
        var globe = ModAsset.MoonPhaseHighlight.Value;
        var globeHitbox = new Rectangle(boundary.X + 110, boundary.Y + 112, globe.Width, globe.Height);
        if (globeHitbox.Contains(Main.MouseScreen.ToPoint()))
        {
            int targetMoonPhase = Main.moonPhase + 1;
            if (targetMoonPhase >= 8)
                targetMoonPhase = 0;
            SetMoonPhasePacket.SetTo(targetMoonPhase);
            Main.NewText(GetText("UI.WeatherGUI.MoonPhaseAdjusted", MoonPhaseToText(targetMoonPhase)), tipColor);
        }

        // 音乐盒和雨天调节
        var musixBoxHitbox = new Rectangle(boundary.X + 110, boundary.Y + 144, 36, 36);
        if (musixBoxHitbox.Contains(Main.MouseScreen.ToPoint()))
        {
            if (Main.raining)
                Main.NewText(GetText("UI.WeatherGUI.RainOff"), tipColor);
            else
                Main.NewText(GetText("UI.WeatherGUI.RainOn"), tipColor);

            SetRainPacket.ToggleRain();
        }

        // 音乐盒和沙尘暴调节
        var sandstormMusixBoxHitbox = new Rectangle(boundary.X + 348, boundary.Y + 95, 36, 36);
        if (sandstormMusixBoxHitbox.Contains(Main.MouseScreen.ToPoint()))
        {
            if (!Sandstorm.HasSufficientWind())
            {
                ProcessWindSetting();
            }

            if (Sandstorm.Happening)
                Main.NewText(GetText("UI.WeatherGUI.SandstormOff"), tipColor);
            else
                Main.NewText(GetText("UI.WeatherGUI.SandstormOn"), tipColor);

            SetSandstormPacket.ToggleSandstorm();
        }

        void ProcessWindSetting()
        {
            var setToStage = Main.windSpeedCurrent switch
            {
                > -0.4f and < 0.4f => SetWindPacket.WindStage.West,
                >= 0.4f => SetWindPacket.WindStage.East,
                _ => SetWindPacket.WindStage.No
            };
            Main.NewText(GetText($"UI.WeatherGUI.Wind{setToStage}"), tipColor);
            SetWindPacket.SetTo(setToStage);
        }
    }

    public override void RightMouseDown(UIMouseEvent evt)
    {
        base.RightMouseDown(evt);

        var tipColor = Color.Lime;
        var dimensions = GetDimensions();
        var boundary = new Rectangle((int)dimensions.X, (int)dimensions.Y, 400, 208);

        // 风车和风速调节
        var pinWheelHitbox = new Rectangle(boundary.X + 160, boundary.Y + 136, 32, 48);
        if (pinWheelHitbox.Contains(Main.MouseScreen.ToPoint()))
        {
            WeatherLockerPacket.ToggleWind();
            Main.NewText(
                WeatherController.WindLocked
                    ? GetText("UI.WeatherGUI.WindLocked")
                    : GetText("UI.WeatherGUI.WindUnlocked"), tipColor);
        }

        // 月玻璃球和月相调节
        var globe = ModAsset.MoonPhaseHighlight.Value;
        var globeHitbox = new Rectangle(boundary.X + 110, boundary.Y + 112, globe.Width, globe.Height);
        if (globeHitbox.Contains(Main.MouseScreen.ToPoint()))
        {
            WeatherLockerPacket.ToggleMoonPhase();
            Main.NewText(
                WeatherController.MoonPhaseLocked
                    ? GetText("UI.WeatherGUI.MoonPhaseLocked", MoonPhaseToText(Main.moonPhase))
                    : GetText("UI.WeatherGUI.MoonPhaseUnlocked"), tipColor);
        }

        // 音乐盒和雨天调节
        var musixBoxHitbox = new Rectangle(boundary.X + 110, boundary.Y + 144, 36, 36);
        if (musixBoxHitbox.Contains(Main.MouseScreen.ToPoint()))
        {
            WeatherLockerPacket.ToggleRain();
            Main.NewText(
                WeatherController.RainLocked
                    ? GetText("UI.WeatherGUI.RainLocked")
                    : GetText("UI.WeatherGUI.RainUnlocked"), tipColor);
        }

        // 音乐盒和沙尘暴调节
        var sandstormMusixBoxHitbox = new Rectangle(boundary.X + 348, boundary.Y + 95, 36, 36);
        if (sandstormMusixBoxHitbox.Contains(Main.MouseScreen.ToPoint()))
        {
            WeatherLockerPacket.ToggleSandstorm();
            Main.NewText(
                WeatherController.SandstormLocked
                    ? GetText("UI.WeatherGUI.SandstormLocked")
                    : GetText("UI.WeatherGUI.SandstormUnlocked"), tipColor);
        }

        // 钟和时间调节（暂不开放时间锁定）
        // var clock = ModAsset.ClockHighlight.Value;
        // var clockHitbox = new Rectangle(boundary.X + 322, boundary.Y + 54, clock.Width, clock.Height);
        // if (clockHitbox.Contains(Main.MouseScreen.ToPoint()))
        // {
        // }
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        _hoverText = "";
        var sb = Main.spriteBatch;
        var dimensions = GetDimensions();
        var boundary = new Rectangle((int)dimensions.X, (int)dimensions.Y, 400, 208);

        DrawAmbient(boundary);

        if (string.IsNullOrWhiteSpace(_hoverText)) return;

        const float textScale = 0.8f;
        var textPosition = boundary.Location.ToVector2() + new Vector2(4f);
        var textSize = ChatManager.GetStringSize(FontAssets.MouseText.Value, _hoverText, textScale * Vector2.One);

        // 给文字绘制个背景，至于这个偏移值是GetStringSize有问题造成的
        var textBackgroundDestination =
            new Rectangle(boundary.X, boundary.Y - 1, (int)textSize.X + 10, (int)textSize.Y - 4);
        sb.Draw(TextureAssets.MagicPixel.Value, textBackgroundDestination, Color.Black * 0.5f);

        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, _hoverText,
            textPosition, Color.White, 0f, Vector2.Zero, Vector2.One * textScale, spread: 1f);
    }

    private void DrawAmbient(Rectangle boundary)
    {
        var foreground = ModAsset.AmbientForeground.Value;
        var background = ModAsset.AmbientBackground.Value;
        var tiles = ModAsset.AmbientTiles.Value;
        GetColors(out var colorOfTheSkies, out var tileColor, out var sunColor, out var moonColor);

        Main.spriteBatch.Draw(background, boundary.Location.ToVector2(), colorOfTheSkies);
        DrawStars(boundary, Color.White, colorOfTheSkies);
        DrawSunAndMoon(boundary, sunColor, moonColor);
        Main.spriteBatch.Draw(foreground, boundary.Location.ToVector2(), colorOfTheSkies);
        DrawClouds(boundary, colorOfTheSkies);
        DrawSandstormFilter(boundary);
        DrawLivingObjects(boundary, tileColor);
        Main.spriteBatch.Draw(tiles, boundary.Location.ToVector2(), tileColor);
        DrawHighlights(boundary, tileColor);
        DrawRainMusicBox(boundary, tileColor);
        DrawSandstormMusicBox(boundary, tileColor);
        DrawPinWheel(boundary, tileColor);
        DrawHands(boundary, tileColor); // 时钟指针: hands
        DrawEasterEgg(boundary, tileColor);
        DrawSandstormDust(boundary, tileColor);
        DrawRainingOverlay(boundary, Color.White, tileColor);
        DrawSandstormVignette(boundary);
    }

    private static void GetColors(out Color skiesColor, out Color tileColor, out Color sunColor, out Color moonColor)
    {
        var oldColorOfTheSkies = Main.ColorOfTheSkies;
        Main.InfoToSetBackColor info = default;
        info.BloodMoonActive = Main.bloodMoon;
        Main.SetBackColor(info, out sunColor, out moonColor);
        skiesColor = Main.ColorOfTheSkies;
        Main.ColorOfTheSkies = oldColorOfTheSkies;

        tileColor = new Color
        {
            A = byte.MaxValue,
            R = (byte)((skiesColor.R + skiesColor.G + skiesColor.B + skiesColor.R * 7) / 10),
            G = (byte)((skiesColor.R + skiesColor.G + skiesColor.B + skiesColor.G * 7) / 10),
            B = (byte)((skiesColor.R + skiesColor.G + skiesColor.B + skiesColor.B * 7) / 10)
        };
        tileColor = SkyManager.Instance.ProcessTileColor(tileColor);
        tileColor.R = (byte)MathHelper.Clamp(tileColor.R, 48, 255);
        tileColor.G = (byte)MathHelper.Clamp(tileColor.G, 48, 255);
        tileColor.B = (byte)MathHelper.Clamp(tileColor.B, 48, 255);
    }

    private static void DrawSunAndMoon(Rectangle boundary, Color sunColor, Color moonColor)
    {
        var sunOrMoon = Main.dayTime switch
        {
            true when Main.eclipse => TextureAssets.Sun3.Value,
            true when Main.LocalPlayer.head is ArmorIDs.Head.Sunglasses => TextureAssets.Sun2.Value,
            true => TextureAssets.Sun.Value,
            false when Main.pumpkinMoon => TextureAssets.PumpkinMoon.Value,
            false when Main.snowMoon => TextureAssets.SnowMoon.Value,
            false => TextureAssets.Moon[Main.moonType].Value
        };
        var sunMoonFrame = Main.dayTime switch
        {
            true when Main.remixWorld => new Rectangle(0, 0, 1, 1),
            true => sunOrMoon.Frame(),
            false when Main.pumpkinMoon || Main.snowMoon => sunOrMoon.Frame(),
            false => sunOrMoon.Frame(verticalFrames: 8, frameY: Main.moonPhase)
        };
        var color = Main.dayTime ? sunColor : moonColor;
        float rainFactor = (1f - Main.maxRaining) * 0.96f + 0.04f;
        color *= rainFactor;

        float timeLength = (float)(Main.dayTime ? Main.dayLength : Main.nightLength); // 时间长度
        float timePercentage = (float)Main.time / timeLength; // 将时间映射到 0.0 ~ 1.0
        float offsetAngle = MathHelper.Lerp(0f, MathHelper.PiOver2, timePercentage); // 0 ~ 90°
        float realAngle = (offsetAngle + MathHelper.PiOver4) + MathHelper.Pi; // 180° ~ 270°
        Vector2 center = boundary.Bottom() + new Vector2(0f, 160f); // 中心点

        var position = center + realAngle.ToRotationVector2() * 340f; // 最终位置
        var rotation = realAngle + MathHelper.PiOver2; // 旋转角度
        Main.spriteBatch.Draw(sunOrMoon, position, sunMoonFrame, color, rotation,
            sunMoonFrame.Size() / 2f, 1f, SpriteEffects.None, 0f);
    }

    private void DrawClouds(Rectangle boundary, Color color)
    {
        var clouds = ModAsset.AmbientClouds.Value;
        var startPosition = boundary.Location.ToVector2();

        var pos1 = startPosition + new Vector2(_cloudScroller1, 0f);
        Main.spriteBatch.Draw(clouds, pos1, color * _cloudFade);

        var pos2 = startPosition + new Vector2(_cloudScroller2, 0f);
        Main.spriteBatch.Draw(clouds, pos2, color * _cloudFade);
    }

    private void DrawSandstormFilter(Rectangle boundary)
    {
        boundary.Height -= 4;
        float num = Math.Min(1f, Sandstorm.Severity * 1.5f);
        Color color3 = new Color(new Vector4(0.85f, 0.66f, 0.33f, 1f) * 0.8f * Main.ColorOfTheSkies.ToVector4()) * (SkyManager.Instance["Sandstorm"] as SandstormSky)._opacity * num;
        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, boundary, color3);
    }

    private static void DrawHighlights(Rectangle boundary, Color color)
    {
        var clock = ModAsset.ClockHighlight.Value;
        var clockHitbox = new Rectangle(boundary.X + 322, boundary.Y + 54, clock.Width, clock.Height);
        bool clockHovered = clockHitbox.Contains(Main.MouseScreen.ToPoint());
        if (!Main.hardMode)
        {
            Main.spriteBatch.Draw(ModAsset.ClockLocked.Value, clockHitbox.Location.ToVector2(), color);
            if (clockHovered)
                _hoverText = GetText("UI.WeatherGUI.TimeUnlockCondition");
        }
        else if (clockHovered)
        {
            Main.spriteBatch.Draw(clock, clockHitbox.Location.ToVector2(), color);
            _hoverText = GetText("UI.WeatherGUI.Time");
        }

        var globe = ModAsset.MoonPhaseHighlight.Value;
        var globeHitbox = new Rectangle(boundary.X + 110, boundary.Y + 112, globe.Width, globe.Height);
        if (globeHitbox.Contains(Main.MouseScreen.ToPoint()))
        {
            Main.spriteBatch.Draw(globe, globeHitbox.Location.ToVector2(), color);
            _hoverText = GetText("UI.WeatherGUI.MoonPhase") + "\n" +
                         GetText($"UI.WeatherGUI.MoonPhaseLock{WeatherController.MoonPhaseLocked}");
        }

        if (WeatherController.MoonPhaseLocked)
            DrawLock(globeHitbox, color);
    }

    private static void DrawRainMusicBox(Rectangle boundary, Color color)
    {
        var musixBoxHitbox = new Rectangle(boundary.X + 110, boundary.Y + 144, 36, 36);
        var position = musixBoxHitbox.Location.ToVector2();

        var musixBoxHighlight = ModAsset.RainHighlight.Value;
        var musixBoxTexture = Main.raining ? ModAsset.RainActive.Value : ModAsset.RainInactive.Value;

        Main.spriteBatch.Draw(musixBoxTexture, position, color);
        if (musixBoxHitbox.Contains(Main.MouseScreen.ToPoint()))
        {
            Main.spriteBatch.Draw(musixBoxHighlight, position, color);
            _hoverText = Main.raining
                ? GetText("UI.WeatherGUI.RainInactive")
                : GetText("UI.WeatherGUI.RainActive");
            _hoverText += "\n" + GetText($"UI.WeatherGUI.RainLock{WeatherController.RainLocked}");
        }

        if (WeatherController.RainLocked)
            DrawLock(musixBoxHitbox, color);
    }
    private static void DrawSandstormMusicBox(Rectangle boundary, Color color)
    {
        var musixBoxHitbox = new Rectangle(boundary.X + 348, boundary.Y + 95, 36, 36);
        var position = musixBoxHitbox.Location.ToVector2();

        var musixBoxHighlight_1 = ModAsset.SandstormHighlight_1.Value;
        var musixBoxHighlight_2 = ModAsset.SandstormHighlight_2.Value;
        var musixBoxTexture = Sandstorm.Happening ? ModAsset.SandstormActive.Value : ModAsset.SandstormInactive.Value;

        Main.spriteBatch.Draw(musixBoxTexture, position, color);
        if (musixBoxHitbox.Contains(Main.MouseScreen.ToPoint()))
        {
            if (Sandstorm.Happening)
                Main.spriteBatch.Draw(musixBoxHighlight_2, position, color);
            else
                Main.spriteBatch.Draw(musixBoxHighlight_1, position, color);

            _hoverText = Sandstorm.Happening
                ? GetText("UI.WeatherGUI.SandstormInactive")
                : GetText("UI.WeatherGUI.SandstormActive");
            _hoverText += "\n" + GetText($"UI.WeatherGUI.SandstormLock{WeatherController.SandstormLocked}");
        }

        if (WeatherController.SandstormLocked)
            DrawLock(musixBoxHitbox, color);
    }

    private static void DrawPinWheel(Rectangle boundary, Color color)
    {
        var pinPosition = new Vector2(boundary.X + 172, boundary.Y + 152);
        var center = new Vector2(boundary.X + 176, boundary.Y + 146);
        var pinWheelHitbox = new Rectangle(boundary.X + 160, boundary.Y + 136, 32, 48);
        bool hovered = pinWheelHitbox.Contains(Main.MouseScreen.ToPoint());

        var wheelTexture = hovered ? ModAsset.WheelHighlight.Value : ModAsset.Wheel.Value;
        var pinHighlight = ModAsset.PinHighlight.Value;
        var wheelPinTexture = ModAsset.WheelPin.Value; // 风车中间的轴，不随风车一起转，不然很怪

        if (hovered)
        {
            Main.spriteBatch.Draw(pinHighlight, pinPosition, color);
            _hoverText = GetText("UI.WeatherGUI.Wind") + "\n" +
                         GetText($"UI.WeatherGUI.WindLock{WeatherController.WindLocked}");
        }

        Main.spriteBatch.Draw(wheelTexture, center, null, color, _pinWheelRotation,
            wheelTexture.Size() / 2f, 1f, SpriteEffects.None, 0f);

        var wheelPinPosition = center - new Vector2(4f, 4f);
        Main.spriteBatch.Draw(wheelPinTexture, wheelPinPosition, color);

        if (WeatherController.WindLocked)
            DrawLock(pinWheelHitbox, color);
    }

    private static void DrawHands(Rectangle boundary, Color color)
    {
        if (!Main.hardMode) return;

        float actualTime = (float)Main.time + (Main.dayTime ? 0 : 54000); // 以秒数表示的24小时制时间
        actualTime += 16200; // 修正4:30a.m. 泰拉里在4:30a.m.时Main.time为0
        if (actualTime >= 86400)
            actualTime -= 86400; // 修正24:00，确保0:00时该值为0，且值域为0 ~ 86400

        float timeHours = (actualTime / 86400 * 24) % 12; // 以小时数表示的12小时制时间
        float timeMinutes = actualTime / 60 % 60; // 该小时内的分钟数
        float factorHours = timeHours / 12f; // 小时转换为 0 ~ 1
        float factorMinutes = timeMinutes / 60f; // 分钟转换为 0 ~ 1
        float angleHours = MathHelper.Lerp(0f, MathHelper.TwoPi, factorHours) - MathHelper.PiOver2; // 小时转换为角度
        float angleMinutes = MathHelper.Lerp(0f, MathHelper.TwoPi, factorMinutes) - MathHelper.PiOver2; // 分钟转换为角度

        var hourHand = ModAsset.HourHand.Value;
        var minuteHand = ModAsset.MinuteHand.Value;
        var center = new Vector2(boundary.X + 336, boundary.Y + 74); // 表盘中心
        var origin = new Vector2(0f, 1f); // 旋转锚点，两指针都一样
        Main.spriteBatch.Draw(hourHand, center, null, color, angleHours, origin, 1f, SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(minuteHand, center, null, color, angleMinutes, origin, 1f, SpriteEffects.None, 0f);
    }

    private void DrawSandstormDust(Rectangle boundary, Color tileColor)
    {
        var dust = ModAsset.SandstormDust.Value;
        var startPosition = boundary.Location.ToVector2();

        if (Sandstorm.ShouldSandstormDustPersist())
            for (int i = 0; i < 5f * Sandstorm.Severity; i++)
                if (Main.rand.NextBool(5))
                {
                    WeightedRandom<Color> weightedRandom = new WeightedRandom<Color>();
                    weightedRandom.Add(new Color(200, 160, 20, 180), Main.SceneMetrics.GetTileCount(53) + Main.SceneMetrics.GetTileCount(396) + Main.SceneMetrics.GetTileCount(397));
                    weightedRandom.Add(new Color(103, 98, 122, 180), Main.SceneMetrics.GetTileCount(112) + Main.SceneMetrics.GetTileCount(400) + Main.SceneMetrics.GetTileCount(398));
                    weightedRandom.Add(new Color(135, 43, 34, 180), Main.SceneMetrics.GetTileCount(234) + Main.SceneMetrics.GetTileCount(401) + Main.SceneMetrics.GetTileCount(399));
                    weightedRandom.Add(new Color(213, 196, 197, 180), Main.SceneMetrics.GetTileCount(116) + Main.SceneMetrics.GetTileCount(403) + Main.SceneMetrics.GetTileCount(402));

                    if (Main.windSpeedCurrent > 0)
                        _dustScroller1.Add((new Vector3(-8, Main.rand.NextFloat(boundary.Height - 40), Main.rand.NextFloat(6.00f, 10.00f)), weightedRandom.Get().PackedValue));
                    else
                        _dustScroller1.Add((new Vector3(boundary.Width + 8, Main.rand.NextFloat(boundary.Height), Main.rand.NextFloat(-10.00f, -6.00f)), weightedRandom.Get().PackedValue));
                }

        for (int i = _dustScroller1.Count - 1; i >= 0; i--)
        {
            var vec = _dustScroller1[i].Item1;

            var pos1 = startPosition + new Vector2(vec.X, vec.Y);
            _dustScroller1[i] = (new Vector3(vec.X + vec.Z, vec.Y + (float)Math.Sin(i / 11f) * 1.5f + 0.7f, vec.Z), _dustScroller1[i].Item2);
            vec = _dustScroller1[i].Item1;

            if (vec.X > boundary.Width + 10 || vec.X < -10 || new Vector2(0, boundary.Height).Distance(new Vector2(vec.X, vec.Y)) < 8 || new Vector2(boundary.Width, boundary.Height).Distance(new Vector2(vec.X, vec.Y)) < 8)
                _dustScroller1.RemoveAt(i);
            else
                Main.spriteBatch.Draw(dust, pos1, new Rectangle(0, 8 * (Math.Abs((int)(vec.Z * 100)) % 3), 8, 8), new Color(_dustScroller1[i].Item2), 0, new Vector2(4, 4), 1, SpriteEffects.None, 0);
        }

        if (_dustScroller1.Count == 0)
            return;

        var overlayOnRain = ModAsset.OverlayOnRain.Value;
        Main.spriteBatch.Draw(overlayOnRain, boundary.Location.ToVector2(), tileColor);
    }

    private void DrawRainingOverlay(Rectangle boundary, Color color, Color tileColor)
    {
        if (!Main.IsItRaining || Main.LocalPlayer.ZoneSandstorm)
            return;

        float scaleX = 1.4f;
        float scaleY = 2.5f;

        var overlay = ModAsset.RainScrollerOverlay.Value;
        var origin = overlay.Size() / 2f;
        float windFactor = Utils.GetLerpValue(-0.8f, 0.8f, Main.WindForVisuals, true);
        float rotation = MathHelper.Lerp(MathHelper.PiOver4, -MathHelper.PiOver4, windFactor); // 这里俩值没写反！
        float cloudAlpha = Main.cloudAlpha * 1.8f;
        if (cloudAlpha > 0.68f)
            cloudAlpha = 0.68f;
        var ambientColor = Main.bloodMoon ? Color.OrangeRed : Color.White;
        var rainColor = color.MultiplyRGB(ambientColor);

        var startPosition = boundary.Center.ToVector2();
        var position = startPosition;
        position += new Vector2(0f, _rainScroller1 * scaleY).RotatedBy(rotation);
        Main.spriteBatch.Draw(overlay, position, null, rainColor * cloudAlpha, rotation, origin,
            new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);

        position = startPosition;
        position += new Vector2(0f, _rainScroller2 * scaleY).RotatedBy(rotation);
        Main.spriteBatch.Draw(overlay, position, null, rainColor * cloudAlpha, rotation, origin,
            new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);

        var overlayOnRain = ModAsset.OverlayOnRain.Value;
        Main.spriteBatch.Draw(overlayOnRain, boundary.Location.ToVector2(), tileColor);
    }

    private static void DrawSandstormVignette(Rectangle boundary)
    {
        Main.spriteBatch.Draw(ModAsset.SandstormVignette.Value, boundary, Color.White * (SkyManager.Instance["Sandstorm"] as SandstormSky)._opacity * 0.5f);
    }

    private static void DrawStars(Rectangle boundary, Color color, Color colorOfTheSkies)
    {
        // 这是原版，很神奇吧
        float opacity = (255f * (1f - Main.cloudAlpha) - colorOfTheSkies.R - 25f) / 255f;
        if (opacity <= 0f)
            return;

        color.A = 0;
        color *= opacity;
        int seed = StarRandomSeed;
        UnifiedRandom starRandom = new(seed);
        for (int i = 0; i < 12; i++)
        {
            var position = starRandom.NextVector2FromRectangle(boundary);
            int type = starRandom.Next(4);
            var tex = TextureAssets.Star[type].Value;
            var origin = tex.Size() / 2f;
            float rotation = starRandom.NextFloat() * MathHelper.TwoPi;
            float scale = starRandom.NextFloat(0.4f, 0.7f);
            scale *= Main.mouseTextColor / 230f;
            Main.spriteBatch.Draw(tex, position, null, color, rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// 会随风摆动的东西
    /// </summary>ever
    private void DrawLivingObjects(Rectangle boundary, Color color)
    {
        void DrawGrass(Asset<Texture2D> texture, int x, int y, bool flipped, float swayIntensity = 0.21f)
        {
            float windFactor = _windSimulator.Sum(t => TrUtils.GetLerpValue(200, 0, Math.Abs(x - t), true));
            windFactor = TrUtils.SmoothStep(0f, 1f, windFactor);

            var tex = texture.Value;
            var position = new Vector2(x, y) + boundary.Location.ToVector2();
            var origin = new Vector2(tex.Width / 2, tex.Height);
            var rotation = windFactor * swayIntensity * Main.WindForVisuals;
            var spriteEffects = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            position.Y += Math.Abs(rotation * 5f);
            Main.spriteBatch.Draw(tex, position, null, color, rotation, origin, 1f, spriteEffects, 0f);

            if (!Testing)
                return;

            foreach (var t in _windSimulator)
            {
                var simulatorPosition = boundary.Location.ToVector2() + new Vector2(t, 0);
                Main.spriteBatch.Draw(ModAsset.icon.Value, simulatorPosition, Color.White);
            }
        }

        void DrawBranch(int x, int y, bool flipped)
        {
            float windFactor = _windSimulator.Sum(t => TrUtils.GetLerpValue(200, 0, Math.Abs(x - t), true));
            windFactor = TrUtils.SmoothStep(0f, 1f, windFactor);

            var tex = ModAsset.Branch.Value;
            var position = new Vector2(x, y) + boundary.Location.ToVector2();
            var origin = new Vector2(flipped ? 0f : 30f, 22f);
            var rotation = windFactor * 0.07f * Main.WindForVisuals;
            var spriteEffects = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Main.spriteBatch.Draw(tex, position, null, color, rotation, origin, 1f, spriteEffects, 0f);
        }

        DrawBranch(48, 132, false);
        DrawBranch(62, 150, true);
        DrawGrass(ModAsset.Treetop, 55, 106, false, swayIntensity: 0.09f);

        DrawGrass(ModAsset.Grass1, 8, 178, false);
        DrawGrass(ModAsset.Grass2, 20, 178, false);
        DrawGrass(ModAsset.Grass1, 88, 178, true);
        DrawGrass(ModAsset.Grass3, 202, 178, true);
        DrawGrass(ModAsset.Grass5, 216, 178, true);
        DrawGrass(ModAsset.Grass1, 264, 146, false);
        DrawGrass(ModAsset.Grass4, 296, 130, false);
        DrawGrass(ModAsset.Grass3, 308, 130, false);
        if (!_easterEggActivated)
            DrawGrass(ModAsset.Mushroom, 394, 130, false);

        if (!Testing)
            return;

        for (int i = 0; i < 400; i += 16)
            DrawGrass(ModAsset.Grass1, i, 108, true);
    }

    private static void DrawLock(Rectangle parentHitbox, Color color)
    {
        var center = parentHitbox.Center.ToVector2();
        var texture = ModAsset.Lock.Value;
        var origin = texture.Size() / 2f;
        Main.spriteBatch.Draw(texture, center, null, color, 0f, origin, 1f, SpriteEffects.None, 0f);
    }
}