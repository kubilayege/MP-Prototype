using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public sealed class Practice : IGameMode
{
    public bool IsFinished { get; }

    private LifetimeScope _bootstrap;
    private NetworkSceneManager _networkSceneManager;
    private NetworkSpawnManager _networkSpawnManager;
    private NetworkService _networkService;
    private NetworkRunner _networkRunner;
    private GameSignals _signals;
    private BaseLobby _currentLobby;

    private readonly Dictionary<NetworkId, Player> _playerByNetworkId = new();
    private Player _chaserPlayer;
    private PowerUpGenerator _powerUpGenerator;

    public async UniTask StartAsync(MatchConfigurations configurations)
    {
        await _networkSceneManager.ChangeSceneAsync($"Map - {configurations.Map}");
        SpawnPlayerCharacterControllers();
        SetRandomChaser();
        CreatePowerUpGenerator();
        
        _signals.OnPlayerTagged.RegisterListener(OnPlayerTagged);
    }
    
    public void Tick()
    {
        _powerUpGenerator?.Tick(_networkRunner);
    }

    public UniTask FinishAsync()
    {
        return default;
    }

    public bool HasGameModeRequirementsAreMet()
    {
        _bootstrap = LifetimeScope.Find<Bootstrap>();
        _networkSceneManager = _bootstrap.Container.Resolve<NetworkSceneManager>();
        _networkSpawnManager = _bootstrap.Container.Resolve<NetworkSpawnManager>();
        _networkService = _bootstrap.Container.Resolve<NetworkService>();
        _networkRunner = _bootstrap.Container.Resolve<NetworkRunner>();
        _signals = _bootstrap.Container.Resolve<GameSignals>();
        _currentLobby = _networkService.CurrentLobby;
        return _currentLobby.GetPersistentPlayerCount() > 0;
    }

    private void SpawnPlayerCharacterControllers()
    {
        var spawnPoints = Object.FindObjectsOfType<SpawnPoint>().ToList();
        foreach (var persistentPlayer in _currentLobby.PersistentPlayers)
        {
            var playerInstance = _networkSpawnManager.SpawnFromServer<Player>(_networkService.PlayerPrefabRef, inputAuthority: persistentPlayer.Object.InputAuthority);
            var randomSpawnPoint = spawnPoints.GetRandomElement();
            playerInstance.GoToSpawnPointClientRpc(randomSpawnPoint.transform.position, randomSpawnPoint.transform.rotation);
            spawnPoints.Remove(randomSpawnPoint);
            _playerByNetworkId.Add(playerInstance.Object.Id, playerInstance);
        }
    }

    private void SetRandomChaser()
    {
        foreach (var player in _playerByNetworkId.Values)
        {
            player.TagController.SetEvaderRpc();
        }

        var randomPlayer = _playerByNetworkId.Values.ToList().GetRandomElement();
        randomPlayer.TagController.SetChaserRpc(taggedByPlayer: false);
        _chaserPlayer = randomPlayer;
    }

    private void CreatePowerUpGenerator()
    {
        _powerUpGenerator = new PowerUpGenerator();
        _powerUpGenerator.Start();
    }

    private void OnPlayerTagged(OnPlayerTaggedSignalArgs args)
    {
        var newChaserPlayer = _playerByNetworkId[args.PlayerId];
        _chaserPlayer.TagController.SetEvaderRpc();
        _chaserPlayer = newChaserPlayer;
        _chaserPlayer.TagController.SetChaserRpc(taggedByPlayer: true);
    }
}