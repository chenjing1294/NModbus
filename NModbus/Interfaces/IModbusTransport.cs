using System;
using NModbus.IO;

namespace NModbus
{
    public interface IModbusTransport : IDisposable
    {
        int Retries { get; set; }

        uint RetryOnOldResponseThreshold { get; set; }

        bool SlaveBusyUsesRetryCount { get; set; }

        int WaitToRetryMilliseconds { get; set; }

        int ReadTimeout { get; set; }

        int WriteTimeout { get; set; }

        T UnicastMessage<T>(IModbusMessage message) where T : IModbusMessage, new();

        /// <summary>
        /// 作为从站：读取主站发送的数据帧
        /// </summary>
        byte[] ReadRequest();

        byte[] BuildMessageFrame(IModbusMessage message);

        void Write(IModbusMessage message);

        IStreamResource StreamResource { get; }
    }
}