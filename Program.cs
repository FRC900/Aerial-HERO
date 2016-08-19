namespace Aerial_HERO
{
    public class Program
    {
        public static void Main()
        {
            ZSDK.Gamepad Gamepad = new ZSDK.LogitechGamepad(CTRE.UsbHostDevice.GetInstance());
            ZSDK.RobotModule[] Modules = {
                new DriveModule(),
                new BallLaunchModule(),
            };

            ZSDK.HERO AerialHERO = new ZSDK.HERO(Modules, Gamepad);
            AerialHERO.Start();
        }
    }
}
