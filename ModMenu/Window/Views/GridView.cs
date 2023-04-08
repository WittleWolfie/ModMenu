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
  internal class GridView : ViewBase<GridViewVM>
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

  internal class GridViewVM : BaseDisposable, IViewModel
  {
    private readonly GridBuilder Grid;

    internal GridViewVM(GridBuilder grid)
    {
      Grid = grid;
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

      switch (Grid.Type)
      {
        case GridType.Abilities:
          var abilities =
            UIUtilityUnit.ClearFromDublicatedFeatures(
              UIUtilityUnit.CollectAbilityFeatures(Unit),
              UIUtilityUnit.CollectAbilities(Unit),
              UIUtilityUnit.CollectActivatableAbilities(Unit));
          foreach (var ability in abilities)
            Features.Add(new(ability));
          break;
      }

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
