using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NModbus.Extensions;
using NModbus.IO;
using NModbus.Logging;

namespace NModbus.Device.Network
{
    /// <summary>
    /// Modbus Serial Over UDP/IP 服务器端
    /// </summary>
    internal class ModbusSerialOverUdpNetwork : ModbusNetwork
    {
        private readonly UdpClient _udpClient;
        private readonly IModbusSerialTransport _serialTransport;

        public ModbusSerialOverUdpNetwork(UdpClient udpClient, IModbusFactory modbusFactory, IModbusLogger logger)
            : base(new ModbusRtuOverUdpTransport(new UdpClientAdapter(udpClient), modbusFactory, logger), modbusFactory, logger)
        {
            _udpClient = udpClient;
            _serialTransport = (IModbusSerialTransport) Transport;
        }

        /// <summary>
        ///     Start slave listening for requests.
        /// </summary>
        public override Task ListenAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            Logger.Information("Start Modbus Udp Server.");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // read request and build message
                    byte[] frame = _serialTransport.ReadRequest();

                    //Create the request
                    IModbusMessage request = ModbusFactory.CreateModbusRequest(frame);

                    //Check the message
                    if (_serialTransport.CheckFrame && !_serialTransport.ChecksumsMatch(request, frame))
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
                        _serialTransport.Write(response);
                    }
                }
            }
            catch (SocketException se)
            {
                // this happens when slave stops
                if (se.SocketErrorCode != SocketError.Interrupted)
                {
                    throw;
                }
            }

            return Task.FromResult(0);
        }
    }
}