using Microsoft.SPOT;
using System;

namespace Aerial_HERO
{
    internal class DriveModule : ZSDK.RobotModule
    {
        public const Boolean USE_SPEED_MODE = false;

        /* By default we drive in 'SLOW' mode. */
        /* If kSpeed mode is being used configure the max RPM here. */
        public const Single 
            SLOW_MULT = 0.5f,
            MAX_RPM = 1000.0f;

        public const Boolean LEFT_INVT = true;

        /* Configure TalonSRX IDs here. */
        public const UInt16
            RIGHT_TALONSRX_ID = 2,
            RIGHT_SLAVE_TALONSRX_ID = 1,
            LEFT_TALONSRX_ID = 4,
            LEFT_SLAVE_TALONSRX_ID = 3;

        protected const UInt16
            RIGHT_EncTPR = 360,
            LEFT_EncTPR = 360;

        /* Configure PID(F) values here. */
        protected Single
            RIGHT_P = 1.00f,
            RIGHT_I = 0.10f,
            RIGHT_D = 0.01f,
            RIGHT_F = 0.00f,

            LEFT_P = 1.00f,
            LEFT_I = 0.10f,
            LEFT_D = 0.01f,
            LEFT_F = 0.00f;

        protected CTRE.TalonSrx Right, RightSlave;
        protected CTRE.TalonSrx Left, LeftSlave;

        public DriveModule() : base("DRIVE")
        {
            /* Create our device objects */
            Right = new CTRE.TalonSrx(RIGHT_TALONSRX_ID);
            RightSlave = new CTRE.TalonSrx(RIGHT_SLAVE_TALONSRX_ID);
            Left = new CTRE.TalonSrx(LEFT_TALONSRX_ID);
            LeftSlave = new CTRE.TalonSrx(LEFT_SLAVE_TALONSRX_ID);
        }

        public override Int32 Begin()
        {
#if DEBUG
            Debug.Print(ToString() + " [BEGIN]");
#endif
            /* Setup Left and Right followers */
            RightSlave.SetControlMode(CTRE.TalonSrx.ControlMode.kFollower);
            RightSlave.Set(RIGHT_TALONSRX_ID);

            LeftSlave.SetControlMode(CTRE.TalonSrx.ControlMode.kFollower);
            LeftSlave.Set(LEFT_TALONSRX_ID);

            /* Configure the Left TalonSRX */
            Left.SetInverted(LEFT_INVT);
            Left.ConfigLimitMode(CTRE.TalonSrx.LimitMode.kLimitMode_SrxDisableSwitchInputs);
            Left.ConfigFwdLimitSwitchNormallyOpen(true);
            Left.ConfigRevLimitSwitchNormallyOpen(true);
            LeftSlave.ConfigLimitMode(CTRE.TalonSrx.LimitMode.kLimitMode_SrxDisableSwitchInputs);
            LeftSlave.ConfigFwdLimitSwitchNormallyOpen(true);
            LeftSlave.ConfigRevLimitSwitchNormallyOpen(true);

            /* Configure the Right TalonSRX */
            Right.SetInverted(!LEFT_INVT);
            Right.ConfigLimitMode(CTRE.TalonSrx.LimitMode.kLimitMode_SrxDisableSwitchInputs);
            Right.ConfigFwdLimitSwitchNormallyOpen(true);
            Right.ConfigRevLimitSwitchNormallyOpen(true);
            RightSlave.ConfigLimitMode(CTRE.TalonSrx.LimitMode.kLimitMode_SrxDisableSwitchInputs);
            RightSlave.ConfigFwdLimitSwitchNormallyOpen(true);
            RightSlave.ConfigRevLimitSwitchNormallyOpen(true);

            if (USE_SPEED_MODE)
            {
                /* Setup Left and Right leaders with PID */
                Right.SetFeedbackDevice(CTRE.TalonSrx.FeedbackDevice.QuadEncoder);
                Right.SetSensorDirection(false);
                Right.ConfigEncoderCodesPerRev(RIGHT_EncTPR);

                Right.SetControlMode(CTRE.TalonSrx.ControlMode.kSpeed);
                Right.SetPID(0, RIGHT_P, RIGHT_I, RIGHT_D);
                Right.SetF(0, RIGHT_F);

                Left.SetFeedbackDevice(CTRE.TalonSrx.FeedbackDevice.QuadEncoder);
                Left.SetSensorDirection(false);
                Left.ConfigEncoderCodesPerRev(LEFT_EncTPR);

                Left.SetControlMode(CTRE.TalonSrx.ControlMode.kSpeed);
                Left.SetPID(0, LEFT_P, LEFT_I, LEFT_D);
                Left.SetF(0, LEFT_F);
            }

            /* Enable the TalonSRXs */
            Right.Enable();
            RightSlave.Enable();
            Left.Enable();
            LeftSlave.Enable();
            return 0;
        }

        public override Int32 Finish()
        {
#if DEBUG
            Debug.Print(ToString() + " [FINISH]");
#endif
            /* Disable the TalonSRXs */
            Right.Disable();
            RightSlave.Disable();
            Left.Disable();
            LeftSlave.Disable();
            return 0;
        }

        /// <summary>
        /// Deadband the control value from the gamepad. ABS Values under 0.10f are made 0.
        /// </summary>
        /// <param name="val">ref value to process</param>
        private void Deadband(ref Single val)
        { if (System.Math.Abs(val) < 0.10) val = 0; }

        private void ReadGamepad(ZSDK.Gamepad gamepad, ref Single LeftVal, ref Single RightVal, ref Boolean Slow)
        {
#if DEBUG
            for (int idx = 0; idx < gamepad.Axes.Length; ++idx)
                Debug.Print("[AXIS] [" + idx.ToString() + "] Value: " + gamepad.Axes[idx].ToString());
#endif

            if (gamepad is ZSDK.LogitechGamepad)
            {
                /* Tank Drive */
                LeftVal = (gamepad as ZSDK.LogitechGamepad).Axis_LY;
                RightVal = (gamepad as ZSDK.LogitechGamepad).Axis_RY;
                Boolean Fast = (gamepad as ZSDK.LogitechGamepad).Button_LB || (gamepad as ZSDK.LogitechGamepad).Button_RB;
                Slow = !Fast; // Herp-Derp :P
            }
            else
            {
                /* Arcade fallback */
                Single Y = gamepad.Axes[1], X = gamepad.Axes[2];
                LeftVal = Y + X;
                RightVal = Y - X;
                Slow = !gamepad.Buttons[0];
            }
            Deadband(ref LeftVal);
            Deadband(ref RightVal);
        }

        public override Int32 Run(ZSDK.Gamepad gamepad)
        {
            Single LeftVal = 0, RightVal = 0;
            Boolean Slow = true;
            ReadGamepad(gamepad, ref LeftVal, ref RightVal, ref Slow);

#if DEBUG
            Debug.Print(ToString() + " [RAW] L:" + LeftVal.ToString() + " - R:" + RightVal.ToString());
#endif

            /* Apply RPM conversion to values. */
            if (USE_SPEED_MODE)
            {
                LeftVal *= MAX_RPM;
                RightVal *= MAX_RPM;
            }

            /* Drive in slow mode. */
            if (Slow)
            {
                LeftVal *= SLOW_MULT;
                RightVal *= SLOW_MULT;
            }

#if DEBUG
            Debug.Print(ToString() + " [SET] L:" + LeftVal.ToString() + " - R:" + RightVal.ToString());
#endif

            Right.Set(RightVal);
            Left.Set(LeftVal);
            return 0;
        }
    }
}
 