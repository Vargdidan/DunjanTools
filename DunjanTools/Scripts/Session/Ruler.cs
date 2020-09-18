using Godot;
using System;

public class Ruler : Node2D
{
    public Label LengthLabel { set; get; }
    public ClientVariables ClientVariables { get; set; }
    private Color shapeColor = new Color(0.2f, 0.2f, 1f, 0.7f);
    private Color originColor = new Color(1f, 0.2f, 0.2f, 0.7f);
    private Vector2 startPos = new Vector2();
    private Vector2 endPos = new Vector2();
    private Boolean posIsSet = false;
    private int halfTile = 32;
    private int fullTile = 64;

    public override void _Ready()
    {
        LengthLabel = (Label)GetNode("Length");
        ClientVariables = (ClientVariables)GetNode("/root/ClientVariables");
        fullTile = ClientVariables.TileSize;
        halfTile = fullTile/2;
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionPressed("ui_space") && !Input.IsActionPressed("ui_shift"))
        {
            Vector2 mousePos = GetGlobalMousePosition();
            endPos = mousePos.Snapped(new Vector2(fullTile, fullTile));
            if (!posIsSet)
            {
                startPos = endPos;
                posIsSet = true;
                GlobalPosition = mousePos;
                LengthLabel.Visible = true;
                Input.SetMouseMode(Input.MouseMode.Hidden);
            }
            LengthLabel.SetGlobalPosition(mousePos);
            Update();
        }

        if (Input.IsActionJustReleased("ui_space"))
        {
            posIsSet = false;
            LengthLabel.Visible = false;
            Input.SetMouseMode(Input.MouseMode.Visible);
            Update();
        }
    }

    public override void _Draw()
    {
        if (posIsSet)
        {
            Vector2 centerStart = ToLocal(new Vector2(startPos.x-halfTile, startPos.y-halfTile));
            Vector2 centerEnd = ToLocal(new Vector2(endPos.x-halfTile, endPos.y-halfTile));
            float centerLength = MathUtil.GetDistance(centerStart, centerEnd);
            int lengthFt = (int)Math.Round(MathUtil.GetDistance(startPos/fullTile, endPos/fullTile)*5f);

            DrawCircle(centerStart, 3f, originColor);
            DrawLine(centerStart, centerEnd, shapeColor, 3, true);
            LengthLabel.Text = lengthFt.ToString() + " ft";
        }
    }
}
