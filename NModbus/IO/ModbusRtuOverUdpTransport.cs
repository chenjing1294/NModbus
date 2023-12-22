namespace NModbus.IO
{
    internal class ModbusRtuOverUdpTransport : ModbusRtuTransport
    {
        internal ModbusRtuOverUdpTransport(IStreamResource streamResource, IModbusFactory modbusFactory, IModbusLogger logger) : base(streamResource, modbusFactory, logger)
        {
        }
    }
}