using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// put this on the Canvas, shows level name, hint, cuts left, size, mass, view mode, and short messages
public class HUD : MonoBehaviour
{
    [Header("Texts")]
    public TMP_Text levelText;
    public TMP_Text statsText;
    public TMP_Text modeText;
    public TMP_Text messageText;

    [Header("Links")]
    public LevelManager levelManager;
    public CuttablePiece player;
    public CameraRig cameraRig;
    public CutController cutController;

    public float messageDuration = 2.5f;
    float messageTimer;

    void Awake()
    {
        if (messageText != null) messageText.text = "";
    }

    void Start()
    {
        int number = SceneManager.GetActiveScene().buildIndex + 1;
        levelText.text = $"Level {number}: {levelManager.levelTitle}\n<size=70%>{levelManager.levelHint}</size>";
    }

    void Update()
    {
        Vector3 s = player.Size;
        statsText.text = $"Cuts left: {levelManager.CutsLeft} / {levelManager.cutLimit}    " +
                         $"Size: {s.x:0.0} x {s.y:0.0} x {s.z:0.0}    Mass: {player.Volume:0.0}";

        if (cutController.CurrentPhase == CutController.Phase.Drawing) modeText.text = "Cut Mode\n<size=70%>" + cameraRig.ViewName + "</size>";
        else if (cutController.CurrentPhase == CutController.Phase.Choosing) modeText.text = "Choose a piece\n<size=70%>1 = blue, 2 = orange</size>";
        else modeText.text = cameraRig.Is2D ? "2D\n<size=70%>" + cameraRig.ViewName + "</size>" : "3D\n<size=70%>Free camera</size>";

        if (messageTimer > 0f)
        {
            messageTimer -= Time.unscaledDeltaTime;
            if (messageTimer <= 0f) messageText.text = "";
        }
    }

    public void ShowMessage(string message)
    {
        if (messageText == null) return;
        messageText.text = message;
        messageTimer = messageDuration;
    }
}
