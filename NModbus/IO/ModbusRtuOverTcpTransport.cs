namespace NModbus.IO
{
    internal class ModbusRtuOverTcpTransport : ModbusRtuTransport
    {
        internal ModbusRtuOverTcpTransport(IStreamResource streamResource, IModbusFactory modbusFactory, IModbusLogger logger) : base(streamResource, modbusFactory, logger)
        {
        }
    }
}