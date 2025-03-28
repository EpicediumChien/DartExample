using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VcpCore.Common;

namespace VcpCore.Plugins
{
    public class VcpLockCache
    {
        private readonly object _CacheTablelock = new object();
        private Dictionary<EDID, Dictionary<object, object>> _CacheTable;
        private Logs _logs;

        public VcpLockCache(Logs logs)
        {
            _logs = logs;
            _CacheTable ??= new Dictionary<EDID, Dictionary<object, object>>();
        }

        public object GetFromCacheTable(MonitorInfo_complex MonitorInfo, object key)
        {
            lock (_CacheTablelock)
            {
                try
                {
                    _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache GetFromCacheTable Let's go ...");
                    _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache GetFromCacheTable TargetMonitor AliasDeviceName is " + MonitorInfo.AliasDeviceName);
                    _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache GetFromCacheTable Key is " + ((key is string) ? key.ToString() : Convert.ToByte(key).ToString("X")));

                    if (_CacheTable.Count > 0)
                    {
                        var EDIDs = _CacheTable.Keys.ToList();
                        foreach (var EDID in EDIDs)
                        {
                            if (EDID.Equals(MonitorInfo.edid) &&
                                _CacheTable[EDID].ContainsKey(key))
                            {
                                bool rc = false;
                                var result = new object();
                                rc = _CacheTable[EDID].TryGetValue(key, out result);

                                _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache Is GetFromCacheTable success?? : result => " + rc.ToString());
                                return (rc ? result : null);
                            }
                        }
                    }

                    _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache GetFromCacheTable finish : result => null");

                    return null;
                }
                catch (Exception ex)
                {
                    _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache GetFromCacheTable into catch: " + ex.Message);
                    return null;
                }
            }
        }

        public bool SetToCacheTable(MonitorInfo_complex MonitorInfo, object key, object value)
        {
            lock (_CacheTablelock)
            {
                var result = false;
                try
                {
                    _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache SetToCacheTable Let's go ...");
                    _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache SetToCacheTable TargetMonitor AliasDeviceName is " + MonitorInfo.AliasDeviceName);
                    _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache SetToCacheTable Key is " + ((key is string) ? key.ToString() : Convert.ToByte(key).ToString("X")));

                    var ignoreCodes = new List<byte> { 0x02, 0x04, 0x05, 0x10, 0x12, 0x52, 0x60, 0xE9, 0xEC, 0x62, 0x8D };

                    if ((key != null) && (!(key is string)) && (ignoreCodes.Contains(Convert.ToByte(key))))
                    {
                        _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache ctr-code is in IgnoreCodes...Do not SetToCacheTable");
                        result = false;
                    }
                    else
                    {
                        if (_CacheTable.Count > 0)
                        {
                            result = AddValue(_CacheTable.Keys.ToList());
                            if (!result)
                            {
                                if (!string.IsNullOrWhiteSpace(MonitorInfo.CapabilityString))
                                {
                                    _CacheTable.Add(MonitorInfo.edid, new Dictionary<object, object>() { { "CapabilityString".ToLower(CultureInfo.InvariantCulture), MonitorInfo.CapabilityString }, { key, value } });
                                    result = AddValue(_CacheTable.Keys.ToList());
                                    _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache SetToCacheTable " + (result ? "Pass" : "Fail"));
                                }
                                else
                                {
                                    _CacheTable.Add(MonitorInfo.edid, new Dictionary<object, object>() { { key, value } });
                                    result = AddValue(_CacheTable.Keys.ToList());
                                    _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache SetToCacheTable " + (result ? "Pass" : "Fail"));
                                }
                            }
                            else
                            {
                                _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache SetToCacheTable Pass");
                                result = true;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(MonitorInfo.CapabilityString))
                            {
                                _CacheTable.Add(MonitorInfo.edid, new Dictionary<object, object>() { { "CapabilityString".ToLower(CultureInfo.InvariantCulture), MonitorInfo.CapabilityString }, { key, value } });
                                result = AddValue(_CacheTable.Keys.ToList());
                                _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache SetToCacheTable " + (result ? "Pass" : "Fail"));
                            }
                            else
                            {
                                _CacheTable.Add(MonitorInfo.edid, new Dictionary<object, object>() { { key, value } });
                                result = AddValue(_CacheTable.Keys.ToList());
                                _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache SetToCacheTable " + (result ? "Pass" : "Fail"));
                            }
                        }
                    }
                    _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache SetToCacheTable finish");
                    return result;
                }
                catch (Exception ex)
                {
                    _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache SetToCacheTable into catch: " + ex.Message);
                    return false;
                }

                bool AddValue(List<EDID> EDIDS)
                {
                    foreach (var EDID in EDIDS)
                    {
                        if (EDID.Equals(MonitorInfo.edid))
                        {
                            if (_CacheTable[EDID].ContainsKey(key))
                                (_CacheTable[EDID])[key] = value;
                            else
                                (_CacheTable[EDID]).Add(key, value);

                            return true;
                        }
                    }
                    return false;
                }
            }
        }

        public Dictionary<EDID, Dictionary<object, object>> GetVCPCacheTable()
        {
            _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache VcpCorePlugin GetVCPCacheTable  ...");

            return _CacheTable;
        }

        public void InitializeCacheTable(List<(MonitorInfo_complex, MonitorInfo)> _AllInfoMonitors_Mix, CancellationToken token)
        {
            lock (_CacheTablelock)
            {
                try
                {
                    _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache InitializeCacheTable Let's go ...");

                    if (_AllInfoMonitors_Mix.Count > 0)
                    {
                        for (int i = 0; (i < _AllInfoMonitors_Mix.Count && (!token.IsCancellationRequested)); i++)
                        {
                            var MonitorInfo = _AllInfoMonitors_Mix[i].Item1;

                            bool IsExist = false;
                            var EDIDS = _CacheTable.Keys.ToList();
                            foreach (var EDIE in EDIDS)
                            {
                                if (token.IsCancellationRequested)
                                    return;

                                if (EDIE.Equals(MonitorInfo.edid))
                                {
                                    IsExist = true;
                                    break;
                                }
                            }

                            if (IsExist)
                            {
                                if (!string.IsNullOrWhiteSpace(MonitorInfo.CapabilityString))
                                    SetToCacheTable(MonitorInfo, "CapabilityString".ToLower(CultureInfo.InvariantCulture), MonitorInfo.CapabilityString);
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(MonitorInfo.CapabilityString))
                                    _CacheTable.Add(MonitorInfo.edid, new Dictionary<object, object>() { { "CapabilityString".ToLower(CultureInfo.InvariantCulture), MonitorInfo.CapabilityString } });
                            }
                        }
                    }

                    foreach (var item in _CacheTable)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        var Keys = (item.Value).Keys.ToList();
                        foreach (var Key in Keys)
                        {
                            if (token.IsCancellationRequested)
                                return;

                            if ((Key is string) && (Key.ToString().Equals("CapabilityString".ToLower(CultureInfo.InvariantCulture))))
                                continue;
                            else if ((Key is string) && (Key.ToString().Equals("InputSourceList".ToLower(CultureInfo.InvariantCulture))))
                                continue;
                            else
                                item.Value.Remove(Key);
                        }
                    }

                    _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache InitializeCacheTable finish : CacheTable count => " + _CacheTable.Count);
                }
                catch (TaskCanceledException)
                {
                    _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache InitializeCacheTable cancellation happened...");
                    return;
                }
                catch (OperationCanceledException)
                {
                    _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache InitializeCacheTable cancellation happened...");
                    return;
                }
                catch (Exception ex)
                {
                    _logs.DebugMsg("[VcpCorePlugin] Class_VcpLockCache InitializeCacheTable into catch: " + ex.Message);
                    return;
                }
            }
        }
    }
}