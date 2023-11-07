using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Localization;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM;
using Kingmaker.UI.MVVM._PCView.ServiceWindows.Journal;
using Kingmaker.UI.MVVM._PCView.Settings;
using Kingmaker.UI.MVVM._PCView.Settings.Entities;
using Kingmaker.UI.MVVM._PCView.Settings.Entities.Decorative;
using Kingmaker.UI.MVVM._VM.Settings;
using Kingmaker.UI.MVVM._VM.Settings.Entities;
using Kingmaker.UI.MVVM._VM.Settings.Entities.Decorative;
using Kingmaker.UI.MVVM._VM.Settings.Entities.Difficulty;
using Kingmaker.UI.SettingsUI;
using Kingmaker.Utility;
using ModMenu.Settings;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Kingmaker.UI.SettingsUI.UISettingsManager;
using Object = UnityEngine.Object;

namespace ModMenu.NewTypes
{
  internal class SettingsEntityPatches
  {
    internal static readonly FieldInfo OverrideType =
      AccessTools.Field(typeof(VirtualListLayoutElementSettings), "m_OverrideType");

    /// <summary>
    /// Patch to prevent exceptions on deserializing settings
    /// </summary>
    [HarmonyPatch]
    static class DictionarySettingsProviderPatcher
      {
        [HarmonyTargetMethod]
        static MethodInfo TargetMethod()
        {
          return typeof(DictionarySettingsProvider)
                .GetMethod(nameof(DictionarySettingsProvider.GetValue))
                .MakeGenericMethod(typeof(ModsMenuEntry));
        }

        [HarmonyPrefix]
        public static bool DeserializeSettingEntry(string key, ref ModsMenuEntry __result)
        {
          if (key.Equals(SettingsEntityModMenuEntry.instance.Key))
          {
            __result = ModsMenuEntry.EmptyInstance;
            return false;
          }
          return true;
        }
      }

    /// <summary>
    /// Patch to change the way dropdown options are generated so that the settings description would show individual mod descriptions.
    /// </summary>
    [HarmonyPatch]
    static class SettingsEntityDropdownPCView_Patch
    {
      [HarmonyPatch(typeof(SettingsEntityDropdownPCView), nameof(SettingsEntityDropdownPCView.SetupDropdown))]
      static bool Prefix(SettingsEntityDropdownPCView __instance)
      {
        if (__instance.ViewModel.m_UISettingsEntity is not UISettingsEntityDropdownModMenuEntry) return true;

        else
        __instance.Dropdown.gameObject.SetActive(true);
        __instance.Dropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> options = new();
        var vm = GameObject.Find("CommonPCView(Clone)/Canvas/SettingsView/")?.GetComponent<SettingsPCView>()?.ViewModel;
        foreach (var modEntry in ModsMenuEntity.ModEntries)
        {
          options.Add(
          new DropdownOptionWithHighlightCallback()
          {
            m_Text = modEntry.ModInfo.ModName,
            OnMouseEnter = new() { new(_ => {
              if (vm is null)
                Main.Logger.Warning("SettingsEntityDropdownPCView_Patch - settings VM is null!");
              else
                vm.HandleShowSettingsDescription(
                  title: UIUtility.GetSaberBookFormat(modEntry.ModInfo.ModName, default(Color), 140, null, 0f),
                  description: modEntry.ModInfo.GenerateDescription());
              })}
          });
        }
        __instance.Dropdown.AddOptions(options);
        return false;
      }
    }

    /// <summary>
    /// Patch to return the correct view model for <see cref="UISettingsEntityImage"/>
    /// </summary>
    [HarmonyPatch(typeof(SettingsVM))]
    static class SettingsVM_Patch
    {
      [HarmonyPatch(nameof(SettingsVM.GetVMForSettingsItem)), HarmonyPrefix]
      static bool Prefix(
        UISettingsEntityBase uiSettingsEntity, ref VirtualListElementVMBase __result)
      {
        try
        {
          if (uiSettingsEntity is UISettingsEntityImage imageEntity)
          {
            Main.Logger.NativeLog("Returning SettingsEntityImageVM.");
            __result = new SettingsEntityImageVM(imageEntity);
            return false;
          }
          if (uiSettingsEntity is UISettingsEntityButton buttonEntity)
          {
            Main.Logger.NativeLog("Returning SettingsEntityButtonVM.");
            __result = new SettingsEntityButtonVM(buttonEntity);
            return false;
          }
          if (uiSettingsEntity is UISettingsEntitySubHeader subHeaderEntity)
          {
            Main.Logger.NativeLog("Returning SettingsEntitySubHeaderVM.");
            __result = new SettingsEntitySubHeaderVM(subHeaderEntity);
            return false;
          }
          if (uiSettingsEntity is UISettingsEntityDropdownButton dropdownButton)
          {
            Main.Logger.NativeLog("Returning SettingsEntityDropdownButtonVM.");
            __result = new SettingsEntityDropdownButtonVM(dropdownButton);
            return false;
          }
        }
        catch (Exception e)
        {
          Main.Logger.LogException("SettingsVM.GetVMForSettingsItem", e);
        }
        return true;
      }

      [HarmonyPatch(nameof(SettingsVM.SwitchSettingsScreen)), HarmonyPrefix]
      static bool Prefix(UISettingsManager.SettingsScreen settingsScreen, SettingsVM __instance)
      {
        if (settingsScreen != ModsMenuEntity.SettingsScreenId) return true;
        try
        {
        Main.Logger.NativeLog("Collecting setting entities.");

        __instance.m_SettingEntities.Clear();
        __instance.m_SettingEntities.Add(__instance.AddDisposableAndReturn(SettingsVM.GetVMForSettingsItem(UISettingsEntityDropdownModMenuEntry.instance)));
          if (UISettingsEntityDropdownModMenuEntry.instance.Setting.GetTempValue() == ModsMenuEntry.EmptyInstance)
            return false;
        //__instance.m_SettingEntities.Add(__instance.AddDisposableAndReturn(SettingsVM.GetVMForSettingsItem(separator)));

            //Here should be a toggle for mod disabling, but do we need it?
          SettingsEntitySubHeaderVM subheader;
          foreach (var uisettingsGroup in ModsMenuEntity.CollectSettingGroups)
          {
            __instance.m_SettingEntities.Add(__instance.AddDisposableAndReturn(new SettingsEntityHeaderVM(uisettingsGroup.Title)));
            subheader = null;
            foreach (UISettingsEntityBase uisettingsEntityBase in uisettingsGroup.VisibleSettingsList)
            {
              if (uisettingsEntityBase is UISettingsEntitySubHeader sub)
              {
                subheader = new SettingsEntitySubHeaderVM(sub);
                __instance.m_SettingEntities.Add(__instance.AddDisposableAndReturn(subheader));
                continue;
              }
              VirtualListElementVMBase element = __instance.AddDisposableAndReturn(SettingsVM.GetVMForSettingsItem(uisettingsEntityBase));
              __instance.m_SettingEntities.Add(element);
              subheader?.SettingsInGroup.Add(element);
            }
          }

        }
        catch (Exception e)
        {
          Main.Logger.LogException("SettingsVM.SwitchSettingsScreen", e);
        }
        return false;
      }

      /*[HarmonyPatch(nameof(SettingsVM.SwitchSettingsScreen)), HarmonyPostfix]
      static void Postfix(UISettingsManager.SettingsScreen settingsScreen, SettingsVM __instance)
      {
        try
        {
          if (settingsScreen != ModsMenuEntity.SettingsScreenId) { return; }
          Main.Logger.NativeLog("Configuring header buttons.");

          // Add all settings in each group to the corresponding expand/collapse button
          SettingsEntityCollapsibleHeaderVM headerVM = null;
          SettingsEntitySubHeaderVM subHeaderVM = null;
          for (int i = 0; i < __instance.m_SettingEntities.Count; i++)
          {
            var entity = __instance.m_SettingEntities[i];
            if (entity is SettingsEntitySubHeaderVM subHeader)
            {
              subHeaderVM = subHeader;
              if (headerVM is not null)
                headerVM.SettingsInGroup.Add(subHeaderVM); // Sub headers are nested in headers
              continue;
            }
            else if (entity is SettingsEntityHeaderVM header)
            {
              headerVM = new SettingsEntityCollapsibleHeaderVM(header.Tittle);
              __instance.m_SettingEntities[i] = headerVM;
              subHeaderVM = null; // Make sure we stop counting sub header entries
              continue;
            }

            if (headerVM is not null)
              headerVM.SettingsInGroup.Add(entity);
            if (subHeaderVM is not null)
              subHeaderVM.SettingsInGroup.Add(entity);
          }
        }
        catch (Exception e)
        {
          Main.Logger.LogException("SettingsVM.SwitchSettingsScreen", e);
        }
      }*/
    }

    /// <summary>
    /// Patch to add new setting type prefabs.
    /// </summary>
    [HarmonyPatch(typeof(SettingsPCView.SettingsViews))]
    static class SettingsViews_Patch
    {
      [HarmonyPatch(nameof(SettingsPCView.SettingsViews.InitializeVirtualList)), HarmonyPrefix]
      static bool Prefix(SettingsPCView.SettingsViews __instance, VirtualListComponent virtualListComponent)
      {
        try
        {
          Main.Logger.NativeLog("Adding new type prefabs.");

          // Copy the bool settings
          var copyFrom = __instance.m_SettingsEntityBoolViewPrefab.gameObject;
          var imageTemplate = CreateImageTemplate(Object.Instantiate(copyFrom));
          var buttonTemplate =
            CreateButtonTemplate(Object.Instantiate(copyFrom),
            __instance.m_SettingsEntitySliderVisualPerceptionViewPrefab?.m_ResetButton);

          var headerTemplate =
            CreateCollapsibleHeaderTemplate(
              Object.Instantiate(__instance.m_SettingsEntityHeaderViewPrefab.gameObject));
          var subHeaderTemplate = CreateSubHeaderTemplate(Object.Instantiate(headerTemplate.gameObject));

          // Copy dropdown since you know, it seems like close to dropdown button right?
          var dropdownButtonTemplate =
            CreateDropdownButtonTemplate(
              Object.Instantiate(__instance.m_SettingsEntityDropdownViewPrefab.gameObject),
              __instance.m_SettingsEntitySliderVisualPerceptionViewPrefab?.m_ResetButton);

          virtualListComponent.Initialize(new IVirtualListElementTemplate[]
          {
            new VirtualListElementTemplate<SettingsEntityHeaderVM>(__instance.m_SettingsEntityHeaderViewPrefab),
            new VirtualListElementTemplate<SettingsEntityBoolVM>(__instance.m_SettingsEntityBoolViewPrefab),
            new VirtualListElementTemplate<SettingsEntityDropdownVM>(__instance.m_SettingsEntityDropdownViewPrefab, 0),
            new VirtualListElementTemplate<SettingsEntitySliderVM>(__instance.m_SettingsEntitySliderViewPrefab, 0),
            new VirtualListElementTemplate<SettingEntityKeyBindingVM>(__instance.m_SettingEntityKeyBindingViewPrefab),
            new VirtualListElementTemplate<SettingsEntityDropdownVM>(__instance.m_SettingsEntityDropdownDisplayModeViewPrefab, 1),
            new VirtualListElementTemplate<SettingsEntityDropdownGameDifficultyVM>(__instance.m_SettingsEntityDropdownGameDifficultyViewPrefab, 0),
            new VirtualListElementTemplate<SettingsEntitySliderVM>(__instance.m_SettingsEntitySliderVisualPerceptionViewPrefab, 1),
            new VirtualListElementTemplate<SettingsEntitySliderVM>(__instance.m_SettingsEntitySliderVisualPerceptionWithImagesViewPrefab, 2),
            new VirtualListElementTemplate<SettingsEntityStatisticsOptOutVM>(__instance.m_SettingsEntityStatisticsOptOutViewPrefab),
            new VirtualListElementTemplate<SettingsEntityImageVM>(imageTemplate),
            new VirtualListElementTemplate<SettingsEntityButtonVM>(buttonTemplate),
            new VirtualListElementTemplate<SettingsEntityCollapsibleHeaderVM>(headerTemplate),
            new VirtualListElementTemplate<SettingsEntitySubHeaderVM>(subHeaderTemplate),
            new VirtualListElementTemplate<SettingsEntityDropdownButtonVM>(dropdownButtonTemplate, 0),
          });
        }
        catch (Exception e)
        {
          Main.Logger.LogException("SettingsViews_Patch", e);
        }
        return false;
      }

      private static SettingsEntityButtonView CreateButtonTemplate(GameObject prefab, OwlcatButton buttonPrefab)
      {
        Main.Logger.NativeLog("Creating button template.");

        // Destroy the stuff we don't want from the source prefab
        Object.DestroyImmediate(prefab.GetComponent<SettingsEntityBoolPCView>());
        Object.DestroyImmediate(prefab.transform.Find("MultiButton").gameObject);
        Object.DontDestroyOnLoad(prefab);

        OwlcatButton buttonControl = null;
        TextMeshProUGUI buttonLabel = null;

        // Add in our own button
        if (buttonPrefab != null)
        {
          var button = Object.Instantiate(buttonPrefab.gameObject, prefab.transform);
          buttonControl = button.GetComponent<OwlcatButton>();
          buttonLabel = button.GetComponentInChildren<TextMeshProUGUI>();

          var layout = button.AddComponent<LayoutElement>();
          layout.ignoreLayout = true;

          var rect = button.transform as RectTransform;

          rect.anchorMin = new(1, 0.5f);
          rect.anchorMax = new(1, 0.5f);
          rect.pivot = new(1, 0.5f);

          rect.anchoredPosition = new(-55, 0);
          rect.sizeDelta = new(430, 45);
        }

        // Add our own View (after destroying the Bool one)
        var templatePrefab = prefab.AddComponent<SettingsEntityButtonView>();

        // Wire up the fields that would have been deserialized if coming from a bundle
        templatePrefab.HighlightedImage =
          prefab.transform.Find("HighlightedImage").gameObject.GetComponent<Image>();
        templatePrefab.Title =
          prefab.transform.Find("HorizontalLayoutGroup/Text").gameObject.GetComponent<TextMeshProUGUI>();
        templatePrefab.Button = buttonControl;
        templatePrefab.ButtonLabel = buttonLabel;

        return templatePrefab;
      }

      private static SettingsEntityImageView CreateImageTemplate(GameObject prefab)
      {
        Main.Logger.NativeLog("Creating image template.");

        //Destroy the stuff we don't want from the source prefab
        Object.DestroyImmediate(prefab.GetComponent<SettingsEntityBoolPCView>());
        Object.DestroyImmediate(prefab.transform.Find("MultiButton").gameObject);
        Object.DestroyImmediate(prefab.transform.Find("HorizontalLayoutGroup").gameObject);
        Object.DestroyImmediate(prefab.transform.Find("HighlightedImage").gameObject);
        Object.DontDestroyOnLoad(prefab);

        // Add our own View (after destroying the Bool one)
        var templatePrefab = prefab.AddComponent<SettingsEntityImageView>();

        // Create an imagePrefab as a child of the view so it can be scaled independently
        var imagePrefab = new GameObject("banner", typeof(RectTransform));
        imagePrefab.transform.SetParent(templatePrefab.transform, false);

        // Wire up the fields that would have been deserialized if coming from a bundle
        templatePrefab.Icon = imagePrefab.AddComponent<Image>();
        templatePrefab.Icon.preserveAspect = true;
        templatePrefab.TopBorder = prefab.transform.Find("TopBorderImage").gameObject;

        return templatePrefab;
      }

      private static SettingsEntityCollapsibleHeaderView CreateCollapsibleHeaderTemplate(GameObject prefab)
      {
        Main.Logger.NativeLog("Creating collapsible header template.");

        // Destroy the stuff we don't want from the source prefab
        Object.DestroyImmediate(prefab.GetComponent<SettingsEntityHeaderView>());
        Object.DontDestroyOnLoad(prefab);

        var buttonPC = prefab.GetComponentInChildren<ExpandableCollapseMultiButtonPC>();
        var buttonPrefab = buttonPC.gameObject;
        buttonPrefab.transform.Find("_CollapseArrowImage").gameObject.SetActive(true);
        var button = buttonPrefab.GetComponent<OwlcatMultiButton>();
        button.Interactable = true;

        // Add our own View
        var templatePrefab = prefab.AddComponent<SettingsEntityCollapsibleHeaderView>();
        templatePrefab.Title = prefab.transform.FindRecursive("Label").GetComponent<TextMeshProUGUI>();
        templatePrefab.Button = button;
        templatePrefab.ButtonPC = buttonPC;
        return templatePrefab;
      }

      // Prefab from the SettingsEntityCollapsibleHeaderView
      private static SettingsEntitySubHeaderView CreateSubHeaderTemplate(GameObject prefab)
      {
        Main.Logger.NativeLog("Creating sub header template.");

        // Destroy the stuff we don't want from the source prefab
        Object.DestroyImmediate(prefab.GetComponent<SettingsEntityCollapsibleHeaderView>());
        Object.DontDestroyOnLoad(prefab);

        // Add our own view
        var templatePrefab = prefab.AddComponent<SettingsEntitySubHeaderView>();
        templatePrefab.Title = prefab.transform.FindRecursive("Label").GetComponent<TextMeshProUGUI>();
        templatePrefab.Button = prefab.GetComponentInChildren<OwlcatMultiButton>();
        templatePrefab.ButtonPC = prefab.GetComponentInChildren<ExpandableCollapseMultiButtonPC>();
        return templatePrefab;
      }

      private static SettingsEntityDropdownButtonView CreateDropdownButtonTemplate(
        GameObject prefab, OwlcatButton buttonPrefab)
      {
        Main.Logger.NativeLog("Creating dropdown button template.");

        // Destroy the stuff we don't want from the source prefab
        Object.DestroyImmediate(prefab.GetComponent<SettingsEntityDropdownPCView>());
        Object.DestroyImmediate(prefab.transform.Find("SetConnectionMarkerIamSet").gameObject);
        Object.DontDestroyOnLoad(prefab);

        OwlcatButton buttonControl = null;
        TextMeshProUGUI buttonLabel = null;

        // Add in our own button
        if (buttonPrefab != null)
        {
          var button = Object.Instantiate(buttonPrefab.gameObject, prefab.transform);
          buttonControl = button.GetComponent<OwlcatButton>();
          buttonLabel = button.GetComponentInChildren<TextMeshProUGUI>();

          var layout = button.AddComponent<LayoutElement>();
          layout.ignoreLayout = true;

          var rect = button.transform as RectTransform;

          rect.anchorMin = new(1, 0.5f);
          rect.anchorMax = new(1, 0.5f);
          rect.pivot = new(1, 0.5f);

          rect.anchoredPosition = new(-510, 0);
          rect.sizeDelta = new(215, 45);
        }

        // Add our own View (after destroying the Bool one)
        var templatePrefab = prefab.AddComponent<SettingsEntityDropdownButtonView>();

        // Wire up the fields that would have been deserialized if coming from a bundle
        templatePrefab.HighlightedImage =
          prefab.transform.Find("HighlightedImage").gameObject.GetComponent<Image>();
        templatePrefab.Title =
          prefab.transform.Find("HorizontalLayoutGroup/Text").gameObject.GetComponent<TextMeshProUGUI>();
        templatePrefab.Dropdown = prefab.GetComponentInChildren<TMP_Dropdown>();
        templatePrefab.Button = buttonControl;
        templatePrefab.ButtonLabel = buttonLabel;

        return templatePrefab;
      }

    }

    [HarmonyPatch]
    internal static class DefaultButtonPatcher
    {
      [HarmonyPatch(typeof(SettingsVM), nameof(SettingsVM.SetSettingsList))]
      [HarmonyTranspiler]
      static IEnumerable<CodeInstruction> SettingsVM_SetSettingsList_Transpiler_ToEnableDefaultButtonOnModsTab(IEnumerable<CodeInstruction> instructions)
      {
        var _inst = instructions.ToList();
        int length = _inst.Count;
        int index = -1;
        for (int i = 0; i < length; i++)
        {
          if (
            _inst[i + 0].opcode == OpCodes.Ldloc_0 &&
            _inst[i + 1].opcode == OpCodes.Ldfld && _inst[i + 1].operand is FieldInfo fi && fi.Name.Contains("settingsScreen") &&
            _inst[i + 2].opcode == OpCodes.Ldc_I4_4 &&
            _inst[i + 3].opcode == OpCodes.Beq_S || _inst[i + 3].opcode == OpCodes.Beq)
          {
            index = i;
            break;
          }
        }

        if (index == -1)
        {
          Main.Logger.Error("DefaultButtonPatcher - failed to find the index when transpile SettingsVM.SetSettingsList. Default button will not be enabled on the Mods tab of settings screen.");
          return instructions;
        }

        _inst.InsertRange(index + 4, new CodeInstruction[4] {
          new (_inst[index + 0]),
          new (_inst[index + 1]),
          new (OpCodes.Ldc_I4, ModsMenuEntity.SettingsScreenValue),
          new (_inst[index + 3]),
        });

        return _inst;
      }

      /// <summary>
      /// Will make Default button affect the mod selected on the Mod tab
      /// </summary>
      /// <returns></returns>
      [HarmonyPatch(typeof(SettingsController), nameof(SettingsController.ResetToDefault))]
      [HarmonyTranspiler]
      static IEnumerable<CodeInstruction> SettingsController_ResetToDefault_Transpiler_ToCollectModSettings(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
      {
        var _inst = instructions.ToList();
        int length = _inst.Count;
        int index = -1;
        FieldInfo settingsManagerInfo = typeof(Kingmaker.Game).GetField(nameof(Kingmaker.Game.UISettingsManager));
        MethodInfo gameGetter = typeof(Kingmaker.Game).GetProperty(nameof(Kingmaker.Game.Instance)).GetMethod;


        for (int i = 0; i < length; i++)
        {
          if (
            ((_inst[i + 0].opcode == OpCodes.Call || _inst[i + 0].opcode == OpCodes.Callvirt) && _inst[i + 0].operand is MethodInfo mi1 && mi1 == gameGetter) &&
            (_inst[i + 1].opcode == OpCodes.Ldfld && _inst[i + 1].operand is FieldInfo fi && fi == settingsManagerInfo) &&
            _inst[i + 2].opcode == OpCodes.Ldarg_0 &&
            _inst[i + 3].opcode == OpCodes.Newobj &&
            ((_inst[i + 4].opcode == OpCodes.Call || _inst[i + 4].opcode == OpCodes.Callvirt) && _inst[i + 4].operand is MethodInfo mi2 && mi2.Name.Contains("GetSettingsList")))
          {
            index = i;
            break;
          }
        }

        if (index == -1)
        {
          Main.Logger.Error("DefaultButtonPatcher - failed to find the index when transpile SettingsController.ResetToDefault. Default button will do nothing on the Mods tab.");
          return instructions;
        }

        Label labelNotMods = gen.DefineLabel();
        _inst[index].labels.Add(labelNotMods);

        Label labelIsMods = gen.DefineLabel();
        _inst[index+5].labels.Add(labelIsMods);

        MethodInfo mi = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList)).MakeGenericMethod(typeof(UISettingsGroup));

        _inst.InsertRange(index, new CodeInstruction[] {
          new CodeInstruction(OpCodes.Ldarg_0),
          //CodeInstruction.Call((UISettingsManager.SettingsScreen e) => Convert.ToInt32(e)), //WHY DOES IT NOT WORK?!?!?!?!?!
          //new CodeInstruction(OpCodes.Ldc_I4, ModsMenuEntity.SettingsScreenValue),
          //new CodeInstruction(OpCodes.Ceq),
          CodeInstruction.Call((UISettingsManager.SettingsScreen e) => AnotherScreenCheck(e)),
          new CodeInstruction(OpCodes.Brfalse_S, labelNotMods),
          new CodeInstruction(OpCodes.Call, typeof(ModsMenuEntity).GetProperty(nameof(ModsMenuEntity.CollectSettingGroups), BindingFlags.Static | BindingFlags.NonPublic).GetMethod),
          new CodeInstruction(OpCodes.Callvirt, mi),
          new CodeInstruction(OpCodes.Br_S, labelIsMods)
        });;

        return _inst;
      }

      static bool AnotherScreenCheck(UISettingsManager.SettingsScreen e) => e == (UISettingsManager.SettingsScreen)ModsMenuEntity.SettingsScreenValue;

      [HarmonyPatch(typeof(SettingsVM), nameof(SettingsVM.OpenDefaultSettingsDialog))]
      [HarmonyTranspiler]
      static internal IEnumerable<CodeInstruction> SettingsVM_OpenDefaultSettingsDialog_Transpiler_ToChangeDefaultDialogMessage(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
      {
        var _inst = instructions.ToList();
        int length = _inst.Count;
        int indexStart = -1;
        int indexEnd = -1;
        newDefaultMessage = Helpers.CreateString(
          key: "ModsMenuNewDefaultButtonMessage",
          enGB: "Revert all settings of the mod {0} to their default values?",
          ruRU: "Вернуть все настройки для мода {0} к их значениям по-умолчанию?",
          zhCN: "还原所有{0}模组设置到默认值？",
          deDE: "Alle Einstellungen des Mods {0} auf ihre Standardwerte zurücksetzen?",
          frFR: "Rétablir les valeurs par défaut de tous les paramètres du mod {0}?");

        for (int i = 0; i < length; i++)
        {
          if (
            _inst[i].Calls(typeof(Kingmaker.Game).GetProperty(nameof(Kingmaker.Game.Instance)).GetMethod) &&
            _inst[i + 1].Calls(typeof(Kingmaker.Game).GetProperty(nameof(Kingmaker.Game.BlueprintRoot)).GetMethod) &&
            _inst[i + 2].opcode == OpCodes.Ldfld && _inst[i + 2].operand is FieldInfo fi1 && fi1 == AccessTools.Field(typeof(BlueprintRoot), nameof(BlueprintRoot.LocalizedTexts)) &&
            _inst[i + 3].opcode == OpCodes.Ldfld && _inst[i + 3].operand is FieldInfo fi2 && fi2 == AccessTools.Field(typeof(LocalizedTexts), nameof(LocalizedTexts.UserInterfacesText)) &&
            _inst[i + 4].opcode == OpCodes.Ldfld && _inst[i + 4].operand is FieldInfo fi3 && fi3 == AccessTools.Field(typeof(UIStrings), nameof(UIStrings.SettingsUI)) &&
            _inst[i + 5].opcode == OpCodes.Ldfld && _inst[i + 5].operand is FieldInfo fi4 && fi4 == AccessTools.Field(typeof(UITextSettingsUI), nameof(UITextSettingsUI.RestoreAllDefaultsMessage))
            )
          {
            indexStart = i;
            break;
          }
        }

        if (indexStart == -1)
        {
          Main.Logger.Error("DefaultButtonPatcher - failed to find the starting index when transpile SettingsVM.OpenDefaultSettingsDialog. Default button message will not be altered.");
          return instructions;
        }

        for (int i = indexStart + 6; i < length; i++)
        {
          if (
            _inst[i].opcode == OpCodes.Call && _inst[i].operand is MethodInfo { Name: nameof(string.Format)} &&
            _inst[i + 1].opcode == OpCodes.Stfld && _inst[i + 1].operand is FieldInfo { Name: "text" }
            )
          {
            indexEnd = i;
            break;
          }
        }

        if (indexEnd == -1)
        {
          Main.Logger.Error("DefaultButtonPatcher - failed to find the ending index when transpile SettingsVM.OpenDefaultSettingsDialog. Default button message will not be altered.");
          return instructions;
        }

        Label labelNotMod = gen.DefineLabel();
        _inst[indexStart].labels.Add(labelNotMod);

        Label labelIsMod = gen.DefineLabel();
        _inst[indexEnd +1].labels.Add(labelIsMod);

        _inst.InsertRange(indexStart, new CodeInstruction[]
        {
          CodeInstruction.Call(() => CheckForSelectedSettingsScreenType()),
          new CodeInstruction(OpCodes.Brfalse_S, labelNotMod),
          CodeInstruction.Call(() => MakeMeDefaultButtonMessage()),
          new CodeInstruction(OpCodes.Br_S, labelIsMod)
        });

        return _inst;
      }
      static LocalizedString newDefaultMessage;
      static bool CheckForSelectedSettingsScreenType() =>  RootUIContext.Instance?.CommonVM.SettingsVM.Value?.SelectedMenuEntity.Value?.SettingsScreenType == (UISettingsManager.SettingsScreen)ModsMenuEntity.SettingsScreenValue;
      
      static string MakeMeDefaultButtonMessage()
      {
        return string.Format(newDefaultMessage, SettingsEntityModMenuEntry.instance.m_TempValue.ModInfo.ModName);
      }
    }
  }
}