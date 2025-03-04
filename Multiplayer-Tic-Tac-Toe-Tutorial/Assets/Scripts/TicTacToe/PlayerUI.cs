using System;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] GameObject crossArrow;
    [SerializeField] GameObject circleArrow;
    [SerializeField] GameObject crossYouText;
    [SerializeField] GameObject circleYouText;
    [SerializeField] TextMeshProUGUI crossScoreText;
    [SerializeField] TextMeshProUGUI circleScoreText;

    private void Awake()
    {
        crossArrow.SetActive(false);
        crossYouText.SetActive(false);
        circleArrow.SetActive(false);
        circleYouText.SetActive(false);
    }

    private void Start()
    {
        GameManager.I.OnGameStarted += GameManager_OnGameStarted;
        GameManager.I.OnCurrentPlayablePlayerTypeChanged += GameManager_OnCurrentPlayablePlayerTypeChanged;
        GameManager.I.OnScoreChanged += GameManager_OnScoreChanged; ;
    }

    private void GameManager_OnScoreChanged(object sender, EventArgs e)
    {
        GameManager.I.GetScores(out int playerCrossScore, out int playerCircleScore);
        crossScoreText.text = playerCrossScore.ToString();
        circleScoreText.text = playerCircleScore.ToString();
    }

    private void GameManager_OnCurrentPlayablePlayerTypeChanged(object sender, EventArgs e)
    {
        UpdateCurrentArrow();
    }

    void GameManager_OnGameStarted(object sender, System.EventArgs e)
    {
        if (GameManager.I.LocalPlayerType == GameManager.PlayerType.Cross)
            crossYouText.SetActive(true);
        else
            circleYouText.SetActive(true);

        crossScoreText.text = "0";
        circleScoreText.text = "0";

        UpdateCurrentArrow();
    }

    private void UpdateCurrentArrow()
    {
        if (GameManager.I.GetCurrentPlayablePlayerType() == GameManager.PlayerType.Cross)
        {
            crossArrow.SetActive(true);
            circleArrow.SetActive(false);
        }
        else
        {
            circleArrow.SetActive(true);
            crossArrow.SetActive(false);
        }
    }
}
