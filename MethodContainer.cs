using System.Reflection;

namespace BotCore {
  public class MethodContainer<TMetadata> {
    public MethodInfo Method { get; }
    public TMetadata Metadata { get; }

    public MethodContainer(MethodInfo method, TMetadata metadata) {
      Method = method;
      Metadata = metadata;
    }
  }
}
