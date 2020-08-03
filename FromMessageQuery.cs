using BotCore.Interfaces.BotEntities;

namespace BotCore {
  public class FromMessageQuery : IBotInputQuery {
    public int ChatId { get; }

    public string Payload { get; }

    public FromMessageQuery(int chatId, string payload) {
      ChatId = chatId;
      Payload = payload;
    }
  }
}
