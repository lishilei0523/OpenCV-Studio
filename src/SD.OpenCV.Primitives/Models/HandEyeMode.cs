using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace SD.OpenCV.Primitives.Models
{
    /// <summary>
    /// 手眼模式
    /// </summary>
    [Serializable]
    [DataContract]
    public enum HandEyeMode
    {
        /// <summary>
        /// 眼在手上
        /// </summary>
        [EnumMember]
        [Description("眼在手上")]
        EyeInHand = 0,

        /// <summary>
        /// 眼在手外
        /// </summary>
        [EnumMember]
        [Description("眼在手外")]
        EyeToHand = 1
    }
}
