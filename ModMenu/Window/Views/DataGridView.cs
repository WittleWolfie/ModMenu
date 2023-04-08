using Kingmaker;
using Kingmaker.UI.Common;
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
  internal class DataGridView : ViewBase<DataGridViewVM>
  {
    private readonly List<Transform> Children = new();

    public override void BindViewImplementation()
    {
      gameObject.SetActive(true);
      Refresh();
      ViewModel.OnRefresh(Refresh);
    }

    public override void DestroyViewImplementation()
    {
      gameObject.SetActive(false);

      Children.ForEach(child => GameObject.DestroyImmediate(child.gameObject));
      Children.Clear();
    }

    private void Refresh()
    {
      ViewModel.Features.ForEach(
        feature =>
        {
          Main.Logger.Log("Binding a feature");
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
    private readonly WindowBuilder.GetFeatures FeatureProvider;

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
      Main.Logger.Log($"Refreshing: {Unit}");
      foreach (var vm in Features)
        vm.Dispose();

      Features = new();

      if (Unit is null)
        return;

      foreach (var feature in FeatureProvider.Invoke(Unit))
        Features.Add(new(feature));

      Main.Logger.Log($"Feature count for {Unit.CharacterName}: {Features.Count}");
      RefreshView?.Invoke();
    }

    internal void OnRefresh(Action refreshView)
    {
      RefreshView = refreshView;
    }

    private Action RefreshView;
    private UnitDescriptor Unit => SelectedUnit.Value;
    internal readonly ReactiveProperty<UnitDescriptor> SelectedUnit = new();

    internal List<CharInfoFeatureVM> Features = new();
  }
}
