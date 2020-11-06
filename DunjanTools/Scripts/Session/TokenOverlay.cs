using Godot;
using System;

public class TokenOverlay : Node2D
{
    private int tileSize = 64;
    public ClientVariables ClientVariables { set; get; }
    public Token Token { set; get; }
    private Boolean selected { set; get; }
    private Color selectedColor = new Color(0.956f, 0.635f, 0.38f, 1f);
    private Color dragColor = new Color(0.913f, 0.768f, 0.415f, 1f);
    public override void _Ready()
    {
        ClientVariables = (ClientVariables)GetNode("/root/ClientVariables");
        Token = (Token)GetParent().GetParent();
    }

    public override void _Process(float delta)
    {
        selected = ClientVariables.SelectedTokens.Contains(Token);
        Update();
    }

    public override void _Draw()
    {
        if (selected)
        {
            Vector2 currentSize = Token.TokenSprite.GetRect().Size * Token.Scale;
            Rect2 rect = new Rect2(Token.Position, currentSize);
            DrawRect(rect, selectedColor, false, 3, true);

            if (Input.IsActionPressed("ui_mouse_click"))
            {
                Vector2 distanceToMove = Token.DragEndPosition - Token.DragStartPosition;
                Vector2 finalPosition = Token.TargetPosition + distanceToMove;
                Vector2 start = ToLocal(new Vector2(Token.TargetPosition.x + tileSize / 2, Token.TargetPosition.y + tileSize / 2));
                finalPosition = ToLocal(new Vector2(finalPosition.x + tileSize / 2, finalPosition.y + tileSize / 2));
                DrawLine(start, finalPosition, dragColor, 5, true);
            }
        }
    }
}
