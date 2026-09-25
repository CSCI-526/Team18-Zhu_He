using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HUD : MonoBehaviour
{
    public TMP_Text levelText;
    public TMP_Text statsText;
    public TMP_Text modeText;
    public TMP_Text messageText;

    public LevelManager levelManager;
    public CuttablePiece player;
    public CameraRig camScript;
    public CutController cutScript;

    public float messageTime = 2.5f;
    private float timer;

    void Awake()
    {
        if (messageText != null)
        {
            messageText.text = "";
        }
    }

    void Start()
    {
        int levelNum = SceneManager.GetActiveScene().buildIndex + 1;
        levelText.text = "Level " + levelNum + ": " + levelManager.levelName + "\n<size=70%>" + levelManager.hint + "</size>";
    }

    void Update()
    {
        Vector3 size = player.GetSize();
        statsText.text = "Cuts left: " + levelManager.cutsLeft + " / " + levelManager.maxCuts +
                         "    Size: " + size.x.ToString("0.0") + " x " + size.y.ToString("0.0") + " x " + size.z.ToString("0.0") +
                         "    Mass: " + player.volume.ToString("0.0");

        if (cutScript.state == CutController.State.Drawing)
        {
            modeText.text = "Cut Mode\n<size=70%>" + camScript.GetViewName() + "</size>";
        }
        else if (cutScript.state == CutController.State.Choosing)
        {
            modeText.text = "Choose a piece\n<size=70%>1 = blue, 2 = orange</size>";
        }
        else
        {
            if (camScript.is2D)
            {
                modeText.text = "2D\n<size=70%>" + camScript.GetViewName() + "</size>";
            }
            else
            {
                modeText.text = "3D\n<size=70%>Free camera</size>";
            }
        }

        if (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;
            if (timer <= 0f)
            {
                messageText.text = "";
            }
        }
    }

    public void ShowMessage(string msg)
    {
        if (messageText == null) return;
        messageText.text = msg;
        timer = messageTime;
    }
}
