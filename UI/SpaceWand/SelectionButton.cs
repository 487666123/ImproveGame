namespace ImproveGame.UI.SpaceWand;

public sealed class SelectionButton : UIElement
{
    private SelectionPiece _pieceHoveredOn;
    private SelectionPiece _materialPiece;
    private SelectionPiece _slopePiece;
    private SelectionPiece _shapePiece;
    public float Opacity;

    private SelectionPiece GetHoveredPiece()
    {
        var centerToCursorVector = Main.MouseScreen - GetDimensions().Center();
        var angle = centerToCursorVector.ToRotation();

        // 转换成只有正数的角度
        if (angle < 0)
            angle = MathHelper.TwoPi + angle;

        // 这些角度判断我也不能描述，详见贴图
        return angle switch
        {
            < MathHelper.PiOver4 or > MathHelper.TwoPi - MathHelper.PiOver2 => _slopePiece,
            >= MathHelper.PiOver4 and <= MathHelper.PiOver4 * 3 => _shapePiece,
            _ => _materialPiece
        };
    }

    public SelectionButton(SpaceWandGUI parent)
    {
        this.SetSize(ModAsset.RoundBackground.Size());

        _materialPiece = new SelectionPiece(
            MyUtils.GetText("SpaceWandGUI.PlaceType"),
            ModAsset.SelectionPieceMaterial_Hover,
            ModAsset.SelectionPieceMaterial,
            () => SpaceWandGUI.CurrentPage is SpaceWandGUI.PageType.Material);

        _slopePiece = new SelectionPiece(
            MyUtils.GetText("SpaceWandGUI.BlockType"),
            ModAsset.SelectionPieceSlope_Hover,
            ModAsset.SelectionPieceSlope,
            () => SpaceWandGUI.CurrentPage is SpaceWandGUI.PageType.Slope);

        _shapePiece = new SelectionPiece(
            MyUtils.GetText("SpaceWandGUI.ShapeType"),
            ModAsset.SelectionPieceShape_Hover,
            ModAsset.SelectionPieceShape,
            () => SpaceWandGUI.CurrentPage is SpaceWandGUI.PageType.Shape);

        OnLeftMouseDown += (_, _) =>
        {
            var piece = GetHoveredPiece();
            if (piece == _materialPiece)
                SpaceWandGUI.CurrentPage = SpaceWandGUI.PageType.Material;
            if (piece == _slopePiece)
                SpaceWandGUI.CurrentPage = SpaceWandGUI.PageType.Slope;
            if (piece == _shapePiece)
                SpaceWandGUI.CurrentPage = SpaceWandGUI.PageType.Shape;

            parent.SetupPage();

            parent.Recalculate();
            parent.UpdateButton();
        };

        OnRightMouseDown += (_, _) =>
        {
            if (SpaceWandGUI.Visible && parent.timer.Opened)
                parent.Close();
        };
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _materialPiece.Update();
        _slopePiece.Update();
        _shapePiece.Update();

        SelectionPiece curPieceHoveredOn = !IsMouseHovering ? null : GetHoveredPiece();
        if (curPieceHoveredOn != _pieceHoveredOn)
        {
            _pieceHoveredOn?.MouseOut();
            _pieceHoveredOn = curPieceHoveredOn;
            _pieceHoveredOn?.MouseOver();
        }
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        CalculatedStyle dimensions = GetDimensions();
        Vector2 position = dimensions.Position() + this.GetSize() / 2f;
        Color color = Color.White * Opacity;

        var background = ModAsset.RoundBackground.Value;
        spriteBatch.Draw(background, position, null, color, 0, background.Size() / 2f, 1f, 0, 0f);

        _materialPiece.DrawSelf(spriteBatch, this);
        _slopePiece.DrawSelf(spriteBatch, this);
        _shapePiece.DrawSelf(spriteBatch, this);

        if (_pieceHoveredOn is not null)
        {
            var textColor = new Color(135, 0, 180);
            MyUtils.DrawString(MyUtils.MouseScreenOffset(20), _pieceHoveredOn.HoverText, Color.White, textColor, spread: 1f);
            Main.LocalPlayer.cursorItemIconEnabled = false;
        }
    }
}