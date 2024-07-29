namespace Descindie.Legion.EventBus
{
  using System.Diagnostics;
  using System;

  public sealed class IMulticastDebugView<T>
  where T : Delegate
  {
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public T[] InvocationList => _multicast.ToArray();

    private readonly IMulticast<T> _multicast;

    /// <exception cref="ArgumentNullException" />
    public IMulticastDebugView(IMulticast<T> multicast) => _multicast = multicast ?? throw new ArgumentNullException(nameof(multicast));
  }
}