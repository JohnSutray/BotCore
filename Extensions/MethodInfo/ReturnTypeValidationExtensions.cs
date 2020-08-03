using System;

namespace BotCore.Extensions.MethodInfo {
  public static class ReturnTypeValidationExtensions {
    public static void ValidateReturnType(
      this System.Reflection.MethodInfo method,
      Type expectedType
    ) {
      if (method.ReturnType == expectedType) return;

      throw new ArgumentOutOfRangeException(
        "Invalid controller action return type. " +
        $"${method.DeclaringType.Name}.${method.Name}() => ${method.ReturnType.Name} " +
        $"Return data must be of type ${expectedType.Name}"
      );
    }
  }
}
