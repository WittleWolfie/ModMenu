using Kingmaker;
using Kingmaker.UI.MVVM._PCView.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.UI.MVVM._VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.UnitLogic;
using ModMenu.Utils;
using Owlcat.Runtime.UI.MVVM;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace ModMenu.Window.Views
{
  /// <summary>
  /// Scrolling grid based on the spellbook known spells grid
  /// </summary>
  internal class DataGridView : ViewBase<DataGridViewVM>
  {
    // Track children so they are not retained when the window is destroyed
    private readonly List<Transform> Children = new();

    public override void BindViewImplementation()
    {
      gameObject.SetActive(true);
      Refresh();
      // Subscribe to the view model which will call this whenever the view should update
      ViewModel.Subscribe(Refresh);
    }

    public override void DestroyViewImplementation()
    {
      gameObject.SetActive(false);

      Children.ForEach(child => GameObject.DestroyImmediate(child.gameObject));
      Children.Clear();
    }

    private void Refresh()
    {
      // TODO: Should there be a bind callback here?
      // TODO: How to handle clicks on items?

      // CharInfoFeatures are copied as is from Owlcat so there's no Element class or styling.
      ViewModel.CharInfoFeatures.ForEach(
        feature =>
        {
          var view = GameObject.Instantiate<CharInfoFeaturePCView>(Prefabs.Feature);
          view.Bind(feature);
          view.transform.AddTo(Grid);
          Children.Add(view.transform);
        });
    }

    internal Transform Grid; 
  }

  internal class DataGridViewVM : BaseDisposable, IViewModel
  {
    internal List<CharInfoFeatureVM> CharInfoFeatures = new();

    private readonly WindowBuilder.GetFeatures FeatureProvider;

    private Action OnRefresh;
    private UnitDescriptor Unit => SelectedUnit.Value;
    private readonly ReactiveProperty<UnitDescriptor> SelectedUnit = new();

    internal DataGridViewVM(WindowBuilder.GetFeatures featureProvider)
    {
      FeatureProvider = featureProvider;
      AddDisposable(
        Game.Instance.SelectionCharacter.SelectedUnit.Subscribe(
          unit =>
          {
            SelectedUnit.Value = unit.Value;
            Refresh();
          }));
    }

    public override void DisposeImplementation() { }

    private void Refresh()
    {
      Main.Logger.NativeLog($"Refreshing: {Unit}");
      foreach (var vm in CharInfoFeatures)
        vm.Dispose();

      CharInfoFeatures = new();

      if (Unit is null)
        return;

      foreach (var feature in FeatureProvider.Invoke(Unit))
        CharInfoFeatures.Add(new(feature));

      Main.Logger.NativeLog($"Feature count for {Unit.CharacterName}: {CharInfoFeatures.Count}");
      OnRefresh?.Invoke();
    }

    internal void Subscribe(Action onRefresh)
    {
      OnRefresh = onRefresh;
    }
  }
}
