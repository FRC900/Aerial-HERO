using CTRE;
using System;

namespace Aerial_HERO.ZSDK
{
    public sealed class LogitechGamepad : Gamepad
    {
        public LogitechGamepad(ISingleGamepadValuesProvider Provider) : base(Provider) { }

        public Single Axis_LX { get { return Axes[0]; } }
        public Single Axis_LY { get { return Axes[1]; } }

        public Single Axis_RX { get { return Axes[2]; } }
        public Single Axis_RY { get { return Axes[3]; } }

        public Single Axis_Unk1 { get { return Axes[4]; } }
        public Single Axis_Unk2 { get { return Axes[5]; } }

        public Boolean Button_X { get { return Buttons[0]; } }
        public Boolean Button_A { get { return Buttons[1]; } }
        public Boolean Button_B { get { return Buttons[2]; } }
        public Boolean Button_Y { get { return Buttons[3]; } }

        public Boolean Button_LB { get { return Buttons[4]; } }
        public Boolean Button_RB { get { return Buttons[5]; } }

        public Boolean Button_Back { get { return Buttons[6]; } }
        public Boolean Button_Start { get { return Buttons[7]; } }
    }
}
