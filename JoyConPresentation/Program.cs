using HidSharp;

class Program
{
    static void Main()
    {
        HidDevice? device = GetDevice();
        if(device == null)
        {
            Console.WriteLine("JoyCon が見つかりませんでした");
            return;
        }
    }

    static HidDevice? GetDevice()
    {
        DeviceList list = DeviceList.Local;
        var nintendos = list.GetHidDevices(0x057e);
        return nintendos.FirstOrDefault();
    }
}