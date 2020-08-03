namespace BotCore.Interfaces {
  public interface IBotChat {
    public int Id { get; }
    public string LastExecutedQuery { get; }
    public string Address { get; }
    public string Phone { get; }
  }
}
