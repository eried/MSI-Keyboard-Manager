﻿using System;
using System.Linq;
using HidSharp;

namespace MSI_Keyboard_Manager
{
    class HidManager
    {
        private HidStream _stream;
        private HidDevice _device;

        public HidManager()
        {
            var loader = new HidDeviceLoader();
            _device = loader.GetDevices(0x1770, 0xff00).First();
            _device.TryOpen(out _stream);
        }

        public void SetColor(Constants.Regions region, Constants.Colors color, Constants.Intensities intensity)
        {
            byte[] array = {1, 2, 66, (byte) region, (byte) color, (byte) intensity, 0, 236};
            _stream.SetFeature(array);
        }

        public void SetMode(Constants.Modes mode)
        {
            byte[] array = {1, 2, 65, (byte) mode, 0, 0, 0, 236};
            _stream.SetFeature(array);
        }

        public void SetDualColorMode(Constants.Intensities intensity, Tuple<Constants.Colors, Constants.Colors> leftColors,
             Tuple<Constants.Colors, Constants.Colors> middleColors, Tuple<Constants.Colors, Constants.Colors> rightColors,  int speed)
        {
            SendCommandForDualModeRegion(Constants.Regions.Left, (byte) leftColors.Item1, (byte)leftColors.Item2,
                (byte) intensity, (byte) speed);
            SendCommandForDualModeRegion(Constants.Regions.Middle, (byte) middleColors.Item1, (byte)middleColors.Item2,
                (byte) intensity, (byte) speed);
            SendCommandForDualModeRegion(Constants.Regions.Right, (byte) rightColors.Item1, (byte) rightColors.Item2,
                (byte) intensity, (byte) speed);

            SendCommand(65, 6, 0, 0);
        }

        private void SendCommandForDualModeRegion(Constants.Regions region, byte firstColor, byte secondColor,
            byte intensity, byte speed)
        {
            int modifier;
            switch (region)
            {
                case Constants.Regions.Left:
                    modifier = 0;
                    break;
                case Constants.Regions.Middle:
                    modifier = 3;
                    break;
                case Constants.Regions.Right:
                    modifier = 6;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(region), region, null);
            }
            SendCommand(67, (byte) (Constants.Regions.Left + modifier), firstColor, intensity);
            SendCommand(67, (byte) (Constants.Regions.Middle + modifier), secondColor, intensity);
            SendCommand(67, (byte) (Constants.Regions.Right + modifier), speed, speed, speed);

        }


        private void SendCommand(byte command, byte areaOrMode, byte color, byte intensity, byte last = 0)
        {
            byte[] array = {1, 2, command, areaOrMode, color, intensity, last, 236};
            _stream.SetFeature(array);
        }

        public void Close()
        {
            _stream.Close();
        }
    }
}