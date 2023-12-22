using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NModbus.Extensions;
using NModbus.Logging;

namespace NModbus.Device.Network
{
    /// <summary>
    /// Modbus Serial 从站端
    /// </summary>
    internal class ModbusSerialNetwork : ModbusNetwork
    {
        public ModbusSerialNetwork(IModbusSerialTransport transport, IModbusFactory modbusFactory, IModbusLogger logger)
            : base(transport, modbusFactory, logger)
        {
            SerialTransport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        private IModbusSerialTransport SerialTransport { get; }

        public override Task ListenAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // read request and build message
                    byte[] frame = SerialTransport.ReadRequest();

                    //Create the request
                    IModbusMessage request = ModbusFactory.CreateModbusRequest(frame);

                    //Check the message
                    if (SerialTransport.CheckFrame && !SerialTransport.ChecksumsMatch(request, frame))
                    {
                        string msg = $"Checksums failed to match {string.Join(", ", request.MessageFrame)} != {string.Join(", ", frame)}.";
                        Logger.Warning(msg);
                        throw new IOException(msg);
                    }

                    //Apply the request
                    IModbusMessage response = ApplyRequest(request);

                    if (response == null)
                    {
                        // _serialTransport.IgnoreResponse();
                    }
                    else
                    {
                        SerialTransport.Write(response);
                    }
                }
                catch (IOException ioe)
                {
                    Logger.Warning($"IO Exception encountered while listening for requests - {ioe.Message}");
                    SerialTransport.DiscardInBuffer();
                }
                catch (TimeoutException te)
                {
                    Logger.Trace($"Timeout Exception encountered while listening for requests - {te.Message}");
                    SerialTransport.DiscardInBuffer();
                }
                catch (InvalidOperationException)
                {
                    // when the underlying transport is disposed
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error($"{GetType()}: {ex.Message}");
                    SerialTransport.DiscardInBuffer();
                }
            }

            return Task.FromResult(0);
        }
    }
}