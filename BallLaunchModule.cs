using Microsoft.SPOT;
using System;

namespace Aerial_HERO
{
    public class BallLaunchModule : ZSDK.RobotModule
    {
        protected const Single
            WINCH_DIRECTION_START = -0.3f,
            WINCH_DIRECTION_END = -1f,
            DOG_FIRE_DIRECTION = 0.25f,
            DOG_RESET_DIRECTION = -0.25f;

        protected const Int64
            FIRE_WAIT = 2 * TimeSpan.TicksPerSecond,
            WINCH_START_WAIT = 4 * TimeSpan.TicksPerSecond;

        public const UInt16
            WINCH_TALONSRX_ID = 6,
            DOG_TALONSRX_ID = 5;
        protected CTRE.TalonSrx Winch, Dog;

        public enum FireMode
        {
            Normal,
            NoReset
        }
        public FireMode Mode { get; protected set; }

        public enum BallLaunchState
        {
            Unknown,
            Loaded,
            Loading,
            Released,
            Releasing,
            Reseting
        }
        public BallLaunchState State { get; protected set; }
        public DateTime WinchStartTime { get; protected set; }
        public DateTime FireTime { get; protected set; }

        public BallLaunchModule() : base("BALL LAUNCH")
        {
            Winch = new CTRE.TalonSrx(WINCH_TALONSRX_ID);
            Dog = new CTRE.TalonSrx(DOG_TALONSRX_ID);
            State = BallLaunchState.Unknown;
            Mode = FireMode.Normal;
        }

        public override Int32 Begin()
        {
#if DEBUG
            Debug.Print(ToString() + " [BEGIN]");
#endif
            // TODO: Set and check config for winch and dog.
            Winch.ConfigLimitMode(CTRE.TalonSrx.LimitMode.kLimitMode_SwitchInputsOnly);
            Dog.ConfigLimitMode(CTRE.TalonSrx.LimitMode.kLimitMode_SwitchInputsOnly);

            Winch.ConfigFwdLimitSwitchNormallyOpen(true);
            Winch.ConfigRevLimitSwitchNormallyOpen(false);

            Dog.ConfigFwdLimitSwitchNormallyOpen(false);
            Dog.ConfigRevLimitSwitchNormallyOpen(false);

            Winch.Enable();
            Dog.Enable();
            return 0;
        }

        public override Int32 Finish()
        {
#if DEBUG
            Debug.Print(ToString() + " [FINISH]");
#endif
            Winch.Disable();
            Dog.Disable();
            return 0;
        }

        public override Int32 Run(ZSDK.Gamepad gamepad)
        {
            Boolean Fire = false;
            if (gamepad is ZSDK.LogitechGamepad)
            {
                if ((gamepad as ZSDK.LogitechGamepad).Button_A) { Fire = true; Mode = FireMode.Normal; }
                else if ((gamepad as ZSDK.LogitechGamepad).Button_Y) { Fire = true; Mode = FireMode.NoReset; }
            }
            else
            {
                if (gamepad.Buttons[0]) { Fire = true; Mode = FireMode.Normal; }
                else if (gamepad.Buttons[1]) { Fire = true; Mode = FireMode.NoReset; }
            }

            if (!Winch.GetForwardLimitOK() || !Winch.GetReverseLimitOK()) Winch.Set(0);
            if (!Dog.GetForwardLimitOK() || !Dog.GetReverseLimitOK()) Dog.Set(0);

#if DEBUG
            Debug.Print(ToString() + " [STATE] " + State.ToString());
#endif
            switch (State)
            {
                default:
                case BallLaunchState.Unknown:
                    // State 0
                    // Assume we are started Released
                    Winch.Set(0);
                    Dog.Set(0);
                    State = BallLaunchState.Released;
                    break;

                case BallLaunchState.Loaded:
                    // State 1
                    Winch.Set(0);
                    Dog.Set(0);
                    if (Fire)
                    {
                        // Run Dog TalonSRX to fire.
                        Dog.Set(DOG_FIRE_DIRECTION);
                        State = BallLaunchState.Releasing;
                    }
                    break;

                case BallLaunchState.Loading:
                    // State 2
                    // Start the winch.
                    // Check for winch limit switch.
                    Boolean WinchLimitOK = Winch.GetReverseLimitOK();
                    if (!WinchLimitOK)
                    {
                        Winch.Set(0);
                        State = BallLaunchState.Loaded;
                    }
                    else
                    {
                        Boolean WinchStart = DateTime.Now.Subtract(WinchStartTime).Ticks <= WINCH_START_WAIT;
                        Single WinchSpeed = WinchStart ? WINCH_DIRECTION_START : WINCH_DIRECTION_END;
#if DEBUG
                        Debug.Print(ToString() + " [WINCH SPEED] " + WinchSpeed.ToString());
#endif
                        Winch.Set(WinchSpeed);
                    }
                    break;

                case BallLaunchState.Released:
                    // State 3
                    // Reset the Dog Gear.
                    // Check for dog limit switch.
                    Boolean DogLimitOK = Dog.GetReverseLimitOK();
                    if (!DogLimitOK)
                    {
                        Dog.Set(0);
                        switch (Mode)
                        {
                            default:
                            case FireMode.Normal:
                                State = BallLaunchState.Loading;
                                break;

                            case FireMode.NoReset:
                                State = BallLaunchState.Loaded;
                                break;
                        }
                        WinchStartTime = DateTime.Now;
                    }
                    else Dog.Set(DOG_RESET_DIRECTION);
                    break;

                case BallLaunchState.Releasing:
                    // State 4
                    // Wait for a complete fire.

                    Boolean DogFireLimitOK = Dog.GetForwardLimitOK();
                    if (!DogFireLimitOK)
                    {
                        FireTime = DateTime.Now;
                        State = BallLaunchState.Reseting;
                    }
                    break;

                case BallLaunchState.Reseting:
                    Int64 fire_ticks = DateTime.Now.Subtract(FireTime).Ticks;
                    if (fire_ticks >= FIRE_WAIT)
                    { State = BallLaunchState.Released; }
                    break;
            }
            return 0;
        }
    }
}
