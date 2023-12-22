using System.IO;
using System.Linq;

namespace NModbus.Message
{
    public class ReportSlaveIdResponse : IModbusMessage
    {
        public ReportSlaveIdResponse()
        {
        }

        public byte[] RawBytes { get; set; }

        public ReportSlaveIdResponse(byte slaveAddress, byte functionCode)
        {
            SlaveAddress = slaveAddress;
            FunctionCode = functionCode;
            //PDU=功能码+[字节数+从设备ID+运行状态+附加情报1+附加情报2+附加情报3]
            ProtocolDataUnit = new byte[7] {FunctionCode, 5, SlaveAddress, 0xFF, (byte) 'M', (byte) 'S', (byte) 'E'};
            var pdu = ProtocolDataUnit;
            var stream = new MemoryStream(1 + pdu.Length);
            stream.WriteByte(SlaveAddress);
            stream.Write(pdu, 0, pdu.Length);
            MessageFrame = stream.ToArray();
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
            //PDU=功能码+[字节数+从设备ID+运行状态+附加情报1+附加情报2+附加情报3+...]
            ProtocolDataUnit = frame.Skip(1).Take(frame.Length - 3).ToArray();
            MessageFrame = frame.Take(frame.Length - 2).ToArray();
            RawBytes = frame;
        }
    }
}