using Cysharp.Threading.Tasks;

public interface IGameMode
{
    public bool IsFinished { get; }
    
    public UniTask StartAsync(MatchConfigurations configurations);
    public void Tick();
    public UniTask FinishAsync();
    public bool HasGameModeRequirementsAreMet();
}