namespace ImproveGame.UIFramework.UIElements
{
    internal class ModUIPanel : UIPanel
    {
        public bool KeepPressed;
        internal bool Resizeable;
        internal bool Resizing;
        private readonly int _minResizeWidth;
        private readonly int _minResizeHeight;

        internal bool Draggable;
        internal bool Dragging;

        internal Vector2 Offset;

        internal event ElementEvent OnRecalculate;
        internal event ElementEvent OnDrag;
        internal event ElementEvent OnResize;

        internal Asset<Texture2D> ResizeIndicator;

        public ModUIPanel(bool draggable = true, bool resizeable = false, int minResizeWidth = 160, int minResizeHeight = 90)
        {
            Draggable = draggable;
            Resizeable = resizeable;
            OnLeftMouseDown += DragStart;
            OnLeftMouseUp += DragEnd;
            _minResizeWidth = minResizeWidth;
            _minResizeHeight = minResizeHeight;
            ResizeIndicator = MyUtils.GetTexture("UI/Resize");
        }

        public override void Recalculate()
        {
            base.Recalculate();
            OnRecalculate?.Invoke(this);
        }

        public Rectangle ResizeRectangle
        {
            get
            {
                CalculatedStyle innerDimensions = GetInnerDimensions();
                return new Rectangle((int)(innerDimensions.X + innerDimensions.Width - 12), (int)(innerDimensions.Y + innerDimensions.Height - 12), 12 + (int)PaddingRight, 12 + (int)PaddingBottom);
            }
        }

        // 可拖动/调整大小界面
        private void DragStart(UIMouseEvent evt, UIElement listeningElement)
        {
            CalculatedStyle innerDimensions = GetInnerDimensions();
            if (Resizeable && ResizeRectangle.Contains(evt.MousePosition.ToPoint()))
            {
                Offset = new Vector2(evt.MousePosition.X - innerDimensions.X - innerDimensions.Width - 12, evt.MousePosition.Y - innerDimensions.Y - innerDimensions.Height - 12);
                Resizing = true;
            }
            else if (Draggable)
            {
                Offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
                Dragging = true;
            }
        }

        // 可拖动/调整大小界面
        private void DragEnd(UIMouseEvent evt, UIElement listeningElement)
        {
            Dragging = false;
            Resizing = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (Resizeable)
            {
                var dimensions = GetInnerDimensions();
                var position = dimensions.ToRectangle().BottomRight() - new Vector2(4f);
                var texture = ResizeIndicator.Value;

                spriteBatch.Draw(texture, position, BorderColor);

                if (ResizeRectangle.Contains(Main.MouseScreen.ToPoint()) || Resizing)
                {
                    Main.cursorOverride = CursorOverrideID.GamepadDefaultCursor;
                    Main.cursorColor = Color.SkyBlue;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            CalculatedStyle dimensions = GetOuterDimensions();

            if (Dragging)
            {
                Left.Set(Main.mouseX - Offset.X, 0f);
                Top.Set(Main.mouseY - Offset.Y, 0f);
                Recalculate();
                OnDrag?.Invoke(this);
            }
            if (Resizing)
            {
                Width.Pixels = Main.MouseScreen.X - dimensions.X - Offset.X;
                Height.Pixels = Main.MouseScreen.Y - dimensions.Y - Offset.Y;
                // 限制
                if (Width.Pixels <= _minResizeWidth)
                {
                    Width.Pixels = _minResizeWidth;
                }
                if (Height.Pixels <= _minResizeHeight)
                {
                    Height.Pixels = _minResizeHeight;
                }
                OnResize?.Invoke(this);
                Recalculate();
            }
        }

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);
            KeepPressed = true;
        }

        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            KeepPressed = false;
        }
    }
}
