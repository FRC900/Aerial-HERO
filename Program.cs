namespace Aerial_HERO
{
    public class Program
    {
        public static void Main()
        {
            /* If you are not using a Logitech F710 gamepad, change `ZSDK.LogitechGamepad` to `ZSDK.Gamepad` */
            ZSDK.Gamepad Gamepad = new ZSDK.LogitechGamepad(CTRE.UsbHostDevice.GetInstance());

            /* Modules to load */
            ZSDK.RobotModule[] Modules = {
                new DriveModule(),
                new BallLaunchModule(),
            };

            ZSDK.HERO AerialHERO = new ZSDK.HERO(Modules, Gamepad);
            AerialHERO.Start();
        }
    }
}
