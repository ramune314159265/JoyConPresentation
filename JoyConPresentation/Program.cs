using HidSharp;
using JoyConPresentation;
using System.Windows.Forms;
using wtf.cluster.JoyCon;
using wtf.cluster.JoyCon.Calibration;
using wtf.cluster.JoyCon.ExtraData;
using wtf.cluster.JoyCon.InputData;
using wtf.cluster.JoyCon.InputReports;
using wtf.cluster.JoyCon.Rumble;

class Program
{
    static public Screen targetScreen = Screen.AllScreens.Last();
    static float sensitivityX = 35.0f;
    static float sensitivityY = 35.0f;
    static JoyCon? joycon;
    static ButtonsFull? previousState = null;
    static CalibrationData? calibration;
    static int currentMouseX;
    static int currentMouseY;

    static async Task Main()
    {
        HidDevice? device = GetDevice();
        if (device == null)
        {
            Console.WriteLine("JoyCon が見つかりませんでした");
            return;
        }

        joycon = new JoyCon(device);
        Console.WriteLine("JoyConを開始しています");
        joycon.Start();
        await joycon.SetInputReportModeAsync(JoyCon.InputReportType.Full);
        await joycon.EnableRumbleAsync(true);
        await joycon.SetPlayerLedsAsync(JoyCon.LedState.On, JoyCon.LedState.Off, JoyCon.LedState.Off, JoyCon.LedState.Off);
        CalibrationData facCal = await joycon.GetFactoryCalibrationAsync();
        CalibrationData userCal = await joycon.GetUserCalibrationAsync();
        calibration = facCal + userCal;
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
            var slideshowWindow = PowerPoint.GetSlideShowWindow();
            if (slideshowWindow is null)
            {
                return Task.CompletedTask;
            }
            slideshowWindow.View.Next();
            RumbleFeedback(sender);
        }
        if (j.Buttons.Y && !previousState.Y)
        {
            var slideshowWindow = PowerPoint.GetSlideShowWindow();
            if (slideshowWindow is null)
            {
                return Task.CompletedTask;
            }
            slideshowWindow.View.Previous();
            RumbleFeedback(sender);
        }
        if (j.Buttons.ZR)
        {
            if (!previousState.ZR)
            {
                joycon.EnableImuAsync(true);
                currentMouseX = targetScreen.Bounds.Width / 2;
                currentMouseY = targetScreen.Bounds.Height / 2;
                var slideshowWindow = PowerPoint.GetSlideShowWindow();
                if (slideshowWindow is not null)
                {
                    slideshowWindow.View.LaserPointerEnabled = true;
                }
                RumbleFeedback(sender);
            }
            var imuFrame = j.Imu.Frames[1];
            var calibrated = imuFrame.GetCalibrated(calibration.ImuCalibration!);

            float deltaX = (float)calibrated.GyroZ * sensitivityX * 0.015f;
            float deltaY = -(float)calibrated.GyroY * sensitivityY * 0.015f;

            currentMouseX += (int)deltaX;
            currentMouseY += (int)deltaY;

            currentMouseX = Math.Clamp(currentMouseX, 0, targetScreen.Bounds.Width);
            currentMouseY = Math.Clamp(currentMouseY, 0, targetScreen.Bounds.Height);

            JoyConPresentation.Cursor.Move(Screen.AllScreens.Length - 1, currentMouseX, currentMouseY);
        }
        if(!j.Buttons.ZR && previousState.ZR)
        {
            var slideshowWindow = PowerPoint.GetSlideShowWindow();
            if (slideshowWindow is not null)
            {
                slideshowWindow.View.LaserPointerEnabled = false;
                slideshowWindow.View.PointerType = Microsoft.Office.Interop.PowerPoint.PpSlideShowPointerType.ppSlideShowPointerNone;
            }
            joycon.EnableImuAsync(false);
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