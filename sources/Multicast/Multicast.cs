namespace Descindie.Legion.EventBus
{
  using System.Diagnostics;
  using System;

  /// <inheritdoc />
  [DebuggerTypeProxy(typeof(IMulticastDebugView<>))]
  [DebuggerDisplay("Count = {Count}")]
  public sealed class Multicast : BaseMulticast<Action>
  {
    public Multicast(int capacity = CAPACITY_DEFAULT)
    : base(capacity) { }

    public void Invoke()
    {
      using var enumerator = GetEnumerator();
      while (enumerator.MoveNext(out var invoke))
        invoke();
    }
  }
}