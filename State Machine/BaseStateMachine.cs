using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseStateMachine : MonoBehaviour
{
    private readonly Dictionary<Type, IState> _stateByType = new();
    private readonly Dictionary<(Type state, Type trigger), object> _triggerEventsByTypes = new();
    public BasePlayerState Current { get; private set; }
    public BasePlayerState Previous { get; private set; }
    
    public virtual void Update()
    {
        Current?.OnUpdate();
    }
    
    public bool TryChangeTo(BasePlayerState state)
    {
        var stateType = state.GetType();
        if (Current == state) return false;
        if (!_stateByType.ContainsKey(stateType)) return false;
        if (!state.IsValid()) return false;
        Current?.OnExit();
        Previous = Current;
        Current = state;
        Current.OnEnter();
        return true;
    }
    
    public T Get<T>() where T : BasePlayerState
    {
        var stateType = typeof(T);
        if (!_stateByType.ContainsKey(stateType)) return default;
        return (T) _stateByType[stateType];
    }
    
    public bool TryAdd<T>(T state) where T : BasePlayerState
    {
        var stateType = state.GetType();
        var isAdded = _stateByType.TryAdd(stateType, state);
        if (isAdded && _stateByType.Count == 1)
            TryChangeTo(state);
        return isAdded;
    }
    
    public bool TryRemove<T>() where T : BasePlayerState
    {
        var stateType = typeof(T);
        return _stateByType.Remove(stateType);
    }
    
    public void SetTrigger<T>(T trigger) where T : ITrigger
    {
        if (Current == null) return;
        var stateType = Current.GetType();
        var triggerType = typeof(T);
        var triggerEventFilter = (stateType, triggerType);
        if (!_triggerEventsByTypes.ContainsKey(triggerEventFilter)) return;
        var action = _triggerEventsByTypes[triggerEventFilter] as Action<T>;
        action?.Invoke(trigger);
    }
    
    public void AddTriggerListener<T>(BasePlayerState state, Action<T> trigger) where T : ITrigger
    {
        var stateType = state.GetType();
        var triggerType = typeof(T);
        var triggerEventFilter = (stateType, triggerType);
        if (_triggerEventsByTypes.ContainsKey(triggerEventFilter)) return;
        _triggerEventsByTypes.Add(triggerEventFilter, trigger);
    }
    
    public void RemoveTriggerListener<T>(BasePlayerState state) where T : ITrigger
    {
        var stateType = state.GetType();
        var triggerType = typeof(T);
        var triggerEventFilter = (stateType, triggerType);
        _triggerEventsByTypes.Remove(triggerEventFilter);
    }
}