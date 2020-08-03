namespace BotCore.Interfaces {
  public interface IBotInputMessage {
    public int Id { get; }
    public int ChatId { get; }
    public string Content { get; }
  }
}
