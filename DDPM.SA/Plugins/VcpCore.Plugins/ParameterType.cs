using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VcpCore.Common;

namespace VcpCore.Plugins
{
    public class ParameterType
    {
        public Queue_CommandType CommandType { get; }

        public object Parameter { get; }

        public ParameterType(Queue_CommandType commandType, object parameter)
        {
            CommandType = commandType;
            Parameter = parameter;
        }
    }

    public class Type_Initialize0x52toEmpty
    {
        public Guid guid { get; }

        public Type_Initialize0x52toEmpty(Guid _guid)
        {
            guid = _guid;
        }
    }

    public class Type_Watcher0x52
    {
        public Guid guid { get; }

        public Type_Watcher0x52(Guid _guid)
        {
            guid = _guid;
        }
    }

    public class Type_GetCapabilitiesString
    {
        public Guid guid { get; }

        public MonitorInfo_complex monitorInfoX { get; }

        public Type_GetCapabilitiesString(Guid _guid, MonitorInfo_complex _monitorInfoX)
        {
            guid = _guid;
            monitorInfoX = _monitorInfoX;
        }
    }

    public class Type_GetVCPCapabilities
    {
        public Guid guid { get; }

        public MonitorInfo_complex monitorInfoX { get; }

        public Type_GetVCPCapabilities(Guid _guid, MonitorInfo_complex _monitorInfoX)
        {
            guid = _guid;
            monitorInfoX = _monitorInfoX;
        }
    }

    public class Type_GetVCPCapability_I
    {
        public Guid guid { get; }

        public MonitorInfo_complex monitorInfoX { get; }

        public byte code { get; }

        public int opt { get; }

        public Type_GetVCPCapability_I(Guid _guid, MonitorInfo_complex _monitorInfoX, byte _code, int _opt = 0)
        {
            guid = _guid;
            monitorInfoX = _monitorInfoX;
            code = _code;
            opt = _opt;
        }
    }

    public class Type_GetVCPCapability_II
    {
        public Guid guid { get; }

        public MonitorInfo_complex monitorInfoX { get; }

        public string FunctionName { get; }

        public int opt { get; }

        public Type_GetVCPCapability_II(Guid _guid, MonitorInfo_complex _monitorInfoX, string _FunctionName, int _opt = 0)
        {
            guid = _guid;
            monitorInfoX = _monitorInfoX;
            FunctionName = _FunctionName;
            opt = _opt;
        }
    }

    public class Type_SetVCPCapability_I
    {
        public Guid guid { get; }

        public MonitorInfo_complex monitorInfoX { get; }

        public byte code { get; }

        public uint val { get; }

        public Type_SetVCPCapability_I(Guid _guid, MonitorInfo_complex _monitorInfoX, byte _code, uint _val)
        {
            guid = _guid;
            monitorInfoX = _monitorInfoX;
            code = _code;
            val = _val;
        }
    }

    public class Type_SetVCPCapability_II
    {
        public Guid guid { get; }

        public MonitorInfo_complex monitorInfoX { get; }

        public string FunctionName { get; }

        public string val { get; }

        public Type_SetVCPCapability_II(Guid _guid, MonitorInfo_complex _monitorInfoX, string _FunctionName, string _val)
        {
            guid = _guid;
            monitorInfoX = _monitorInfoX;
            FunctionName = _FunctionName;
            val = _val;
        }
    }

    public enum Queue_CommandType
    {
        GetCapabilitiesString,
        GetVCPCapabilities,
        GetVCPCapability_I,
        GetVCPCapability_II,
        SetVCPCapability_I,
        SetVCPCapability_II,
        Initialize0x52toEmpty,
        Watcher0x52
    }
}