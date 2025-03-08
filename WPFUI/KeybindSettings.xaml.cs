using AbilityTrackerLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WindowsKeyHooker_WinForms;

namespace WPFUI;
/// <summary>
/// Interaktionslogik für KeybindSettings.xaml
/// </summary>
public partial class KeybindSettings : Window
{
    private Hook keyboardHook = new Hook("Globalaction Link");
    private BarFileAdapter barAdapter = null;
    private BarKeybindAdapter barKeybindAdapter = null;
    private AbilityFileAdapter abilityAdapter = null;
    private AbilityKeybindAdapter abilityKeybindAdapter = null;
    private FileAdapter<Profile> profileAdapter = null;

    #region Loading / Initializing
    public KeybindSettings()
    {
        InitializeComponent();

        try
        {
            keyboardHook.KeyDownEvent += KeyDownEventMethod;
            LoadBarData();
            LoadAbilityData();
            LoadAdditionalEntityData();
            LoadProfileData();
        }
        catch (Exception ex)
        {
            MessageDisplayer.ShowErrorOnWindowInitializing(ex);
            throw;
        }
    }

    private void LoadBarData()
    {
        barAdapter = new BarFileAdapter(GlobalApplicationSettings.FilePathSettings.BarImages, GlobalApplicationSettings.FilePathSettings.BarJsonFile);
        cbx_BarsToSelectForBarBinding.ItemsSource = barAdapter.TList.OrderBy(i => i.Name);
        cbx_BarsToSelectForDependantBarBinding.ItemsSource = barAdapter.TList.OrderBy(i => i.Name);

        IOrderedEnumerable<BarClass> barItemsForAbilityBinding = barAdapter.TList.OrderBy(i => i.Name);
        if (barItemsForAbilityBinding != null)
        {
            List<BarClass> barListForAbilityBinding = barItemsForAbilityBinding.ToList();
            barListForAbilityBinding.Add(new BarClass(GlobalApplicationSettings.OtherSettings.AllBarsName, GlobalApplicationSettings.FilePathSettings.KeybindForAllBarsImage));
            cbx_BarsToSelectForAbilityBinding.ItemsSource = barListForAbilityBinding;
        }

        barKeybindAdapter = new BarKeybindAdapter(GlobalApplicationSettings.FilePathSettings.BarImages, GlobalApplicationSettings.FilePathSettings.BarKeybindJsonFile);
        dgr_BarList.ItemsSource = barKeybindAdapter.TList.OrderBy(i => i.Bar.Name);
    }

    private void LoadAbilityData() 
    {
        abilityAdapter = new AbilityFileAdapter(GlobalApplicationSettings.FilePathSettings.AbilityImages, GlobalApplicationSettings.FilePathSettings.AbilityJsonFile);
        cbx_AbilityToSelect.ItemsSource = abilityAdapter.TList.OrderBy(i => i.Name);

        abilityKeybindAdapter = new AbilityKeybindAdapter(GlobalApplicationSettings.FilePathSettings.AbilityKeybindJsonFile);
        dgr_AbilityList.ItemsSource = abilityKeybindAdapter.TList.OrderBy(i => i.Ability.Name);
    }

    private void LoadAdditionalEntityData()
    {
        if (barKeybindAdapter.TList == null || abilityKeybindAdapter.TList == null) return;

        //Set Amount of Dependencies
        foreach (BarKeybindClass barItem in barKeybindAdapter.TList)
        {
            IEnumerable<KeybindClass> result = abilityKeybindAdapter.TList.Where(abil => abil.Bar.Name == barItem.Bar.Name);
            barItem.Dependencies = result != null ? result.Count() : 0;
        }

        foreach (KeybindClass abilityItem in abilityKeybindAdapter.TList)
        {
            IEnumerable<BarKeybindClass> result = barKeybindAdapter.TList.Where(bar => bar.Bar.Name == abilityItem.Bar.Name);
            abilityItem.Dependencies = result != null ? result.Count() : 0;
        }

        //Set Amount of Occurances
        IEnumerable<IGrouping<string, KeybindClass>> resultGroups = abilityKeybindAdapter.TList.GroupBy(abil => abil.Ability.Name);
        foreach (IGrouping<string, KeybindClass> item in resultGroups)
            foreach (KeybindClass subItem in item)
                subItem.Occurances = item.Count();

        IEnumerable<IGrouping<string, BarKeybindClass>> resultGroups2 = barKeybindAdapter.TList.GroupBy(bar => bar.Bar.Name);
        foreach (IGrouping<string, BarKeybindClass> item in resultGroups2)
            foreach (BarKeybindClass subItem in item)
                subItem.Occurances = item.Count();
    }

    private void LoadProfileData()
    {
        profileAdapter = new(GlobalApplicationSettings.FilePathSettings.ProfileJsonFile);
        foreach (Profile item in profileAdapter.TList.OrderBy(i => i.ProfileName))
            cbx_Profiles.Items.Add(item);

        if (cbx_Profiles.Items.Count > 0)
            cbx_Profiles.SelectedIndex = 0;
    }
    #endregion Loading

    #region BarTab
    private void btn_AddBarKeybinding_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (cbx_BarsToSelectForBarBinding.SelectedItem is BarClass newBarBinding
                // ToDo: Is this check required?
                && cbx_BarsToSelectForDependantBarBinding.SelectedItem is BarClass newDependencyBarBinding)
            {
                lbl_InfoMessageForBarBinding.Content = "Trying to add new bar keybinding...";

                if (barKeybindAdapter.TryConvertKeybindBar(keyInput: tbx_pressedKeyForBarBinding.Text, barObject: newBarBinding,
                                                    barDependency: newDependencyBarBinding,
                                                    abilitiesToCompare: abilityKeybindAdapter.TList, message: out string message,
                                                    barKeybinding: out BarKeybindClass barKeybinding))
                {
                    barKeybindAdapter.SaveNewObject(barKeybinding, GlobalApplicationSettings.FilePathSettings.BarKeybindJsonFile);
                    dgr_BarList.ItemsSource = barKeybindAdapter.TList.OrderBy(i => i.Bar.Name); //dgr_BarList.Items.Add(convertedBar);
                    if (string.IsNullOrWhiteSpace(barKeybinding.Modifier))
                        lbl_InfoMessageForBarBinding.Content = $"Successfully added new bar keybinding \"{barKeybinding.Key}\" (\"{barKeybinding.Bar.Name}\")!";
                    else
                        lbl_InfoMessageForBarBinding.Content = $"Successfully added new bar keybinding \"{barKeybinding.Modifier}+{barKeybinding.Key}\" (\"{barKeybinding.Bar.Name}\")!";
                    LoadAdditionalEntityData(); //Refresh dependency columns
                }
                else
                    lbl_InfoMessageForBarBinding.Content = message;
            }
            else
                lbl_InfoMessageForBarBinding.Content = $"Please select a valid bar and dependency.";
        }
        catch (Exception ex)
        {
            lbl_InfoMessageForBarBinding.Content = $"Could not save bar Keybinding. See Error Message for more details.";
            MessageDisplayer.ShowErrorOnAction("Error while adding the new bar", ex);
        }
    }

    private void Context_Bars_ActivateSelection(object sender, RoutedEventArgs e)
    {
        try
        {
            IList selectedRows = dgr_BarList.SelectedItems;
            MessageBoxResult result = MessageBox.Show($"This will invert the activation of {selectedRows.Count} items in the current selected profile", "Info", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                foreach (object item in selectedRows)
                    ((BarKeybindClass)item).InvertActivation();
                dgr_BarList.Items.Refresh();
            }
        }
        catch (Exception ex)
        {
            ErrorDisplayer.ShowErrorOnAction("Error while trying to check/uncheck the selected bars", ex);
        }
    }

    private void Context_Bars_DeleteSelection(object sender, RoutedEventArgs e)
    {
        try
        {
            IList selectedRows = dgr_BarList.SelectedItems;

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete {selectedRows.Count} selected Bars?", "Delete Bars", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                foreach (object selectedRow in selectedRows)
                    barKeybindAdapter.DeleteTObject((BarKeybindClass)selectedRow, GlobalApplicationSettings.FilePathSettings.AbilityKeybindJsonFile);

                LoadAbilityData();
                LoadAdditionalEntityData(); //Refresh dependency columns 
                lbl_InfoMessageForAbilityBinding.Content = $"Successfully deleted {selectedRows.Count} bars.";
            }
        }
        catch (Exception ex)
        {
            ErrorDisplayer.ShowErrorOnAction("Error while trying to delete the selected bars", ex);
        }
    }

    #endregion BarTab

    #region AbilityTab
    private void btn_AddAbilityKeybinding_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (cbx_AbilityToSelect.SelectedItem is Ability selectedAbilityBinding)
            {
                lbl_InfoMessageForAbilityBinding.Content = "Trying to add new ability keybinding...";

                if (cbx_BarsToSelectForAbilityBinding.SelectedItem is BarClass selectedBarBinding)
                {
                    if (abilityKeybindAdapter.TryConvertKeybindAbility(keyInput: tbx_pressedKeyForAbilityBinding.Text, abilityObject: selectedAbilityBinding,
                                        barObject: selectedBarBinding, barsToCompare: barKeybindAdapter.TList, message: out string message, abilityKeybinding: out KeybindClass abilityKeybinding))
                    {
                        abilityKeybindAdapter.SaveNewObject(abilityKeybinding, GlobalApplicationSettings.FilePathSettings.AbilityKeybindJsonFile);
                        dgr_AbilityList.ItemsSource = abilityKeybindAdapter.TList.OrderBy(i => i.Ability.Name); //dgr_BarList.Items.Add(convertedBar);
                        if (string.IsNullOrWhiteSpace(abilityKeybinding.Modifier))
                            lbl_InfoMessageForAbilityBinding.Content = $"Added new ability keybinding \'{abilityKeybinding.Key}\' (\'{abilityKeybinding.Ability.Name}\') for the bar \'{abilityKeybinding.Bar.Name}\'!";
                        else
                            lbl_InfoMessageForAbilityBinding.Content = $"Added new ability keybinding \'{abilityKeybinding.Modifier}+{abilityKeybinding.Key}\' (\'{abilityKeybinding.Ability.Name}\') for the bar \'{abilityKeybinding.Bar.Name}\'!";
                        LoadAdditionalEntityData(); //Refresh dependency columns
                    }
                    else
                        lbl_InfoMessageForAbilityBinding.Content = message;
                }
                else
                    lbl_InfoMessageForAbilityBinding.Content = $"Please select a valid bar.";
            }
            else
                lbl_InfoMessageForAbilityBinding.Content = $"Please select a valid ability.";
        }
        catch (Exception ex)
        {
            lbl_InfoMessageForAbilityBinding.Content = $"Could not save ability Keybinding. See Error Message for more details.";
            MessageDisplayer.ShowErrorOnAction("Error while adding the new bar", ex);
        }
    }

    private void Context_Abilities_ActivateSelection(object sender, RoutedEventArgs e)
    {
        try
        {
            IList selectedRows = dgr_AbilityList.SelectedItems;
            MessageBoxResult result = MessageBox.Show($"This will invert the activation of {selectedRows.Count} items in the current selected profile", "Info", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                foreach (object item in selectedRows)
                    ((KeybindClass)item).InvertActivation();
                dgr_AbilityList.Items.Refresh();
            }
        }
        catch (Exception ex)
        {
            ErrorDisplayer.ShowErrorOnAction("Error while trying to check/uncheck the selected abilities", ex);
        }
    }

    private void Context_Abilities_DeleteSelection(object sender, RoutedEventArgs e)
    {
        try
        {
            IList selectedRows = dgr_AbilityList.SelectedItems;

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete {selectedRows.Count} selected Abilities?", "Delete Abilities", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                foreach (object selectedRow in selectedRows)
                    abilityKeybindAdapter.DeleteTObject((KeybindClass)selectedRow, GlobalApplicationSettings.FilePathSettings.AbilityKeybindJsonFile);

                LoadAbilityData();
                LoadAdditionalEntityData(); //Refresh dependency columns 
                lbl_InfoMessageForAbilityBinding.Content = $"Successfully deleted {selectedRows.Count} Abilities.";
            }
        }
        catch (Exception ex)
        {
            ErrorDisplayer.ShowErrorOnAction("Error while trying to delete the selected abilities", ex);
        }
    }
    #endregion AbilityTab

    #region Profiles
    private void btn_AddNewProfile_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // https://stackoverflow.com/questions/2796470/wpf-create-a-dialog-prompt
            EnterTextPopupDialog dialog = new EnterTextPopupDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult == true)
            {
                if (string.IsNullOrEmpty(dialog.ResponseText) == false)
                {
                    Profile newProfile = new Profile() { ProfileName = dialog.ResponseText };
                    if (cbx_Profiles.Items.Contains(newProfile) == false)
                    {
                        cbx_Profiles.Items.Add(newProfile);
                        cbx_Profiles.SelectedValue = newProfile;
                        profileAdapter.SaveNewObject(newProfile, GlobalApplicationSettings.FilePathSettings.ProfileJsonFile);
                    }
                    else
                        MessageBox.Show($"The entered name \'{newProfile}\' for the new profile already exists.", "Bar Profile already exists", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                    MessageBox.Show($"The entered name must not be empty!", "Value not valid", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        catch (Exception ex)
        {
            MessageDisplayer.ShowErrorOnAction("Error while adding the new profile", ex);
        }
    }

    private void btn_SaveProfileChanges_Click(object sender, RoutedEventArgs e)
    {
        if (cbx_Profiles.SelectedItem is Profile barKeybindingProfile)
        {
            lbl_InfoMessageForAbilityBinding.Content = "Saving...";
            lbl_InfoMessageForBarBinding.Content = "Saving...";

            //ToDo: Work with IDs instead for comparison (only save IDs)
            List<BarKeybindClass> barList = new();
            foreach (object dgrObject in dgr_BarList.Items)
                if (dgrObject is BarKeybindClass dgrObjectConverted && dgrObjectConverted.IsActivatedForCurrentProfile == true)
                    barList.Add(dgrObjectConverted);

            List<KeybindClass> keybindList = new();
            foreach (object dgrObject in dgr_AbilityList.Items)
                if (dgrObject is KeybindClass dgrObjectConverted && dgrObjectConverted.IsActivatedForCurrentProfile == true)
                    keybindList.Add(dgrObjectConverted);

            profileAdapter.TList.FirstOrDefault(obj => obj.ProfileName == barKeybindingProfile.ProfileName).ActiveBarKeybindObjects = barList;
            profileAdapter.TList.FirstOrDefault(obj => obj.ProfileName == barKeybindingProfile.ProfileName).ActiveAbilityKeybindObjects = keybindList;
            profileAdapter.SaveData(GlobalApplicationSettings.FilePathSettings.ProfileJsonFile);
            lbl_InfoMessageForAbilityBinding.Content = "The data has been saved";
            lbl_InfoMessageForBarBinding.Content = "The data has been saved";
        }
    }

    private void cbx_Profiles_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        //lbl_ProfileInfo.Content = "";
        //Change the checkbox value for all items across the datagrid depending on the selected Profile (UI update)
        if (cbx_Profiles.SelectedItem is Profile profileObj)
        {
            int foundBars_Count = 0;
            int foundAbils_Count = 0;
            // Set the checkboxes for bars
            foreach (object dgrObject in dgr_BarList.Items)
            {
                if (dgrObject is BarKeybindClass dgrObjectConverted)
                {
                    dgrObjectConverted.IsActivatedForCurrentProfile = false;

                    //ToDo: Work with IDs instead for comparison
                    IEnumerable<BarKeybindClass> currentKeybindings = profileObj.ActiveBarKeybindObjects.Where(
                        obj => obj.Bar.Name == dgrObjectConverted.Bar.Name && obj.Bar.Img == dgrObjectConverted.Bar.Img &&
                                obj.BarDependency.Name == dgrObjectConverted.BarDependency.Name && obj.BarDependency.Img == dgrObjectConverted.BarDependency.Img &&
                                obj.Key == dgrObjectConverted.Key && obj.Modifier == dgrObjectConverted.Modifier
                        );

                    if (currentKeybindings?.Count() > 0)
                    {
                        dgrObjectConverted.IsActivatedForCurrentProfile = true;
                        foundBars_Count++;
                    }
                }
            }
            dgr_BarList.Items.Refresh();
            // Set the checkboxes for abilities
            foreach (object dgrObject in dgr_AbilityList.Items)
            {
                if (dgrObject is KeybindClass dgrObjectConverted)
                {
                    dgrObjectConverted.IsActivatedForCurrentProfile = false;

                    //ToDo: Work with IDs instead for comparison
                    IEnumerable<KeybindClass> currentKeybindings = profileObj.ActiveAbilityKeybindObjects.Where(
                        obj => obj.Bar.Name == dgrObjectConverted.Bar.Name && obj.Bar.Img == dgrObjectConverted.Bar.Img &&
                               obj.Ability.Name == dgrObjectConverted.Ability.Name &&
                               obj.Ability.FriendlyName == dgrObjectConverted.Ability.FriendlyName &&
                               obj.Ability.Img == dgrObjectConverted.Ability.Img &&
                               obj.Ability.CooldownInSec == dgrObjectConverted.Ability.CooldownInSec &&
                               obj.Key == dgrObjectConverted.Key && obj.Modifier == dgrObjectConverted.Modifier
                        );

                    if (currentKeybindings?.Count() > 0)
                    {
                        dgrObjectConverted.IsActivatedForCurrentProfile = true;
                        foundAbils_Count++;
                    }
                }
            }
            dgr_AbilityList.Items.Refresh();
            //lbl_ProfileInfo.Content = $"{foundBars_Count}/{profileObj.BarCount} Bars, {foundAbils_Count}/{profileObj.AbilityCount} Abilities";
        }
    }
    #endregion Profiles

    #region Other
    private void KeyDownEventMethod(KeyboardHookEventArgs e)
    {
        string pressedKey = "";
        if (e.isAltPressed)
            pressedKey += "ALT+";
        if (e.isCtrlPressed)
            pressedKey += "CTRL+";
        if (e.isShiftPressed)
            pressedKey += "SHIFT+";
        if (e.isWinPressed)
            pressedKey += "WIN+";

        if (tabItem_Bar.IsSelected)
            tbx_pressedKeyForBarBinding.Text = pressedKey + e.Key.ToString();
        else if (tabItem_Ability.IsSelected)
            tbx_pressedKeyForAbilityBinding.Text = pressedKey + e.Key.ToString();
    }

    private void CloseCommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) => this.Close();
    #endregion Other

}
