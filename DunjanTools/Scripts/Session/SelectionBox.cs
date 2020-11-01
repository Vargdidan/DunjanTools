using Godot;
using System;

public class SelectionBox : Node2D
{
    private Boolean isHolding = false;
    private Color selectedColor = new Color(1, 0.2f, 0.2f, 0.8f);
    private Vector2 startHoldPos = new Vector2();
    private Vector2 endHoldPos = new Vector2();
    public ClientVariables ClientVariables { get; set; }
    public override void _Ready()
    {
        ClientVariables = (ClientVariables)GetNode("/root/ClientVariables");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (Input.IsActionPressed("ui_mouse_click"))
        {
            if (!isHolding)
            {
                startHoldPos = GetGlobalMousePosition();
                isHolding = true;
            }
            else
            {
                endHoldPos = GetGlobalMousePosition();
                
                if (Input.IsActionPressed("ui_shift") && !Input.IsActionPressed("ui_alt"))
                {
                    float distanceX = Math.Abs(endHoldPos.x - startHoldPos.x);
                    float distanceY = Math.Abs(endHoldPos.y - startHoldPos.y);

                    //Important that the origin is set at the rectangle's left upper corner for collision detection to work
                    Vector2 origin = new Vector2(Math.Min(startHoldPos.x, endHoldPos.x), Math.Min(startHoldPos.y, endHoldPos.y));
                    ClientVariables.SelectionBox = new Rect2(origin, distanceX, distanceY);
                    Update();
                }
            }
        }

        if (Input.IsActionJustReleased("ui_mouse_click"))
        {
            isHolding = false;
            Update();
        }
    }

    public override void _Draw()
    {
        if (isHolding)
        {
            DrawRect(ClientVariables.SelectionBox, selectedColor, false, 2, true);
        }
    }
}
