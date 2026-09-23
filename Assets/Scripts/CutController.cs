using UnityEngine;

/// <summary>
/// Cut Mode. Press C to pause and draw a line with the left mouse button.
/// The line becomes a cut plane that goes straight away from the camera.
/// Then click a piece (or press 1 / 2) to keep it; the other piece stays behind as a leftover.
/// Links: Player, Player Controller, Camera Rig, Level Manager, Cut Line, and three materials.
/// </summary>
public class CutController : MonoBehaviour
{
    [Header("Links")]
    public CuttablePiece player;
    public PlayerController playerController;
    public CameraRig cameraRig;
    public LevelManager levelManager;
    public LineRenderer cutLine;

    [Header("Materials")]
    public Material pieceAMaterial;   // preview color for piece 1 (blue)
    public Material pieceBMaterial;   // preview color for piece 2 (orange)
    public Material leftoverMaterial; // color of pieces left behind

    [Header("Rules")]
    public float minPieceVolume = 0.25f;
    public bool leftoversCanRotate = false;
    public float lineWidthPixels = 4f;

    public enum Phase { Playing, Drawing, Choosing }
    public Phase CurrentPhase { get; private set; } = Phase.Playing;

    Vector3 dragStart;
    bool dragging;
    readonly GameObject[] previews = new GameObject[2];
    readonly ConvexShape[] candidates = new ConvexShape[2];
    Renderer playerRenderer;
    Collider playerCollider;

    void Awake()
    {
        playerRenderer = player.GetComponent<Renderer>();
        playerCollider = player.GetComponent<Collider>();
        if (cutLine != null) cutLine.enabled = false;
    }

    void Update()
    {
        switch (CurrentPhase)
        {
            case Phase.Playing:
                if (Input.GetKeyDown(KeyCode.C)) EnterCutMode();
                break;

            case Phase.Drawing:
                if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Escape)) { ExitCutMode("Cut cancelled"); break; }
                HandleDrawing();
                break;

            case Phase.Choosing:
                if (Input.GetKeyDown(KeyCode.Alpha1)) Choose(0);
                else if (Input.GetKeyDown(KeyCode.Alpha2)) Choose(1);
                else if (Input.GetKeyDown(KeyCode.Escape)) CancelChoice();
                else if (Input.GetMouseButtonDown(0)) ClickToChoose();
                break;
        }
    }

    void EnterCutMode()
    {
        if (!levelManager.HasCutsLeft)
        {
            levelManager.ShowMessage("No cuts left. Press R to restart the level.");
            return;
        }
        CurrentPhase = Phase.Drawing;
        playerController.StopMoving();
        playerController.inputEnabled = false;
        Time.timeScale = 0f;
        levelManager.ShowMessage("Cut Mode: drag a line across your body");
    }

    void ExitCutMode(string message)
    {
        CurrentPhase = Phase.Playing;
        Time.timeScale = 1f;
        playerController.inputEnabled = true;
        cameraRig.AllowInput = true;
        dragging = false;
        if (cutLine != null) cutLine.enabled = false;
        if (message != null) levelManager.ShowMessage(message);
    }

    void HandleDrawing()
    {
        if (Input.GetMouseButtonDown(0)) { dragStart = Input.mousePosition; dragging = true; }
        if (!dragging) return;

        UpdateLine(dragStart, Input.mousePosition);

        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
            if ((Input.mousePosition - dragStart).magnitude > 12f) TryCut(dragStart, Input.mousePosition);
            else if (cutLine != null) cutLine.enabled = false;
        }
    }

    void UpdateLine(Vector3 a, Vector3 b)
    {
        if (cutLine == null) return;
        Camera cam = cameraRig.cam;
        float depth = cam.nearClipPlane + 0.5f;
        cutLine.enabled = true;
        cutLine.useWorldSpace = true;
        cutLine.positionCount = 2;
        cutLine.SetPosition(0, cam.ScreenToWorldPoint(new Vector3(a.x, a.y, depth)));
        cutLine.SetPosition(1, cam.ScreenToWorldPoint(new Vector3(b.x, b.y, depth)));
        float unitsPerPixel = cam.orthographic
            ? 2f * cam.orthographicSize / Screen.height
            : 2f * depth * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) / Screen.height;
        cutLine.startWidth = cutLine.endWidth = lineWidthPixels * unitsPerPixel;
    }

    void TryCut(Vector3 a, Vector3 b)
    {
        Camera cam = cameraRig.cam;
        Ray r1 = cam.ScreenPointToRay(a);
        Ray r2 = cam.ScreenPointToRay(b);

        // Three points: both ends of the line, and one point further along the first ray.
        // They define a plane containing the drawn line and the viewing direction.
        Vector3 p1 = r1.GetPoint(1f), p2 = r2.GetPoint(1f), p3 = r1.GetPoint(5f);
        Vector3 normal = Vector3.Cross(p2 - p1, p3 - p1);
        if (normal.sqrMagnitude < 1e-10f) { cutLine.enabled = false; return; }
        normal.Normalize();
        float d = Vector3.Dot(normal, p1);

        if (!player.WorldShape().Split(normal, d, out ConvexShape front, out ConvexShape back))
        {
            levelManager.ShowMessage("That line misses your body. Draw it across the blue shape.");
            cutLine.enabled = false;
            return;
        }
        if (front.Volume() < minPieceVolume || back.Volume() < minPieceVolume)
        {
            levelManager.ShowMessage("One piece would be too small. Try a different line.");
            cutLine.enabled = false;
            return;
        }

        candidates[0] = front;
        candidates[1] = back;
        previews[0] = CreatePreview(front, pieceAMaterial, "Piece 1 (preview)");
        previews[1] = CreatePreview(back, pieceBMaterial, "Piece 2 (preview)");
        playerRenderer.enabled = false;
        playerCollider.enabled = false;
        cutLine.enabled = false;
        cameraRig.AllowInput = false;
        CurrentPhase = Phase.Choosing;
        levelManager.ShowMessage("Click the piece to keep, or press 1 (blue) / 2 (orange). Esc cancels.");
    }

    GameObject CreatePreview(ConvexShape worldShape, Material mat, string name)
    {
        var go = new GameObject(name);
        Mesh mesh = worldShape.BuildMesh();
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        var col = go.AddComponent<MeshCollider>();
        col.convex = true;
        col.isTrigger = true; // only used for clicking
        col.sharedMesh = mesh;
        return go;
    }

    void ClickToChoose()
    {
        Ray ray = cameraRig.cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, ~0, QueryTriggerInteraction.Collide)) return;
        for (int i = 0; i < 2; i++)
            if (previews[i] != null && hit.collider.gameObject == previews[i]) { Choose(i); return; }
    }

    void Choose(int index)
    {
        ConvexShape keep = candidates[index];
        ConvexShape leave = candidates[1 - index];

        player.SetWorldShape(keep);
        playerRenderer.enabled = true;
        playerCollider.enabled = true;
        SpawnLeftover(leave);
        DestroyPreviews();
        levelManager.UseCut();
        ExitCutMode(null);
    }

    void CancelChoice()
    {
        DestroyPreviews();
        playerRenderer.enabled = true;
        playerCollider.enabled = true;
        ExitCutMode("Cut cancelled");
    }

    void SpawnLeftover(ConvexShape shape)
    {
        var go = new GameObject("Leftover Piece");
        var piece = go.AddComponent<CuttablePiece>(); // also adds MeshFilter, MeshRenderer, MeshCollider, Rigidbody
        piece.density = player.density;
        piece.SetWorldShape(shape);
        go.GetComponent<MeshRenderer>().sharedMaterial = leftoverMaterial;
        go.GetComponent<MeshCollider>().sharedMaterial = playerCollider.sharedMaterial;

        var body = go.GetComponent<Rigidbody>();
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        if (!leftoversCanRotate) body.constraints = RigidbodyConstraints.FreezeRotation;

        go.AddComponent<Fadeable>();
    }

    void DestroyPreviews()
    {
        for (int i = 0; i < 2; i++)
        {
            if (previews[i] == null) continue;
            Destroy(previews[i].GetComponent<MeshFilter>().sharedMesh);
            Destroy(previews[i]);
            previews[i] = null;
        }
    }
}
