using System;
using Testflow.CoreCommon;
using Testflow.Data;
using Testflow.Usr;

namespace Testflow.SlaveCore.Runner.Convertors
{
    internal class StringConvertor : ValueConvertorBase
    {
        protected override void InitializeConvertFuncs()
        {
            ConvertFuncs.Add(typeof(decimal).Name, (object sourceValue, out object castValue) =>
            {
                decimal value;
                bool parseSuccess = false;
                parseSuccess = decimal.TryParse((string)sourceValue, out value);
                castValue = value;
                return parseSuccess;
            });
            ConvertFuncs.Add(typeof(double).Name, (object sourceValue, out object castValue) =>
            {
                double value;
                bool parseSuccss = false;
                parseSuccss = double.TryParse((string)sourceValue, out value);
                castValue = value;
                return parseSuccss;
            });
            ConvertFuncs.Add(typeof(float).Name, (object sourceValue, out object castValue) => {
                float value;
                bool parseSuccss = false;
                parseSuccss = float.TryParse((string)sourceValue, out value);
                castValue = value;
                return parseSuccss;
            });
            ConvertFuncs.Add(typeof(long).Name, (object sourceValue, out object castValue) => {
                long value;
                bool parseSuccss = false;
                parseSuccss = long.TryParse((string)sourceValue, out value);
                castValue = value;
                return parseSuccss;
            });
            ConvertFuncs.Add(typeof(ulong).Name, (object sourceValue, out object castValue) => {
                ulong value;
                bool parseSuccss = false;
                parseSuccss = ulong.TryParse((string)sourceValue, out value);
                castValue = value;
                return parseSuccss;
            });
            ConvertFuncs.Add(typeof(int).Name, (object sourceValue, out object castValue) => {
                int value;
                bool parseSuccss = false;
                parseSuccss = int.TryParse((string)sourceValue, out value);
                castValue = value;
                return parseSuccss;
            });
            ConvertFuncs.Add(typeof(uint).Name, (object sourceValue, out object castValue) => {
                uint value;
                bool parseSuccss = false;
                parseSuccss = uint.TryParse((string)sourceValue, out value);
                castValue = value;
                return parseSuccss;
            });
            ConvertFuncs.Add(typeof(short).Name, (object sourceValue, out object castValue) => {
                short value;
                bool parseSuccss = false;
                parseSuccss = short.TryParse((string)sourceValue, out value);
                castValue = value;
                return parseSuccss;
            });
            ConvertFuncs.Add(typeof(ushort).Name, (object sourceValue, out object castValue) => {
                ushort value;
                bool parseSuccss = false;
                parseSuccss = ushort.TryParse((string)sourceValue, out value);
                castValue = value;
                return parseSuccss;
            });
            ConvertFuncs.Add(typeof(char).Name, (object sourceValue, out object castValue) => {
                char value;
                bool parseSuccss = false;
                parseSuccss = char.TryParse((string)sourceValue, out value);
                castValue = value;
                return parseSuccss;
            });
            ConvertFuncs.Add(typeof (byte).Name, (object sourceValue, out object castValue) => {
                byte value;
                bool parseSuccss = false;
                parseSuccss = byte.TryParse((string)sourceValue, out value);
                castValue = value;
                return parseSuccss;
            });
            ConvertFuncs.Add(typeof(bool).Name, (object sourceValue, out object castValue) => {
                string sourceValueStr = ((string)sourceValue).Trim();
                if ("true".Equals(sourceValueStr, StringComparison.OrdinalIgnoreCase))
                {
                    castValue = true;
                    return true;
                }
                if ("false".Equals(sourceValueStr, StringComparison.OrdinalIgnoreCase))
                {
                    castValue = false;
                    return true;
                }
                castValue = sourceValue;
                return false;
            });
            ConvertFuncs.Add(typeof(DateTime).Name, (object sourceValue, out object castValue) =>
            {
                DateTime value;
                bool parseSuccss = false;
                parseSuccss = DateTime.TryParse((string)sourceValue, out value);
                castValue = value;
                return parseSuccss;
            });
//            ConvertFuncs.Add(typeof(string).Name, sourceValue => sourceValue.ToString());
        }

        public override object GetDefaultValue()
        {
            return "";
        }
    }
}