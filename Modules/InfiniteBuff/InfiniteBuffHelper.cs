using ImproveGame.Common.Configs;

namespace ImproveGame.Modules.InfiniteBuff;

/// <summary>
/// 无限 BUFF 帮助类
/// </summary>
internal static class InfiniteBuffHelper
{
    private static string LeftClickDisable => MyUtils.GetText("BuffController.LeftClick.Disable");
    private static string LeftClickEnable => MyUtils.GetText("BuffController.LeftClick.Enable");
    public static string GetLeftClickString(bool enable) => enable ? LeftClickDisable : LeftClickEnable;
    private static string RightClickDisable => MyUtils.GetText("BuffController.RightClick.Disable");
    private static string RightClickEnable => MyUtils.GetText("BuffController.RightClick.Enable");
    public static string GetRightClickString(bool enable) => enable ? RightClickDisable : RightClickEnable;

    public static float RemapSliderToSpawnRate(float value, int digits = 0) =>
        RemapSliderToSpawnRate(value, ImproveConfigs.Instance.SpawnRateMinValue, ImproveConfigs.Instance.SpawnRateMaxValue, digits);

    public static float RemapSpawnRateToSlider(float value) =>
        RemapSpawnRateToSlider(value, ImproveConfigs.Instance.SpawnRateMinValue, ImproveConfigs.Instance.SpawnRateMaxValue);

    /// <summary>
    /// 重映射滑块至生成率
    /// </summary>
    public static float RemapSliderToSpawnRate(float value, float min, float max, int digits)
    {
        if (min >= max) return max;

        value = Math.Clamp(value, 0f, 1f);

        if (min < 1)
        {
            value = value * 2f;

            if (value < 1f)
            {
                return MathF.Round(MathHelper.Lerp(min, 1, value), 2);
            }

            value -= 1f;
            return MathF.Round(MathHelper.Lerp(1, max, value), digits);
        }

        return MathF.Round(MathHelper.Lerp(min, max, value), digits);
    }

    /// <summary>
    /// 将生成率重新映射到滑块
    /// </summary>
    public static float RemapSpawnRateToSlider(float rate, float min, float max)
    {
        // 直接返回
        if (min >= max) return 0.5f;
        if (max < 1) return 0.5f;

        rate = Math.Clamp(rate, min, max);

        // 最小值小于 1
        if (min < 1)
        {
            if (rate < 1)
            {
                rate = MathF.Round(rate, 2);

                return Math.Clamp((rate - min) / (1 - min) / 2f, 0f, 0.5f);
            }

            rate = MathF.Round(rate);

            return Math.Clamp(((rate - 1) / (max - 1) + 1f) / 2f, 0.5f, 1f);
        }

        rate = MathF.Round(rate, 2);

        return Math.Clamp((rate - min) / (max - min), 0f, 1f);
    }
}