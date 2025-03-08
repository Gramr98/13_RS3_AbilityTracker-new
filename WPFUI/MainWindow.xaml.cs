using AbilityTrackerLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace WPFUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    //DataAdapter
    private FileAdapter<Profile> profileAdapter = null;

    //Tracker Child Window
    private AbilityDisplayer abilityDisplayerInstance;
    private bool TrackingWindowIsOpen = false;

    //GCD
    public const int DEFAULT_GLOBAL_COOLDOWN_IN_MS = 1800;
    private int newCalculatedGCD = DEFAULT_GLOBAL_COOLDOWN_IN_MS;
    private int bleedStartForMagicDWT95TimerInS = 30;

    public MainWindow()
    {
        try
        {
            InitializeComponent();
            
            //Should not be requied becuause this already happens in ApplySettings()
            try
            {
                BindProfiles();
                BindAbilityBars();
            }
            catch (Exception ex)
            {
                MessageDisplayer.ShowErrorOnWindowInitializing(ex);
                throw;
            }

            try
            {
                FileAdapter<StartSettings> startSettingsAdapter = new(GlobalApplicationSettings.FilePathSettings.StartJsonFile);
                if (startSettingsAdapter.TList != null && startSettingsAdapter.TList.Count > 0)
                    ApplySettings(startSettingsAdapter.TList.FirstOrDefault());
            }
            catch (Exception ex)
            {
                MessageDisplayer.ShowErrorMessage($"An error has occured while loading the latest used settings. The default Settings will be used instead.", ex);
                throw;
            }
        }
        catch (Exception ex)
        {
            System.IO.File.WriteAllText(GlobalApplicationSettings.OtherSettings.StartLoggingPath, ex.ToString());
        }
    }

    private void ApplySettings(StartSettings settings)
    {
        this.cbx_trackCD.IsChecked = settings.TrackAbilityCD;
        this.cbx_trackGCD.IsChecked = settings.TrackGCD;
        this.cbx_displayOnTop.IsChecked = settings.DisplayOnTop;
        this.cbx_canResize.IsChecked = settings.CanResize;
        this.cbx_displayBleedTimerForMagicT95DW.IsChecked = settings.DisplayBleedTimerForMagicT95DW;
        this.tbx_bleedTimerForMagicT95DW.Text = settings.BleedTimerForMagicT95DW;
        this.cbx_HasDoubleSurge.IsChecked = settings.HasDoubleSurgeOrEscape;
        this.tbx_GCD_Reduction.Text = settings.GCDReductionText;
        this.cbx_HasMobilePerk.IsChecked = settings.HasMobilePerkOrRelic;
        BindProfiles(settings.SelectedProfileIndex);
        BindAbilityBars(settings.SelectedBarIndex);
    }

    private void SaveSettings()
    {
        StartSettings settings = new();
        settings.TrackAbilityCD = this.cbx_trackCD.IsChecked == true;
        settings.TrackGCD = this.cbx_trackGCD.IsChecked == true;
        settings.DisplayOnTop = this.cbx_displayOnTop.IsChecked == true;
        settings.CanResize = this.cbx_canResize.IsChecked == true;
        settings.DisplayBleedTimerForMagicT95DW = this.cbx_displayBleedTimerForMagicT95DW.IsChecked == true;
        settings.BleedTimerForMagicT95DW = tbx_bleedTimerForMagicT95DW.Text;
        settings.SelectedProfileIndex = cbx_Profile.SelectedIndex;
        settings.SelectedBarIndex = cbx_CombatMode.SelectedIndex;
        settings.HasDoubleSurgeOrEscape = cbx_HasDoubleSurge.IsChecked == true;
        settings.GCDReductionText = tbx_GCD_Reduction.Text;
        settings.HasMobilePerkOrRelic = this.cbx_HasMobilePerk.IsChecked == true;
        new FileAdapter<StartSettings>(GlobalApplicationSettings.FilePathSettings.StartJsonFile, false).SaveNewObject(settings, GlobalApplicationSettings.FilePathSettings.StartJsonFile);
    }

    private void BindProfiles(int defaultSelectedProfileIndex = 0)
    {
        profileAdapter = new(GlobalApplicationSettings.FilePathSettings.ProfileJsonFile);
        cbx_Profile.ItemsSource = profileAdapter.TList.OrderBy(i => i.ProfileName);

        if (profileAdapter.TList != null && profileAdapter.TList.Count > 0)
            cbx_Profile.SelectedIndex = defaultSelectedProfileIndex;
    }

    private void BindAbilityBars(int defaultSelectedBarIndex = 0)
    {
        if (cbx_Profile.SelectedItem is Profile profile && profile.ActiveBarKeybindObjects.Count > 0)
        {
            // Group the barKeybindings by combat style by Bar Name, then display only the first bar of each group.
            // Otherwise there would be for example 4 times "Mage Bar 2h" listed in the combobox if we have 4 keybindings for it configured.
            IEnumerable<BarClass> barKeybindings = profile.ActiveBarKeybindObjects.Select(obj => obj.Bar);
            IEnumerable<IGrouping<string, BarClass>> barGroups = barKeybindings.GroupBy(barKeybinding => barKeybinding.Name);

            List<BarClass> barList = new();
            foreach (IGrouping<string, BarClass> grp in barGroups)
                if (grp != null && grp.Count() > 0)
                    barList.Add(grp.FirstOrDefault());

            cbx_CombatMode.ItemsSource = barList;

            if (barList.Count > 0)
                cbx_CombatMode.SelectedIndex = defaultSelectedBarIndex;
        }
    }

    private void btn_OpenAbilityMapping_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            AbilitySettings abilitySettings = new();
            abilitySettings.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageDisplayer.ShowErrorOnWindowOpening(ex);
        }
    }

    private void btn_OpenBarMapping_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            AbilityBar abilityBar = new();
            abilityBar.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageDisplayer.ShowErrorOnWindowOpening(ex);
        }
    }

    private void btn_OpenKeybindMapping_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            KeybindSettings keybindSettings = new();
            keybindSettings.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageDisplayer.ShowErrorOnWindowOpening(ex);
        }
    }

    private void btn_StartTracking_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            newCalculatedGCD = DEFAULT_GLOBAL_COOLDOWN_IN_MS;
            if (string.IsNullOrWhiteSpace(tbx_GCD_Reduction.Text) == false)
            {
                //Only do validation if the field is not empty.
                if (int.TryParse(tbx_GCD_Reduction.Text, out int gcdReductionInMs) == false)
                {
                    MessageBox.Show($"The entered GCD-Reduction value of \'{tbx_GCD_Reduction.Text}\' ms is not a valid value. Please leave the field empty or input a whole number.");
                    return;
                }

                if (gcdReductionInMs < 0)
                {
                    MessageBox.Show($"The entered GCD-Reduction value of \'{tbx_GCD_Reduction.Text}\' ms must not be negative. Please enter the reduction in ms as a positive number.");
                    return;
                }

                newCalculatedGCD = DEFAULT_GLOBAL_COOLDOWN_IN_MS - gcdReductionInMs;

                if (newCalculatedGCD <= 0)
                {
                    MessageBox.Show($"The entered GCD-Reduction of \'{tbx_GCD_Reduction.Text}\' is not valid because the result would be less than or equals to 0." +
                        $"{Environment.NewLine}=> {DEFAULT_GLOBAL_COOLDOWN_IN_MS} ms - {gcdReductionInMs} ms = new GCD of {newCalculatedGCD}");
                    return;
                }
            }

            if (cbx_CombatMode.SelectedItem is BarClass selectedBar && cbx_Profile.SelectedItem is Profile selectedProfile)
            {
                if (TrackingWindowIsOpen)
                    abilityDisplayerInstance?.Close();

                AbilityDisplayerSettings settings = new AbilityDisplayerSettings( 
                    cbx_trackCD.IsChecked == true, cbx_trackGCD.IsChecked == true,
                    cbx_displayOnTop.IsChecked == true, DEFAULT_GLOBAL_COOLDOWN_IN_MS, newCalculatedGCD, 
                    cbx_HasDoubleSurge.IsChecked == true, cbx_HasMobilePerk.IsChecked == true,
                    cbx_displayBleedTimerForMagicT95DW.IsChecked == true, bleedStartForMagicDWT95TimerInS);
                abilityDisplayerInstance = new(selectedProfile, selectedBar, settings);
                abilityDisplayerInstance.Show();
                TrackingWindowIsOpen = true;
                btn_StartTracking.Content = "Restart";
                btn_CloseTracker.Visibility = Visibility.Visible;
            }
            else
                MessageDisplayer.ShowMessage("Please select a valid combat style.");
        }
        catch (Exception ex)
        {
            MessageDisplayer.ShowErrorOnWindowOpening(ex);
        }
    }

    private void btn_Exit_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SaveSettings();
        }
        catch (Exception ex)
        {
            MessageDisplayer.ShowErrorMessage($"An error has occured while trying to save the settings from the main window. Next time you open the application, the default settings will be applied.", ex);
        }

        abilityDisplayerInstance?.Close();
        Environment.Exit(1);
    }

    private void CanResize_Checked(object sender, RoutedEventArgs e)
    {
        if (abilityDisplayerInstance != null)
            abilityDisplayerInstance.ResizeMode = ResizeMode.CanResize;
    }

    private void CanResize_Unchecked(object sender, RoutedEventArgs e)
    {
        if (abilityDisplayerInstance != null)
            abilityDisplayerInstance.ResizeMode = ResizeMode.NoResize;
    }

    private void btn_Reload_Click(object sender, RoutedEventArgs e)
    {
        BindAbilityBars(cbx_CombatMode.SelectedIndex);
        BindProfiles(cbx_Profile.SelectedIndex);
    }

    private void btn_CloseTracker_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            abilityDisplayerInstance?.Close();
            TrackingWindowIsOpen = false;
            btn_StartTracking.Content = "Start";
            btn_CloseTracker.Visibility = Visibility.Hidden;
        }
        catch (Exception ex)
        {
            MessageDisplayer.ShowErrorMessage("Error while closing the Tracker Window", ex);
        }
    }

    private void cbx_Profile_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        BindAbilityBars();
    }

    private void btn_OpenExplorer_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start("explorer.exe", System.IO.Directory.GetCurrentDirectory());
        }
        catch (Exception ex)
        {
            MessageDisplayer.ShowErrorMessage("Error while trying to open the local folder", ex);
        }
    }

    private void tbx_GCD_Reduction_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        newCalculatedGCD = DEFAULT_GLOBAL_COOLDOWN_IN_MS;
        if (string.IsNullOrWhiteSpace(tbx_GCD_Reduction.Text) == false)
        {
            if (int.TryParse(tbx_GCD_Reduction.Text, out int gcdReductionInMs) == false)
            {
                lbl_GCD_Reduction_ResultInfo.Content = "(No text allowed)";
                lbl_GCD_Reduction_ResultInfo.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            if (gcdReductionInMs < 0)
            {
                lbl_GCD_Reduction_ResultInfo.Content = "(Value must not be negative)";
                lbl_GCD_Reduction_ResultInfo.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            newCalculatedGCD = DEFAULT_GLOBAL_COOLDOWN_IN_MS - gcdReductionInMs;

            lbl_GCD_Reduction_ResultInfo.Content = $"({DEFAULT_GLOBAL_COOLDOWN_IN_MS} ms - {gcdReductionInMs} ms = {newCalculatedGCD} ms)";
            lbl_GCD_Reduction_ResultInfo.Foreground = System.Windows.Media.Brushes.Green;

            if (newCalculatedGCD <= 0)
            {
                lbl_GCD_Reduction_ResultInfo.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
        else
        {
            lbl_GCD_Reduction_ResultInfo.Content = $"({DEFAULT_GLOBAL_COOLDOWN_IN_MS} ms - 0 ms = {DEFAULT_GLOBAL_COOLDOWN_IN_MS} ms)";
            lbl_GCD_Reduction_ResultInfo.Foreground = System.Windows.Media.Brushes.Green;
        }
    }

    private void tbx_BleedTimerForMagicT95DW_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        bleedStartForMagicDWT95TimerInS = 30;

        if (int.TryParse(tbx_bleedTimerForMagicT95DW.Text, out int convertedValue) == false)
        {
            lbl_bleedTimerForMagicT95DW.Content = "(No text allowed)";
            lbl_bleedTimerForMagicT95DW.Foreground = System.Windows.Media.Brushes.Red;
            return;
        }

        if (convertedValue < 0)
        {
            lbl_bleedTimerForMagicT95DW.Content = "(Value must not be negative)";
            lbl_bleedTimerForMagicT95DW.Foreground = System.Windows.Media.Brushes.Red;
            return;
        }

        bleedStartForMagicDWT95TimerInS = convertedValue;

        lbl_bleedTimerForMagicT95DW.Content = $"Start-Timer of {bleedStartForMagicDWT95TimerInS} s has been applied!";
        lbl_bleedTimerForMagicT95DW.Foreground = System.Windows.Media.Brushes.Green;
    }
}
