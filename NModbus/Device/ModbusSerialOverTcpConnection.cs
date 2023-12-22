using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using NModbus.IO;
using NModbus.Logging;
using NModbus.Extensions;

namespace NModbus.Device
{
    /// <summary>
    /// Represents an incoming connection from a Modbus master. Contains the slave's logic to process the connection.
    /// </summary>
    internal class ModbusSerialOverTcpConnection : ModbusDevice, IDisposable
    {
        private readonly TcpClient _client;
        private readonly string _endPoint;
        private readonly IModbusSlaveNetwork _slaveNetwork;
        private readonly IModbusFactory _modbusFactory;
        private readonly Task _requestHandlerTask;
        private readonly IModbusSerialTransport _serialTransport;

        public ModbusSerialOverTcpConnection(TcpClient client, IModbusSlaveNetwork slaveNetwork, IModbusFactory modbusFactory, IModbusLogger logger)
            : base(new ModbusRtuOverTcpTransport(new TcpClientAdapter(client), modbusFactory, logger))
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _endPoint = client.Client.RemoteEndPoint.ToString();
            _slaveNetwork = slaveNetwork ?? throw new ArgumentNullException(nameof(slaveNetwork));
            _modbusFactory = modbusFactory ?? throw new ArgumentNullException(nameof(modbusFactory));
            _serialTransport = (IModbusSerialTransport) Transport;
            _requestHandlerTask = Task.Run(HandleRequestAsync);
        }

        /// <summary>
        ///     Occurs when a Modbus master TCP connection is closed.
        /// </summary>
        public event EventHandler<TcpConnectionEventArgs> ModbusMasterTcpConnectionClosed;

        public IModbusLogger Logger { get; }

        public string EndPoint => _endPoint;

        public TcpClient TcpClient => _client;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _client.Dispose();
            }

            base.Dispose(disposing);
        }

        private void HandleRequestAsync()
        {
            try
            {
                while (true)
                {
                    // read request and build message
                    byte[] frame = _serialTransport.ReadRequest();

                    //Create the request
                    IModbusMessage request = _modbusFactory.CreateModbusRequest(frame);

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
            catch (IOException e)
            {
                Logger.Warning(e.Message);
                ModbusMasterTcpConnectionClosed?.Invoke(this, new TcpConnectionEventArgs(EndPoint));
            }
        }

        protected IModbusMessage ApplyRequest(IModbusMessage request)
        {
            //Attempt to find a slave for this address
            IModbusSlave slave = _slaveNetwork.GetSlave(request.SlaveAddress);

            // only service requests addressed to our slaves
            if (slave == null)
            {
                Console.WriteLine($"NModbus Slave Network ignoring request intended for NModbus Slave {request.SlaveAddress}");
            }
            else
            {
                // perform action
                return slave.ApplyRequest(request);
            }

            return null;
        }
    }
}