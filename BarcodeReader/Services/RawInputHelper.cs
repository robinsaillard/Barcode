using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace BarcodeReader.Services
{
    internal class RawInputHelper
    {
        #region Constants
        private const int RID_INPUT = 0x10000003;
        private const int RIM_TYPEKEYBOARD = 1;
        private const int WM_INPUT = 0x00FF;
        #endregion

        #region Structs
        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICE
        {
            public ushort usUsagePage;
            public ushort usUsage;
            public uint dwFlags;
            public IntPtr hwndTarget;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTHEADER
        {
            public uint dwType;
            public uint dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWKEYBOARD
        {
            public ushort MakeCode;
            public ushort Flags;
            public ushort Reserved;
            public ushort VKey;
            public uint Message;
            public uint ExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUT
        {
            public RAWINPUTHEADER header;
            public RAWKEYBOARD keyboard;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUTDEVICELIST
        {
            public IntPtr hDevice;
            public uint dwType;
        }
        #endregion

        #region P/Invoke
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterRawInputDevices(
            RAWINPUTDEVICE[] pRawInputDevices,
            uint uiNumDevices,
            uint cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetRawInputData(
            IntPtr hRawInput,
            uint uiCommand,
            IntPtr pData,
            ref uint pcbSize,
            uint cbSizeHeader);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetRawInputDeviceList(
            IntPtr pRawInputDeviceList,
            ref uint puiNumDevices,
            uint cbSize);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint GetRawInputDeviceInfo(
            IntPtr hDevice,
            uint uiCommand,
            IntPtr pData,
            ref uint pcbSize);

        private const uint RIDI_DEVICENAME = 0x20000007;

        #endregion

        #region Hook clavier
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc _hookCallback;
        private IntPtr _hookHandle = IntPtr.Zero;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        #endregion

        #region Fields
        private IntPtr _zapetteHandle = IntPtr.Zero;
        private readonly string _vendorId = "0581";
        private readonly string _productId = "011A";
        private readonly StringBuilder _inputBuffer = new StringBuilder();
        #endregion

        #region Events
        public event EventHandler<string> BarcodeReceived;
        #endregion

        #region Public Methods
        public void Initialize(Window window)
        {
            System.Diagnostics.Debug.WriteLine("=== RawInputHelper.Initialize() called ===");

            _zapetteHandle = FindDeviceHandle(_vendorId, _productId);
            System.Diagnostics.Debug.WriteLine(_zapetteHandle != IntPtr.Zero
                ? $"✅ Zapette trouvée : handle={_zapetteHandle}"
                : "❌ Zapette NON trouvée");

            var hwnd = new WindowInteropHelper(window).Handle;
            RegisterKeyboardRawInput(hwnd);

            var source = HwndSource.FromHwnd(hwnd);
            if (source == null)
            {
                System.Diagnostics.Debug.WriteLine("❌ HwndSource est NULL !");
                return;
            }
            source.AddHook(WndProc);

            // Hook clavier bas niveau pour bloquer les touches zapette
            _hookCallback = KeyboardHookProc;
            using (var process = System.Diagnostics.Process.GetCurrentProcess())
            using (var module = process.MainModule)
            {
                _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookCallback,
                    GetModuleHandle(module.ModuleName), 0);
            }

            System.Diagnostics.Debug.WriteLine(_hookHandle != IntPtr.Zero
                ? "✅ Hook clavier installé"
                : $"❌ Hook clavier échoué : {Marshal.GetLastWin32Error()}");
        }

        private volatile bool _isReceivingFromZapette = false;

        private IntPtr KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var kb = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

                // Vérifier si l'event vient de la zapette via Raw Input
                // On utilise dwExtraInfo ou on compare avec le device actif
                // Bloquer si c'est un KeyDown/KeyUp qui vient de la zapette
                if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN ||
                    wParam == (IntPtr)WM_KEYUP)
                {
                    // Si on est en train de recevoir des données de la zapette → bloquer
                    if (_isReceivingFromZapette)
                    {
                        return (IntPtr)1; // Bloque la touche — ne passe pas au système
                    }
                }
            }
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        public void Dispose(Window window)
        {
            var rid = new RAWINPUTDEVICE[]
            {
                new RAWINPUTDEVICE
                {
                    usUsagePage = 0x01,
                    usUsage     = 0x06,
                    dwFlags     = 0x00000001,
                    hwndTarget  = IntPtr.Zero
                }
            };
            RegisterRawInputDevices(rid, 1, (uint)Marshal.SizeOf<RAWINPUTDEVICE>());

            var hwnd = new WindowInteropHelper(window).Handle;
            var source = HwndSource.FromHwnd(hwnd);
            source?.RemoveHook(WndProc);
            _inputBuffer.Clear();
        }
        #endregion

        #region Private Methods
        private IntPtr FindDeviceHandle(string vendorId, string productId)
        {
            uint deviceCount = 0;
            uint structSize = (uint)Marshal.SizeOf<RAWINPUTDEVICELIST>();
            GetRawInputDeviceList(IntPtr.Zero, ref deviceCount, structSize);

            if (deviceCount == 0) return IntPtr.Zero;

            var listBuffer = Marshal.AllocHGlobal((int)(structSize * deviceCount));
            var handles = new System.Collections.Generic.List<IntPtr>(); // ← collecte TOUS les handles VID/PID
            try
            {
                GetRawInputDeviceList(listBuffer, ref deviceCount, structSize);

                for (int i = 0; i < deviceCount; i++)
                {
                    var device = Marshal.PtrToStructure<RAWINPUTDEVICELIST>(
                        listBuffer + (int)(i * structSize));

                    // On ne prend que les claviers (type=1)
                    if (device.dwType != 1) continue;

                    uint nameSize = 0;
                    GetRawInputDeviceInfo(device.hDevice, RIDI_DEVICENAME, IntPtr.Zero, ref nameSize);
                    if (nameSize == 0) continue;

                    var nameBuffer = Marshal.AllocHGlobal((int)(nameSize * 2));
                    try
                    {
                        GetRawInputDeviceInfo(device.hDevice, RIDI_DEVICENAME, nameBuffer, ref nameSize);
                        string name = Marshal.PtrToStringAuto(nameBuffer) ?? "";

                        //System.Diagnostics.Debug.WriteLine($"Device clavier trouvé : handle={device.hDevice} | {name}");

                        if (name.IndexOf($"VID_{vendorId}", StringComparison.OrdinalIgnoreCase) >= 0 &&
                            name.IndexOf($"PID_{productId}", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            handles.Add(device.hDevice);
                            //System.Diagnostics.Debug.WriteLine($"  ✅ Correspond VID/PID → handle={device.hDevice}");
                        }
                    }
                    finally { Marshal.FreeHGlobal(nameBuffer); }
                }
            }
            finally { Marshal.FreeHGlobal(listBuffer); }

            if (handles.Count == 0)
            {
                //System.Diagnostics.Debug.WriteLine("❌ Aucun device trouvé pour ce VID/PID");
                return IntPtr.Zero;
            }

            handles.Sort((a, b) => a.ToInt64().CompareTo(b.ToInt64()));

            //System.Diagnostics.Debug.WriteLine($"Handles trouvés : {string.Join(", ", handles)}");
            //System.Diagnostics.Debug.WriteLine($"Handle retenu   : {handles[0]}");

            return handles[0]; // ← retourne 65657 au lieu de 65673
        }

        private void RegisterKeyboardRawInput(IntPtr hwnd)
        {
            var rid = new RAWINPUTDEVICE[]
            {
                new RAWINPUTDEVICE
                {
                    usUsagePage = 0x01,
                    usUsage     = 0x06,
                    dwFlags     = 0x00000100 | 0x00000030, // RIDEV_INPUTSINK | RIDEV_NOLEGACY
                    hwndTarget  = hwnd
                }
            };

            bool ok = RegisterRawInputDevices(rid, 1, (uint)Marshal.SizeOf<RAWINPUTDEVICE>());
            System.Diagnostics.Debug.WriteLine(ok
                ? "RegisterRawInputDevices OK"
                : $"RegisterRawInputDevices échoué : {Marshal.GetLastWin32Error()}");
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            if (msg == WM_INPUT)
            {
                ProcessRawInput(lParam);
                handled = false;
            }
            return IntPtr.Zero;
        }
        private bool _shiftPressed = false;
        private bool _altGrPressed = false;

        private void ProcessRawInput(IntPtr lParam)
        {
            uint size = 0;
            uint headerSize = (uint)Marshal.SizeOf<RAWINPUTHEADER>();
            GetRawInputData(lParam, (uint)RID_INPUT, IntPtr.Zero, ref size, headerSize);

            if (size == 0) return;

            var buffer = Marshal.AllocHGlobal((int)size);
            try
            {
                GetRawInputData(lParam, (uint)RID_INPUT, buffer, ref size, headerSize);
                var raw = Marshal.PtrToStructure<RAWINPUT>(buffer);

                if (raw.header.dwType != RIM_TYPEKEYBOARD) return;
                if (_zapetteHandle != IntPtr.Zero && raw.header.hDevice != _zapetteHandle) return;

                // ← On est sûr que c'est la zapette
                _isReceivingFromZapette = true;

                ushort vkey = raw.keyboard.VKey;
                bool keyDown = raw.keyboard.Flags == 0;

                // Tracker Shift
                if (vkey == 0x10 || vkey == 0xA0 || vkey == 0xA1)
                {
                    _shiftPressed = keyDown;
                    _isReceivingFromZapette = false;
                    return;
                }

                if (vkey == 0x11 || vkey == 0x12 ||
                    vkey == 0xA2 || vkey == 0xA3 ||
                    vkey == 0xA4 || vkey == 0xA5)
                {
                    _altGrPressed = keyDown;
                    _isReceivingFromZapette = false;
                    return;
                }

                if (!keyDown)
                {
                    _isReceivingFromZapette = false;
                    return;
                }

                char? c = VKeyToCharAzerty(vkey, _shiftPressed, _altGrPressed);

                if (c == '\r')
                {
                    string barcode = _inputBuffer.ToString();
                    _inputBuffer.Clear();
                    _isReceivingFromZapette = false;
                    if (!string.IsNullOrWhiteSpace(barcode))
                        BarcodeReceived?.Invoke(this, barcode);
                }
                else
                {
                    if (c != null) _inputBuffer.Append(c);
                    _isReceivingFromZapette = false;
                }
            }
            finally { Marshal.FreeHGlobal(buffer); }
        }

        private static char? VKeyToCharAzerty(ushort vkey, bool shift, bool altGr)
        {
            switch (vkey)
            {
                // Contrôle
                case 0x0D: return '\r';
                case 0x20: return ' ';
                case 0x08: return '\b';
                case 0x09: return '\t';

                // Lettres — Shift respecté normalement
                case 0x41: return shift ? 'A' : 'a';
                case 0x42: return shift ? 'B' : 'b';
                case 0x43: return shift ? 'C' : 'c';
                case 0x44: return shift ? 'D' : 'd';
                case 0x45: return shift ? 'E' : 'e';
                case 0x46: return shift ? 'F' : 'f';
                case 0x47: return shift ? 'G' : 'g';
                case 0x48: return shift ? 'H' : 'h';
                case 0x49: return shift ? 'I' : 'i';
                case 0x4A: return shift ? 'J' : 'j';
                case 0x4B: return shift ? 'K' : 'k';
                case 0x4C: return shift ? 'L' : 'l';
                case 0x4D: return shift ? 'M' : 'm';
                case 0x4E: return shift ? 'N' : 'n';
                case 0x4F: return shift ? 'O' : 'o';
                case 0x50: return shift ? 'P' : 'p';
                case 0x51: return shift ? 'Q' : 'q';
                case 0x52: return shift ? 'R' : 'r';
                case 0x53: return shift ? 'S' : 's';
                case 0x54: return shift ? 'T' : 't';
                case 0x55: return shift ? 'U' : 'u';
                case 0x56: return shift ? 'V' : 'v';
                case 0x57: return shift ? 'W' : 'w';
                case 0x58: return shift ? 'X' : 'x';
                case 0x59: return shift ? 'Y' : 'y';
                case 0x5A: return shift ? 'Z' : 'z';

                // Chiffres — Shift IGNORÉ (la zapette envoie Shift+chiffre pour les chiffres)
                case 0x30: return '0';
                case 0x31: return shift ? '1' : '&';
                case 0x32: return shift ? '2' : 'é';
                case 0x33: return shift ? '3' : '"';
                case 0x34: return shift ? '4' : '\'';
                case 0x35: return shift ? '5' : '(';
                case 0x36: return shift ? '6' : '-';
                case 0x37: return shift ? '7' : '`';
                case 0x38: return shift ? '8' : '\\';
                case 0x39: return shift ? '9' : '^';

                // OEM — Shift IGNORÉ pour / et .
                case 0xBF: return shift ? '/' : ':';    // Shift=True ou False → toujours /
                case 0xBE: return shift ? '.' : ';';  
                case 0xBC: return shift ? '?' : ',';
                case 0xBD: return '-';
                case 0xBB: return '=';
                case 0xBA: return shift ? ':' : ';';
                case 0xC0: return '`';
                case 0xDB: return '[';
                case 0xDC: return '\\';
                case 0xDD: return ']';
                case 0xDE: return '\'';
                case 0xE2: return shift ? '>' : '<';
                case 0xDF: return shift ? '%' : 'ù';

                // Pavé numérique
                case 0x60: return '0';
                case 0x61: return '1';
                case 0x62: return '2';
                case 0x63: return '3';
                case 0x64: return '4';
                case 0x65: return '5';
                case 0x66: return '6';
                case 0x67: return '7';
                case 0x68: return '8';
                case 0x69: return '9';
                case 0x6A: return '*';
                case 0x6B: return '+';
                case 0x6D: return '-';
                case 0x6E: return '.';
                case 0x6F: return '/';

                default: return null;
            }
        }
        #endregion
    }
}