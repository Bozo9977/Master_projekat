﻿namespace Common.SCADA
{
	public enum EModbusFunctionCode : byte
	{
		READ_COILS = 0x01,
		READ_DISCRETE_INPUTS = 0x02,
		READ_HOLDING_REGISTERS = 0x03,
		READ_INPUT_REGISTERS = 0x04,
		WRITE_SINGLE_COIL = 0x05,
		WRITE_SINGLE_REGISTER = 0x06,
	}

	public enum EPointType : byte
	{
		DIGITAL_OUTPUT = 0x01,
		DIGITAL_INPUT = 0x02,
		ANALOG_INPUT = 0x03,
		ANALOG_OUTPUT = 0x04,
		HR_LONG = 0x05,
	}

	public enum EModbusExceptionCode : byte
	{
		NO_EXCEPTION = 0,
		ILLEGAL_FUNCTION = 1,
		ILLEGAL_DATA_ADDRESS = 2,
		ILLEGAL_DATA_VALUE = 3,
		SLAVE_DEVICE_FAILURE = 4,
		ACKNOWLEDGE = 5,
		SLAVE_DEVICE_BUSY = 6,
		NEGATIVE_ACKNOWLEDGE = 7,
		MEMORY_PARITY_ERROR = 8,
		GATEWAY_PATH_UNAVAILABLE = 10,
		GATEWAY_TARGET_DEVICE_FAILED_TO_RESPOND = 11
	}
}