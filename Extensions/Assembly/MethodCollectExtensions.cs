using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BotCore.Extensions.Assembly {
  public static class MethodCollectExtensions {
    private static bool HasAttribute<TAttribute>(this System.Reflection.MethodInfo methodInfo) where TAttribute : Attribute
      => methodInfo.GetCustomAttributes().Any(a => a is TAttribute);

    private static IEnumerable<System.Reflection.MethodInfo> GetMethodsWithAttribute<TAttribute>(this Type type)
      where TAttribute : Attribute =>
      type.GetMethods().Where(m => m.HasAttribute<TAttribute>());

    private static bool HasMethodsWithAttribute<TAttribute>(this Type type) where TAttribute : Attribute =>
      type.GetMethodsWithAttribute<TAttribute>().Any();

    public static List<MethodContainer<TAttribute>> CollectMethodsByAttribute<TAttribute>(
      this System.Reflection.Assembly assembly
    ) where TAttribute : Attribute {
      return assembly.GetTypes()
        .Where(type => type.HasMethodsWithAttribute<TAttribute>())
        .SelectMany(type => type.GetMethodsWithAttribute<TAttribute>())
        .Select(method => new MethodContainer<TAttribute>(method, method.GetCustomAttribute<TAttribute>()))
        .ToList();
    }
  }
}
