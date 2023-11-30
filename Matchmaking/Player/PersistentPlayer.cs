using Fusion;
using VContainer;

public sealed class PersistentPlayer : BaseNetworkBehavior
{
    [Networked] public NetworkString<_16> Nickname { get; private set; }

    [Inject] private NetworkService _networkService;
    [Inject] private GameSignals _signals;

    public override void Spawned()
    {
        base.Spawned();

        _signals.OnPersistentPlayerSpawn.Raise(new OnPersistentPlayerSpawnArgs
        {
            PersistentPlayer = this
        });

        if (!HasInputAuthority) return;
        
        var currentLobby = _networkService.CurrentLobby;
        SetNicknameRpc(currentLobby.GetLocalPersistentPlayerNickname());
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
        
        _signals.OnPersistentPlayerDespawn.Raise(new OnPersistentPlayerDespawnArgs
        {
            PersistentPlayer = this
        });
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void SetNicknameRpc(string nickname)
    {
        Nickname = nickname;
    }
}