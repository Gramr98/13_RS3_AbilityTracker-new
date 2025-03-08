using AbilityTrackerLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace WPFUI;

/// <summary>
/// Interaktionslogik für AbilitySettings.xaml
/// </summary>
public partial class AbilitySettings : Window
{
    private AbilityFileAdapter adapter = null;
    public AbilitySettings()
    {
        InitializeComponent();

        try
        {
            LoadData();
        }
        catch (Exception ex)
        {
            MessageDisplayer.ShowErrorOnWindowInitializing(ex);
        }
    }

    private void LoadData()
    {
        adapter = new AbilityFileAdapter(GlobalApplicationSettings.FilePathSettings.AbilityImages, GlobalApplicationSettings.FilePathSettings.AbilityJsonFile);
        cbx_AbilityIcon.ItemsSource = adapter.TIconList;
        dgr_AbilityList.ItemsSource = adapter.TList;
        LoadAdditionalEntityData();
    }

    private void LoadAdditionalEntityData()
    {
        AbilityKeybindAdapter abilityKeybindAdapter = new(GlobalApplicationSettings.FilePathSettings.AbilityKeybindJsonFile);
        if (adapter.TList == null || abilityKeybindAdapter.TList == null) return;

        foreach (Ability abilityItem in adapter.TList)
        {
            IEnumerable<KeybindClass> result = abilityKeybindAdapter.TList.Where(keybind => keybind.Ability.Name == abilityItem.Name);
            abilityItem.Dependencies = result != null ? result.Count() : 0;
        }
    }

    private void btn_AddAbilityToGrid_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (cbx_AbilityIcon.SelectedItem is Icon newAbilityIcon)
            {
                lbl_InfoMessage.Content = "Trying to add ability...";

                if (adapter.TryConvertAbility(name: tbx_AbilityName.Text, cooldown: tbx_AbilityCooldown.Text,
                    abilityImagePath: newAbilityIcon.IconPath, cbx_ActivateInsideGCD.IsChecked, cbx_IsBleedAbility.IsChecked, message: out string message, ability: out Ability convertedAbility))
                {
                    adapter.SaveNewObject(convertedAbility, GlobalApplicationSettings.FilePathSettings.AbilityJsonFile);
                    dgr_AbilityList.ItemsSource = adapter.TList; //dgr_AbilityList.Items.Add(convertedAbility); //.OrderBy(i => i.Name); //dgr_AbilityList.Items.Add(convertedAbility);
                    lbl_InfoMessage.Content = $"Successfully added new ability \"{convertedAbility.Name}\"!";

                    //Ability abilityToSelect = adapter.TList.FirstOrDefault(obj => obj.Name == convertedAbility.Name);
                    //if (abilityToSelect != null)
                    //    dgr_AbilityList.SelectedItem = abilityToSelect;
                }
                else
                    lbl_InfoMessage.Content = message;
            }
            else
                lbl_InfoMessage.Content = $"Please select a valid Ability Icon.";

        }
        catch (Exception ex)
        {
            lbl_InfoMessage.Content = $"Could not add ability. See Error Message for more details.";
            MessageDisplayer.ShowErrorOnAction("Error while adding the new ability", ex);
        }
    }

    private async void btn_ImportFromWiki_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            MessageBoxResult userResponse = MessageBox.Show($"This will delete all existing abilities in the folder \'{GlobalApplicationSettings.FilePathSettings.AbilityImages}\' and re-import all abilities from the official Wiki. Existing ability keybindings will also be deleted. The bar keybindings will not be affected however.{Environment.NewLine}{Environment.NewLine}Are you sure you want to continue?", "Continue Import?", MessageBoxButton.YesNo,MessageBoxImage.Question);

            if (userResponse == MessageBoxResult.Yes)
            {
                lbl_InfoMessage.Content = "Importing abilities and entities from the RS Wiki...";
                btn_ImportFromWiki.IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;
                cbx_AbilityIcon.ItemsSource = null;
                dgr_AbilityList.ItemsSource = null;
                adapter = null;
                WikiImporter importer = new WikiImporter();
                //WikiImportReport report = await importer.ReadAndSaveAbilities(GlobalApplicationSettings.FilePathSettings.AbilityImages, GlobalApplicationSettings.FilePathSettings.AbilityJsonFile, GlobalApplicationSettings.FilePathSettings.AbilityKeybindJsonFile);
                await importer.ReadAndSaveAbilities(GlobalApplicationSettings.FilePathSettings.AbilityImages, GlobalApplicationSettings.FilePathSettings.AbilityJsonFile, GlobalApplicationSettings.FilePathSettings.AbilityKeybindJsonFile);
                LoadData();
                lbl_InfoMessage.Content = $"Imported elements from the wiki!";
                //lbl_InfoMessage.Content = $"Imported {report.TotalCount} elements from the wiki!";
                //MessageBox.Show($"Import-Report: {Environment.NewLine}{report.ToString()}", "Import Finished!", MessageBoxButton.OK);
            }
        }
        catch (Exception ex)
        {
            lbl_InfoMessage.Content = $"Error: {ex.Message}";
            MessageDisplayer.ShowErrorOnAction("Error while importing the abilities from the Wiki:", ex);
        }
        finally
        {
            btn_ImportFromWiki.IsEnabled = true;
            Mouse.OverrideCursor = null;
        }
    }

    private void btn_SaveChanges_Click(object sender, RoutedEventArgs e)
    {
        adapter.SaveData(GlobalApplicationSettings.FilePathSettings.AbilityJsonFile);
        LoadData();
    }

    private void btn_DeleteSelected_Click(object sender, RoutedEventArgs e)
    {
        IList selectedRows = dgr_AbilityList.SelectedItems;

        foreach (object selectedRow in selectedRows)
        {
            if (selectedRow is Ability selectedAbility)
            {
                //dgr_AbilityList.Items.Remove(selectedAbility); //dgr_AbilityList.Items.Remove(selectedRow);
                adapter.DeleteTObject(selectedAbility, GlobalApplicationSettings.FilePathSettings.AbilityJsonFile);
            }
        }
        LoadData();
    }
}