using UnityEngine;

public class HexMapCamera : MonoBehaviour
{

    private Transform swivel, stick;

    //value of 0 means that it's fully zoomed out
    //value of 1 means that it's fully zoomed in
    private float zoom = 1f;

    private float rotationAngle;

    [SerializeField] private float stickMinZoom, stickMaxZoom;
    [SerializeField] private float swivelMinZoom, swivelMaxZoom;

    //When zooming out, the move speed should be faster
    [SerializeField] private float moveSpeedMinZoom, moveSpeedMaxZoom;
    [SerializeField] private float rotationSpeed;

    [SerializeField] private HexGrid hexGrid;


    private void Awake()
    {
        swivel = transform.GetChild(0);
        stick = swivel.GetChild(0);
    }

    private void Update()
    {
        //if moving the mouse wheel, the zoom is adjusted
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");

        if (zoomDelta != 0f)
        {
            AdjustZoom(zoomDelta);
        }

        float rotationDelta = Input.GetAxis("Rotation");
        if(rotationDelta != 0f)
        {
            AdjustRotation(rotationDelta);
        }

        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");

        if (xDelta != 0f || zDelta != 0f)
        {
            AdjustPosition(xDelta, zDelta);
        }


    }


    private void AdjustZoom(float delta)
    {
        //clamp the zoom so that it's between 0 and 1
        zoom = Mathf.Clamp01(zoom + delta);

        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0f, 0f, distance);

        //interpolate to find the appropriate zoom angle
        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }

    private void AdjustPosition(float xDelta, float zDelta)
    {
        //Get the current position of the camera Rig and add the x and z deltas to it

        //Normalize the the delta vector to use it as direction
        Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;

        //damping factor to make the camera stop moving instantly when releasing a button
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) * damping * Time.deltaTime;

        Vector3 position = transform.localPosition;
        position += direction * distance;
        transform.localPosition = ClampPosition(position);
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        //To limit the moving out of the map
        //X position has a minimum of zero and a maximum defined by map size

        float xMax = (hexGrid.chunkCountX * HexMetrics.chunkSizeX - 1) * (2f * HexMetrics.innerRadius);
        position.x = Mathf.Clamp(position.x, 0f, xMax);

        //subtract half a cell size because we want to end the camera at the center of the last cell
        float zMax = (hexGrid.chunkCountZ * HexMetrics.chunkSizeZ - 0.5f) * (1.5f * HexMetrics.outerRadius);
        position.z = Mathf.Clamp(position.z, 0f, zMax);
        return position;
    }

    private void AdjustRotation(float delta)
    {
        rotationAngle += delta * rotationSpeed * Time.deltaTime;
        if (rotationAngle < 0f)
        {
            rotationAngle += 360f;
        }
        else if (rotationAngle >= 360f)
        {
            rotationAngle -= 360f;
        }
        transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
    }
}
