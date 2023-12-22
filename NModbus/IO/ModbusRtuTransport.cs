using System;
using System.IO;
using System.Linq;
using NModbus.Extensions;
using NModbus.Logging;
using NModbus.Utility;

namespace NModbus.IO
{
    internal class ModbusRtuTransport : ModbusSerialTransport, IModbusRtuTransport
    {
        public const int RequestFrameStartLength = 7;
        public const int ResponseFrameStartLength = 4;

        internal ModbusRtuTransport(IStreamResource streamResource, IModbusFactory modbusFactory, IModbusLogger logger)
            : base(streamResource, modbusFactory, logger)
        {
            if (modbusFactory == null) throw new ArgumentNullException(nameof(modbusFactory));
        }

        internal int RequestBytesToRead(byte[] frameStart)
        {
            byte functionCode = frameStart[1];

            IModbusFunctionService service = ModbusFactory.GetFunctionServiceOrThrow(functionCode);

            return service.GetRtuRequestBytesToRead(frameStart);
        }

        internal int ResponseBytesToRead(byte[] frameStart)
        {
            byte functionCode = frameStart[1];

            if (functionCode > Modbus.ExceptionOffset)
            {
                return 1;
            }

            IModbusFunctionService service = ModbusFactory.GetFunctionServiceOrThrow(functionCode);

            return service.GetRtuResponseBytesToRead(frameStart);
        }

        public virtual byte[] Read(int count)
        {
            byte[] frameBytes = new byte[count];
            int numBytesReadTotal = 0;

            while (numBytesReadTotal != count)
            {
                int numBytesRead = StreamResource.Read(frameBytes, numBytesReadTotal, count - numBytesReadTotal);
                if (numBytesRead == 0)
                {
                    throw new IOException("Read resulted in 0 bytes returned.");
                }

                numBytesReadTotal += numBytesRead;
            }

            return frameBytes;
        }

        public override byte[] BuildMessageFrame(IModbusMessage message)
        {
            var messageFrame = message.MessageFrame;
            var crc = ModbusUtility.CalculateCrc(messageFrame);
            var messageBody = new MemoryStream(messageFrame.Length + crc.Length);

            messageBody.Write(messageFrame, 0, messageFrame.Length);
            messageBody.Write(crc, 0, crc.Length);

            return messageBody.ToArray();
        }

        public override bool ChecksumsMatch(IModbusMessage message, byte[] messageFrame)
        {
            ushort messageCrc = BitConverter.ToUInt16(messageFrame, messageFrame.Length - 2);
            ushort calculatedCrc = BitConverter.ToUInt16(ModbusUtility.CalculateCrc(message.MessageFrame), 0);

            return messageCrc == calculatedCrc;
        }

        public override IModbusMessage ReadResponse<T>()
        {
            byte[] frame = ReadResponse();

            Logger.LogFrameRx(frame);

            return CreateResponse<T>(frame);
        }

        private byte[] ReadResponse()
        {
            byte[] frameStart = Read(ResponseFrameStartLength);
            byte[] frameEnd = Read(ResponseBytesToRead(frameStart));
            byte[] frame = frameStart.Concat(frameEnd).ToArray();

            return frame;
        }

        public override void IgnoreResponse()
        {
            byte[] frame = ReadResponse();

            Logger.LogFrameIgnoreRx(frame);
        }

        public override byte[] ReadRequest()
        {
            byte[] frame = null;
            byte[] bytes = Read(2); //读取从设备地址和功能码
            byte functionCode = bytes[1];
            switch (functionCode)
            {
                case ModbusFunctionCodes.ReportSlaveId:
                    byte[] read = Read(RequestBytesToRead(bytes));
                    frame = bytes.Concat(read).ToArray();
                    break;
                default:
                    byte[] frameStart = Read(RequestFrameStartLength - 2);
                    frameStart = bytes.Concat(frameStart).ToArray();
                    byte[] frameEnd = Read(RequestBytesToRead(frameStart));
                    frame = frameStart.Concat(frameEnd).ToArray();
                    break;
            }

            Logger.LogFrameRx(frame);

            return frame;
        }
    }
}