using UnityEngine;
using UnityEngine.InputSystem; // Nuevo sistema de Input

public class FixedCameraMovement3D : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 35f; // Velocidad de WASD

    [Header("Zoom")]
    public float zoomSpeed = 100f; // Velocidad de zoom físico
    public float minZoom = 5f;    // Distancia mínima de zoom
    public float maxZoom = 40f;   // Distancia máxima de zoom

    [Header("Límites del mapa")]
    public float minX = -300f;
    public float maxX = 300f;
    public float minZ = -300f;
    public float maxZ = 300f;

    [Header("Posición inicial")]
    public Vector3 startPosition = new Vector3(200f, 75f, 75f);

    private Vector3 initialDirection; // Dirección fija de la cámara

    void Start()
    {
        // Posición inicial de la cámara
        transform.position = startPosition;

        // Dirección fija de la cámara (mirando en diagonal hacia abajo)
        initialDirection = new Vector3(0f, -0.7f, 0.7f).normalized;
        transform.rotation = Quaternion.LookRotation(initialDirection, Vector3.up);
    }

    void Update()
    {
        // --- Movimiento con WASD ---
        Vector3 movement = Vector3.zero;

        if (Keyboard.current.wKey.isPressed)
            movement += Vector3.forward;
        if (Keyboard.current.sKey.isPressed)
            movement += Vector3.back;
        if (Keyboard.current.aKey.isPressed)
            movement += Vector3.left;
        if (Keyboard.current.dKey.isPressed)
            movement += Vector3.right;

        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);

        // --- Rotación fija ---
        transform.rotation = Quaternion.LookRotation(initialDirection, Vector3.up);

        // --- Zoom físico con la rueda del mouse ---
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 zoomMovement = initialDirection * scroll * zoomSpeed * Time.deltaTime;
            transform.position += zoomMovement;
        }

        // --- Limitar posición al mapa ---
        Vector3 clampedPos = transform.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, minX, maxX);
        clampedPos.z = Mathf.Clamp(clampedPos.z, minZ, maxZ);
        transform.position = clampedPos;
    }
}