using System.Diagnostics.CodeAnalysis;

namespace NModbus
{
    /// <summary>
    /// 代表主站的响应消息或从站的请求消息
    /// </summary>
    public interface IModbusMessage
    {
        /// <summary>
        ///     The function code tells the server what kind of action to perform.
        /// </summary>
        byte FunctionCode { get; set; }

        /// <summary>
        ///     Address of the slave (server).
        /// </summary>
        byte SlaveAddress { get; set; }

        /// <summary>
        /// 从站地址+PDU，不包括校验
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        byte[] MessageFrame { get; }

        /// <summary>
        /// 协议数据单元：功能码+数据
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        byte[] ProtocolDataUnit { get; }

        /// <summary>
        ///     A unique identifier assigned to a message when using the IP protocol.
        /// </summary>
        ushort TransactionId { get; set; }

        /// <summary>
        /// 将主站发送的数据帧初始化
        /// </summary>
        /// <param name="frame">Bytes of Modbus frame.</param>
        void Initialize(byte[] frame);
    }
}