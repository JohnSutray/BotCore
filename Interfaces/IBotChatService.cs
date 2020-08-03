namespace BotCore.Interfaces {
  public interface IBotChatService {
    public void UpdateLastExecutedQuery(IBotInputChat chat, string query);
    public void EnsureChatSaved(IBotInputChat inputChat);
    public IBotChat FindChat(IBotInputChat inputChat);
  }
}
