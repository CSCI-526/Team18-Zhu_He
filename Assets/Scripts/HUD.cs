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
    public float messageSize = 40f;
    private float timer;

    public float tipSize = 56f;
    public float tipY = -170f;
    private TextMeshProUGUI bigTipText;

    public float restartTipDelay = 20f;
    private bool showedRestartTip;

    void Awake()
    {
        if (messageText != null)
        {
            messageText.text = "";
            messageText.fontSize = messageSize;
            messageText.fontStyle = FontStyles.Bold;
            messageText.alignment = TextAlignmentOptions.Bottom;
            messageText.outlineWidth = 0.25f;
            messageText.outlineColor = Color.white;

            RectTransform rect = messageText.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 60f);
            rect.sizeDelta = new Vector2(1600f, 150f);
        }
    }

    void Start()
    {
        int levelNum = SceneManager.GetActiveScene().buildIndex + 1;
        levelText.text = "Level " + levelNum + ": " + levelManager.levelName;
        if (levelManager.hint != "")
        {
            levelText.text += "\n<size=70%>" + levelManager.hint + "</size>";
        }

        if (levelManager.bigTip != "")
        {
            MakeBigTip(levelManager.bigTip);
        }
    }

    void MakeBigTip(string tip)
    {
        GameObject tipObj = new GameObject("BigTip", typeof(RectTransform));
        tipObj.transform.SetParent(transform, false);

        bigTipText = tipObj.AddComponent<TextMeshProUGUI>();
        bigTipText.font = levelText.font;
        bigTipText.text = tip;
        bigTipText.fontSize = tipSize;
        bigTipText.fontStyle = FontStyles.Bold;
        bigTipText.alignment = TextAlignmentOptions.Top;
        bigTipText.color = levelText.color;
        bigTipText.outlineWidth = 0.25f;
        bigTipText.outlineColor = Color.white;
        bigTipText.raycastTarget = false;

        RectTransform rect = tipObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, tipY);
        rect.sizeDelta = new Vector2(1500f, 300f);
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

        if (timer > 0f && cutScript.state == CutController.State.Normal)
        {
            timer -= Time.unscaledDeltaTime;
            if (timer <= 0f)
            {
                messageText.text = "";
            }
        }

        if (!showedRestartTip && levelManager.cutsLeft == 0 && cutScript.state == CutController.State.Normal)
        {
            if (Time.time - levelManager.lastCutTime > restartTipDelay)
            {
                showedRestartTip = true;
                if (bigTipText == null)
                {
                    MakeBigTip("");
                }
                bigTipText.text = "Stuck? Press R to restart";
                bigTipText.color = new Color(0.8f, 0.2f, 0.2f, 1f);
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
