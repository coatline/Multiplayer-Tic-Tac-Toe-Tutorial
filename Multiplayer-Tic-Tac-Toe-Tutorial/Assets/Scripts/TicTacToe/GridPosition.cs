using UnityEngine;

namespace TicTacToe
{
    public class GridPosition : MonoBehaviour
    {
        [SerializeField] int x;
        [SerializeField] int y;

        private void OnMouseDown()
        {
            GameManager.I.ClickedOnGridPositionRPC(x, y, GameManager.I.LocalPlayerType);
        }
    }
}
