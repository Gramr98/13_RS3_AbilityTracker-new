﻿using System.Runtime.InteropServices;

namespace WindowsKeyHooker_WinForms;

public class KeyboardHookEventArgs
{
    #region PInvoke
    [DllImport("user32.dll")]
    static extern short GetKeyState(VirtualKeyStates nVirtKey);

    private enum VirtualKeyStates : int
    {
        VK_LWIN = 0x5B,
        VK_RWIN = 0x5C,
        VK_LSHIFT = 0xA0,
        VK_RSHIFT = 0xA1,
        VK_LCONTROL = 0xA2,
        VK_RCONTROL = 0xA3,
        VK_LALT = 0xA4, //aka VK_LMENU
        VK_RALT = 0xA5, //aka VK_RMENU
    }

    private const int KEY_PRESSED = 0x8000;
    #endregion

    public Keys Key { get; private set; }

    public bool isAltPressed { get { return isLAltPressed || isRAltPressed; } }
    public bool isLAltPressed { get; private set; }
    public bool isRAltPressed { get; private set; }

    public bool isCtrlPressed { get { return isLCtrlPressed || isRCtrlPressed; } }
    public bool isLCtrlPressed { get; private set; }
    public bool isRCtrlPressed { get; private set; }

    public bool isShiftPressed { get { return isLShiftPressed || isRShiftPressed; } }
    public bool isLShiftPressed { get; private set; }
    public bool isRShiftPressed { get; private set; }

    public bool isWinPressed { get { return isLWinPressed || isRWinPressed; } }
    public bool isLWinPressed { get; private set; }
    public bool isRWinPressed { get; private set; }

    internal KeyboardHookEventArgs(Hook.KBDLLHOOKSTRUCT lParam)
    {
        this.Key = (Keys)lParam.vkCode;

        //Control.ModifierKeys doesn't capture alt/win, and doesn't have r/l granularity
        this.isLAltPressed = Convert.ToBoolean(GetKeyState(VirtualKeyStates.VK_LALT) & KEY_PRESSED) || this.Key == Keys.LMenu;
        this.isRAltPressed = Convert.ToBoolean(GetKeyState(VirtualKeyStates.VK_RALT) & KEY_PRESSED) || this.Key == Keys.RMenu;

        this.isLCtrlPressed = Convert.ToBoolean(GetKeyState(VirtualKeyStates.VK_LCONTROL) & KEY_PRESSED) || this.Key == Keys.LControlKey;
        this.isRCtrlPressed = Convert.ToBoolean(GetKeyState(VirtualKeyStates.VK_RCONTROL) & KEY_PRESSED) || this.Key == Keys.RControlKey;

        this.isLShiftPressed = Convert.ToBoolean(GetKeyState(VirtualKeyStates.VK_LSHIFT) & KEY_PRESSED) || this.Key == Keys.LShiftKey;
        this.isRShiftPressed = Convert.ToBoolean(GetKeyState(VirtualKeyStates.VK_RSHIFT) & KEY_PRESSED) || this.Key == Keys.RShiftKey;

        this.isLWinPressed = Convert.ToBoolean(GetKeyState(VirtualKeyStates.VK_LWIN) & KEY_PRESSED) || this.Key == Keys.LWin;
        this.isRWinPressed = Convert.ToBoolean(GetKeyState(VirtualKeyStates.VK_RWIN) & KEY_PRESSED) || this.Key == Keys.RWin;

        if (new[] { Keys.LMenu, Keys.RMenu, Keys.LControlKey, Keys.RControlKey, Keys.LShiftKey, Keys.RShiftKey, Keys.LWin, Keys.RWin }.Contains(this.Key))
            this.Key = Keys.None;
    }

    public override string ToString()
    {
        return string.Format("Key={0}; Win={1}; Alt={2}; Ctrl={3}; Shift={4}", new object[] { Key, isWinPressed, isAltPressed, isCtrlPressed, isShiftPressed });
    }
}

