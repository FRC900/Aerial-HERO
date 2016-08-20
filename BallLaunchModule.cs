using Microsoft.SPOT;
using System;

namespace Aerial_HERO
{
    public class BallLaunchModule : ZSDK.RobotModule
    {
        protected const Single
            WINCH_DIRECTION_START = 0.5f,
            WINCH_DIRECTION_END = 1f,
            DOG_FIRE_DIRECTION = 1f,
            DOG_RESET_DIRECTION = -1f;

        protected const Int64
            FIRE_WAIT = 2 * TimeSpan.TicksPerSecond,
            WINCH_START_WAIT = 3 * TimeSpan.TicksPerSecond;

        public const UInt16
            WINCH_TALONSRX_ID = 6,
            DOG_TALONSRX_ID = 5;
        protected CTRE.TalonSrx Winch, Dog;

        public enum BallLaunchState
        {
            Unknown,
            Loaded,
            Loading,
            Released,
            Releasing
        }
        public BallLaunchState State { get; protected set; }
        public DateTime WinchStartTime { get; protected set; }
        public DateTime FireTime { get; protected set; }

        public BallLaunchModule() : base("BALL LAUNCH")
        {
            Winch = new CTRE.TalonSrx(WINCH_TALONSRX_ID);
            Dog = new CTRE.TalonSrx(DOG_TALONSRX_ID);
            State = BallLaunchState.Unknown;
        }

        public override Int32 Begin()
        {
            Debug.Print(ToString() + " [BEGIN]");
            // TODO: Set and check config for winch and dog.
            Winch.ConfigLimitMode(CTRE.TalonSrx.LimitMode.kLimitMode_SwitchInputsOnly);
            Dog.ConfigLimitMode(CTRE.TalonSrx.LimitMode.kLimitMode_SwitchInputsOnly);

            Winch.ConfigFwdLimitSwitchNormallyOpen(true);
            Winch.ConfigRevLimitSwitchNormallyOpen(true);

            Dog.ConfigFwdLimitSwitchNormallyOpen(true);
            Dog.ConfigRevLimitSwitchNormallyOpen(true);

            Winch.Enable();
            Dog.Enable();
            return 0;
        }

        public override Int32 Finish()
        {
            Debug.Print(ToString() + " [FINISH]");
            Winch.Disable();
            Dog.Disable();
            return 0;
        }

        public override Int32 Run(ZSDK.Gamepad gamepad)
        {
            Boolean Fire = gamepad.Buttons[0];
            if (gamepad is ZSDK.LogitechGamepad) Fire = (gamepad as ZSDK.LogitechGamepad).Button_A;

            if (!Winch.GetForwardLimitOK() || !Winch.GetReverseLimitOK()) Winch.Set(0);
            if (!Dog.GetForwardLimitOK() || !Dog.GetReverseLimitOK()) Dog.Set(0);
            
            switch (State)
            {
                default:
                case BallLaunchState.Unknown:
                    // Assume we are started Released
                    State = BallLaunchState.Released;
                    break;

                case BallLaunchState.Loaded:
                    if (Fire)
                    {
                        // Run Dog TalonSRX to fire.
                        Dog.Set(DOG_FIRE_DIRECTION);
                        State = BallLaunchState.Releasing;
                        FireTime = DateTime.Now;
                    }
                    break;

                case BallLaunchState.Loading:
                    // Start the winch.
                    // Check for winch limit switch.
                    if (!Winch.GetForwardLimitOK())
                    { State = BallLaunchState.Loaded; }
                    else
                    {
                        Boolean WinchStart = WinchStartTime.Subtract(DateTime.Now).Ticks <= WINCH_START_WAIT;
                        Winch.Set(WinchStart ? WINCH_DIRECTION_START : WINCH_DIRECTION_END);
                    }
                    break;

                case BallLaunchState.Released:
                    // Reset the Dog Gear.
                    // Check for dog limit switch.
                    if (!Dog.GetReverseLimitOK())
                    { State = BallLaunchState.Loading; WinchStartTime = DateTime.Now; }
                    else Dog.Set(DOG_RESET_DIRECTION);
                    break;

                case BallLaunchState.Releasing:
                    // Wait for a complete fire.
                    
                    if (FireTime.Subtract(DateTime.Now).Ticks >= FIRE_WAIT)
                    { State = BallLaunchState.Released; }
                    break;
            }
            return 0;
        }
    }
}
