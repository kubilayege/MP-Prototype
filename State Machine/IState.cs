public interface IState
{
    public void Initialize();
    public void OnEnter();
    public void OnUpdate();
    public void OnExit();
}