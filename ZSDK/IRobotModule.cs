using System;

namespace Aerial_HERO.ZSDK
{
    public interface IRobotModule
    {
        Int32 Begin();
        Int32 Finish();
        Int32 Run(Gamepad gamepad);
    }
}
