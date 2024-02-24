using System;
using System.Runtime.InteropServices;

public class DisplayInterface
{
    [StructLayout(LayoutKind.Sequential)]
    struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
        public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

[DllImport("user32.dll")]
static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

    public static (int Width, int Height) GetMainDisplaySize()
    {
        const int ENUM_CURRENT_SETTINGS = -1;

        DEVMODE devMode = default;
        devMode.dmSize = (short)Marshal.SizeOf(devMode);

        if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref devMode))
        {
            return (devMode.dmPelsWidth, devMode.dmPelsHeight);
        }
        else
        {
            throw new Exception("Failed to get display settings");
        }
    }
}
