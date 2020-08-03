using BotCore.Interfaces.BotEntities;

namespace BotCore.Interfaces {
  public delegate void MessageHandler(IBotInputMessage inputMessage,  IBotInputChat inputChat);
  public delegate void QueryHandler(IBotInputQuery message, IBotInputChat inputChat);

  public interface IBotEventSource {
    event MessageHandler OnMessage;
    event QueryHandler OnQuery;
  }
}
