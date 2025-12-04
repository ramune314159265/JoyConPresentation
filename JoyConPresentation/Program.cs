using AHRS;
using HidSharp;
using JoyConPresentation;
using System.Numerics;
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
    static InputSimulator inputSimulator = new();
    static MadgwickAHRS filter = new(0.015f, 0.1f);
    static Pointer? pointer;
    static double px = 0;
    static double py = 0;
    static CalibrationData? calibration;
    static StickParametersSet? sticksParameters;
    static public Screen targetScreen = Screen.AllScreens.Last();

    static async Task Main()
    {
        Thread pointerThread = new(() =>
        {
            Application.EnableVisualStyles();
            pointer = new Pointer();
            Application.Run(pointer);
            pointer.SetVisible(false);
        });
        pointerThread.SetApartmentState(ApartmentState.STA);
        pointerThread.Start();
        HidDevice? device = GetDevice();
        if (device == null)
        {
            Console.WriteLine("JoyCon が見つかりませんでした");
            return;
        }

        var joycon = new JoyCon(device);
        Console.WriteLine("JoyConを開始しています");
        joycon.Start();
        await joycon.SetInputReportModeAsync(JoyCon.InputReportType.Full);
        await joycon.EnableRumbleAsync(true);
        await joycon.EnableImuAsync(true);
        await joycon.SetPlayerLedsAsync(JoyCon.LedState.On, JoyCon.LedState.Off, JoyCon.LedState.Off, JoyCon.LedState.Off);
        CalibrationData facCal = await joycon.GetFactoryCalibrationAsync();
        CalibrationData userCal = await joycon.GetUserCalibrationAsync();
        calibration = facCal + userCal;
        sticksParameters = await joycon.GetStickParametersAsync();
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
            RumbleFeedback(sender);
        }
        if (j.Buttons.Y && !previousState.Y)
        {
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.LEFT);
            RumbleFeedback(sender);
        }
        if (j.Buttons.ZR)
        {
            if (!previousState.ZR)
            {
                pointer.SetVisible(true);
                px = targetScreen.Bounds.Width / 2;
                py = targetScreen.Bounds.Height / 2;
                RumbleFeedback(sender);
            }

            StickPositionCalibrated rightStickCalibrated = j.RightStick.GetCalibrated(calibration.RightStickCalibration!, sticksParameters.RightStickParameters.DeadZone);

            px += rightStickCalibrated.X * 15;
            py += -rightStickCalibrated.Y * 15;

            px = Math.Clamp(0, px, targetScreen.Bounds.Width);
            py = Math.Clamp(0, py, targetScreen.Bounds.Height);

            pointer.MovePoint((int)px, (int)py);
        }
        if (!j.Buttons.ZR && previousState.ZR)
        {
            pointer.SetVisible(false);
        }
        previousState = j.Buttons;
        return Task.CompletedTask;
    }

    static async void RumbleFeedback(JoyCon joycon)
    {
        await joycon.WriteRumble(new RumbleSet(0, 0, 300, 0.3));
        await Task.Delay(50);
        await joycon.WriteRumble(new RumbleSet(0, 0, 0, 0));
    }

    static HidDevice? GetDevice()
    {
        DeviceList list = DeviceList.Local;
        var nintendos = list.GetHidDevices(0x057e);
        return nintendos.FirstOrDefault();
    }
}