using Microsoft.SPOT;
using System;

namespace Aerial_HERO.ZSDK
{
    public abstract class RobotModule: IRobotModule
    {
        public String Name { get; private set; }
        public RobotModule(String Name)
        {
            this.Name = Name;
        }

        public virtual int Begin()
        {
            Debug.Print(ToString() + " [BEGIN] Override me! Don't call base.Begin();");
            return 0;
        }

        public virtual int Finish()
        {
            Debug.Print(ToString() + " [FINISH] Override me! Don't call base.Finish();");
            return 0;
        }

        public virtual int Run(Gamepad gamepad)
        {
            Debug.Print(ToString() + " [RUN] Override me! Don't call base.Run();");
            return 0;
        }

        public override String ToString()
        {
            return "[" + base.ToString() + "] [ROBOTMODULE] [" + Name + "]";
        }
    }
}
