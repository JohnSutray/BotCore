using System;
using JetBrains.Annotations;

namespace BotCore.Attributes {
  [MeansImplicitUse]
  [AttributeUsage(AttributeTargets.Method)]
  public class BotQueryAction: Attribute {
    public string Template { get; }
    public bool ClearDisplayBeforeHandle { get; }

    public BotQueryAction(
      string template,
      bool clearDisplayBeforeHandle = true
    ) {
      Template = template;
      ClearDisplayBeforeHandle = clearDisplayBeforeHandle;
    }
  }
}
