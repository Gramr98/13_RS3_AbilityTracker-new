using AbilityTrackerLibrary;
using AbilityTrackerLibrary.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WindowsKeyHooker_WinForms;

namespace WPFUI;

/// <summary>
/// Interaktionslogik für AbilityDisplayer.xaml
/// </summary>
public partial class AbilityDisplayer : Window
{
    // KeyboardHook => Read the Keyboard Input
    private Hook keyboardHook = new Hook("Globalaction Link");

    // Settings
    public readonly int DEFAULT_GCD = 0;
    public readonly int CONFIGURED_GCD = 0;
    private bool trackAbilityCooldown = true;
    private bool trackGlobalCooldown = true;
    private bool hasDblSurgeAndEscape = true;
    private bool hasMobilePerk = true;
    private bool displayBleedTimerForMagicT95DW = true;
    private int configuredBleedTimerForMagicT95DW = 30;
    //ToDo: Add Column to the ability grids with a checkable checkbox and implement this behavior in the Model Class instead of hard-coding this here.
    private string surgeAbilityToLowerName = "surge";
    private string escapeAbilityToLowerName = "escape";
    private string bladedDiveAbilityToLowerName = "bladed dive";
    private string diveAbilityToLowerName = "dive";
    //private bool allowAbilityQueueing = true;

    // Keybinding-Objects
    private readonly Profile profile;

    private List<BarKeybindClass> barKeybindings = new List<BarKeybindClass>();
    private List<KeybindClass> abilityKeybindings = new List<KeybindClass>();

    private List<KeybindClass> previousPressedKeys = new List<KeybindClass>();

    // Sonstige
    private Stopwatch gcdStopwatch = new Stopwatch();
    //private BarKeybindAdapter barKeybindAdapter = null;
    //private AbilityKeybindAdapter abilityKeybindAdapter = null;
    private BarClass currentStyle = null;
    private Timer timer10ms = new Timer(10);

    private CountDownTimer bleedTimerForMagicT95DW = null;

    public AbilityDisplayer(Profile profile, BarClass bar, AbilityDisplayerSettings settings) //, bool allowAbilityQueueing = true)
    {
        InitializeComponent();
        try
        {
            base.Topmost = settings.PinWindowOnTop;
            this.trackAbilityCooldown = settings.TrackAbilityCooldown;
            this.trackGlobalCooldown = settings.TrackGlobalCooldown;
            this.hasDblSurgeAndEscape = settings.HasDblSurgeAndEscape;
            this.hasMobilePerk = settings.HasMobilePerk;
            this.displayBleedTimerForMagicT95DW = settings.DisplayBleedTimerForMagicT95DW;

            if(displayBleedTimerForMagicT95DW == false) // Hide Bleed Timer if condition is not met by the user
                this.lbl_bleedInfoTimerForMagicT95DW.Content = string.Empty;
            this.configuredBleedTimerForMagicT95DW = settings.BleedStartForMagicDWT95TimerInS;
            bleedTimerForMagicT95DW = new CountDownTimer(new TimeSpan(0, 0, configuredBleedTimerForMagicT95DW));
            bleedTimerForMagicT95DW.TimeChanged += BleedTimerElapsed;

            DEFAULT_GCD = settings.DefaultGCD;
            CONFIGURED_GCD = settings.ConfiguredGCD;
            //this.allowAbilityQueueing = allowAbilityQueueing;
            keyboardHook.KeyDownEvent += KeyDownEventMethod;

            //barKeybindAdapter = new BarKeybindAdapter(GlobalApplicationSettings.FilePathSettings.BarImages, GlobalApplicationSettings.FilePathSettings.BarKeybindJsonFile);
            //abilityKeybindAdapter = new AbilityKeybindAdapter(GlobalApplicationSettings.FilePathSettings.AbilityKeybindJsonFile);
            //barKeybindings = barKeybindAdapter.TList;
            barKeybindings = profile.ActiveBarKeybindObjects;
            //lbl_profileInfo.Content = $"Profile: {profile.ProfileName}";

            this.profile = profile;
            ApplyNewCombatStyle(bar);

            #region Timer and Stopwatch Configuration
            timer10ms.Elapsed += ActionEvery10MS;
            displayCd10.displayer = this;
            displayCd9.displayer = this;
            displayCd8.displayer = this;
            displayCd7.displayer = this;
            displayCd6.displayer = this;
            displayCd5.displayer = this;
            displayCd4.displayer = this;
            displayCd3.displayer = this;
            displayCd2.displayer = this;
            displayCd1.displayer = this;
            displayCd0.displayer = this;
            this.timer10ms.Elapsed += displayCd10.ActionEvery10MS;
            this.timer10ms.Elapsed += displayCd9.ActionEvery10MS;
            this.timer10ms.Elapsed += displayCd8.ActionEvery10MS;
            this.timer10ms.Elapsed += displayCd7.ActionEvery10MS;
            this.timer10ms.Elapsed += displayCd6.ActionEvery10MS;
            this.timer10ms.Elapsed += displayCd5.ActionEvery10MS;
            this.timer10ms.Elapsed += displayCd4.ActionEvery10MS;
            this.timer10ms.Elapsed += displayCd3.ActionEvery10MS;
            this.timer10ms.Elapsed += displayCd2.ActionEvery10MS;
            this.timer10ms.Elapsed += displayCd1.ActionEvery10MS;
            this.timer10ms.Elapsed += displayCd0.ActionEvery10MS;
            timer10ms.Start();
            gcdStopwatch.Start();
            #endregion Timer and Stopwatch Configuration
        }
        catch (Exception ex)
        {
            MessageDisplayer.ShowErrorOnWindowInitializing(ex);
            // ToDo: Logging mit Serilog einbauen
        }
    }

    //* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * Settings * * * * * * * * * * * * * * * * * * * * * * * * * * *
    public void ApplyNewCombatStyle(BarClass selectedBar)
    {
        //abilityKeybindings = abilityKeybindAdapter.TList;
        abilityKeybindings = profile.ActiveAbilityKeybindObjects;
        abilityKeybindings = abilityKeybindings.Where(
                            obj => obj.Bar.Name == selectedBar.Name ||
                            obj.Bar.Name == GlobalApplicationSettings.OtherSettings.AllBarsName).ToList();

        currentStyle = selectedBar;
        lbl_styleInfo.Content = $"Current Style: {currentStyle.Name}";

        string imagePath = Path.Combine(Environment.CurrentDirectory, selectedBar.Img);
        imgCombatStyle.Source = new BitmapImage(new Uri(imagePath));

        //Logic for cooldown of surge / escape when using mobile perk
        if (hasMobilePerk)
        {
            IEnumerable<KeybindClass> surgeEscapeAbilities = abilityKeybindings.Where(obj => obj.Ability.Name.ToLower().Equals(surgeAbilityToLowerName) ||
                                                                                       obj.Ability.Name.ToLower().Equals(escapeAbilityToLowerName) ||
                                                                                       obj.Ability.Name.ToLower().Equals(bladedDiveAbilityToLowerName) ||
                                                                                       obj.Ability.Name.ToLower().Equals(diveAbilityToLowerName));
            if (surgeEscapeAbilities != null && surgeEscapeAbilities.Count() > 0)
                foreach (KeybindClass abil in surgeEscapeAbilities)
                    abil.Ability.CooldownInSec = 10.2;
        }
    }

    public void ApplyNewCombatStyle(BarKeybindClass selectedBar) => ApplyNewCombatStyle(selectedBar.Bar);

    //* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * Display * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    private void MoveImages()
    {
        displayImg0.Source = displayImg1.Source;
        displayImg1.Source = displayImg2.Source;
        displayImg2.Source = displayImg3.Source;
        displayImg3.Source = displayImg4.Source;
        displayImg4.Source = displayImg5.Source;
        displayImg5.Source = displayImg6.Source;
        displayImg6.Source = displayImg7.Source;
        displayImg7.Source = displayImg8.Source;
        displayImg8.Source = displayImg9.Source;
        displayImg9.Source = displayImg10.Source;
        displayName0.Content = displayName1.Content;
        displayName1.Content = displayName2.Content;
        displayName2.Content = displayName3.Content;
        displayName3.Content = displayName4.Content;
        displayName4.Content = displayName5.Content;
        displayName5.Content = displayName6.Content;
        displayName6.Content = displayName7.Content;
        displayName7.Content = displayName8.Content;
        displayName8.Content = displayName9.Content;
        displayName9.Content = displayName10.Content;
        displayCd0.Ability = displayCd1.Ability;
        displayCd1.Ability = displayCd2.Ability;
        displayCd2.Ability = displayCd3.Ability;
        displayCd3.Ability = displayCd4.Ability;
        displayCd4.Ability = displayCd5.Ability;
        displayCd5.Ability = displayCd6.Ability;
        displayCd6.Ability = displayCd7.Ability;
        displayCd7.Ability = displayCd8.Ability;
        displayCd8.Ability = displayCd9.Ability;
        displayCd9.Ability = displayCd10.Ability;
    }

    private async Task DisplayAbility(Ability currentAbility)
    {
        MoveImages();

        string currentImagePath = currentAbility.Img;
        string imagePath = Path.Combine(Environment.CurrentDirectory, currentImagePath);

        await Task.Run(() =>
        {
            this.Dispatcher.Invoke(() =>
            {
                BitmapImage image = new BitmapImage(new Uri(imagePath));
                displayImg10.Source = image;
                displayName10.Content = currentAbility.FriendlyName; //displayName10.Content = currentAbility.Name;
                displayCd10.Ability = currentAbility;
            });
        });
    }

    //* * * * * * * * * * * * * * * * * * * Main Logic / Ability CD and GCD Logic * * * * * * * * * * * * * * * * * * * * * * * *
    //private KeyboardHookEventArgs storedKeyInputForAbilityQueueing = null;
    private async Task InputCycleRegistered(KeyboardHookEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(e?.Key.ToString()) == true || e?.Key.ToString().ToLower().Equals("none") == true) return;

            // Get all Abilities for the current key input
            string modifier = GetInputModifier(e);

            KeybindClass abilityKeybind = abilityKeybindings.FirstOrDefault(ability => ability.Key.ToLower() == e.Key.ToString().ToLower() &&
                                          ability.Modifier.ToString().ToLower() == modifier.ToLower());

            // Logic if the current key input is a ability keybinding
            if (abilityKeybind != null && abilityKeybind.Ability != null)
            {
                Ability currentAbility = abilityKeybind.Ability;

                //cancel if: GCD is set to be tracked and we're currently INSIDE the gcd. Skip this check though, if the ability can be activated INSIDE gcd
                if (currentAbility.CanActivateDuringGCD == false)
                    if (trackGlobalCooldown && gcdStopwatch.ElapsedMilliseconds < CONFIGURED_GCD)
                    {
                        //if (allowAbilityQueueing)
                        //{
                        //    AbilityQueuingGCDHelperFlag = true;
                        //    storedKeyInputForAbilityQueueing = e;
                        //}
                        return;
                    }

                // Cancel this InputCycle if the ability is on cooldown and the cooldown tracking is activated
                if (trackAbilityCooldown && AbilityIsOnCooldown(currentAbility))
                    return;

                //if (allowAbilityQueueing) storedKeyInputForAbilityQueueing = null;

                KeybindClass pressedKey = new KeybindClass()
                {
                    Modifier = modifier,
                    Key = e.Key.ToString(),
                    Ability = currentAbility
                };

                //Reset GCD if: We're currently have a ability that can trigger GCD and GCD is set to be tracked
                if (currentAbility.CanActivateDuringGCD == false && trackGlobalCooldown)
                    gcdStopwatch.Restart();

                pressedKey.Ability.AbilityStopwatch.Restart(); //Start()
                previousPressedKeys.Add(pressedKey);

                await DisplayAbility(currentAbility);

                // Refresh Bleed-Timer if visible
                if(displayBleedTimerForMagicT95DW && currentAbility.IsBleedAbility)
                {
                    if (bleedTimerForMagicT95DW.IsRunning)
                    {
                        bleedTimerForMagicT95DW.Restart();
                    }
                    else
                    {
                        bleedTimerForMagicT95DW.Start();
                    }
                }
            }

            // Logic if the current key input is a bar change keybinding instead and/or as well. (The same key can be binded to both a ability (or switch) and a bar.
            if (barKeybindings != null)
            {
                // Get all Bar Keybindings equivalent to current style that fits the keybinding...
                IEnumerable<BarKeybindClass> equivalentBarsToCurrentStyle = equivalentBarsToCurrentStyle = barKeybindings?.Where(p => p.Key.ToLower().Equals(e.Key.ToString().ToLower()));
                equivalentBarsToCurrentStyle = equivalentBarsToCurrentStyle?.Where(p => p.Modifier.ToLower().Equals(modifier.ToLower()));
                equivalentBarsToCurrentStyle = equivalentBarsToCurrentStyle?.Where(p => p.Bar.Name.ToLower().Equals(currentStyle.Name.ToLower()) == true); //equivalentBarsToCurrentStyle = equivalentBarsToCurrentStyle.Where(p => p.Bar.Name.ToLower().Equals(currentStyle.Name.ToLower()) == false || p.Bar.Name.Equals("ALL"));

                if (equivalentBarsToCurrentStyle != null && equivalentBarsToCurrentStyle.Count() > 0)
                {
                    // ...then check if any of these bars can switch to another bar...
                    IEnumerable<BarKeybindClass> barsToSwitch = equivalentBarsToCurrentStyle?.Where(bar => bar.BarDependency != null);
                    if (barsToSwitch != null && barsToSwitch.Count() > 0)
                    {
                        // ...and apply the new combat style if the current bar can switch to another bar.
                        ApplyNewCombatStyle(barsToSwitch.FirstOrDefault().BarDependency);
                    }
                }

                //barsToSwitch = barKeybindings.FirstOrDefault(
                //                p => p.Key.ToLower().Equals(e.Key.ToString().ToLower()) &&
                //                p.Modifier.ToLower().Equals(modifier.ToLower()) &&
                //                (p.Bar.Name.ToLower().Equals(currentStyle.Name.ToLower()) == false || p.Bar.Name.Equals("ALL")));

                //if (listBarChange != null)
                //    ApplyNewCombatStyle(listBarChange);
            }

        }
        catch (Exception ex)
        {
            // ToDo: Logging mit Serilog einbauen
        }
    }

    private string GetInputModifier(KeyboardHookEventArgs e)
    {
        string modifier = string.Empty;
        if (e.isAltPressed)
            modifier = "ALT";
        else if (e.isCtrlPressed)
            modifier = "CTRL";
        else if (e.isShiftPressed)
            modifier = "SHIFT";
        else if (e.isWinPressed)
            modifier = "WIN";
        return modifier;
    }

    private bool AbilityIsOnCooldown(Ability currentAbility)
    {
        KeybindClass previousMatchingAbility = previousPressedKeys.LastOrDefault(key => key.Ability.Name.ToLower().Equals(currentAbility.Name.ToLower()));

        if (previousMatchingAbility != null && previousMatchingAbility.Ability.IsOnCooldown)
        {
            //ToDo: Add Column to the ability grids with a checkable checkbox and implement this behavior in the Model Class instead of hard-coding this here.
            //Logic for Double Surge and Double Escape. Doesn't consider however, that one single surge does put escape on cooldown as well (and the other way around)
            if (hasDblSurgeAndEscape &&
               (previousMatchingAbility.Ability.Name.ToLower().Equals(surgeAbilityToLowerName) || previousMatchingAbility.Ability.Name.ToLower().Equals(escapeAbilityToLowerName)))
            {
                //If double surge has been activated the first time without cooldown:
                if (previousMatchingAbility.Ability.IsOnCooldown == false && currentAbility.DoubleAbilitySecondActivation == false)
                {
                    currentAbility.DoubleAbilitySecondActivation = false;
                    return false;
                }
                //If surge is used the second time:
                if (previousMatchingAbility.Ability.IsOnCooldown == true && currentAbility.DoubleAbilitySecondActivation == false)
                {
                    currentAbility.DoubleAbilitySecondActivation = true;
                    return false;
                }
                //If 2x surge has been used previously, and the first used surge comes off cooldown again: 
                else if (previousMatchingAbility.Ability.IsOnCooldown == false && currentAbility.DoubleAbilitySecondActivation == true)
                {
                    currentAbility.DoubleAbilitySecondActivation = false;
                    return false;
                }
                //If both surges are on cooldown:
                else if (previousMatchingAbility.Ability.IsOnCooldown == true && currentAbility.DoubleAbilitySecondActivation == true)
                {
                    return true;
                }
            }

            return true;
        }

        // The ability is otherwise marked as ready by default. (=false => off cooldown)
        return false;
    }

    //* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * Event Hooks * * * * * * * * * * * * * * * * * * * * * * * * * *
    private async void KeyDownEventMethod(KeyboardHookEventArgs e) => await InputCycleRegistered(e);

    private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            this.DragMove();
    }

    //private bool AbilityQueuingGCDHelperFlag = false; //Required, because otherwise GCD might get triggered twice if spamming buttons.
    private void ActionEvery10MS(object sender, ElapsedEventArgs e)
    {
        try
        {
            //Trigger InputCycleRegistered() if ability queueing is enabled and a keyinput was previously identified during GCD
            //if(allowAbilityQueueing && storedKeyInputForAbilityQueueing != null && AbilityQueuingGCDHelperFlag)
            //{
            //    AbilityQueuingGCDHelperFlag = false;
            //    InputCycleRegistered(storedKeyInputForAbilityQueueing);
            //}

            //Refresh GCD Display every 10 ms
            this.Dispatcher.Invoke(() =>
            {
                long displayingConfiguredGCD = CONFIGURED_GCD - gcdStopwatch.ElapsedMilliseconds;

                if (CONFIGURED_GCD == DEFAULT_GCD)
                {
                    if (displayingConfiguredGCD > 0)
                        lbl_gcdInfo.Content = $"GCD: {Math.Round(displayingConfiguredGCD / 1000.0, 1)} s";
                    else
                        lbl_gcdInfo.Content = $"GCD: Ready";
                }
                else
                {
                    long displayingDefaultGCD = DEFAULT_GCD - gcdStopwatch.ElapsedMilliseconds;
                    if (displayingConfiguredGCD > 0 && displayingDefaultGCD > 0)
                        lbl_gcdInfo.Content = $"GCD: {Math.Round(displayingConfiguredGCD / 1000.0, 1)} s ({Math.Round(displayingDefaultGCD / 1000.0, 1)} s)";
                    else if (displayingConfiguredGCD > 0 && displayingDefaultGCD <= 0)
                        lbl_gcdInfo.Content = $"GCD: {Math.Round(displayingConfiguredGCD / 1000.0, 1)} s (Ready)";
                    else if (displayingConfiguredGCD <= 0 && displayingDefaultGCD > 0)
                        lbl_gcdInfo.Content = $"GCD: Ready ({Math.Round(displayingDefaultGCD / 1000.0, 1)} s)";
                    else
                        lbl_gcdInfo.Content = $"GCD: Ready";
                }
            });
        }
        catch (Exception ex)
        {
            // ToDo: Logging mit Serilog einbauen
        }
    }
    private void BleedTimerElapsed(object sender, ElapsedEventArgs e)
    {
        try
        {
            this.Dispatcher.Invoke(() =>
            {
                lbl_bleedInfoTimerForMagicT95DW.Content = $"Bleed Timer: {Convert.ToInt32(bleedTimerForMagicT95DW.TimeLeft.TotalSeconds)} s";
            });
        }
        catch (Exception ex)
        {
            // ToDo: Logging mit Serilog einbauen
        }
    }

}
