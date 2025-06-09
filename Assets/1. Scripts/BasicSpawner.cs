using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using UnityEngine.InputSystem;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;

    async void StartGame(GameMode mode)
    {
        // NetworkRunner를 생성하고 입력ProvideInput을 넣어줄거다 라고 지정하고
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // 현재 씬을 사용할거다라고 씬데이터를 만들어주고
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid) {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        //게임 시작하라고 매개변수들을 만들어줌
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }
    
    //게임시작하는 간단한 UI방식
    private void OnGUI()
    {
        if (_runner == null)
        {
            if (GUI.Button(new Rect(0,0,200,40), "Host"))
            {
                StartGame(GameMode.Host);
            }
            if (GUI.Button(new Rect(0,40,200,40), "Join"))
            {
                StartGame(GameMode.Client);
            }
        }
    }
    
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 5, 1, 0);
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            _spawnedCharacters.Add(player, networkPlayerObject);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    // 🔥 중요! 여기서 입력을 수집합니다
    public void OnInput(NetworkRunner runner, NetworkInput input) 
    { 
        var data = new NetworkInputData();
        
        // Settings 스크립트 찾기 (조이스틱/터치 모드 확인용)
        Settings settings = FindObjectOfType<Settings>();
        bool useJoystick = settings != null && settings.isJoyStick;
        
        if (useJoystick)
        {
            // 조이스틱 입력 처리
            Vector2 joystickInput = JoyStick.GetJoystickInput();
            bool joystickActive = JoyStick.GetJoystickActive();
            
            data.moveDirection = joystickInput;
            data.isWalking = joystickActive;
        }
        else
        {
            // 터치/마우스 입력 처리
            if (Mouse.current.leftButton.isPressed)
            {
                // UI 위가 아닐 때만 이동
                if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    // 화면 중심에서 마우스 위치까지의 방향 계산
                    Vector3 mousePos = Mouse.current.position.ReadValue();
                    Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
                    Vector3 direction = (mousePos - screenCenter).normalized;
                    
                    data.moveDirection = new Vector2(direction.x, direction.y);
                    data.isWalking = true;
                }
                else
                {
                    data.moveDirection = Vector2.zero;
                    data.isWalking = false;
                }
            }
            else
            {
                data.moveDirection = Vector2.zero;
                data.isWalking = false;
            }
        }
        
        // 네트워크로 입력 데이터 전송
        input.Set(data);
    }
    
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data){ }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress){ }
}