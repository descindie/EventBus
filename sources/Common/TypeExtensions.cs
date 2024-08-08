namespace Descindie.Legion
{
  using System.Reflection;
  using System;
  using UnityEngine;

  public static class TypeExtensions
  {
    public static string GetFullName(this Type target)
    {
      var attribute = target.GetCustomAttribute<AddComponentMenu>();
      return (attribute != null) ? attribute.componentMenu : target.Name;
    }
  }
}