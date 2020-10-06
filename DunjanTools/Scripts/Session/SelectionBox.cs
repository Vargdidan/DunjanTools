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
                    float distanceX = endHoldPos.x - startHoldPos.x;
                    float distanceY = endHoldPos.y - startHoldPos.y;
                    ClientVariables.SelectionBox = new Rect2(startHoldPos, distanceX, distanceY);
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
