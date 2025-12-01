using HidSharp;
using WindowsInput;
using WindowsInput.Native;
using wtf.cluster.JoyCon;
using wtf.cluster.JoyCon.Calibration;
using wtf.cluster.JoyCon.ExtraData;
using wtf.cluster.JoyCon.InputData;
using wtf.cluster.JoyCon.InputReports;
using wtf.cluster.JoyCon.Rumble;

class Program
{
    static ButtonsFull? previousState = null;
    static InputSimulator inputSimulator = new ();

    static async Task Main()
    {
        HidDevice? device = GetDevice();
        if(device == null)
        {
            Console.WriteLine("JoyCon が見つかりませんでした");
            return;
        }

        var joycon = new JoyCon(device);
        Console.WriteLine("JoyConを開始しています");
        joycon.Start();
        await joycon.SetInputReportModeAsync(JoyCon.InputReportType.Full);
        DeviceInfo deviceInfo = await joycon.GetDeviceInfoAsync();
        Console.WriteLine($"コントローラー {deviceInfo.ControllerType} ({deviceInfo.FirmwareVersionMajor}.{deviceInfo.FirmwareVersionMinor})");

        joycon.ReportReceived += ReportHandle;
        await Task.Delay(-1);
    }

    static Task ReportHandle(JoyCon sender, IJoyConReport input)
    {
        if (input is not InputFullWithImu j)
        {
            return Task.CompletedTask;
        }
        previousState ??= j.Buttons;
        if (j.Buttons.A && !previousState.A)
        {
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
        }
        if (j.Buttons.Y && !previousState.Y)
        {
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.LEFT);
        }
        previousState = j.Buttons;
        return Task.CompletedTask;
    }

    static HidDevice? GetDevice()
    {
        DeviceList list = DeviceList.Local;
        var nintendos = list.GetHidDevices(0x057e);
        return nintendos.FirstOrDefault();
    }
}