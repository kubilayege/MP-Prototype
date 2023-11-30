public sealed class PlayerStateMachine : BaseStateMachine
{
    private Player _player;

    private void Awake()
    {
        _player = GetComponent<Player>();
        
        AddStates();
        AddTriggers();
    }

    private void AddState<T>(T state) where T : BasePlayerState
    {
        TryAdd(state);
        state.Initialize();
    }

    private void AddStates()
    {
        AddState(new PlayerMainState(_player, this));
        AddState(new PlayerJumpState(_player, this));
        AddState(new PlayerSlideState(_player, this));
        AddState(new PlayerStunState(_player, this));
        AddState(new PlayerClimbState(_player, this));
    }
    
    private void AddTriggers()
    {
        {
            AddTriggerListener<PlayerApplyGravityTrigger>(Get<PlayerMainState>(), OnApplyGravityTrigger);
        }
        {
            AddTriggerListener<PlayerDoClimbTrigger>(Get<PlayerJumpState>(), OnClimbTrigger);
        }
        {
            AddTriggerListener<PlayerDoJumpTrigger>(Get<PlayerMainState>(), OnJumpTrigger);
            AddTriggerListener<PlayerDoJumpTrigger>(Get<PlayerSlideState>(), OnJumpTrigger);
        }
        {
            AddTriggerListener<PlayerMoveTrigger>(Get<PlayerMainState>(), OnMoveTrigger);
            AddTriggerListener<PlayerMoveTrigger>(Get<PlayerJumpState>(), OnMoveTrigger);
            AddTriggerListener<PlayerMoveTrigger>(Get<PlayerSlideState>(), OnMoveTrigger);
        }
        {
            AddTriggerListener<PlayerDoSlideTrigger>(Get<PlayerMainState>(), OnSlideTrigger);
            AddTriggerListener<PlayerDoSlideTrigger>(Get<PlayerJumpState>(), OnSlideTrigger);
        }
        {
            AddTriggerListener<PlayerStunTrigger>(Get<PlayerMainState>(), OnStunTrigger);
            AddTriggerListener<PlayerStunTrigger>(Get<PlayerClimbState>(), OnStunTrigger);
            AddTriggerListener<PlayerStunTrigger>(Get<PlayerJumpState>(), OnStunTrigger);
            AddTriggerListener<PlayerStunTrigger>(Get<PlayerSlideState>(), OnStunTrigger);
        }
        {
            AddTriggerListener<PlayerDoTagTrigger>(Get<PlayerMainState>(), OnTagTrigger);
            AddTriggerListener<PlayerDoTagTrigger>(Get<PlayerJumpState>(), OnTagTrigger);
            AddTriggerListener<PlayerDoTagTrigger>(Get<PlayerSlideState>(), OnTagTrigger);
        }
        {
            AddTriggerListener<PlayerUpdateGroundedStateTrigger>(Get<PlayerMainState>(), OnUpdateGroundedStateTrigger);
            AddTriggerListener<PlayerUpdateGroundedStateTrigger>(Get<PlayerJumpState>(), OnUpdateGroundedStateTrigger);
            AddTriggerListener<PlayerUpdateGroundedStateTrigger>(Get<PlayerSlideState>(), OnUpdateGroundedStateTrigger);
        }
        {
            AddTriggerListener<PlayerUsePowerUpTrigger>(Get<PlayerMainState>(), OnUsePowerUpTrigger);
            AddTriggerListener<PlayerUsePowerUpTrigger>(Get<PlayerJumpState>(), OnUsePowerUpTrigger);
            AddTriggerListener<PlayerUsePowerUpTrigger>(Get<PlayerSlideState>(), OnUsePowerUpTrigger);
        }
    }
    
    private void OnApplyGravityTrigger(PlayerApplyGravityTrigger trigger)
    {
        _player.MovementController.ApplyGravity();
    }

    private void OnClimbTrigger(PlayerDoClimbTrigger trigger)
    {
        TryChangeTo(Get<PlayerClimbState>());
    }

    private void OnJumpTrigger(PlayerDoJumpTrigger trigger)
    {
        TryChangeTo(Get<PlayerJumpState>());
    }

    private void OnMoveTrigger(PlayerMoveTrigger trigger)
    {
        _player.MovementController.ApplyMove(trigger.SmoothDirection);
        _player.MovementController.ApplyLook(trigger.SmoothDirection);
    }

    private void OnSlideTrigger(PlayerDoSlideTrigger trigger)
    {
        TryChangeTo(Get<PlayerSlideState>());
    }

    private void OnStunTrigger(PlayerStunTrigger trigger)
    {
        TryChangeTo(Get<PlayerStunState>());
    }

    private void OnTagTrigger(PlayerDoTagTrigger trigger)
    {
        _player.AnimatorController.RpcDoTag();
        _player.TagController.RpcTryTag(_player.Object.Id);
    }

    private void OnUpdateGroundedStateTrigger(PlayerUpdateGroundedStateTrigger trigger)
    {
        _player.MovementController.UpdateGroundedState();
    }
    
    private void OnUsePowerUpTrigger(PlayerUsePowerUpTrigger trigger)
    {
        _player.RpcUsePowerUp();
    }
}