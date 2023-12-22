namespace NModbus
{
    /// <summary>
    /// Supported function codes
    /// </summary>
    public static class ModbusFunctionCodes
    {
        public const byte ReadCoils = 1; //读取线圈状态
        public const byte ReadInputs = 2; //读取离散输入状态
        public const byte ReadHoldingRegisters = 3; //读保持寄存器
        public const byte ReadInputRegisters = 4; //读输入寄存器
        public const byte WriteSingleCoil = 5; //写单个线圈
        public const byte WriteSingleRegister = 6; //写单个保持寄存器
        public const byte Diagnostics = 8; //诊断功能
        public const ushort DiagnosticsReturnQueryData = 0; //诊断子功能码 0：Return Query Data
        public const byte WriteMultipleCoils = 15; //写多个线圈
        public const byte WriteMultipleRegisters = 16; //写多个保持寄存器
        public const byte ReportSlaveId = 17; //报告从站ID
        public const byte WriteFileRecord = 21; //
        public const byte ReadWriteMultipleRegisters = 23; //
    }
}