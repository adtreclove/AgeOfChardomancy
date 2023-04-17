using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{
    [SerializeField]
    private int x, z;
    public int X { get { return x; } }
    public int Z { get { return z; } }

    //All coordinates add up to zero, you can alays derive each coordinate from the other two
    public int Y { get { return -X - Z; } }

    public HexCoordinates(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public static HexCoordinates FromOffsetCoordinates(int x, int z)
    {
        return new HexCoordinates(x - z / 2, z);
    }

    public static HexCoordinates FromPosition(Vector3 position)
    {
        //y is the mirror of x
        float x = position.x / (HexMetrics.innerRadius * 2f);
        float y = -x;

        //every two rows should be a shift of an entire unit to the left

        float offset = position.z / (HexMetrics.outerRadius * 3f);
        x -= offset;
        y -= offset;

        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(-x - y);

        if (iX + iY + iZ != 0)
        {
            //Debug.LogWarning("rounding error");
            //because of the rounding there can be rounding errors
            //to resolve it: discard the coordinate with the largest rounding error and reconstruct the coordinates 

            float dX = Mathf.Abs(x - iX);
            float dY = Mathf.Abs(y - iY);
            float dZ = Mathf.Abs(-x - y - iZ);

            if (dX > dY && dX > dZ)
            {
                iX = -iY - iZ;
            }
            else if (dZ > dY)
            {
                iZ = -iX - iY;
            }
        }

        return new HexCoordinates(iX, iZ);

    }


    //Override to return the coordinates on a single line
    public override string ToString()
    {
        return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
    }

    //Method to put the coordinates on seperate lines
    public string ToStringOnSeperateLines()
    {
        return X.ToString() + "\n" + Y.ToString() + "\n " + Z.ToString();
    }
}
