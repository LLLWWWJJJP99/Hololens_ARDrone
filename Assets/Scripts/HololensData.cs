using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyUDP
{
    public class HololensData
    {
        private UInt16 syncro;
        private Byte bOrder;
        private UInt16 cParameter;
        private UInt16 dParameter;
        private UInt16 edParameter;
        private UInt16 fParameter;
        private Byte verification;

        public ushort Syncro
        {
            get
            {
                return syncro;
            }

            set
            {
                syncro = value;
            }
        }

        public byte BOrder
        {
            get
            {
                return bOrder;
            }

            set
            {
                bOrder = value;
            }
        }

        public ushort CParameter
        {
            get
            {
                return cParameter;
            }

            set
            {
                cParameter = value;
            }
        }

        public ushort DParameter
        {
            get
            {
                return dParameter;
            }

            set
            {
                dParameter = value;
            }
        }

        public ushort EdParameter
        {
            get
            {
                return edParameter;
            }

            set
            {
                edParameter = value;
            }
        }

        public ushort FParameter
        {
            get
            {
                return fParameter;
            }

            set
            {
                fParameter = value;
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
    }
}
