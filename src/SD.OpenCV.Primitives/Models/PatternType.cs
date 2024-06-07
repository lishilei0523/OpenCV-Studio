using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace SD.OpenCV.Primitives.Models
{
    /// <summary>
    /// 标定板类型
    /// </summary>
    [Serializable]
    [DataContract]
    public enum PatternType
    {
        /// <summary>
        /// 棋盘格
        /// </summary>
        [EnumMember]
        [Description("棋盘格")]
        Chessboard = 0,

        /// <summary>
        /// 圆形格
        /// </summary>
        [EnumMember]
        [Description("圆形格")]
        CirclesGrid = 1
    }
}
