using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkedSingleton<GameManager>
{
    public event System.EventHandler<OnClickedOnGridPositionArgs> OnClickedOnGridPosition;
    public event System.EventHandler<OnGameWinEventArgs> OnGameWin;
    public event System.EventHandler OnCurrentPlayablePlayerTypeChanged;
    public event System.EventHandler OnScoreChanged;
    public event System.EventHandler OnPlacedObject;
    public event System.EventHandler OnGameStarted;
    public event System.EventHandler OnGameTied;
    public event System.EventHandler OnRematch;

    public class OnGameWinEventArgs : System.EventArgs
    {
        public Line line;
        public PlayerType winPlayerType;
    }


    public class OnClickedOnGridPositionArgs : System.EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public PlayerType LocalPlayerType { get; private set; }
    NetworkVariable<PlayerType> currentPlayablePlayerType = new NetworkVariable<PlayerType>();
    NetworkVariable<int> playerCircleScore = new NetworkVariable<int>();
    NetworkVariable<int> playerCrossScore = new NetworkVariable<int>();
    PlayerType[,] playerTypeArray;
    List<Line> lineList;

    protected override void Awake()
    {
        base.Awake();
        playerTypeArray = new PlayerType[3, 3];
        lineList = new List<Line>()
        {
            // Horizontal
            new Line
            {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0)},
                centerGridPosition = new Vector2Int(1, 0),
                orientation = Orientation.Horizontal
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1)},
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Horizontal
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,2), new Vector2Int(1,2), new Vector2Int(2,2)},
                centerGridPosition = new Vector2Int(1, 2),
                orientation = Orientation.Horizontal
            },

            // Vertical
            new Line
            {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,2)},
                centerGridPosition = new Vector2Int(0, 1),
                orientation = Orientation.Vertical
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(1,2)},
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Vertical
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(2,0), new Vector2Int(2,1), new Vector2Int(2,2)},
                centerGridPosition = new Vector2Int(2, 1),
                orientation = Orientation.Vertical
            },

            // Diagonals
            new Line
            {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,0), new Vector2Int(1,1), new Vector2Int(2,2)},
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalA
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,2), new Vector2Int(1,1), new Vector2Int(2,0)},
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalB
            },};
    }

    public override void OnNetworkSpawn()
    {
        LocalPlayerType = (PlayerType)(NetworkManager.LocalClientId + 1);

        if (IsServer)
            NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectCallback;

        currentPlayablePlayerType.OnValueChanged += (PlayerType oldPlayerType, PlayerType newPlayerType) =>
        {
            OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
        };

        playerCrossScore.OnValueChanged += (int prevScore, int newScore) =>
        {
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
        };

        playerCircleScore.OnValueChanged += (int prevScore, int newScore) =>
        {
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    private void NetworkManager_OnClientConnectCallback(ulong obj)
    {
        if (NetworkManager.ConnectedClientsList.Count == 2)
        {
            // Start Game
            currentPlayablePlayerType.Value = PlayerType.Cross;
            TriggerOnGameStartedRPC();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void TriggerOnGameStartedRPC()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRPC(int x, int y, PlayerType playerType)
    {
        if (playerType != currentPlayablePlayerType.Value)
            return;

        if (playerTypeArray[x, y] != PlayerType.None)
            return;

        playerTypeArray[x, y] = playerType;
        TriggerOnPlacedObjectRPC();

        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionArgs()
        {
            x = x,
            y = y,
            playerType = playerType
        });

        if (currentPlayablePlayerType.Value == PlayerType.Cross)
            currentPlayablePlayerType.Value = PlayerType.Circle;
        else
            currentPlayablePlayerType.Value = PlayerType.Cross;

        TestWinner();
    }

    [Rpc(SendTo.ClientsAndHost)]
    void TriggerOnPlacedObjectRPC()
    {
        OnPlacedObject?.Invoke(this, EventArgs.Empty);
    }

    bool TestWinnerLine(Line line)
    {
        return TestWinnerLine(playerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
            playerTypeArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
            playerTypeArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]);
    }

    bool TestWinnerLine(PlayerType aPlayerType, PlayerType bPlayerType, PlayerType cPlayerType)
    {
        return aPlayerType != PlayerType.None &&
            aPlayerType == bPlayerType &&
            bPlayerType == cPlayerType;
    }

    void TestWinner()
    {
        for (int i = 0; i < lineList.Count; i++)
        {
            Line line = lineList[i];

            if (TestWinnerLine(line))
            {
                print("Winner!");

                currentPlayablePlayerType.Value = PlayerType.None;

                PlayerType winPlayerType = playerTypeArray[line.centerGridPosition.x, line.centerGridPosition.y];

                if (winPlayerType == PlayerType.Cross)
                    playerCrossScore.Value++;
                else
                    playerCircleScore.Value++;

                TriggerOnGameWinRPC(i, winPlayerType);
                return;
            }
        }

        bool hasTie = true;
        for (int x = 0; x < playerTypeArray.GetLength(0); x++)
            for (int y = 0; y < playerTypeArray.GetLength(1); y++)
            {
                if (playerTypeArray[x, y] == PlayerType.None)
                {
                    hasTie = false;
                    break;
                }
            }

        if (hasTie)
            TriggerOnGameTiedRPC();
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameTiedRPC()
    {
        OnGameTied?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameWinRPC(int lineIndex, PlayerType winningPlayerType)
    {
        Line line = lineList[lineIndex];
        OnGameWin?.Invoke(this, new OnGameWinEventArgs()
        {
            line = line,
            winPlayerType = winningPlayerType
        });
    }

    public PlayerType GetCurrentPlayablePlayerType() => currentPlayablePlayerType.Value;

    [Rpc(SendTo.Server)]
    public void RematchRPC()
    {
        for (int x = 0; x < playerTypeArray.GetLength(0); x++)
        {
            for (int y = 0; y < playerTypeArray.GetLength(1); y++)
            {
                playerTypeArray[x, y] = PlayerType.None;
            }
        }

        currentPlayablePlayerType.Value = PlayerType.Cross;
        TriggerOnRematchRPC();
    }

    [Rpc(SendTo.ClientsAndHost)]
    void TriggerOnRematchRPC()
    {
        OnRematch?.Invoke(this, EventArgs.Empty);
    }

    public void GetScores(out int playerCrossScore, out int playerCircleScore)
    {
        playerCircleScore = this.playerCircleScore.Value;
        playerCrossScore = this.playerCrossScore.Value;
    }

    public struct Line
    {
        public List<Vector2Int> gridVector2IntList;
        public Vector2Int centerGridPosition;
        public Orientation orientation;
    }

    public enum Orientation
    {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB
    }

    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }
}
