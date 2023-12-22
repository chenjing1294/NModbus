using NModbus.Message;

namespace NModbus
{
    /// <summary>
    /// Modbus消息处理器，可以作为从站，也可以作为主站
    /// </summary>
    public interface IModbusFunctionService
    {
        /// <summary>
        /// 该 Service 示例所能处理的功能码
        /// </summary>
        byte FunctionCode { get; }

        /// <summary>
        /// 作为从站：将主站发起的原始字节帧包装起来
        /// </summary>
        /// <param name="frame">原始的完整的字节帧</param>
        IModbusMessage CreateRequest(byte[] frame);

        /// <summary>
        /// 作为从站：处理主站发起的请求
        /// </summary>
        /// <param name="request"></param>
        /// <param name="dataStore"></param>
        /// <returns></returns>
        IModbusMessage HandleSlaveRequest(IModbusMessage request, ISlaveDataStore dataStore);

        /// <summary>
        /// 作为从站：主站发起的请求已经读取了一些字节，还需要读取多少字节
        /// </summary>
        /// <param name="frameStart">已经读取的字节</param>
        /// <returns>还需要读取多少字节</returns>
        int GetRtuRequestBytesToRead(byte[] frameStart);

        /// <summary>
        /// 作为主站：已经读取了一些从站返回的响应，还需要读取多少字节
        /// </summary>
        /// <param name="frameStart">已经读取的字节</param>
        /// <returns>还需要读取多少字节</returns>
        int GetRtuResponseBytesToRead(byte[] frameStart);
    }
}