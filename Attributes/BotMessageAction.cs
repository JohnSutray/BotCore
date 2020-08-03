using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace BotCore.Attributes {
  [MeansImplicitUse]
  [AttributeUsage(AttributeTargets.Method)]
  public class BotMessageAction : Attribute {
    // TODO: remove this field and type
    public Regex MessagePattern { get; }
    public Regex LatestQuery { get; }
    public int Priority { get; }

    public BotMessageAction(
      [RegexPattern] string messagePattern,
      [RegexPattern] string queryPattern,
      int priority = 1
    ) {
      MessagePattern = new Regex(messagePattern);
      LatestQuery = new Regex(queryPattern);
      Priority = priority;
    }
  }
}
