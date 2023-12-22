using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NModbus.IO;
using NModbus.Logging;

namespace NModbus.Device.Network
{
    /// <summary>
    /// Modbus Serial Over TCP/IP 服务器端
    /// </summary>
    internal class ModbusSerialOverTcpNetwork : ModbusNetwork
    {
        private const int TimeWaitResponse = 1000;
        private readonly object _serverLock = new object();

        private readonly ConcurrentDictionary<string, ModbusSerialOverTcpConnection> _masters =
            new ConcurrentDictionary<string, ModbusSerialOverTcpConnection>();

        private TcpListener _server;

        internal ModbusSerialOverTcpNetwork(TcpListener tcpListener, IModbusFactory modbusFactory, IModbusLogger logger)
            : base(new EmptyTransport(modbusFactory), modbusFactory, logger)
        {
            _server = tcpListener ?? throw new ArgumentNullException(nameof(tcpListener));
        }

        /// <summary>
        ///     Gets the Modbus TCP Masters connected to this Modbus TCP Slave.
        /// </summary>
        public ReadOnlyCollection<TcpClient> Masters
        {
            get { return new ReadOnlyCollection<TcpClient>(_masters.Values.Select(mc => mc.TcpClient).ToList()); }
        }

        /// <summary>
        ///     Gets the server.
        /// </summary>
        /// <value>The server.</value>
        /// <remarks>
        ///     This property is not thread safe, it should only be consumed within a lock.
        /// </remarks>
        private TcpListener Server
        {
            get
            {
                if (_server == null)
                {
                    throw new ObjectDisposedException("Server");
                }

                return _server;
            }
        }

        /// <summary>
        ///     Start slave listening for requests.
        /// </summary>
        public override async Task ListenAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            Logger.Information("Start Modbus Tcp Server.");
            // TODO: add state {stopped, listening} and check it before starting
            Server.Start();
            // Cancellation code based on https://stackoverflow.com/a/47049129/11066760
            using (cancellationToken.Register(() => Server.Stop()))
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        TcpClient client = await Server.AcceptTcpClientAsync().ConfigureAwait(false);
                        ModbusSerialOverTcpConnection masterConnection = new ModbusSerialOverTcpConnection(client, this, ModbusFactory, Logger);
                        masterConnection.ModbusMasterTcpConnectionClosed += OnMasterConnectionClosedHandler;
                        _masters.TryAdd(client.Client.RemoteEndPoint.ToString(), masterConnection);
                    }
                }
                catch (InvalidOperationException)
                {
                    // Either Server.Start wasn't called (a bug!)
                    // or the CancellationToken was cancelled before
                    // we started accepting (giving an InvalidOperationException),
                    // or the CancellationToken was cancelled after
                    // we started accepting (giving an ObjectDisposedException).
                    //
                    // In the latter two cases we should surface the cancellation
                    // exception, or otherwise rethrow the original exception.
                    cancellationToken.ThrowIfCancellationRequested();
                    throw;
                }
            }
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        ///     unmanaged resources.
        /// </param>
        /// <remarks>Dispose is thread-safe.</remarks>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // double-check locking
                if (_server != null)
                {
                    lock (_serverLock)
                    {
                        if (_server != null)
                        {
                            _server.Stop();
                            _server = null;
                            foreach (var key in _masters.Keys)
                            {
                                if (_masters.TryRemove(key, out ModbusSerialOverTcpConnection connection))
                                {
                                    connection.ModbusMasterTcpConnectionClosed -= OnMasterConnectionClosedHandler;
                                    connection.Dispose();
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool IsSocketConnected(Socket socket)
        {
            bool poll = socket.Poll(TimeWaitResponse, SelectMode.SelectRead);
            bool available = (socket.Available == 0);
            return poll && available;
        }

        private void OnMasterConnectionClosedHandler(object sender, TcpConnectionEventArgs e)
        {
            if (!_masters.TryRemove(e.EndPoint, out ModbusSerialOverTcpConnection connection))
            {
                string msg = $"EndPoint {e.EndPoint} cannot be removed, it does not exist.";
                throw new ArgumentException(msg);
            }

            connection.Dispose();
            Logger.Information($"Removed Master {e.EndPoint}");
        }
    }
}