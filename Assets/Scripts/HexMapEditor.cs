using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
    public HexGrid hexGrid;
    public Color[] colors;

    private Color activeColor;
    private Camera cam;

    private int activeElevation;

    private bool applyColor;
    private bool applyElevation = true;

    private int brushSize;

    private OptionalToggle riverMode;

    private enum OptionalToggle
    {
        Ignore,
        Yes,
        No
    }

    //Support from dragging from one cell to another
    //it's important to know whether there is a valid drag and its direction
    //to detect a drag, we must remember the previous Cell
    private bool isDrag;
    private HexDirections dragDirection;
    private HexCell previousCell;

    private void Awake()
    {
        cam = Camera.main;
        SelectColor(0);
    }

    private void Update()
    {
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }
        else
        {
            //if there is no interaction with the map or noone is dragging, set it to null
            previousCell = null;
        }


    }

    private void HandleInput()
    {
        //Shoot a ray from mouse position
        //if it hits, it triggers the method touchCell()
        Ray inputRay = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(inputRay, out hit))
        {
            HexCell currentCell = hexGrid.GetCell(hit.point);
            if (previousCell && previousCell != currentCell)
            {
                ValidateDrag(currentCell);
            }
            else
            {
                isDrag = false;
            }
            EditCells(currentCell);
            previousCell = currentCell;
        }
        else
        {
            previousCell = null;
        }
    }

    public void SelectColor(int index)
    {
        applyColor = index >= 0;
        if (applyColor)
        {
            activeColor = colors[index];
        }
    }

    private void EditCell(HexCell cell)
    {
        if (cell)
        {
            if (applyColor)
            {
                cell.Color = activeColor;
            }
            if (applyElevation)
            {
                cell.Elevation = activeElevation;
            }
            if (riverMode == OptionalToggle.No)
            {
                cell.RemoveRiver();
            }
            else if (isDrag && riverMode == OptionalToggle.Yes)
            {
                HexCell otherCell = cell.GetNeighbour(dragDirection.Opposite());
                if (otherCell)
                {
                    otherCell.SetOutgoingRiver(dragDirection);

                }
            }
        }
    }

    private void EditCells(HexCell center)
    {
        //the brush size defines the radius of the effect of our edit 
        //At radius 0, just the center cell is affected
        //at radius 2 it includes the neighbours of it's direct neighbours as well ...

        //1. get x and z coordinates of the center
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;

        //2. find the minimum Z coordinate by subtracting the radius -> row zero
        //starting at that row, loop until the row at the center is covered
        for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
        {
            //the first cell of the bottom row has the same X coordinate as the center cell -> It decreases as the row number increases
            //the last cell always has an X coordinate equal to the center's plus the radius
            for (int x = centerX - r; x <= centerX + brushSize; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
        {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
    }

    public void SetElevation(float elevation)
    {
        activeElevation = (int)elevation;
    }

    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    public void SetBrushSize(float size)
    {
        brushSize = (int)size;
    }

    public void ShowUI(bool visible)
    {
        hexGrid.ShowUI(visible);
    }

    public void SetRiverMode(int mode)
    {
        riverMode = (OptionalToggle)mode;
    }

    public void ValidateDrag(HexCell currentCell)
    {
        for (dragDirection = HexDirections.NE; dragDirection <= HexDirections.NW; dragDirection++)
        {
            if (previousCell.GetNeighbour(dragDirection) == currentCell)
            {
                isDrag = true;
                return;
            }
        }
        isDrag = false;
    }
}
