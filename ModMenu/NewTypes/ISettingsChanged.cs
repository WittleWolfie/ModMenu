using Kingmaker.PubSubSystem;

namespace ModMenu.NewTypes
{
  // Event interface that can be subscribed to in PubSubSytem that handles when Apply Settings happens in the Settings UI
  public interface ISettingsChanged : ISubscriber, IGlobalSubscriber
  {
    void HandleApplySettings();
  }
}