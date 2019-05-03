using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;
using Windows.Media.Playback;

namespace B_TrainGoIotCore.Models
{
    [Flags]
    enum EControllerButtons
    {
        None = 0x00,
        Select = 0x01,
        Start = 0x02,
        A = 0x04,
        B = 0x08,
        C = 0x10,
    }

    class MasterController : IDisposable
    {
        #region const

        private const int SpiClockFrequency = 100000;

        private const byte CmdRetStatus = 0x5a;

        private static readonly byte[] CmdReadValue = {0x80, 0x42, 0x00, 0x00, 0x00};

        #endregion

        #region field

        private SpiDevice _device;

        private readonly byte[] _recvBuffer = new byte[5];

        #endregion

        #region property

        public bool IsInitialized => _device != null;

        public byte AccelValue { get; private set; }

        public byte BrakeValue { get; private set; }

        public EControllerButtons ButtonState { get; private set; }

        public EControllerButtons PrevButtons { get; private set; }

        public bool IsEmergency { get; private set; }

        #endregion

        public MasterController()
        {

        }

        public void Dispose()
        {

        }

        public async Task Initialize()
        {
            var settings = new SpiConnectionSettings(0)
            {
                ClockFrequency = SpiClockFrequency,
                Mode = SpiMode.Mode3
            };

            var aqs = SpiDevice.GetDeviceSelector("SPI0");

            var info = await DeviceInformation.FindAllAsync(aqs);

            if (!info.Any())
            {
                throw new Exception("SPI device is not found");
            }

            _device = await SpiDevice.FromIdAsync(info.FirstOrDefault().Id, settings);

            if (_device == null)
            {
                throw new Exception("SPI device is not found");
            }
        }

        public bool IsButtonDown(EControllerButtons button)
        {
            return ButtonState.HasFlag(button);
        }

        public bool IsButtonReleased(EControllerButtons button)
        {
            return !ButtonState.HasFlag(button) && PrevButtons.HasFlag(button);
        }

        public bool ReadState()
        {
            if (_device == null) return false;

            var updated = false;

            _device.TransferFullDuplex(CmdReadValue, _recvBuffer);

            var accel = GetCurrentAccelValue();
            var brake = GetCurrentBrakeValue();

            UpdateButtonState();

            if (!IsEmergency && brake == 9)
            {
                AccelValue = 0;
                IsEmergency = true;
                updated = true;
            }
            else if (IsEmergency)
            {
                IsEmergency = accel > 0 || brake > 0;
                updated = !IsEmergency;
            }
            else if (AccelValue == 0 && BrakeValue == 0)
            {
                // 加減速が無い状態からの変化はブレーキ優先
                if (brake > 0)
                {
                    BrakeValue = brake;
                    updated = true;
                }
                else if (accel > 0)
                {
                    AccelValue = accel;
                    updated = true;
                }
            }
            else if (BrakeValue > 0)
            {
                // ブレーキが有効
                updated = BrakeValue != brake;
                BrakeValue = brake;
            }
            else if (AccelValue > 0)
            {
                updated = AccelValue != accel;
                AccelValue = accel;
            }

            return updated;
        }

        private byte GetCurrentAccelValue()
        {
            var v = (byte)(((~_recvBuffer[3] & 0x0f) << 1) | ((~_recvBuffer[4] & 0x08) >> 3));

            var accel = AccelValue;

            switch (v)
            {
                case 0x1e:
                    accel = 0;
                    break;
                case 0x1d:
                    accel = 1;
                    break;
                case 0x1c:
                    accel = 2;
                    break;
                case 0x17:
                    accel = 3;
                    break;
                case 0x16:
                    accel = 4;
                    break;
                case 0x15:
                    accel = 5;
                    break;
                default:
                    if (IsEmergency) accel = 255;
                    break;
            }

            return accel;
        }

        private byte GetCurrentBrakeValue()
        {
            var v = (byte)((~_recvBuffer[4] & 0xf0) >> 4);

            byte brake = 0;

            switch (v)
            {
                case 0x0d:
                    brake = 0;
                    break;
                case 0x07:
                    brake = 1;
                    break;
                case 0x05:
                    brake = 2;
                    break;
                case 0x0e:
                    brake = 3;
                    break;
                case 0x0c:
                    brake = 4;
                    break;
                case 0x06:
                    brake = 5;
                    break;
                case 0x04:
                    brake = 6;
                    break;
                case 0x0b:
                    brake = 7;
                    break;
                case 0x09:
                    brake = 8;
                    break;
                case 0x0F:
                    return BrakeValue;
                default:
                    brake = 9;
                    break;
            }

            return brake;
        }

        private bool UpdateButtonState()
        {
            PrevButtons = ButtonState;

            var buttons = EControllerButtons.None;

            if (((~_recvBuffer[3] & 0x80) >> 7) > 0) buttons |= EControllerButtons.Select;

            if (((~_recvBuffer[3] & 0x10) >> 4) > 0) buttons |= EControllerButtons.Start;

            if (((~_recvBuffer[4] & 0x01) >> 0) > 0) buttons |= EControllerButtons.A;

            if (((~_recvBuffer[4] & 0x02) >> 1) > 0) buttons |= EControllerButtons.B;

            if (((~_recvBuffer[4] & 0x04) >> 2) > 0) buttons |= EControllerButtons.C;

            ButtonState = buttons;

            return PrevButtons != ButtonState;
        }
    }
}
