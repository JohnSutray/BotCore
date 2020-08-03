using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace BotCore.Extensions.ServiceProvider {
  public static class MethodInjectionExtensions {
    public static TResult InvokeWithInjection<TResult>(
      this System.Reflection.MethodInfo methodInfo,
      object context,
      IServiceProvider provider
    ) {
      var dependencies = methodInfo.GetParameters()
        .Select(p => p.ParameterType)
        .Select(provider.GetRequiredService);

      return (TResult)methodInfo.Invoke(context, dependencies.ToArray());
    }
  }
}
