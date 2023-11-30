using Fusion;
using UnityEngine;
using VContainer;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Player : BaseNetworkBehavior, IPowerUpContainer
{
    [SerializeField] private SkinnedMeshRenderer[] Clothes;
    [SerializeField] private Transform RootBone;
    [SerializeField] private Transform ClothesParentReference;
    [field: SerializeField] public GameObject ChaserIndicatorReference { get; private set; }
    [field: SerializeField] public Transform InterpolationTargetRef { get; private set; }

    public PlayerMovementController MovementController { get; private set; }
    public PlayerAnimatorController AnimatorController { get; private set; }
    public PlayerInputController InputController { get; private set; }
    public PlayerTagController TagController { get; private set; }
    public PlayerStateMachine StateMachine { get; private set; }

    [Inject] private Bootstrap _bootstrap;
    [Inject] public GameSignals Signals;

    private void OnEnable()
    {
        Clothes.TransferTo(RootBone, ClothesParentReference);
    }

    public override void Spawned()
    {
        base.Spawned();
        
        MovementController = GetComponent<PlayerMovementController>();
        AnimatorController = GetComponent<PlayerAnimatorController>();
        InputController = GetComponent<PlayerInputController>();
        TagController = GetComponent<PlayerTagController>();
        StateMachine = GetComponent<PlayerStateMachine>();
    }
    
    public override void OnInput(NetworkRunner runner, NetworkInput input)
    {
        base.OnInput(runner, input);

        var networkButtons = new NetworkButtons();
        networkButtons.Set(PlayerInputButtons.DoJump, InputController.Controls.Player.DoJump.WasPerformedThisFrame());
        networkButtons.Set(PlayerInputButtons.DoSlide, InputController.Controls.Player.DoSlide.WasPerformedThisFrame());
        networkButtons.Set(PlayerInputButtons.DoTag, InputController.Controls.Player.DoTag.WasPerformedThisFrame());
        networkButtons.Set(PlayerInputButtons.UsePowerUp, InputController.Controls.Player.UsePowerUp.WasPerformedThisFrame());
        var playerInput = new PlayerInput
        {
            Movement = InputController.Movement,
            SmoothMovement = InputController.SmoothMovement,
            Buttons = networkButtons
        };

        input.Set(playerInput);
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        
        // !
        if (IsProxy) return;

        StateMachine.SetTrigger(new PlayerApplyGravityTrigger());
        StateMachine.SetTrigger(new PlayerUpdateGroundedStateTrigger());
        
        var input = GetInput<PlayerInput>(out var playerInput);
        
        StateMachine.SetTrigger(new PlayerMoveTrigger
        {
            Direction = playerInput.Movement,
            SmoothDirection = playerInput.SmoothMovement
        });
        
        AnimatorController.Move(MovementController.CurrentSpeed);

        if (!input) return;
        if (!Runner.IsForward) return;

        if (playerInput.Buttons.IsSet(PlayerInputButtons.DoJump)) StateMachine.SetTrigger(new PlayerDoJumpTrigger());
        if (playerInput.Buttons.IsSet(PlayerInputButtons.DoSlide)) StateMachine.SetTrigger(new PlayerDoSlideTrigger());
        
        if (!HasInputAuthority) return;

        if (playerInput.Buttons.IsSet(PlayerInputButtons.DoTag)) StateMachine.SetTrigger(new PlayerDoTagTrigger());
        if (playerInput.Buttons.IsSet(PlayerInputButtons.UsePowerUp)) StateMachine.SetTrigger(new PlayerUsePowerUpTrigger());
    }

    [Rpc]
    public void GoToSpawnPointClientRpc(Vector3 position, Quaternion rotation)
    {
        if (!HasStateAuthority) return;
        MovementController.WarpTo(position, rotation);
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (StateMachine == null || StateMachine.Current == null) return;
        var position = transform.position.AddY(2f);
        Handles.Label(position, $"{StateMachine.Current}");
    }
#endif

    [SerializeField] private PowerUpDatabase PowerUpDatabase;
    public BasePowerUp PowerUp { get; set; }
    
    public bool TryAssign(BasePowerUp powerUp)
    {
        if (powerUp == null) return false;
        RpcAssign(powerUp);
        return true;
    }

    [Rpc]
    private void RpcAssign(PowerUpRef powerUpRef)
    {
        var powerUp = PowerUpDatabase.Get(powerUpRef);
        PowerUp = powerUp;
    }

    [Rpc]
    public void RpcClearPowerUp()
    {
        PowerUp = null;
    }

    [Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.InputAuthority | RpcTargets.StateAuthority)]
    public void RpcUsePowerUp()
    {
        if (PowerUp == null) return;
        PowerUp.Use(this);
    }
}