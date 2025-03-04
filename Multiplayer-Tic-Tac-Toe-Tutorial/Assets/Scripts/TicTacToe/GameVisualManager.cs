using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    public const float GRID_SIZE = 3.1f;

    [SerializeField] Transform crossPrefab;
    [SerializeField] Transform circlePrefab;
    [SerializeField] Transform lineCompletePrefab;

    List<GameObject> visualGobList;

    private void Awake()
    {
        visualGobList = new List<GameObject>();
    }

    void Start()
    {
        GameManager.I.OnClickedOnGridPosition += GameManager_OnClickedOnGridPosition;
        GameManager.I.OnGameWin += GameManager_OnGameWin;
        GameManager.I.OnRematch += GameManager_OnRematch;
    }

    private void GameManager_OnRematch(object sender, System.EventArgs e)
    {
        // Only the server spawns the visuals anyhow
        if (NetworkManager.Singleton.IsServer == false)
            return;

        for (int i = visualGobList.Count - 1; i >= 0; i--)
            Destroy(visualGobList[i]);

        visualGobList.Clear();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (NetworkManager.Singleton.IsServer == false)
            return;

        float eulerZ = 0f;

        switch (e.line.orientation)
        {
            case GameManager.Orientation.Horizontal: eulerZ = 0f; break;
            case GameManager.Orientation.Vertical: eulerZ = 90f; break;
            case GameManager.Orientation.DiagonalA: eulerZ = 45f; break;
            case GameManager.Orientation.DiagonalB: eulerZ = -45f; break;
        }

        Transform line = Instantiate(lineCompletePrefab, GetGridWorldPosition(e.line.centerGridPosition.x, e.line.centerGridPosition.y), Quaternion.Euler(0, 0, eulerZ));
        line.GetComponent<NetworkObject>().Spawn(true);
        visualGobList.Add(line.gameObject);
    }

    void GameManager_OnClickedOnGridPosition(object sender, GameManager.OnClickedOnGridPositionArgs args)
    {
        SpawnObjectRPC(args.x, args.y, args.playerType);
    }

    [Rpc(SendTo.Server)]
    void SpawnObjectRPC(int x, int y, GameManager.PlayerType playerType)
    {
        Transform prefab = crossPrefab;

        if (playerType == GameManager.PlayerType.Circle)
            prefab = circlePrefab;

        Transform cross = Instantiate(prefab, GetGridWorldPosition(x, y), Quaternion.identity);
        cross.GetComponent<NetworkObject>().Spawn(true);
        visualGobList.Add(cross.gameObject);
    }

    Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
}
