using System;

namespace Aerial_HERO
{
    internal class DriveModule : ZSDK.RobotModule
    {
        public const Single SLOW_MULT = 0.5f;
        public const UInt16
            RIGHT_TALONSRX_ID = 4,
            RIGHT_SLAVE_TALONSRX_ID = 3,
            LEFT_TALONSRX_ID = 2,
            LEFT_SLAVE_TALONSRX_ID = 1;

        protected CTRE.TalonSrx Right , RightSlave;
        protected CTRE.TalonSrx Left, LeftSlave;

        public DriveModule() : base("DRIVE")
        {
            Right = new CTRE.TalonSrx(RIGHT_TALONSRX_ID);
            RightSlave = new CTRE.TalonSrx(RIGHT_SLAVE_TALONSRX_ID);
            Left = new CTRE.TalonSrx(LEFT_TALONSRX_ID);
            LeftSlave = new CTRE.TalonSrx(LEFT_SLAVE_TALONSRX_ID);
        }

        public override Int32 Begin()
        {
            RightSlave.SetControlMode(CTRE.TalonSrx.ControlMode.kFollower);
            RightSlave.Set(RIGHT_TALONSRX_ID);

            LeftSlave.SetControlMode(CTRE.TalonSrx.ControlMode.kFollower);
            LeftSlave.Set(LEFT_TALONSRX_ID);

            Right.Enable();
            RightSlave.Enable();
            Left.Enable();
            LeftSlave.Enable();
            return 0;
        }

        public override Int32 Finish()
        {
            Right.Disable();
            RightSlave.Disable();
            Left.Disable();
            LeftSlave.Disable();
            return 0;
        }

        private void Deadband(ref Single val)
        { if (System.Math.Abs(val) < 0.10) val = 0; }

        public override Int32 Run(ZSDK.Gamepad gamepad)
        {
            Single LeftY = 0, RightY = 0;
            Boolean Slow = true;
            if (gamepad is ZSDK.LogitechGamepad)
            {
                // Tank
                LeftY = (gamepad as ZSDK.LogitechGamepad).Axis_LY;
                RightY = (gamepad as ZSDK.LogitechGamepad).Axis_RY;
                Slow = !((gamepad as ZSDK.LogitechGamepad).Button_LB || (gamepad as ZSDK.LogitechGamepad).Button_RB);
            }
            else
            {
                // Arcade
                Single Y = gamepad.Axes[1], X = gamepad.Axes[2];
                LeftY = Y + X;
                RightY = Y - X;
                Slow = !gamepad.Buttons[0];
            }
            Deadband(ref LeftY);
            Deadband(ref RightY);

            // Drive in slow mode.
            if (Slow)
            {
                LeftY *= SLOW_MULT;
                RightY *= SLOW_MULT;
            }

            Right.Set(RightY);
            Left.Set(LeftY);
            return 0;
        }
    }
}