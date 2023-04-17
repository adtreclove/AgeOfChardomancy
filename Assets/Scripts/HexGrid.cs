using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
    public HexCell cellPrefab;
    public Texture2D noiseSource;
    public HexGridChunk chunkPrefab;
    [SerializeField] private Color defaultColor = Color.white;

    //Canvas
    public Text cellLabelPrefab;

    //store cells & chunks in an array to access it later
    private HexCell[] cells;
    private HexGridChunk[] chunks;

    public int chunkCountX = 4, chunkCountZ = 3;

    //width & height
    private int cellCountX;
    private int cellCountZ;


    private void Awake()
    {
        HexMetrics.noiseSource = noiseSource;

        cellCountX = chunkCountX * HexMetrics.chunkSizeX;
        cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;

        CreateChunks();
        CreateCells();

    }

    private void OnEnable()
    {
        HexMetrics.noiseSource = noiseSource;
    }

    private void CreateCell(int x, int z, int i)
    {
        Vector3 position;

        //The distance between the middle of each hexagons to the next one in X direction is equal to 2 * inner Radius 
        //!! Hexagon rows are not directly above each other - Each row has a offset along the x axis by the inner radius 
        //--> add half of z to x before multiplying it 
        //--> also subtract z/2 to make the grid spacing in a ractangular area 

        //The distance to the next row of cells shoud be 1.5 * the outer radius 

        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);

        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.Color = defaultColor;

        //Connect cells from East to West
        //Because every cell in the row except for the first one does have a west neighbour
        if (x > 0)
        {
            cell.SetNeighbour(HexDirections.W, cells[i - 1]);
        }
        if (z > 0)
        {
            //First the even rows
            //SE Neighbour

            // z & 1 -> & is the bitwise AND operator
            //it performs the same logic but on each individual pair of bits of its operands 
            //both bits of a pair need to be 1 for the result to be 1
            if ((z & 1) == 0)
            {
                cell.SetNeighbour(HexDirections.SE, cells[i - cellCountX]);

                if (x > 0)
                {
                    //SW Neighbour
                    //except for the first cell in each row because it doesnt have one 
                    cell.SetNeighbour(HexDirections.SW, cells[i - cellCountX - 1]);
                }
            }
            //odd rows follow the same logic but mirrored
            else
            {
                cell.SetNeighbour(HexDirections.SW, cells[i - cellCountX]);

                if (x < cellCountX - 1)
                {
                    cell.SetNeighbour(HexDirections.SE, cells[i - cellCountX + 1]);
                }
            }
        }


        //Show coordinates 
        //instantiate new text prefab on the coordinate of the object and show its coordinate
        Text label = Instantiate<Text>(cellLabelPrefab);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
        label.text = cell.coordinates.ToStringOnSeperateLines();

        cell.uiRect = label.rectTransform;
        cell.Elevation = 0;

        AddCellToChunk(x, z, cell);

    }

    public HexCell GetCell(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);

        //Convert the cell coodrinates to the appropriate array index
        //Grab the cell, change color and triangulate again

        int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;



        return cells[index];

    }

    public HexCell GetCell(HexCoordinates coordinates)
    {
        int z = coordinates.Z;

        if (z < 0 || z >= cellCountZ)
        {
            return null;
        }

        int x = coordinates.X + z / 2;

        if (x < 0 || x >= cellCountX)
        {
            return null;
        }

        return cells[x + z * cellCountX];
    }

    private void CreateCells()
    {
        cells = new HexCell[cellCountZ * cellCountX];

        for (int z = 0, i = 0; z < cellCountZ; z++)
        {
            for (int x = 0; x < cellCountX; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    private void CreateChunks()
    {
        chunks = new HexGridChunk[chunkCountX * chunkCountZ];

        for (int z = 0, i = 0; z < chunkCountZ; z++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                chunk.transform.SetParent(transform);
            }
        }
    }

    private void AddCellToChunk(int x, int z, HexCell cell)
    {
        //Add each cell to the correct chunk
        //Find the correct chunk via integer divisions of x and z by the chunk size
        //Determine the cell's index local to it's chunk

        int chunkX = x / HexMetrics.chunkSizeX;
        int chunkZ = z / HexMetrics.chunkSizeZ;
        HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

        int localX = x - chunkX * HexMetrics.chunkSizeX;
        int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
        chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
    }

    public void ShowUI(bool visible)
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].ShowUI(visible);
        }
    }


}

