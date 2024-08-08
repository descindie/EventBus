namespace Descindie.Legion.EventBus
{
  using System.Runtime.CompilerServices;
  using System;

  internal static class DelegateExtensions
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeHash<T>(this T invocation) where T : Delegate
    {
      var target = invocation.Target;
      return target != null
        ? target.GetHashCode()
        : invocation.Method.GetHashCode();
    }
  }
}