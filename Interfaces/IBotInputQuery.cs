namespace BotCore.Interfaces.BotEntities {
  public interface IBotInputQuery {
    public int ChatId { get; }
    public string Payload { get; }
  }
}
