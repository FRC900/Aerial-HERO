using System;

namespace Aerial_HERO.ZSDK
{
    public class Gamepad
    {
        protected CTRE.ISingleGamepadValuesProvider Provider { get; private set; }
        protected CTRE.GamepadValues _values = new CTRE.GamepadValues();
        public CTRE.GamepadValues RawValues
        {
            get
            {
                // Read the current gamepad values
                Provider.Get(ref _values);
                // Return updated values.
                return _values;
            }
        }

        public Gamepad(CTRE.ISingleGamepadValuesProvider Provider)
        {
            this.Provider = Provider;
            _buttons = new Boolean[32]; // 32 is the max number of bits for a UInt32.
        }

        protected readonly Boolean[] _buttons;
        public Boolean[] Buttons
        {
            get
            {
                // Read the current gamepad values
                Provider.Get(ref _values);
                // Iterate through the button bits and store.
                for (UInt16 bIdx = 0; bIdx < _buttons.Length; ++bIdx)
                    _buttons[bIdx] = (_values.btns >> bIdx & 1) == 1;
                // Return updated buttons.
                return _buttons;
            }
        }

        public Single[] Axes
        {
            get
            {
                // Read the current gamepad values
                Provider.Get(ref _values);
                // Return updated axes.
                return _values.axes;
            }
        }

        public CTRE.UsbDeviceConnection ConnectionStatus
        {
            get
            {
                Int32 status = Provider.Get(ref _values);
                if (status >= 0) return CTRE.UsbDeviceConnection.Connected;
                return CTRE.UsbDeviceConnection.NotConnected;
            }
        }
    }
}
