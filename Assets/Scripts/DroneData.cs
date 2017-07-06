using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyUDP
{
    public class DroneData
    {
        private Byte state0;
        private Byte state1;
        private UInt16 leftTopX;
        private UInt16 leftTopY;
        private UInt16 rightBottomX;
        private UInt16 rightBottomY;
        private UInt16 ptDirect;
        private UInt16 ptUpDown;
        private UInt16 directOffset;
        private UInt16 upDownOffset;
        private UInt32 sequence;
        private Byte workMode;
        private Byte state2;
        private UInt16 directAngle;
        private UInt16 upDownAngle;

        private Byte verification;

        public byte State0
        {
            get
            {
                return state0;
            }

            set
            {
                state0 = value;
            }
        }

        public byte State1
        {
            get
            {
                return state1;
            }

            set
            {
                state1 = value;
            }
        }

        public ushort LeftTopX
        {
            get
            {
                return leftTopX;
            }

            set
            {
                leftTopX = value;
            }
        }

        public ushort LeftTopY
        {
            get
            {
                return leftTopY;
            }

            set
            {
                leftTopY = value;
            }
        }

        public ushort RightBottomX
        {
            get
            {
                return rightBottomX;
            }

            set
            {
                rightBottomX = value;
            }
        }

        public ushort RightBottomY
        {
            get
            {
                return rightBottomY;
            }

            set
            {
                rightBottomY = value;
            }
        }

        public ushort PtDirect
        {
            get
            {
                return ptDirect;
            }

            set
            {
                ptDirect = value;
            }
        }

        public ushort PtUpDown
        {
            get
            {
                return ptUpDown;
            }

            set
            {
                ptUpDown = value;
            }
        }

        public ushort DirectOffset
        {
            get
            {
                return directOffset;
            }

            set
            {
                directOffset = value;
            }
        }

        public ushort UpDownOffset
        {
            get
            {
                return upDownOffset;
            }

            set
            {
                upDownOffset = value;
            }
        }

        public uint Sequence
        {
            get
            {
                return sequence;
            }

            set
            {
                sequence = value;
            }
        }

        public byte WorkMode
        {
            get
            {
                return workMode;
            }

            set
            {
                workMode = value;
            }
        }

        public byte State2
        {
            get
            {
                return state2;
            }

            set
            {
                state2 = value;
            }
        }

        public ushort DirectAngle
        {
            get
            {
                return directAngle;
            }

            set
            {
                directAngle = value;
            }
        }

        public ushort UpDownAngle
        {
            get
            {
                return upDownAngle;
            }

            set
            {
                upDownAngle = value;
            }
        }

        public byte Verification
        {
            get
            {
                return verification;
            }

            set
            {
                verification = value;
            }
        }

        public FlyData Fly
        {
            get
            {
                return fly;
            }

            set
            {
                fly = value;
            }
        }

        //ctl r ctl e
        public DroneData(Byte[] source)
        {
            state0 = source[0];
            state1 = source[1];
            leftTopX = BitConverter.ToUInt16(source,14);
            leftTopY = BitConverter.ToUInt16(source, 16);
            rightBottomX = BitConverter.ToUInt16(source, 18);
            rightBottomY = BitConverter.ToUInt16(source, 20);
            ptDirect = BitConverter.ToUInt16(source, 22);
            ptUpDown = BitConverter.ToUInt16(source, 24);
            directOffset = BitConverter.ToUInt16(source, 26);
            upDownOffset = BitConverter.ToUInt16(source, 28);
            sequence = BitConverter.ToUInt32(source, 30);
            workMode = source[34];
            state2 = source[35];
            directAngle = BitConverter.ToUInt16(source, 36);
            upDownAngle = BitConverter.ToUInt16(source, 38);
            this.fly = new FlyData(source,46);

        }
        private FlyData fly;
    }

    public class FlyData
    {
        public FlyData(Byte[] source, int index)
        {
            this.UpdownAngle = BitConverter.ToUInt16(source, index);
            this.RotateAngle = BitConverter.ToUInt16(source, index + 2);
            this.MovingAngle = BitConverter.ToUInt16(source, index + 4);
            this.Year = source[index + 6];
            this.Month = source[index + 7];
            this.Date = source[index + 8];
            this.Hour = source[index + 9];
            this.Min = source[index + 10];
            this.Sec = source[index + 11];
            this.PercentSec = source[index + 12];
            this.Latitude = BitConverter.ToInt32(source, index + 13);
            this.Longtitude = BitConverter.ToInt32(source, index + 17);
            this.Stars = source[index + 21];
            this.Height = BitConverter.ToUInt16(source, index + 22);
            this.Speed = BitConverter.ToUInt16(source, index + 24);
            this.TrackAngle = BitConverter.ToUInt16(source, index + 28);
            this.RHeight = BitConverter.ToUInt16(source, index + 30);
        }
        // 8 9
        private UInt16 updownAngle;
        // 10 11
        private UInt16 rotateAngle;
        // 12 13
        private UInt16 movingAngle;
        // 14
        private Byte year;
        // 15
        private Byte month;
        // 16
        private Byte date;
        // 17
        private Byte hour;
        // 18
        private Byte min;
        // 19
        private Byte sec;
        // 20
        private Byte percentSec;
        // 21-24
        private Int32 latitude;//纬度
        // 25-28
        private Int32 longtitude;//经度
        // 29
        private Byte stars;
        // 30 31
        private UInt16 height;
        // 32 33
        private UInt16 speed;
        // 34 35
        // private UInt16
        // 36 37
        private UInt16 trackAngle;
        // 38 39
        private UInt16 rHeight;

        public ushort UpdownAngle
        {
            get
            {
                return updownAngle;
            }

            set
            {
                updownAngle = value;
            }
        }

        public ushort RotateAngle
        {
            get
            {
                return rotateAngle;
            }

            set
            {
                rotateAngle = value;
            }
        }

        public ushort MovingAngle
        {
            get
            {
                return movingAngle;
            }

            set
            {
                movingAngle = value;
            }
        }

        public byte Year
        {
            get
            {
                return year;
            }

            set
            {
                year = value;
            }
        }

        public byte Month
        {
            get
            {
                return month;
            }

            set
            {
                month = value;
            }
        }

        public byte Date
        {
            get
            {
                return date;
            }

            set
            {
                date = value;
            }
        }

        public byte Hour
        {
            get
            {
                return hour;
            }

            set
            {
                hour = value;
            }
        }

        public byte Min
        {
            get
            {
                return min;
            }

            set
            {
                min = value;
            }
        }

        public byte Sec
        {
            get
            {
                return sec;
            }

            set
            {
                sec = value;
            }
        }

        public byte PercentSec
        {
            get
            {
                return percentSec;
            }

            set
            {
                percentSec = value;
            }
        }

        public int Latitude
        {
            get
            {
                return latitude;
            }

            set
            {
                latitude = value;
            }
        }

        public int Longtitude
        {
            get
            {
                return longtitude;
            }

            set
            {
                longtitude = value;
            }
        }

        public byte Stars
        {
            get
            {
                return stars;
            }

            set
            {
                stars = value;
            }
        }

        public ushort Height
        {
            get
            {
                return height;
            }

            set
            {
                height = value;
            }
        }

        public ushort Speed
        {
            get
            {
                return speed;
            }

            set
            {
                speed = value;
            }
        }

        public ushort TrackAngle
        {
            get
            {
                return trackAngle;
            }

            set
            {
                trackAngle = value;
            }
        }

        public ushort RHeight
        {
            get
            {
                return rHeight;
            }

            set
            {
                rHeight = value;
            }
        }
        // 40
        // private Byte
    }
}