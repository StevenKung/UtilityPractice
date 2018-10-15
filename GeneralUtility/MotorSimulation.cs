using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralUtility
{
    public class MotorSimulation
    {
        public enum StatusEnum
        {
            EXECUTING,
            EXECUTE_END
        };

        private int upperLimt = int.MaxValue;
        private int lowerLimt = int.MinValue;
        private int encoderPos;
        private int commandPos;
        private static Random rnd = new Random();
        private StatusEnum mstatus = StatusEnum.EXECUTING;
        public StatusEnum Status { get { return mstatus; } }
        public string DeviceName { get; set; }

        public int GetMechUpperLimit() { return upperLimt; }
        public int GetMechLowerLimit() { return lowerLimt; }
        public int GetEncoderValue() { return encoderPos; }

        public void Drive(int Command)
        {
            commandPos = Command;
            mstatus = StatusEnum.EXECUTING;
            Task.Run(() =>
            {
                for (int i = 0; i <= Command; i++)
                {
                    encoderPos = i + rnd.Next(0, 10);
                    if (i == Command)
                        mstatus = StatusEnum.EXECUTE_END;
                }
            });

        }
    }
}
