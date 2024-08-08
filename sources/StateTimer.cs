namespace Descindie.Legion.EventBus
{
  using UnityEngine;

  [AddComponentMenu("Descindie.Legion/LGN State Timer")]
  public sealed class StateTimer : MonoBehaviour
  {
    public static readonly Multicast OnFixedUpdate = new();
    public static readonly Multicast OnUpdate = new();
    public static readonly Multicast OnLateUpdate = new();

    /// <summary>
    /// Example of use: AI players, weapon.
    /// </summary>
    public static readonly Multicast OnBeforeLateUpdate = new();

    /// <summary>
    /// Example of use: camera SVfx, linked audio listener.
    /// </summary>
    public static readonly Multicast OnAfterLateUpdate = new();

#if UNITY_ASSERTIONS
    private void Awake() => Debugging.AssertExistsSingleInstanceOnly(this);
#endif

    private void FixedUpdate() => OnFixedUpdate.Invoke();
    private void Update() => OnUpdate.Invoke();

    private void LateUpdate()
    {
      OnBeforeLateUpdate.Invoke();
      OnLateUpdate.Invoke();
      OnAfterLateUpdate.Invoke();
    }
  }
}