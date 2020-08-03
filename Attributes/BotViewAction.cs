using System;
using JetBrains.Annotations;

namespace BotCore.Attributes {
  [MeansImplicitUse]
  [AttributeUsage(AttributeTargets.Method)]
  public class BotViewAction: Attribute {
    public BotViewAction(Type viewModelType, string platformId) {
      ViewModelType = viewModelType;
      PlatformId = platformId;
    }

    public Type ViewModelType { get; }
    public string PlatformId { get; }
  }
}
