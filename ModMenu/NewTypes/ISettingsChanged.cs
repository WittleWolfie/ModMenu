using Kingmaker.PubSubSystem;

namespace ModMenu.NewTypes
{
  /// <summary>
  /// Event interface that can be subscribed to on the EventBus that handles when Apply button is pressed in the Settings UI.
  /// </summary>
  /// <remarks>
  /// Your class must subscribe to the EventBus for this to trigger via EventBus.Subscribe(object subscriber)
  /// </remarks>
  public interface ISettingsChanged : ISubscriber, IGlobalSubscriber
  {
    /// <summary>
    /// Method triggered when with SettingsVM.ApplySettings() is called.
    /// </summary>
    void HandleApplySettings();
  }
}