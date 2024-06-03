using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TestLobby : MonoBehaviour
{
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;

    public InputField CodeInput; // Reference to the Input Field
    public Button JoinButton;
    
    private string playername;

    // Start is called before the first frame update
    async void Start()
    {
        playername = "Noushad" + Random.Range(90, 100);
        JoinButton.onClick.AddListener(OnSubmit);
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log(playername);
    }

    void OnSubmit()
    {
        string userInput = CodeInput.text; // Get the input field text
        JoinLobbyByCode(userInput);        // Pass the input to the ProcessInput function
    }
    public async void createLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayer = 4;


            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data= new Dictionary<string, DataObject>
                {
                    { "GameMode",new DataObject(DataObject.VisibilityOptions.Public,"CaptureTheFlag") }
                }
            };
            //it was true first
        
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayer,createLobbyOptions);
            hostLobby = lobby;
            joinedLobby = hostLobby;
            Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers+" Lobby Id: "+lobby.Id+" Lobby code: "+lobby.LobbyCode);
            printPlayers(hostLobby);
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void handleLobbyPollForUpdates()
    {
        if(joinedLobby!=null)
        {
            lobbyUpdateTimer-=Time.deltaTime;
            if(lobbyUpdateTimer < 0f )
            {
                float lobbyUpdateTimerMax = 1.1f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
            }
        }
    }

    public async void listLobby()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions {
                Count = 25,
                Filters= new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,"0",QueryFilter.OpOptions.GT)
                },
                Order= new List<QueryOrder>
                {
                    new QueryOrder(false,QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryresponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            Debug.Log("Lobbies found: " + queryresponse.Results.Count);
            foreach(Lobby lobby in queryresponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers+" " + lobby.Data["GameMode"].Value);
            }
        }catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void JoinLobbyByCode(string GameId)
    {
        try
        {

            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };
            QueryResponse queryresponse = await Lobbies.Instance.QueryLobbiesAsync();
            Lobby lobby=await Lobbies.Instance.JoinLobbyByCodeAsync(GameId, joinLobbyByCodeOptions);
            joinedLobby = lobby;
            printPlayers(lobby);
            

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    private void printPlayers()
    {
        printPlayers(joinedLobby);
    }
    

    // Update is called once per frame
    void Update()
    {
        handleLobbyHeartBeat();
        handleLobbyPollForUpdates();
    }
    private async void handleLobbyHeartBeat()
    {
        if(hostLobby!=null)
        {
            heartbeatTimer -= Time.deltaTime;
            if(heartbeatTimer < 0 )
            {
                float heartbeatMaxTimer = 15f;
                heartbeatTimer = heartbeatMaxTimer;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }
    private async void QuickJoinButton()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    private void printPlayers(Lobby lobby)
    {
        Debug.Log("Players in Lobby: "+lobby.Name +" " + lobby.Data["GameMode"].Value);
        foreach(Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }
    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                {
                    {"PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playername) }
                }
        };
    }
    private async void updateLobbyGameMode(string gameMode)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode",new DataObject(DataObject.VisibilityOptions.Public,gameMode) }
                }
            });
            joinedLobby = hostLobby;
            printPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    private async void UpdatePlayerName(string newplayerName)
    {
        try {
        playername = newplayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject> { { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playername) } }
            });
        }
        catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }
    public async void leaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

        }catch (LobbyServiceException e) { Debug.Log(e); }
    }
    public async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);

        }
        catch (LobbyServiceException e) { Debug.Log(e); }
    }
    private async void MigrateLobbyHost()
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[1].Id
            });
        }
        catch (LobbyServiceException e )
        {
            Debug.Log(e);
        }
    }
    public async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
            leaveLobby();
        }
        catch (LobbyServiceException e) { Debug.Log(e); }
    }
}
