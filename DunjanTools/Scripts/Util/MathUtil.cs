using Godot;
using System;

public class MathUtil
{
    public static float Lerp(float firstFloat, float secondFloat, float by)
    {
        return firstFloat * (1 - by) + secondFloat * by;
    }

    public static Vector2 Lerp(Vector2 firstVector, Vector2 secondVector, float by)
    {
        float retX = Lerp(firstVector.x, secondVector.x, by);
        float retY = Lerp(firstVector.y, secondVector.y, by);
        return new Vector2(retX, retY);
    }

    public static float GetDistance(Vector2 start, Vector2 end)
    {
        float distanceX = Math.Abs(start.x - end.x);
        float distanceY = Math.Abs(start.y - end.y);
        float distance = Math.Max(distanceX, distanceY) + Math.Min(distanceX, distanceY)/2;
        return distance;
    }
}
