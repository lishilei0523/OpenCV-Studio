using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace SD.OpenCV.Primitives.Models
{
    /// <summary>
    /// 缩放模式
    /// </summary>
    [Serializable]
    [DataContract]
    public enum ScaleMode
    {
        /// <summary>
        /// 绝对缩放
        /// </summary>
        [EnumMember]
        [Description("绝对缩放")]
        Absolute = 0,

        /// <summary>
        /// 相对缩放
        /// </summary>
        [EnumMember]
        [Description("相对缩放")]
        Relative = 1,

        /// <summary>
        /// 适应缩放
        /// </summary>
        [EnumMember]
        [Description("适应缩放")]
        Adaptive = 2
    }
}
