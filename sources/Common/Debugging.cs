namespace Descindie.Legion.EventBus
{
  using System.Diagnostics;
  using UnityEngine;
  using static UnityEngine.Debug;

  public static class Debugging
  {
    [Conditional("UNITY_ASSERTIONS")]
    public static void AssertExistsSingleInstanceOnly(Component item)
    {
      var type = item.GetType();
      var objectCount = Object.FindObjectsOfType(type, includeInactive: true).Length;
      Assert(objectCount == 1, $"Component {type.GetFullName()}, must be only one on the scene.", item);
    }
  }
}