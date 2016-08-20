using Microsoft.SPOT;
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
            RunLoop = true;
            this.Modules = Modules;
            this.Gamepad = Gamepad;
        }

        public int Begin()
        {
            int status = 0;
            if (BeginRun) return status;
            foreach (RobotModule Module in Modules)
                status |= Module.Begin();
            BeginRun = true;
            return status;
        }

        public int Finish()
        {
            int status = 0;
            if (FinishRun) return status;
            foreach (RobotModule Module in Modules)
                status |= Module.Finish();
            FinishRun = true;
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
            status |= Begin();

            RunLoop = true;
            Debug.Print("[HERO] [START]");
            while (RunLoop)
            {
                FeedWatchDog();
                status |= Run();
                System.Threading.Thread.Sleep(10);
            }

            status |= Finish();
            return status;
        }
    }
}
