namespace NModbus.Message
{
    public class ReportSlaveIdRequest : IModbusRequest
    {
        public ReportSlaveIdRequest()
        {
        }

        public ReportSlaveIdRequest(byte slaveId)
        {
            SlaveAddress = slaveId;
            FunctionCode = ModbusFunctionCodes.ReportSlaveId;
            MessageFrame = new byte[2] {slaveId, ModbusFunctionCodes.ReportSlaveId};
            ProtocolDataUnit = new byte[1] {ModbusFunctionCodes.ReportSlaveId};
        }

        public byte FunctionCode { get; set; }

        public byte SlaveAddress { get; set; }

        public byte[] MessageFrame { get; private set; }

        public byte[] ProtocolDataUnit { get; private set; }

        public ushort TransactionId { get; set; }

        public void Initialize(byte[] frame)
        {
            SlaveAddress = frame[0];
            FunctionCode = frame[1];
            MessageFrame = new byte[2] {SlaveAddress, FunctionCode};
            ProtocolDataUnit = new byte[1] {FunctionCode};
        }

        public void ValidateResponse(IModbusMessage response)
        {
            //ignore
        }
    }
}