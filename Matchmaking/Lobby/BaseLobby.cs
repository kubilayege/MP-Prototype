using System.Collections.Generic;
using Fusion;

public abstract class BaseLobby : BaseNetworkBehavior
{
    public int RTT => (int) (Runner.GetPlayerRtt(Runner.LocalPlayer) * 1000f);
    
    public PersistentPlayer LocalPersistentPlayer { get; private set; }
    public readonly HashSet<PersistentPlayer> PersistentPlayers = new();
    public readonly Dictionary<PlayerRef, PersistentPlayer> PersistentPlayerByPlayerRef = new();

    public override void Spawned()
    {
        base.Spawned();

        ServiceLocator.Get<NetworkService>().SetLobby(this);
    }

    public abstract string GetLocalPersistentPlayerNickname();

    public virtual void AddPersistentPlayer(PlayerRef playerRef, PersistentPlayer player)
    {
        PersistentPlayers.Add(player);
        PersistentPlayerByPlayerRef.Add(playerRef, player);
        if (player.HasInputAuthority) SetLocalPersistentPlayer(player);
    }

    public virtual void RemovePersistentPlayer(PlayerRef playerRef)
    {
        var persistentPlayer = PersistentPlayerByPlayerRef[playerRef];
        PersistentPlayers.Remove(persistentPlayer);
        PersistentPlayerByPlayerRef.Remove(playerRef);
    }

    private void SetLocalPersistentPlayer(PersistentPlayer player)
    {
        Runner.SetPlayerObject(player.Object.InputAuthority, player.Object);
        LocalPersistentPlayer = player;
    }

    public int GetPersistentPlayerCount()
    {
        return PersistentPlayers.Count;
    }
}