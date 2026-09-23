using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.Graphics2D;
using ReLogic.OS;
using System.Globalization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
namespace ImproveGame.UI.ModernConfig.OptionElements
{
    public class OptionColor : OptionObject
    {
        struct ColorPanelVertex(Vector2 pos, Vector2 coord) : IVertexType
        {
            private static readonly VertexDeclaration _vertexDeclaration = new(
            [
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
            ]);
            public Vector2 Pos = pos;
            public Vector2 Coord = coord;
            public readonly VertexDeclaration VertexDeclaration => _vertexDeclaration;
        }
        class ColorHandler
        {
            private readonly PropertyFieldWrapper memberInfo;
            private readonly object item;
            private readonly IList<Color> array;
            private readonly int index;
            private int hslImmuneCount;//用来锁hsl防止编辑它们的时候因为另外四个的变化函数又间接编辑到它们
            internal Color current;
            internal Vector3 hsl;
            internal List<OptionSlider> sliders = [];
            [LabelKey("$Config.Color.Red.Label")]
            public byte Red
            {
                get => current.R;
                set
                {
                    current.R = value;
                    UpdateHSL();
                    Update();
                }
            }

            [LabelKey("$Config.Color.Green.Label")]
            public byte Green
            {
                get => current.G;
                set
                {
                    current.G = value;
                    UpdateHSL();
                    Update();
                }
            }

            [LabelKey("$Config.Color.Blue.Label")]
            public byte Blue
            {
                get => current.B;
                set
                {
                    current.B = value;
                    UpdateHSL();
                    Update();
                }
            }

            [LabelKey("$Config.Color.Hue.Label")]
            public float Hue
            {
                get => hsl.X;
                set
                {
                    byte a = Alpha;
                    current = Main.hslToRgb(value, Saturation, Lightness);
                    current.A = a;
                    Update();
                    hslImmuneCount = 4;
                    hsl.X = value;
                }
            }

            [LabelKey("$Config.Color.Saturation.Label")]
            public float Saturation
            {
                get => hsl.Y;
                set
                {
                    byte a = Alpha;
                    current = Main.hslToRgb(Hue, value, Lightness);
                    current.A = a;
                    Update();
                    hslImmuneCount = 4;
                    hsl.Y = value;
                }
            }

            [LabelKey("$Config.Color.Lightness.Label")]
            public float Lightness
            {
                get => hsl.Z;
                set
                {
                    byte a = Alpha;
                    current = Main.hslToRgb(Hue, Saturation, value);
                    current.A = a;
                    Update();
                    hslImmuneCount = 4;
                    hsl.Z = value;
                }
            }

            [LabelKey("$Config.Color.Alpha.Label")]
            public byte Alpha
            {
                get => current.A;
                set
                {
                    current.A = value;
                    Update();
                }
            }

            public string Hex
            {
                get => current.Hex3();
                set
                {
                    if (uint.TryParse(value, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var result))
                    {
                        uint b = result & 0xFFu;
                        uint g = (result >> 8) & 0xFFu;
                        uint r = (result >> 16) & 0xFFu;
                        current.R = (byte)r;
                        current.G = (byte)g;
                        current.B = (byte)b;
                        for (int n = 0; n < 3; n++)
                            UpdateHSL();
                        //current.packedValue = result;
                        Update();
                    }
                }
            }

            private void Update()
            {
                if (array == null)
                    memberInfo.SetValue(item, current);
                else
                    array[index] = current;
                foreach (var s in sliders)
                    s.SetColorPendingModified();
            }

            private void UpdateHSL()
            {
                if (hslImmuneCount-- >= 0) return;
                Vector3 neoHsl = Main.rgbToHsl(current);
                if (neoHsl.Z != 0 && neoHsl.Z != 1) //只有亮度不为1或0时剩下两个才有意义
                {
                    hsl.Y = neoHsl.Y;
                    if (neoHsl.Y != 0) //只有饱和度不为0色调才有意义
                        hsl.X = neoHsl.X;
                }
                hsl.Z = neoHsl.Z;
            }

            public ColorHandler(PropertyFieldWrapper memberInfo, object item)
            {
                this.item = item;
                this.memberInfo = memberInfo;
                current = (Color)memberInfo.GetValue(item);
            }

            public ColorHandler(IList<Color> array, int index)
            {
                current = array[index];
                this.array = array;
                this.index = index;
            }
            public ColorHandler(Color color) //仅用于设置单项默认值
            {
                current = color;
                hsl = Main.rgbToHsl(color);
            }
        }
        static void DrawRGBPanel(Vector2 pos, Vector2 size, Color current)
        {
            ColorPanelVertex[] vertexs = new ColorPanelVertex[4];
            for (int n = 0; n < 4; n++)
            {
                Vector2 coord = new Vector2(n / 2, n % 2);
                vertexs[n] = new ColorPanelVertex(pos + size * coord, coord);
            }
            Effect colorPanel = colorPanelEffect.Value;
            colorPanel.Parameters["uColor"].SetValue(current.ToVector3());
            colorPanel.Parameters["uTransform"].SetValue(MyUtils.GetMatrix(true));
            colorPanel.CurrentTechnique.Passes[0].Apply();
            Main.instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, [vertexs[0], vertexs[1], vertexs[2], vertexs[1], vertexs[2], vertexs[3]], 0, 2);
            Main.spriteBatch.spriteEffectPass.Apply();
        }
        static void DrawHSLRing(Vector2 pos, Vector2 size, Vector3 hsl)
        {
            ColorPanelVertex[] vertexs = new ColorPanelVertex[4];
            for (int n = 0; n < 4; n++)
            {
                Vector2 coord = new Vector2(n / 2, n % 2);
                vertexs[n] = new ColorPanelVertex(pos + size * coord, coord);
            }
            Effect colorPanel = colorPanelEffect.Value;
            colorPanel.Parameters["uHsl"].SetValue(hsl);
            colorPanel.Parameters["uHueRotation"].SetValue(Matrix.CreateRotationZ(hsl.X * MathHelper.TwoPi));
            colorPanel.Parameters["uTransform"].SetValue(MyUtils.GetMatrix(true));
            colorPanel.CurrentTechnique.Passes[1].Apply();
            Main.instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, [vertexs[0], vertexs[1], vertexs[2], vertexs[1], vertexs[2], vertexs[3]], 0, 2);
            Main.spriteBatch.spriteEffectPass.Apply();
        }
        public IList<Color> ColorList { get; set; }
        ColorHandler c;
        Color currentColor;
        Vector3 currentHSL;
        View colorPanel;
        string textOnColorPanel;
        View colorfulSliderButton;
        bool colorfulMode;

        View hueButton;
        View slButton;
        View gbButton;
        View rButton;
        View RgbPanel;
        View HslRing;

        bool dragging;
        View currentDragTarget;
        static Asset<Effect> colorPanelEffect = ModAsset.ColorPanels;
        void OutSideEditEnd()
        {
            foreach (var slider in c.sliders)
                slider.OutSideEditEnd();
        }
        protected override void OnBind()
        {
            ColorList = (IList<Color>)List;

            if (ColorList != null)
                c = new ColorHandler(ColorList, index);
            else
                c = new ColorHandler(VariableInfo, Item);

            base.OnBind();

            OptionView.Width.Set(-200, 1);
            showMaxHeight = 480;
            colorPanel = new View()
            {
                Width = new(180, 0),
                Height = new(30, 0),
                Left = new(-400, 1),
                Top = new(8, 0),
                RelativeMode = RelativeMode.None,
                Rounded = new(16)
            };

            colorPanel.OnRightClick += (evt, elem) =>
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
                var str = Platform.Get<IClipboard>().Value;
                if (uint.TryParse(str, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var result))
                {
                    //currentColor.packedValue = result;
                    c.Hex = str;
                    textOnColorPanel = MyUtils.GetText("ModernConfig.ColorPastedPopup");
                }
            };
            colorPanel.OnLeftClick += (evt, elem) =>
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
                string code = currentColor.Hex3();
                Platform.Get<IClipboard>().Value = code;
                textOnColorPanel = MyUtils.GetText("ModernConfig.ColorCopiedPopup");
            };
            colorPanel.JoinParent(this);
            colorfulSliderButton = new View()
            {
                Width = new(24, 0),
                Height = new(24, 0),
                RelativeMode = RelativeMode.Horizontal,
                Rounded = new(12)
            };
            colorfulSliderButton.OnLeftClick += (evt, elem) =>
            {
                colorfulMode = !colorfulMode;
                foreach (var s in c.sliders)
                {
                    s.SetColorPendingModified();
                }
            };
            var rgbPanel = new View()
            {
                Width = new(160, 0),
                Height = new(160, 0),
                Left = new(-180, 1),
                Top = new(60, 0),
                RelativeMode = RelativeMode.None,
                Rounded = new(8f),
                BgColor = Color.Black * .3f
            };
            var hslRing = new View()
            {
                Width = new(160, 0),
                Height = new(160, 0),
                Left = new(-180, 1),
                Top = new(240, 0),
                RelativeMode = RelativeMode.None,
                Rounded = new(8f),
                BgColor = Color.Black * .3f
            };
            rgbPanel.JoinParent(this);
            hslRing.JoinParent(this);
            gbButton = new View()
            {
                Height = new(16, 0),
                Width = new(16, 0),
                RelativeMode = RelativeMode.None,
                Rounded = new(1f),
                BgColor = Color.Transparent
            };
            rButton = new View()
            {
                Height = new(16, 0),
                Width = new(16, 0),
                RelativeMode = RelativeMode.None,
                Rounded = new(1f),
                BgColor = Color.Transparent
            };
            hueButton = new View()
            {
                Height = new(16, 0),
                Width = new(16, 0),
                RelativeMode = RelativeMode.None,
                Rounded = new(1f),
                BgColor = Color.Transparent
            };
            slButton = new View()
            {
                Height = new(16, 0),
                Width = new(16, 0),
                RelativeMode = RelativeMode.None,
                Rounded = new(1f),
                BgColor = Color.Transparent
            };
            gbButton.JoinParent(rgbPanel);
            rButton.JoinParent(rgbPanel);
            hueButton.JoinParent(hslRing);
            slButton.JoinParent(hslRing);
            RgbPanel = rgbPanel;
            HslRing = hslRing;


            rgbPanel.OnLeftMouseDown += (evt, elem) =>
            {
                if (evt.Target != elem) return;
                var dimension = rgbPanel.GetDimensions();
                Vector2 v = Main.MouseScreen - dimension.Position();
                v /= dimension.Size();
                if (MathF.Abs(v.X - .5f) < .3f)
                {
                    if (MathF.Abs(v.Y - .4f) < .3f)
                    {
                        dragging = true;
                        currentDragTarget = gbButton;
                        //c.Green = (byte)(255 * (v.X - .2f) / .6f);
                        //c.Blue = (byte)(255 * (v.Y - .2f) / .6f);
                    }
                    if (MathF.Abs(v.Y - .85f) < .05f)
                    {
                        dragging = true;
                        currentDragTarget = rButton;
                        //c.Red = (byte)(255 * (v.X - .2f) / .6f);
                    }
                }
            };

            hslRing.OnLeftMouseDown += (evt, elem) =>
            {
                if (evt.Target != elem) return;
                var dimension = hslRing.GetDimensions();
                Vector2 v = Main.MouseScreen - dimension.Position();
                v /= dimension.Size();
                v -= new Vector2(.5f);
                float ls = v.LengthSquared();
                if (ls > 0.09 && ls < 0.16)
                {
                    dragging = true;
                    currentDragTarget = hueButton;
                }
                v = Vector2.Transform(v, Matrix.CreateRotationZ(currentHSL.X * MathHelper.TwoPi));
                if (MathF.Abs(v.X) + MathF.Abs(v.Y) < 0.3)
                {
                    dragging = true;
                    currentDragTarget = slButton;
                }
            };

            rgbPanel.OnLeftMouseUp += (evt, elem) =>
            {
                dragging = false;
                currentDragTarget = null;
                OutSideEditEnd();
            };

            hslRing.OnLeftMouseUp += (evt, elem) =>
            {
                dragging = false;
                currentDragTarget = null;
                OutSideEditEnd();
            };


            rButton.OnLeftMouseDown += (evt, elem) =>
            {
                if (evt.Target != elem) return;
                dragging = true;
                currentDragTarget = rButton;
            };
            gbButton.OnLeftMouseDown += (evt, elem) =>
            {
                if (evt.Target != elem) return;
                dragging = true;
                currentDragTarget = gbButton;
            };
            hueButton.OnLeftMouseDown += (evt, elem) =>
            {
                if (evt.Target != elem) return;
                dragging = true;
                currentDragTarget = hueButton;
            };
            slButton.OnLeftMouseDown += (evt, elem) =>
            {
                if (evt.Target != elem) return;
                dragging = true;
                currentDragTarget = slButton;
            };

            rButton.OnLeftMouseUp += (evt, elem) =>
            {
                dragging = false;
                currentDragTarget = null;
                OutSideEditEnd();
            };
            gbButton.OnLeftMouseUp += (evt, elem) =>
            {
                dragging = false;
                currentDragTarget = null;
                OutSideEditEnd();
            };
            hueButton.OnLeftMouseUp += (evt, elem) =>
            {
                dragging = false;
                currentDragTarget = null;
                OutSideEditEnd();
            };
            slButton.OnLeftMouseUp += (evt, elem) =>
            {
                dragging = false;
                currentDragTarget = null;
                OutSideEditEnd();
            };



            rgbPanel.OnUpdate += (elem) =>
            {
                if (!dragging) return;
                var dimension = rgbPanel.GetDimensions();
                Vector2 v = Main.MouseScreen - dimension.Position();
                v /= dimension.Size();
                if (currentDragTarget == gbButton)
                {
                    c.Green = (byte)(255 * MathHelper.Clamp((v.X - .2f) / .6f, 0, 1));
                    c.Blue = (byte)(255 * MathHelper.Clamp((v.Y - .1f) / .6f, 0, 1));
                }
                if (currentDragTarget == rButton)
                    c.Red = (byte)(255 * MathHelper.Clamp((v.X - .2f) / .6f, 0, 1));
            };

            hslRing.OnUpdate += (elem) =>
            {
                if (!dragging) return;
                var dimension = hslRing.GetDimensions();
                Vector2 v = Main.MouseScreen - dimension.Position();
                v /= dimension.Size();
                v -= new Vector2(.5f);
                float ls = v.LengthSquared();
                if (currentDragTarget == hueButton)
                    c.Hue = (v.ToRotation() + 6.283f) % 6.283f / 6.283f;
                v = Vector2.Transform(v, Matrix.CreateRotationZ(-currentHSL.X * MathHelper.TwoPi));
                if (currentDragTarget == slButton)
                {
                    c.Saturation = MathHelper.Clamp(v.X / .6f + .5f, 0.01f, 1);
                    c.Lightness = MathHelper.Clamp(v.Y / (.3f - MathF.Abs(v.X)) * .5f + .5f, 0.01f, 0.99f);//0或1会出现*坍缩*
                }
            };

        }
        //OptionEditableText OptionText;
        public override void Update(GameTime gameTime)
        {
            //if(OptionText == null)
            //    foreach(var o in OptionView.ListView.Elements)
            //        if(o is OptionEditableText optText)
            //            OptionText = optText;
            //OptionText.OnUpdate += (elem) =>
            //{

            //};

            colorPanel.Left.Set(MathHelper.Lerp(colorPanel.Left.Pixels, expanded ? -400 : -214, 0.15f), 1);
            colorPanel.Top.Set(MathHelper.Lerp(colorPanel.Top.Pixels, expanded ? 8 : 4, 0.15f), 0);

            colorPanel.Recalculate();
            if (pendingChanges)
            {
                colorfulSliderButton.Remove();
                if (expanded)
                    colorfulSliderButton.JoinParent(HelperBox);
            }

            rButton.Left.Set(0, MathHelper.Lerp(0.2f, 0.8f, currentColor.R / 255f));
            rButton.Top.Set(0, 0.85f);

            gbButton.SetPos(default, MathHelper.Lerp(0.2f, 0.8f, currentColor.G / 255f), MathHelper.Lerp(0.1f, 0.7f, currentColor.B / 255f));

            var hsl = currentHSL;
            hueButton.SetPos(new(1f), MathF.Cos(hsl.X * MathHelper.TwoPi) * .37f + .5f, MathF.Sin(hsl.X * MathHelper.TwoPi) * .37f + .5f);

            var vec = new Vector2(hsl.Y * .6f - .3f, (hsl.Z * .6f - .3f) * (1 - 2 * MathF.Abs(hsl.Y - .5f)));
            vec = Vector2.Transform(vec, Matrix.CreateRotationZ(hsl.X * MathHelper.TwoPi));

            slButton.SetPos(default, vec.X + .5f, vec.Y + .5f);

            rButton.Recalculate();
            gbButton.Recalculate();
            hueButton.Recalculate();
            slButton.Recalculate();
            base.Update(gameTime);
        }
        protected override void ObjectToOption(object data)
        {
            base.ObjectToOption(c);
        }
        protected override void OnWrapOption(ModernConfigOption option)
        {
            #region 旧版
            /*slider.SetColorMethod(option.VariableInfo.Name switch
            {
                "Red" => t => currentColor with { A = 255, R = (byte)(255 * t) } * (currentColor.A / 255f),
                "Green" => t => currentColor with { A = 255, G = (byte)(255 * t) } * (currentColor.A / 255f),
                "Blue" => t => currentColor with {A = 255, B = (byte)(255 * t) } * (currentColor.A / 255f),
                "Alpha"=> t => currentColor with { A = 255 } * t,
                "Hue" => t => Main.hslToRgb(Main.rgbToHsl(currentColor) with { X = t }) * (currentColor.A / 255f),
                "Saturation" => t => Main.hslToRgb(Main.rgbToHsl(currentColor) with { Y = t }) * (currentColor.A / 255f),
                "Lightness" or _ => t => Main.hslToRgb(Main.rgbToHsl(currentColor) with { Z = t }) * (currentColor.A / 255f)
            });*/
            #endregion
            if (option is not OptionSlider slider) return;
            slider.SetColorMethod(option.VariableInfo.Name switch
            {
                "Red" => t => colorfulMode ? currentColor with { A = 255, R = (byte)(255 * t) } : Color.Black * 0.3f,
                "Green" => t => colorfulMode ? currentColor with { A = 255, G = (byte)(255 * t) } : Color.Black * 0.3f,
                "Blue" => t => colorfulMode ? currentColor with { A = 255, B = (byte)(255 * t) } : Color.Black * 0.3f,
                "Alpha" => t => colorfulMode ? currentColor with { A = 255 } * t : Color.Black * 0.3f,
                "Hue" => t => colorfulMode ? Main.hslToRgb(currentHSL with { X = t }) : Color.Black * 0.3f,
                "Saturation" => t => colorfulMode ? Main.hslToRgb(currentHSL with { Y = t }) : Color.Black * 0.3f,
                "Lightness" or _ => t => colorfulMode ? Main.hslToRgb(currentHSL with { Z = t }) : Color.Black * 0.3f,
            });
            c.sliders.Add(slider);
            base.OnWrapOption(option);
        }
        public override void DrawChildren(SpriteBatch spriteBatch)
        {

            Matrix matrix = MyUtils.GetMatrix(true);
            //currentColor = c.current;
            currentColor = c.current with { A = 255 };
            currentHSL = c.hsl;
            base.DrawChildren(spriteBatch);
            //var dimension = GetDimensions();
            //SDFGraphics.NoBorderRoundedBox(dimension.Position() + new Vector2(dimension.Width - 400, 8), default, new Vector2(180, 30), new(16), currentColor, matrix);
            var colorDimension = colorPanel.GetDimensions();

            if (!colorPanel.IsMouseHovering)
            {
                textOnColorPanel = "";
                SDFGraphics.NoBorderRoundedBox(colorDimension.Position(), default, colorDimension.Size(), colorPanel.Rounded, currentColor, matrix);
            }
            else
            {
                SDFGraphics.HasBorderRoundedBox(colorDimension.Position(), default, colorDimension.Size(), colorPanel.Rounded, currentColor, 2, UIStyle.SwitchBorderHover, matrix);

                if (!string.IsNullOrWhiteSpace(textOnColorPanel))
                {
                    var textColor = c.hsl.Z > 0.5f ? Color.Black : Color.White;
                    var boderColor = c.hsl.Z > 0.5f ? Color.White : Color.Black;
                    MyUtils.DrawString(colorDimension.Position() + new Vector2(8, 8), textOnColorPanel, textColor, boderColor, scale: 0.8f, spread: 1.2f);
                }
            }

            var colorfulDimension = colorfulSliderButton.GetDimensions();
            //SDFGraphics.BarRoundedBox(colorfulDimension.Position(), default, colorfulDimension.Size(), colorPanel.Rounded, TextureAssets.Extra[180].Value,0,0.05f, matrix);
            if (expanded)
            {
                if (colorfulSliderButton.IsMouseHovering)
                    SDFGraphics.NoBorderRound(colorfulDimension.Center() + new Vector2(1), new(.5f), 28, UIStyle.SliderRoundHover, matrix);
                SDFRectangle.BarColor(colorfulDimension.Position(), colorfulDimension.Size(), colorfulSliderButton.Rounded, TextureAssets.Extra[180].Value, Vector2.UnitX * .05f, 0, Main.UIScaleMatrix);


                DrawRGBPanel(RgbPanel.GetDimensions().Position(), RgbPanel.GetDimensions().Size(), currentColor);
                //DrawRGBPanel(RgbPanel.GetDimensions().Position(), RgbPanel.GetDimensions().Size(), currentColor);

                DrawHSLRing(HslRing.GetDimensions().Position(), HslRing.GetDimensions().Size(), currentHSL);

                SDFGraphics.HasBorderRound(gbButton.GetDimensions().Position(), new(.5f), 8f, currentColor, 1, currentDragTarget == gbButton ? UIStyle.SliderRoundHover : Color.Black * .3f, matrix);

                SDFGraphics.HasBorderBox(rButton.GetDimensions().Position(), new(.5f), new Vector2(6, RgbPanel.Height.Pixels * .1f), currentColor, 1, currentDragTarget == rButton ? UIStyle.SliderRoundHover : Color.Black * .3f, matrix);

                SDFGraphics.HasBorderRound(hueButton.GetDimensions().Position(), new(.5f), HslRing.Height.Pixels * .1f, currentColor, 1, currentDragTarget == hueButton ? UIStyle.SliderRoundHover : Color.Black * .3f, matrix);

                SDFGraphics.HasBorderRound(slButton.GetDimensions().Position(), new(.5f), 8f, currentColor, 1, currentDragTarget == slButton ? UIStyle.SliderRoundHover : Color.Black * .3f, matrix);

            }

            // 提示，DrawChildren后再绘制一次，因为子元素的DrawSelf会覆盖提示，我们希望鼠标悬停在这整个Color元素上时显示原本提示
            if (!IsMouseHovering)
                return;

            string text = "";
            if (ReloadRequired)
                text += $" - [c/{Color.Orange.Hex3()}:{Language.GetTextValue("tModLoader.ModReloadRequiredMemberTooltip")}]\n";
            text += Tooltip;

            // 颜色条会显示左右键交互的提示
            if (colorPanel.IsMouseHovering)
            {
                // 这里如果Tooltip为空，就显示默认提示
                if (text == "")
                    text = MyUtils.GetText("ModernConfig.NoTooltip");
                text += $"\n{MyUtils.GetText("ModernConfig.ColorButtonTip")}";
            }

            TooltipPanel.SetText(text);
        }
        protected override void OnSetValueExternal(object value)
        {
            var color = (Color)value;
            c.Red = color.R;
            c.Green = color.G;
            c.Blue = color.B;
            c.Alpha = color.A;
        }
        protected override void CheckAttributes()
        {
            base.CheckAttributes();
            ShowStringValueInLabel = false;
        }
    }
}
