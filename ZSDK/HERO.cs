using System;

namespace Aerial_HERO.ZSDK
{
    public class HERO
    {
        private Boolean BeginRun { get; set; }
        private Boolean FinishRun { get; set; }
        public Boolean RunLoop { get; protected set; }

        public RobotModule[] Modules { get; protected set; }
        public Gamepad Gamepad { get; protected set; }

        public HERO(RobotModule[] Modules, Gamepad Gamepad)
        {
            this.Modules = Modules;
            this.Gamepad = Gamepad;
        }

        public int Begin()
        {
            int status = 0;
            foreach (RobotModule Module in Modules)
                status |= Module.Begin();
            return status;
        }

        public int Finish()
        {
            int status = 0;
            foreach (RobotModule Module in Modules)
                status |= Module.Finish();
            return status;
        }

        private int Run()
        {
            int status = 0;
            foreach (RobotModule Module in Modules)
                status |= Module.Run(Gamepad);
            return status;
        }

        private Boolean FeedWatchDog()
        {
            if (Gamepad.ConnectionStatus == CTRE.UsbDeviceConnection.Connected)
            { CTRE.Watchdog.Feed(); return true; }
            return false;
        }

        public int Start()
        {
            int status = 0;
            if (!BeginRun) status |= Begin();

            RunLoop = true;
            while (RunLoop)
            {
                FeedWatchDog();
                status |= Run();
                System.Threading.Thread.Sleep(10);
            }

            if (!FinishRun) status |= Finish();
            return status;
        }
    }
}
