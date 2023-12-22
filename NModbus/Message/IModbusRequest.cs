namespace NModbus.Message
{
    /// <summary>
    /// 表示从站发起的请求消息
    /// </summary>
    public interface IModbusRequest : IModbusMessage
    {
        /// <summary>
        /// 根据当前请求验证对应的响应
        /// </summary>
        /// <param name="response">该请求对应的响应</param>
        void ValidateResponse(IModbusMessage response);
    }
}