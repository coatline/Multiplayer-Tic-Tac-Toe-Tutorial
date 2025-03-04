using UnityEngine;

namespace TicTacToe
{
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] AudioClip placeSound;
        [SerializeField] AudioClip winSound;
        [SerializeField] AudioClip loseSound;

        private void Start()
        {
            GameManager.I.OnPlacedObject += GameManager_OnPlacedObject;
            GameManager.I.OnGameWin += GameManager_OnGameWin; ;
        }

        private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
        {
            if (e.winPlayerType == GameManager.I.LocalPlayerType)
                AudioSource.PlayClipAtPoint(winSound, Vector2.zero);
            else
                AudioSource.PlayClipAtPoint(loseSound, Vector2.zero);
        }

        private void GameManager_OnPlacedObject(object sender, System.EventArgs e)
        {
            AudioSource.PlayClipAtPoint(placeSound, Vector2.zero);
        }
    }
}