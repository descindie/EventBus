namespace Descindie.Legion.EventBus
{
  using System.Runtime.CompilerServices;
  using System.Diagnostics;

  public delegate void EventBusEventHandler<T>(ref T e)
  where T : struct;

  /// <summary>
  /// Low-allocating event bus.
  /// </summary>
  [DebuggerTypeProxy(typeof(IMulticastDebugView<>))]
  [DebuggerDisplay("Count = {Count}")]
  public sealed class EventBus<T> : BaseMulticast<EventBusEventHandler<T>>
  where T : struct, IEventBusEvent
  {
    public static EventBus<T> Instance
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _instance != null
        ? _instance
        : _instance = new();
    }

    private static EventBus<T> _instance;

    public EventBus(int capacity = CAPACITY_DEFAULT)
    : base(capacity) { }

    public void RaiseEvent(T e)
    {
      using var enumerator = GetEnumerator();
      while (enumerator.MoveNext(out var invoke))
        invoke(ref e);
    }
  }
}