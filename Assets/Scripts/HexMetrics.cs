using UnityEngine;
using UnityEngine.UIElements;

public static class HexMetrics
{

    public static Texture2D noiseSource;

    #region Hex Cells Settings
    //how to get the inner radius of a hexagon: 
    //1. take one of the six triangles
    //2. Inner Radius is equal to the height of this triangle 
    //3. Height = splitting the triangle into two right triangles & use pythagoras
    //4. Crazy ass formula ---> rounded: 0.886 + outer radius
    //Inverse radius because when making rivers between zig zack and straight lines there are pinched lines
    //with this vonversion factors we can convert to the right scale in HexMesh.TriangulateWithRiver
    public const float outerToInner = 0.866025404f;
    public const float innerToOuter = 1f / outerToInner;

    public const float outerRadius = 10f;
    public const float innerRadius = outerRadius * outerToInner;

    //if the solid factor increases, the flat cell centers will become larger
    public const float solidFactor = 0.8f;
    public const float blendFactor = 1f - solidFactor;


    #endregion

    #region Hex Cells Elevation Settings
    //Defines ow steep the difference is between the elevation levels
    public const float elevationStep = 3f;

    public const int terracesPerSlope = 2;
    public const int terraceSteps = terracesPerSlope * 2 + 1;
    public const float horizontalTerraceStepSize = 1f / terraceSteps;
    public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);

    //this offset ensures that the stream bed of the river is kept constant across cells
    public const float streamBedElevationOffset = -1.75f;

    public const float riverSurfaceElevationOffset = -0.5f;

    #endregion

    #region Hex Cells Pertubation Settings
    //Determines how much the cells will be pertubated)
    public const float cellPertubationStrength = 3f; /*3f;*/

    public const float noiseScale = 0.003f;

    //Scale for the elevation pertubation which applys a vertical pertubation per cell
    public const float elevationPerturbStrength = 1.5f;
    #endregion

    #region Hex Map Chunk Settings

    //larger chunks means that there are fewer but larger meshes -> fewer draw calls
    //Smaller chunks work better with frustum culling -> fewer triangles being drawn

    public const int chunkSizeX = 5, chunkSizeZ = 5;

    #endregion

    private static Vector3[] corners =
    {
        //define position of the six corners relative to the cell's center
        //note: there are pointed and flat top hexagons
        //here: flat top
        //start with a corner and add the rest clockwise 

        new Vector3(0f, 0f, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3 (0f, 0f, -outerRadius),
        new Vector3 (-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3 (-innerRadius, 0f, 0.5f *outerRadius),

        //the last corner is a duplicate of the first one, so there cant be a out of range exception
        new Vector3 (0f, 0f, outerRadius)
    };

    public static Vector3 GetFirstCorner(HexDirections direction)
    {
        return corners[(int)direction];
    }

    public static Vector3 GetSecondCorner(HexDirections direction)
    {
        return corners[(int)direction + 1];
    }

    public static Vector3 GetFirstSolidCorner(HexDirections direction)
    {
        //Get the corners of solid inner hexagons that should be one solid color
        return corners[(int)direction] * solidFactor;
    }

    public static Vector3 GetSecondSolidCorner(HexDirections direction)
    {
        return corners[(int)direction + 1] * solidFactor;
    }

    public static Vector3 GetBridge(HexDirections direction)
    {
        return (corners[(int)direction] + corners[(int)direction + 1]) * blendFactor;
    }

    public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
    {
        float h = step * horizontalTerraceStepSize;
        a.x += (b.x - a.x) * h;
        a.z += (b.z - a.z) * h;

        //idk crazy ass formula (interpolation between two values)
        float v = ((step + 1) / 2) * verticalTerraceStepSize;
        a.y += (b.y - a.y) * v;
        return a;
    }

    public static Color TerraceLerp(Color a, Color b, int step)
    {
        float h = step * horizontalTerraceStepSize;
        return Color.Lerp(a, b, h);
    }

    ///  <summary>
    /// Method to determine what kind of connection we're dealing with.
    /// </summary>
    /// <returns>Returns an HexEdgeType</returns>
    public static HexEdgeType GetEdgeType(int elevation1, int elevation2)
    {
        //if elevations are the same, there is a flat edge
        if (elevation1 == elevation2)
        {
            return HexEdgeType.Flat;
        }

        //if the level difference is 1 or -1, there is a slope
        int delta = elevation2 - elevation1;

        if (delta == 1 || delta == -1)
        {
            return HexEdgeType.Slope;
        }

        //else it is a cliff
        return HexEdgeType.Cliff;
    }

    public static Vector4 SampleNoise(Vector3 position)
    {
        return noiseSource.GetPixelBilinear(position.x * noiseScale, position.z * noiseScale);
    }

    public static Vector3 GetSolidEdgeMiddle(HexDirections direction)
    {
        return (corners[(int)direction] + corners[(int)direction + 1]) * (0.5f * solidFactor);
    }

    public static Vector3 Perturb(Vector3 position)
    {
        //Distort the regular "honeycomb" grid by perturbing each vertex individually 
        //this method takes an unperturbed point and returns a perturbed one

        //the y position is untouched: this way the center of the hex is staying flat

        Vector4 sample = SampleNoise(position);

        position.x += (sample.x * 2f - 1f) * cellPertubationStrength;

        position.z += (sample.z * 2f - 1f) * cellPertubationStrength;
        return position;
    }


}
