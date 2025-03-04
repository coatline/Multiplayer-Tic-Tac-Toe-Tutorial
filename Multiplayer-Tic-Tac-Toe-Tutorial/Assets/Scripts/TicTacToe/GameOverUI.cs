using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] Button rematchButton;
    [SerializeField] Color loseColor;
    [SerializeField] Color winColor;
    [SerializeField] Color tieColor;

    private void Awake()
    {
        rematchButton.onClick.AddListener(() => { GameManager.I.RematchRPC(); });
    }

    private void Start()
    {
        GameManager.I.OnGameWin += GameManager_OnGameWin;
        GameManager.I.OnRematch += GameManager_OnRematch;
        GameManager.I.OnGameTied += GameManager_OnGameTied;

        Hide();
    }

    private void GameManager_OnGameTied(object sender, System.EventArgs e)
    {
        resultText.text = "TIE!";
        resultText.color = tieColor;
        Show();
    }

    private void GameManager_OnRematch(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (e.winPlayerType == GameManager.I.LocalPlayerType)
        {
            resultText.text = "YOU WIN!";
            resultText.color = winColor;
        }
        else
        {
            resultText.text = "YOU LOSE!";
            resultText.color = loseColor;
        }

        Show();
    }

    void Show() => gameObject.SetActive(true);
    void Hide() => gameObject.SetActive(false);
}
