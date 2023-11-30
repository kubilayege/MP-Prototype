using System.Collections.Generic;
using Fusion;
using UnityEngine.AddressableAssets;

public sealed class PlayerTagController : BasePlayerController
{
    public PlayerTagData Data { get; private set; }
    public bool IsTagged { get; private set; }

    private readonly List<LagCompensatedHit> _lagCompensatedHits = new();

    private void Start()
    {
        var handle = Addressables.LoadAssetAsync<PlayerTagData>("Player Tag Data");
        handle.Completed += operationHandle => { Data = operationHandle.Result; };
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcTryTag(NetworkId taggerId)
    {
        if (!IsTagged) return;

        var hitCount = Runner.LagCompensation.OverlapSphere(transform.position, Data.Radius, Object.InputAuthority, _lagCompensatedHits, Data.TargetLayer);
        if (hitCount == 0) return;

        Player taggedPlayer = null;
        foreach (var hit in _lagCompensatedHits)
        {
            if (!hit.Hitbox.transform.parent.TryGetComponent<Player>(out var targetPlayer)) continue;
            if (targetPlayer.Object.Id == taggerId) continue;
            taggedPlayer = targetPlayer;
            break;
        }

        if (taggedPlayer == null) return;
        
        Player.Signals.OnPlayerTagged.Raise(new OnPlayerTaggedSignalArgs
        {
            PlayerId = taggedPlayer.Object.Id
        });
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void SetChaserRpc(bool taggedByPlayer)
    {
        IsTagged = true;
        Player.ChaserIndicatorReference.SetActive(true);
        
        if (!taggedByPlayer) return;
        Player.StateMachine.SetTrigger(new PlayerStunTrigger());
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void SetEvaderRpc()
    {
        IsTagged = false;
        Player.ChaserIndicatorReference.SetActive(false);
    }
}