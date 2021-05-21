using Testflow.Data;

namespace Testflow.SlaveCore.Runner.Convertors
{
    internal class BoolConvertor : ValueConvertorBase
    {
        protected override void InitializeConvertFuncs()
        {
            ConvertFuncs.Add(typeof(decimal).Name, (object sourceValue, out object castValue) =>
            {
                castValue = (decimal)((bool) sourceValue ? 1 : 0);
                return true;
            });
//            ConvertFuncs.Add(typeof(double).Name, sourceValue => sourceValue.ToString());
//            ConvertFuncs.Add(typeof(float).Name, sourceValue => sourceValue.ToString());
            ConvertFuncs.Add(typeof(long).Name, (object sourceValue, out object castValue) =>
            {
                castValue = (long)((bool)sourceValue ? 1 : 0);
                return true;
            });
            ConvertFuncs.Add(typeof(ulong).Name, (object sourceValue, out object castValue) =>
            {
                castValue = (ulong)((bool)sourceValue ? 1 : 0);
                return true;
            });
            ConvertFuncs.Add(typeof(int).Name, (object sourceValue, out object castValue) =>
            {
                castValue = (int)((bool)sourceValue ? 1 : 0);
                return true;
            });
            ConvertFuncs.Add(typeof(uint).Name, (object sourceValue, out object castValue) =>
            {
                castValue = (uint)((bool)sourceValue ? 1 : 0);
                return true;
            });
            ConvertFuncs.Add(typeof(short).Name, (object sourceValue, out object castValue) =>
            {
                castValue = (short)((bool)sourceValue ? 1 : 0);
                return true;
            });
            ConvertFuncs.Add(typeof(ushort).Name, (object sourceValue, out object castValue) =>
            {
                castValue = (ushort)((bool)sourceValue ? 1 : 0);
                return true;
            });
            ConvertFuncs.Add(typeof(char).Name, (object sourceValue, out object castValue) =>
            {
                castValue = (char)((bool)sourceValue ? 1 : 0);
                return true;
            });
            ConvertFuncs.Add(typeof(byte).Name, (object sourceValue, out object castValue) =>
            {
                castValue = (byte)((bool)sourceValue ? 1 : 0);
                return true;
            });
//            ConvertFuncs.Add(typeof(bool).Name, sourceValue => sourceValue.ToString());
            ConvertFuncs.Add(typeof(string).Name, (object sourceValue, out object castValue) =>
            {
                castValue = sourceValue.ToString();
                return true;
            });
        }

        public override object GetDefaultValue()
        {
            return false;
        }
    }
}