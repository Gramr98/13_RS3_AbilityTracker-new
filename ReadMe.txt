==== Ability Tracker in C# (.NET Core 6.0) programmed by Gramr98 / Stefan from Q4 of 2022 until now. - Latest Change: 2025-03-02 ====

This application provides a powerful, feature-rich, but also still rather easy to use ability tracker for RS3. It works by 'hooking' to the official Windows OS Files, reads the keyboard input (like any game), and then displays images of abilities that where manually mapped / configured in beforehand. This application has been implemented and tested with the Windows Versions "Windows 10 Home 22H2" to "Windows 11 Home 24H2".

The tracker can also import ability data and images from the official Wiki by downloading the entire html page, automatically going throuigh the relevant tables and storing the relevant data into the JSON-Files. By doing this, you will overwrite all previous configured abilities and keybindings though. The User-Agent for these API calls can be found in the file 'GlobalApplicationSettings.cs'. But consider, that imported abilities from the wiki may not be 100% correct. In some cases, the boolean flag 'Can activate ability during GCD' or the Ability Cooldown may be set wrongly and requires manual change in the JSON-Files before continuing configuring Keybindings and Profiles!
=> Note: The Wiki Import doesn't currently work perfectly fine. The user must manually delete the mentioned data. Read the section 'TO DO (Known Bugs to fix)' below for more information.

Although I coded most of this tracker myself, the source code from Shaggy's Ability Tracker has been used as a template for the original basic logic. However, there isn't much left of their code anymore, because the new features required a more complex implementation. But nonetheless, if he hadn't provided the code via github, I never would have started creating this tracker in the first place. Thank you for publicly providing your code for everyone to use for free!!! https://www.reddit.com/r/runescape/comments/no1la7/rs3_ability_tracker/ https://github.com/ShaggyHW/RS3AbilityTracker

Feel free to message me on GitHub or Discord (@gramr98stefan) if you have any questions or feedback.

* * * * * * * * * * * * * * * * * * Known Bugs (To Fix): * * * * * * * * * * * * * * * * * *

- !!! Since a change in the Wiki from a short while ago, the import for certain abilities doesn't work properly anymore. For the time being I recommend going through all abilities manually and checking the correct import of abilities. Whenever an ability is missing, add a .png-File to the folder "./Images/Abilities" and then create and configure the ability in the application.

- When starting the application and it crashes during start, it may occure in some instances that some of the JSON-Files in the folder "./Data" will become empty or even entirely deleted!
  => WORKAROUND: Until there is a fix, I recommend to make a backup of those .json-Files whenever you make a bigger change to the data (Abilities, Bars, Keybindings and Profiles).

- On a Wiki Import: Delete all images in the Folder "./Images/Abilities" before the import 
  => Currently doesnt work, because of write protection by Visual Studio and / or Windows.
  => Users must manually delete all the images in the mentioned folder before starting the wiki import.

- Consider this case: You create and configure an ability. You then create a profile and an ability keybinding of that profile. If you now do changes to the original ability, the changes don't get applied to the profile and ability keybinding. The reason for this is that the keybinding and the profile stores the ability separately instead of a reference.
  => ToDo ME: Fix this by just referencing an ID-Field or GUID-Field instead of the whole object inside the Ability-Object and Profile-Object. => C#-Comments: "ToDo: Work with IDs instead for comparison"

* * * * * * * * * * * * * * * * * * TO DO: * * * * * * * * * * * * * * * * * *

- !!! Make setup process and installation more friendly and easier for the end user. Compile a single .exe-File with all dependencies for easier download as well. Write a guide and/or record a tutorial video on how to use the ability tracker.

- !!! Implement section "To Fix" above.

- Implement the possibility to rename bars, abilities, profiles, and so on. => Only possible after the ID / GUID Implementation mentioned above.

- Implement behavior of abilities:
  > Right click on a ability or keybinding: Menu Item "Add behavior" > Open new Window
  > Behavior Dropdown or checkbox: A behavior for Surge would be e.g.: "Is affected by mobile perk"
  > Property Enum "AbilityBehavior": { "None", "AffectedByMobilePerk", ... }
  > Implement a Window Popup of the ability keybinding grid or the ability grid to allow linking of the abilities that share cooldowns
	  -> e.g. Surge and Escape share a Cooldown.
	  -> Configurable via right click > Menu item "Add Ability Link"
	  -> Property Enum "AbilityLink": { "None", "Linked" }

- Implement filtering of the grids:
  > https://andydunkel.net/2020/05/15/wpf-filtern-im-datagrid-mit-collectionviewsource/
  > https://stackoverflow.com/questions/15568325/filter-a-datagrid-in-wpf

- Implement Ability Queueing

- Implement Ability Categories to allow easier searching, sorting and handling in the UI

- Implement Logging with Serilog: https://www.youtube.com/watch?v=dLR_D2IJE1M + https://www.youtube.com/watch?v=_iryZxv8Rxw
