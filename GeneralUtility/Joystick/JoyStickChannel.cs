using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneralUtility;

namespace GeneralUtility.Joystick
{

    internal class JoyStickChannel
    {
        protected MotorSimulation motor = null;
        double upperLmt;
        double lowerLmt;
        internal string channelName;
        double currentValue;
        double backupValue;
        internal JoyStickChannel(string ChannelName, MotorSimulation Motor, double Value, double PosLimit, double NegLimit)
        {
            channelName = ChannelName;
            motor = Motor;
            upperLmt = PosLimit;
            lowerLmt = NegLimit;

            backupValue = Value;
            currentValue = Value;
            Bond();
        }

        internal bool Increment(double delta)
        {
            if (motor != null)
            {
                int encoder = motor.GetEncoderValue();
                int var = encoder + (int)Math.Round(delta);
                var = Math.Min(var, motor.GetMechUpperLimit());
                var = Math.Max(var, motor.GetMechLowerLimit());
                delta = var - encoder;
            }
            double value = GetValue() + delta;
            value = Math.Min(value, upperLmt);
            value = Math.Max(value, lowerLmt);
            SetValue(value);

            if (motor != null)
            {
                int encoder = motor.GetEncoderValue();
                motor.Drive(encoder + (int)Math.Round(delta));
                return true;
            }
            return false;
        }

        void Bond()
        {
            if (GetValue() < lowerLmt)
            {
                SetValue(lowerLmt);
            }
            else if (GetValue() > upperLmt)
            {
                SetValue(upperLmt);
            }
        }
        internal double GetValue()
        {
            return currentValue;
        }
        void SetValue(double Value)
        {
            currentValue = Value;
        }
        internal void Restore()
        {
            currentValue = backupValue;
        }

        string GetChannelString() { return channelName; }
        internal MotorSimulation GetMotor() { return motor; }
    }
}
