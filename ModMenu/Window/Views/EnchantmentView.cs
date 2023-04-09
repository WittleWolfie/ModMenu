using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.UI.MVVM._VM.Tooltip.Templates;
using Kingmaker.UI.ServiceWindow.CharacterScreen;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace ModMenu.Window.Views
{
  internal class EnchantmentView : VirtualListElementViewBase<EnchantmentVM>, IWidgetView
  {
    public MonoBehaviour MonoBehaviour => this;

    public override void BindViewImplementation()
    {
      gameObject.SetActive(true);
    }

    public void BindWidgetVM(IViewModel vm)
    {
      Bind(vm as EnchantmentVM);
    }

    public bool CheckType(IViewModel viewModel)
    {
      throw new System.NotImplementedException();
    }

    public override void DestroyViewImplementation()
    {
    }
  }

  internal class EnchantmentVM : VirtualListElementVMBase
  {
    internal Sprite Icon;
    internal string DisplayName;
    internal string Description;
    internal int EnhancementCost;
    internal TooltipBaseTemplate Tooltip;
    internal ReactiveProperty<bool> IsActive = new();

    internal EnchantmentVM(BlueprintAbility ability)
    {
      Icon = ability.Icon;
      DisplayName = ability.Name;
      Description = ability.Description;
      EnhancementCost = 3;
      Tooltip = new TooltipTemplateAbility(ability);
      IsActive.Value = false;
    }

    public override void DisposeImplementation() { }
  }
}
