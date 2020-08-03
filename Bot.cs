using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BotCore.Attributes;
using BotCore.Extensions.Assembly;
using BotCore.Extensions.MethodInfo;
using BotCore.Extensions.Query;
using BotCore.Extensions.ServiceProvider;
using BotCore.Interfaces;
using BotCore.Interfaces.BotEntities;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BotCore {
  using QueryAction = MethodContainer<BotQueryAction>;
  using MessageAction = MethodContainer<BotMessageAction>;
  using ViewAction = MethodContainer<BotViewAction>;

  public abstract class Bot {
    // TODO: create first unremovable message functionality
    private List<QueryAction> _queryActions;
    private List<MessageAction> _messageActions;
    private List<MethodContainer<BotViewAction>> _viewActions;

    protected abstract void ConfigureServices(IServiceCollection services);

    protected IBotEventSource EventSource { get; set; }

    public virtual void Start(Assembly workingAssembly) {
      InitActions(workingAssembly);
      ValidateMessageActions();
      EventSource.OnMessage += HandleMessage;
      EventSource.OnQuery += HandleQuery;
    }

    public virtual void Stop() {
    }

    private void InitActions(Assembly workingAssembly) {
      _queryActions = workingAssembly.CollectMethodsByAttribute<BotQueryAction>().ToList();
      _messageActions = workingAssembly.CollectMethodsByAttribute<BotMessageAction>()
        .OrderByDescending(action => action.Metadata.Priority).ToList();
      _viewActions = workingAssembly.CollectMethodsByAttribute<BotViewAction>();
    }

    private void ValidateMessageActions() => _messageActions.ForEach(
      container => container.Method.ValidateReturnType(typeof(string))
    );

    private IServiceCollection CollectBaseServiceCollection() {
      var collection = new ServiceCollection();
      ConfigureServices(collection);

      void AddToServices<T>(MethodContainer<T> container) =>
        collection.AddTransient(container.Method.DeclaringType);

      _messageActions.ForEach(AddToServices);
      _queryActions.ForEach(AddToServices);
      _viewActions.ForEach(AddToServices);

      return collection;
    }

    private void TryToExecute(Action action) {
      try {
        action();
      }
      catch (Exception e) {
        Console.WriteLine(e);
      }
    }

    private void HandleMessage(IBotInputMessage inputMessage, IBotInputChat inputChat)
      => TryToExecute(() => RootMessageHandler(inputMessage, inputChat));

    private void HandleQuery(IBotInputQuery inputQuery, IBotInputChat inputChat)
      => TryToExecute(() => RootQueryHandler(inputQuery, inputChat));

    private bool IsMessageActionMatch(MessageAction action, IBotChat chat, IBotInputMessage message) =>
      action.Metadata.MessagePattern.IsMatch(message.Content)
      && action.Metadata.LatestQuery.IsMatch(chat.LastExecutedQuery);

    private void Log(object obj) => Console.WriteLine(
      JsonConvert.SerializeObject(
        obj,
        Formatting.Indented,
        new JsonSerializerSettings {
          ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
          MaxDepth = 4
        }
      )
    );

    private void RootMessageHandler(IBotInputMessage inputMessage, IBotInputChat inputChat) {
      using var provider = BuildMessageControllerProvider(inputMessage, inputChat);
      EnsureChatSaved(provider, inputChat);
      var chat = FindChat(provider, inputChat);
      var messageAction = FindMessageAction(chat, inputMessage);
      var query = ConvertMessageToQuery(provider, messageAction, inputChat);

      Log(inputChat);
      Log(chat);
      Log(inputMessage);
      SaveMessage(provider, inputChat, inputMessage);

      if (messageAction == null) {
        throw new ArgumentOutOfRangeException("No message handler for this message");
      }

      RootQueryHandler(query, inputChat);
    }

    private MessageAction FindMessageAction(IBotChat chat, IBotInputMessage message) =>
      _messageActions.FirstOrDefault(action => IsMessageActionMatch(action, chat, message));

    private IBotChat FindChat(IServiceProvider provider, IBotInputChat chat) => provider
      .GetRequiredService<IBotChatService>().FindChat(chat);

    private IBotInputQuery ConvertMessageToQuery(
      IServiceProvider provider, MessageAction action, IBotInputChat chat
    ) => new FromMessageQuery(
      chat.Id,
      action.Method.InvokeWithInjection<string>(
        provider.GetRequiredService(action.Method.DeclaringType),
        provider
      )
    );

    private void SaveMessage(
      IServiceProvider provider, IBotInputChat chat, IBotInputMessage message
    ) => provider.GetRequiredService<IBotMessageService>().SaveMessage(chat, message);

    private void EnsureChatSaved(IServiceProvider provider, IBotInputChat chat) => provider
      .GetRequiredService<IBotChatService>().EnsureChatSaved(chat);

    private void UpdateLastExecutedQuery(
      IServiceProvider provider, QueryAction action, IBotInputChat chat
    ) => provider.GetRequiredService<IBotChatService>().UpdateLastExecutedQuery(chat, action.Metadata.Template);

    private void CheckDisplayClearing(IServiceProvider provider, QueryAction action, IBotInputChat chat) {
      if (action.Metadata.ClearDisplayBeforeHandle) {
        provider.GetRequiredService<IBotMessageService>().ClearMessages(chat);
      }
    }

    private void RootQueryHandler(IBotInputQuery inputQuery, IBotInputChat inputChat) {
      var queryAction = FindQueryAction(inputQuery.Payload);
      var matchedData = queryAction.Metadata.Template.MatchRoute(inputQuery.Payload);
      using var provider = BuildQueryControllerProvider(inputQuery, inputChat, matchedData);

      Log(inputQuery);
      Log(inputChat);
      Log(queryAction.Metadata);
      Log(matchedData);
      EnsureChatSaved(provider, inputChat);
      CheckDisplayClearing(provider, queryAction, inputChat);
      HandleView(HandleQuery(provider, queryAction.Method), inputChat);
      UpdateLastExecutedQuery(provider, queryAction, inputChat);
    }

    private void HandleView(object viewModel, IBotInputChat chat) {
      var provider = BuildViewProvider(chat, viewModel);
      var viewAction = FindViewAction(viewModel, chat);
      var controller = provider.GetRequiredService(viewAction.Method.DeclaringType);

      viewAction.Method.InvokeWithInjection<object>(controller, provider);
    }

    private ViewAction FindViewAction(object viewModel, IBotInputChat chat) => _viewActions.FirstOrDefault(
      action => action.Metadata.ViewModelType == viewModel.GetType()
                && action.Metadata.PlatformId == chat.PlatformId
    );

    private QueryAction FindQueryAction(string queryString) => _queryActions.FirstOrDefault(
      action => action.Metadata.Template.IsRouteMatched(queryString)
    );

    private object HandleQuery(IServiceProvider provider, MethodInfo handler) =>
      handler.InvokeWithInjection<object>(provider.GetRequiredService(handler.DeclaringType), provider);

    private ServiceProvider BuildQueryControllerProvider(
      IBotInputQuery inputQuery, IBotInputChat inputChat, RouteValueDictionary matchedData
    ) => CollectBaseServiceCollection()
      .AddSingleton(inputQuery)
      .AddSingleton(matchedData)
      .AddSingleton(inputChat)
      .BuildServiceProvider();

    private ServiceProvider BuildMessageControllerProvider(
      IBotInputMessage message, IBotInputChat inputChat
    ) => CollectBaseServiceCollection()
      .AddSingleton(message)
      .AddSingleton(inputChat)
      .BuildServiceProvider();

    private ServiceProvider BuildViewProvider(
      IBotInputChat inputChat, object viewModel
    ) => CollectBaseServiceCollection()
      .AddSingleton(inputChat)
      .AddSingleton(viewModel.GetType(), viewModel)
      .BuildServiceProvider();
  }
}
