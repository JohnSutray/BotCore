namespace BotCore.Interfaces {
  public interface IBotMessageService {
    public void ClearMessages(IBotInputChat chat);
    public void SaveMessage(IBotInputChat chat, IBotInputMessage message);
  }
}
