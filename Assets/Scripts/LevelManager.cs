using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One per level scene. Holds the level's rules (cut limit, title, hint),
/// restarts with R or when the player falls, and loads the next scene.
/// Links: Player, HUD (optional).
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Level info")]
    public string levelTitle = "Cut to fit";
    [TextArea(2, 4)] public string levelHint = "";

    [Header("Rules")]
    public int cutLimit = 2;
    public float fallY = -12f;

    [Header("Links")]
    public CuttablePiece player;
    public HUD hud;

    public int CutsLeft { get; private set; }
    public bool HasCutsLeft => CutsLeft > 0;

    bool loading;

    void Awake()
    {
        Time.timeScale = 1f;
        CutsLeft = cutLimit;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) Restart();
        if (player != null && player.transform.position.y < fallY) Restart();
    }

    public void UseCut() => CutsLeft = Mathf.Max(0, CutsLeft - 1);

    public void Restart()
    {
        if (loading) return;
        loading = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void CompleteLevel()
    {
        if (loading) return;
        loading = true;
        Time.timeScale = 1f;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next >= SceneManager.sceneCountInBuildSettings) next = 0; // loop back to Level 1
        SceneManager.LoadScene(next);
    }

    public void ShowMessage(string message)
    {
        if (hud != null) hud.ShowMessage(message);
        else Debug.Log(message);
    }
}
