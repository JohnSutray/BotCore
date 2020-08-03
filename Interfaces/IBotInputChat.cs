namespace BotCore.Interfaces {
  public interface IBotInputChat {
    public int Id { get; }
    public string PlatformId { get; }
    public string FirstName { get; }
    public string LastName { get; }
  }
}
