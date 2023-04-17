using UnityEngine;

public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;
    public RectTransform uiRect;
  
    public HexGridChunk chunk;

    [SerializeField] private HexCell[] neighbours;

    //To prevent the possible skip of computation for the first time the elevation is set to zero (default)
    //int.MinValue -> The lowest value an integer can have  min = -2^31, max = 2^31
    private int elevation = int.MinValue;

    //Adjusts the cell's vertical position whenever its elevation is edited
    public int Elevation
    {
        get { return elevation; }
        set
        {
            if (elevation == value)
            { return; }
            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;
            position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
            transform.localPosition = position;

            //updating the ui so it elevates too
            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = -position.y;
            uiRect.localPosition = uiPosition;

            //make sure rivers can only flow downhill
            if (hasOutgoingRiver && elevation < GetNeighbour(outgoingRiver).elevation)
            { RemoveOutgoingRiver(); }
            if (hasIncomingRiver && elevation > GetNeighbour(incomingRiver).elevation)
            { RemoveIncomingRiver(); }

            Refresh();
        }
    }


    private Color color;
    public Color Color
    {
        get { return color; }
        set
        {
            if (color == value)
            { return; }
            color = value;
            Refresh();
        }
    }

    #region River
    //for river cells - a cell can have a beginning of a river (outgoing),
    //the end of a river (incoming) or a river going through (both)
    //also need to know the direction

    private bool hasIncomingRiver, hasOutgoingRiver;
    private HexDirections incomingRiver, outgoingRiver;

    //Getter properties to access it
    public bool HasIncomingRiver { get { return hasIncomingRiver; } }
    public bool HasOutgoingRiver { get { return hasOutgoingRiver; } }

    public HexDirections IncomingRiver { get { return incomingRiver; } }
    public HexDirections OutgoingRiver { get { return outgoingRiver; } }

    public bool HasRiver { get { return hasIncomingRiver || hasOutgoingRiver; } }

    public bool HasRiverBeginOrEnd { get { return hasIncomingRiver != hasOutgoingRiver; } }


    //property to retrieve the vertical position of its stream bed
    public float StreamBedY { get { return (elevation + HexMetrics.streamBedElevationOffset) * HexMetrics.elevationStep; } }

    public float RiverSurfaceY
    {
        get
        {
            return
                (elevation + HexMetrics.riverSurfaceElevationOffset) * HexMetrics.elevationStep;
        }
    }
    #endregion




    public bool HasRiverThroughEdge(HexDirections direction)
    {
        //to know if a river is flowing through a certain edge
        return hasIncomingRiver && incomingRiver == direction || hasOutgoingRiver && outgoingRiver == direction;
    }

    public void RemoveOutgoingRiver()
    {
        //when removing the begin of an outgoing river 
        //also remove the neighbour with an incoming river 
        if (!hasOutgoingRiver)
        {
            return;
        }
        hasOutgoingRiver = false;

        //only need to refresh the cell itself because other cells are not affected by removing a river
        RefreshSelfOnly();

        HexCell neighbour = GetNeighbour(outgoingRiver);
        neighbour.hasIncomingRiver = false;
        neighbour.RefreshSelfOnly();

    }

    public void RemoveIncomingRiver()
    {
        //when removing the begin of an incoming river 
        //also remove the neighbour with an outgoing river 
        if (!hasIncomingRiver)
        {
            return;
        }
        hasIncomingRiver = false;

        //only need to refresh the cell itself because other cells are not affected by removing a river
        RefreshSelfOnly();

        HexCell neighbour = GetNeighbour(incomingRiver);
        neighbour.hasOutgoingRiver = false;
        neighbour.RefreshSelfOnly();

    }

    public void RemoveRiver()
    {
        RemoveOutgoingRiver();
        RemoveIncomingRiver();
    }

    public void SetOutgoingRiver(HexDirections direction)
    {

        if (hasOutgoingRiver && outgoingRiver == direction)
        {
            return;
        }

        //make sure rivers cant flow if there is no neighbour
        //or if the elevation of the neighbour is higher

        HexCell neighbour = GetNeighbour(direction);
        if (!neighbour || elevation < neighbour.elevation)
        {
            return;
        }

        //clear previous outgoing river 
        //and remove incoming river if it overlaps with the new outgoing river

        RemoveOutgoingRiver();
        if (hasIncomingRiver && incomingRiver == direction)
        {
            RemoveIncomingRiver();
        }

        hasOutgoingRiver = true;
        outgoingRiver = direction;
        RefreshSelfOnly();

        neighbour.RemoveIncomingRiver();
        neighbour.hasIncomingRiver = true;
        neighbour.incomingRiver = direction.Opposite();
        neighbour.RefreshSelfOnly();

    }


    public Vector3 Position { get { return transform.localPosition; } }


    ///  <summary>
    /// Method to get a cell's neighbour in one direction
    /// </summary>
    /// <returns>Returns a HexCell</returns>
    public HexCell GetNeighbour(HexDirections direction)
    {
        return neighbours[(int)direction];
    }


    ///  <summary>
    /// Method to set a neighbour and also the neighbour in the opposite direction.
    /// </summary>
    /// <returns>Returns void</returns>
    public void SetNeighbour(HexDirections direction, HexCell cell)
    {
        neighbours[(int)direction] = cell;
        cell.neighbours[(int)direction.Opposite()] = this;
    }


    ///  <summary>
    /// Method to get a cell's edge type in a certain direction.
    /// </summary>
    /// <returns>Returns an HexEdgeType</returns>
    public HexEdgeType GetEdgeType(HexDirections direction)
    {
        return HexMetrics.GetEdgeType(Elevation, neighbours[(int)direction].elevation);
    }


    ///  <summary>
    /// Method to determine the slope between any two cells.
    /// </summary>
    /// <returns>Returns an HexEdgeType</returns>
    public HexEdgeType GetEdgeType(HexCell otherCell)
    {
        return HexMetrics.GetEdgeType(Elevation, otherCell.elevation);
    }

    private void Refresh()
    {
        //only refresh the chunk if it has been assigned
        if (chunk)
        {
            chunk.Refresh();
        }

        //also refresh the chunks of all neighbours as well, if they're different
        for (int i = 0; i < neighbours.Length; i++)
        {
            HexCell neighbour = neighbours[i];

            if (neighbour != null && neighbour.chunk != chunk)
            {
                neighbour.chunk.Refresh();
            }
        }
    }

    private void RefreshSelfOnly()
    {
        chunk.Refresh();
    }



}
