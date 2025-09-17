using UnityEngine;
using UnityEngine.UI;

public class RandomShapeSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject cubePrefab;
    public GameObject spherePrefab;
    public GameObject capsulePrefab;

    [Header("Cajitas de spawn")]
    public Transform[] spawnPoints;

    [Header("Probabilidades")]
    [Range(0, 100)]
    public int cubeProbability = 70;
    public int sphereProbability = 20; // cápsula = resto

    [Header("UI")]
    public Button deleteButton;
    public Button cancelButton;
    public GameObject intercambioText; // Cartel "Modo intercambio"

    [Header("Highlight")]
    public Material highlightMaterial;

    private Transform hoveredShape;
    private Transform clickedShape;      // Figura seleccionada para botones
    private Transform selectedShape;     // Figura en modo intercambio
    private Renderer moveRenderer;
    private Material moveOriginalMat;
    private bool isMoving = false;

    private Vector3 spawnOffset = new Vector3(0, 0.5f, 0);

    void Start()
    {
        deleteButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);

        if (intercambioText != null)
            intercambioText.SetActive(false);

        deleteButton.onClick.AddListener(DeleteTargetShape);
        cancelButton.onClick.AddListener(CancelSelection);
    }

    void Update()
    {
        HandleMouseHover();
        HandleMouseClick();
    }

    // --- SPAWN ---
    public void SpawnRandomShape()
    {
        int freeIndex = -1;
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            bool occupied = false;
            foreach (Transform child in spawnPoints[i])
            {
                if (child.CompareTag("Shape"))
                {
                    occupied = true;
                    break;
                }
            }
            if (!occupied)
            {
                freeIndex = i;
                break;
            }
        }

        if (freeIndex == -1)
        {
            Debug.Log("⚠ Todas las cajitas están ocupadas!");
            return;
        }

        int rand = Random.Range(0, 100);
        GameObject prefabToSpawn;
        if (rand < cubeProbability)
            prefabToSpawn = cubePrefab;
        else if (rand < cubeProbability + sphereProbability)
            prefabToSpawn = spherePrefab;
        else
            prefabToSpawn = capsulePrefab;

        GameObject spawned = Instantiate(prefabToSpawn);
        Renderer rend = spawned.GetComponent<Renderer>();
        if (rend != null)
        {
            // Cada figura tiene su propio material para no afectar otras
            rend.material = new Material(rend.material);
        }

        spawned.transform.position = spawnPoints[freeIndex].position + spawnOffset;
        spawned.tag = "Shape";

        if (spawned.GetComponent<Collider>() == null)
            spawned.AddComponent<BoxCollider>();

        spawned.transform.SetParent(spawnPoints[freeIndex]);
        spawned.transform.localPosition = spawnOffset;
    }

    // --- HOVER ---
    private void HandleMouseHover()
    {
        if (isMoving) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Transform hitTransform = hit.transform;

            // ⚡ Solo iluminar figuras con tag "Shape" que sean hijas de alguna caja
            if (hitTransform.CompareTag("Shape") && IsChildOfAnyBox(hitTransform))
            {
                if (hoveredShape != null && hoveredShape != hitTransform)
                    ClearHover();

                hoveredShape = hitTransform;
                Renderer rend = hoveredShape.GetComponent<Renderer>();
                if (rend != null && hoveredShape != clickedShape)
                    rend.material = highlightMaterial;
            }
            else
            {
                ClearHover();
            }
        }
        else
        {
            ClearHover();
        }
    }

    private void ClearHover()
    {
        if (hoveredShape != null && hoveredShape != clickedShape)
        {
            Renderer rend = hoveredShape.GetComponent<Renderer>();
            if (rend != null && moveOriginalMat != null)
                rend.material = moveOriginalMat;
        }
        hoveredShape = null;
    }

    // --- CLICK ---
    private void HandleMouseClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Transform hitTransform = hit.transform;

            if (hitTransform.CompareTag("Shape") && IsChildOfAnyBox(hitTransform))
            {
                if (selectedShape != null)
                {
                    // Ya en modo intercambio
                    foreach (Transform box in spawnPoints)
                    {
                        if (hitTransform == box || hitTransform.IsChildOf(box))
                        {
                            MoveShapeToBox(box);
                            return;
                        }
                    }
                }

                if (clickedShape != hitTransform)
                {
                    // Primer click: mostrar botones
                    SelectShape(hitTransform);
                }
                else
                {
                    // Segundo click: modo intercambio
                    StartExchangeMode(hitTransform);
                }
            }
            else if (isMoving)
            {
                // Mover figura a caja vacía
                foreach (Transform box in spawnPoints)
                {
                    if (hitTransform == box || hitTransform.IsChildOf(box))
                    {
                        MoveShapeToBox(box);
                        return;
                    }
                }
            }
        }
    }

    private void SelectShape(Transform shape)
    {
        if (!shape.CompareTag("Shape") || !IsChildOfAnyBox(shape)) return;

        ClearDeleteSelection();

        clickedShape = shape;
        Renderer rend = shape.GetComponent<Renderer>();
        if (rend != null)
        {
            moveOriginalMat = rend.material;
            rend.material = highlightMaterial;
        }

        deleteButton.gameObject.SetActive(true);
        cancelButton.gameObject.SetActive(true);

        if (intercambioText != null)
            intercambioText.SetActive(false);
    }

    private void StartExchangeMode(Transform shape)
    {
        if (!shape.CompareTag("Shape") || !IsChildOfAnyBox(shape)) return;

        selectedShape = shape;
        isMoving = true;

        moveRenderer = shape.GetComponent<Renderer>();
        if (moveRenderer != null)
        {
            moveOriginalMat = moveRenderer.material;
            moveRenderer.material = highlightMaterial;
        }

        clickedShape = null;
        deleteButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);

        if (intercambioText != null)
            intercambioText.SetActive(true);
    }

    private void MoveShapeToBox(Transform targetBox)
{
    if (selectedShape == null) return;

    Transform existingShape = null;
    foreach (Transform child in targetBox)
    {
        if (child.CompareTag("Shape"))
        {
            existingShape = child;
            break;
        }
    }

    Transform originalParent = selectedShape.parent;
    Vector3 originalPosition = selectedShape.localPosition;

    if (existingShape != null)
    {
        // Intercambio
        existingShape.SetParent(originalParent);
        existingShape.localPosition = originalPosition;
    }

    // Mover la figura seleccionada
    selectedShape.SetParent(targetBox);
    selectedShape.localPosition = spawnOffset;

    ClearMoveSelection();

    // ⚡ Seleccionar la figura que quedó en la caja, ya sea la nueva o la intercambiada
    if (targetBox.childCount > 0)
        clickedShape = targetBox.GetChild(0);
    else
        clickedShape = selectedShape; // caja vacía

    Renderer rend = clickedShape.GetComponent<Renderer>();
    if (rend != null)
        rend.material = highlightMaterial;

    deleteButton.gameObject.SetActive(true);
    cancelButton.gameObject.SetActive(true);
}


    private void DeleteTargetShape()
    {
        if (clickedShape != null && clickedShape.CompareTag("Shape") && IsChildOfAnyBox(clickedShape))
        {
            Renderer rend = clickedShape.GetComponent<Renderer>();
            if (rend != null && moveOriginalMat != null)
                rend.material = moveOriginalMat;

            GameObject shapeToDestroy = clickedShape.gameObject;
            ClearDeleteSelection();
            Destroy(shapeToDestroy);
        }
    }

    private void CancelSelection()
    {
        ClearDeleteSelection();
        ClearMoveSelection();
    }

    private void ClearDeleteSelection()
    {
        if (clickedShape != null)
        {
            Renderer rend = clickedShape.GetComponent<Renderer>();
            if (rend != null && moveOriginalMat != null)
                rend.material = moveOriginalMat;
        }

        clickedShape = null;
        deleteButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);

        if (!isMoving && intercambioText != null)
            intercambioText.SetActive(false);
    }

    private void ClearMoveSelection()
    {
        if (moveRenderer != null && moveOriginalMat != null)
            moveRenderer.material = moveOriginalMat;

        selectedShape = null;
        moveRenderer = null;
        isMoving = false;

        if (intercambioText != null)
            intercambioText.SetActive(false);
    }

    // --- Helper ---
    private bool IsChildOfAnyBox(Transform shape)
    {
        foreach (Transform box in spawnPoints)
        {
            if (shape.IsChildOf(box))
                return true;
        }
        return false;
    }
}
