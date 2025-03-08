namespace WindowsKeyHooker_WinForms;

public partial class Hook
{
    public struct KBDLLHOOKSTRUCT
    {
        public UInt32 vkCode;
        public UInt32 scanCode;
        public UInt32 flags;
        public UInt32 time;
        public IntPtr extraInfo;
    }
}
