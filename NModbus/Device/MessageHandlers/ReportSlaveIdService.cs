using NModbus.Message;

namespace NModbus.Device.MessageHandlers
{
    /// <summary>
    /// 功能码17：报告从站ID（仅用于串行链路）
    /// </summary>
    public class ReportSlaveIdService : IModbusFunctionService
    {
        public byte FunctionCode => ModbusFunctionCodes.ReportSlaveId;

        public IModbusMessage CreateRequest(byte[] frame)
        {
            ReportSlaveIdRequest request = new ReportSlaveIdRequest();
            request.Initialize(frame);
            return request;
        }

        public IModbusMessage HandleSlaveRequest(IModbusMessage request, ISlaveDataStore dataStore)
        {
            ReportSlaveIdResponse response = new ReportSlaveIdResponse(request.SlaveAddress, request.FunctionCode);
            return response;
        }

        public int GetRtuRequestBytesToRead(byte[] frameStart)
        {
            return 2;
        }

        public int GetRtuResponseBytesToRead(byte[] frameStart)
        {
            return frameStart[2] + 1;
        }
    }
}