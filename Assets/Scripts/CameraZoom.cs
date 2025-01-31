using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    private float zoom;
    private float zoomMultiplier = 4f;
    private float minZoom = 2f;
    private float maxZoom = 8f;
    private float velocity = 0f;
    private float smoothTime = 0.25f;

    [SerializeField] private Camera cam;
    [SerializeField] private Transform player; // Referencia al transform del jugador

    public Transform Player
    {
        get { return player; }
        set { player = value; }
    }

    private void Start()
    {
        zoom = cam.orthographicSize;
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        zoom -= scroll * zoomMultiplier;
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, zoom, ref velocity, smoothTime);

        // Ajustar la posición de la cámara según la posición del jugador
        if (player != null)
        {
            transform.position = new Vector3(player.position.x, player.position.y, transform.position.z);
        }
    }
}
