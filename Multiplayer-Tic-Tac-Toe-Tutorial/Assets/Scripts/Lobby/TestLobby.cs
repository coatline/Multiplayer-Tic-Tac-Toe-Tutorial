using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TestLobby : MonoBehaviour
{
    string playerName = "Coatline";

    Lobby joinedLobby;
    Lobby hostLobby;

    async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            print($"Signed in {AuthenticationService.Instance.PlayerId}");
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();


    }

    public async void CreateLobby()
    {
        try
        {
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions()
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, "CaptureTheFlag") },
                    {"Stage", new DataObject(DataObject.VisibilityOptions.Public, "Stage 1") }
                }
            };

            string lobbyName = "MyLobby";
            int maxPlayers = 2;

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            print($"Created lobby: {lobby.Name} {lobby.Players}/{lobby.MaxPlayers}, id: {lobby.Id}, code: {lobby.LobbyCode}");

            hostLobby = lobby;
            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }
    }

    async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions()
            {
                Count = 1,
                Filters = new List<QueryFilter>
                {
                    // Greater than 0 available slots
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>()
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            print($"Lobbies found: {queryResponse.Results.Count}:");

            foreach (Lobby lobby in queryResponse.Results)
                print($" - Name: {lobby.Name} Max: {lobby.MaxPlayers} Mode: {lobby.Data["GameMode"].Value}");
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }
    }

    async void JoinLobby()
    {

        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            Lobby lobby = queryResponse.Results[0];
            Debug.Log($"Joining: {lobby.Name} {lobby.Players}/{lobby.MaxPlayers}");
            await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }
    }

    async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

            Debug.Log($"Attempting to join lobby with code: {lobbyCode}");
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;
            Debug.Log($"Joined lobby with code: {lobbyCode}");

            PrintPlayers(joinedLobby);
            //Debug.Log($"Joining: {lobby.Name} {lobby.Players}/{lobby.MaxPlayers}");
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }
    }

    async void QuickJoinLobby()
    {
        try
        {
            Debug.Log("Quick joining a lobby.");
            await LobbyService.Instance.QuickJoinLobbyAsync();
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }
    }

    void PrintPlayers(Lobby lobby)
    {
        Debug.Log($"Player in lobby {lobby.Name} '{lobby.Id}' {lobby.Data["GameMode"].Value}");
        foreach (Player player in lobby.Players)
        {
            Debug.Log($"{player.Id} {player.Data["PlayerName"].Value}");
        }
    }

    async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>{
                { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) }}
            });
            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }
    }

    async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
            }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }


    float heartbeatTimer;

    async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15;
                heartbeatTimer = heartbeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    float lobbyUpdateTimer;

    async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 15;
                lobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
            }
        }
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }

    Player GetPlayer() => new Player { Data = new Dictionary<string, PlayerDataObject> { { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) } } };
}