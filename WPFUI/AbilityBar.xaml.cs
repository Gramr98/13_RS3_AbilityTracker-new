using AbilityTrackerLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace WPFUI;

/// <summary>
/// Interaktionslogik für AbilityBar.xaml
/// </summary>
public partial class AbilityBar : Window
{
    private BarFileAdapter adapter = null;

    public AbilityBar()
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
        adapter = new BarFileAdapter(GlobalApplicationSettings.FilePathSettings.BarImages, GlobalApplicationSettings.FilePathSettings.BarJsonFile);
        dgr_BarList.ItemsSource = adapter.TList.OrderBy(i => i.Name);
        cbx_BarIcon.ItemsSource = adapter.TIconList;
        LoadAdditionalEntityData();
    }

    private void LoadAdditionalEntityData()
    {
        BarKeybindAdapter barKeybindAdapter = new(GlobalApplicationSettings.FilePathSettings.BarKeybindJsonFile);
        if (adapter.TList == null || barKeybindAdapter.TList == null) return;

        foreach (BarClass barItem in adapter.TList)
        {
            IEnumerable<BarKeybindClass> result = barKeybindAdapter.TList.Where(keybind => keybind.Bar.Name == barItem.Name);
            barItem.Dependencies = result != null ? result.Count() : 0;
        }
    }

    private void btn_AddBarToGrid_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (cbx_BarIcon.SelectedItem is Icon newBarIcon)
            {
                lbl_InfoMessage.Content = "Trying to add bar...";

                if (adapter.TryConvertBar(name: tbx_BarName.Text, barImagePath: newBarIcon.IconPath, message: out string message, bar: out BarClass convertedBar))
                {
                    adapter.SaveNewObject(convertedBar, GlobalApplicationSettings.FilePathSettings.BarJsonFile);
                    dgr_BarList.ItemsSource = adapter.TList.OrderBy(i => i.Name); //dgr_BarList.Items.Add(convertedBar);
                    lbl_InfoMessage.Content = $"Successfully added new Bar \"{convertedBar.Name}\"!";

                    BarClass barToSelect = adapter.TList.FirstOrDefault(obj => obj.Name == convertedBar.Name);
                    if (barToSelect != null)
                        dgr_BarList.SelectedItem = barToSelect;
                }
                else
                    lbl_InfoMessage.Content = message;
            }
            else
                lbl_InfoMessage.Content = $"Please select a valid Bar Icon.";
        }
        catch (Exception ex)
        {
            lbl_InfoMessage.Content = $"Could not add ability bar. See Error Message for more details.";
            MessageDisplayer.ShowErrorOnAction("Error while adding the new bar", ex);
        }
    }
}
