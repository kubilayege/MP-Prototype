using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public sealed class Match : MonoBehaviour
{
    private MatchConfigurations _configurations;
    private IGameMode _gameMode => _configurations.GameMode;
    private UniTask? _finishAsync;

    public async UniTask StartAsync(MatchConfigurations configurations)
    {
        _configurations = configurations;
        await _gameMode.StartAsync(configurations);
    }

    public void Update()
    {
        if (_gameMode.IsFinished)
        {
            _finishAsync ??= FinishAsync();
            return;
        }
        
        _gameMode.Tick();
    }

    public async UniTask FinishAsync()
    {
        await _gameMode.FinishAsync();
        
        Destroy(gameObject);
    }

    public bool HasGameModeRequirementsAreMet()
    {
        return _gameMode.HasGameModeRequirementsAreMet();
    }

    public static async UniTask<Match> Create(MatchConfigurations configurations)
    {
        var currentMatch = FindObjectsOfType<Match>();
        if (currentMatch.Length != 0) return default;

        var handle = Addressables.LoadAssetAsync<GameObject>("Match");
        await handle.ToUniTask();
        var matchInstance = Instantiate(handle.Result).GetComponent<Match>();
        
        if (!configurations.GameMode.HasGameModeRequirementsAreMet())
        {
            Destroy(matchInstance.gameObject);
            return default;
        }
        
        await matchInstance.StartAsync(configurations);
        return matchInstance;
    }
}