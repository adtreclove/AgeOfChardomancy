using UnityEngine;
using UnityEngine.UIElements;

public class Direction : MonoBehaviour
{
    //ja ik die klasse hier ist unnötig, ich hab das falsch aufgebaut xD
}
public enum HexDirections
{
    NE,
    E,
    SE,
    SW,
    W,
    NW
}

public static class HexDirectionExtensions
{
    //Extension Method 
    public static HexDirections Opposite(this HexDirections direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }

    public static HexDirections Previous(this HexDirections direction)
    {
        return direction == HexDirections.NE ? HexDirections.NW : (direction - 1);
    }

    public static HexDirections Next(this HexDirections direction)
    {
        return direction == HexDirections.NW ? HexDirections.NE : (direction + 1);
    }

    //to distinguish between the two possible orientations of the river 
    public static HexDirections Previous2(this HexDirections direction)
    {
        direction -= 2;
        return direction >= HexDirections.NE ? direction : (direction + 6);
    }

    public static HexDirections Next2(this HexDirections direction)
    {
        direction += 2;
        return direction <= HexDirections.NW ? direction : (direction - 6);
    }
}
