using Testflow.Data;

namespace Testflow.SlaveCore.Runner.Convertors
{
    internal class ByteConvertor : ValueConvertorBase
    {
        protected override void InitializeConvertFuncs()
        {
            ConvertFuncs.Add(typeof (decimal).Name, (object sourceValue, out object castValue) =>
            {
                castValue = System.Convert.ToDecimal((byte) sourceValue);
                return true;
            });
            ConvertFuncs.Add(typeof (double).Name, (object sourceValue, out object castValue) =>
            {
                castValue = System.Convert.ToDouble((byte) sourceValue);
                return true;
            });
            ConvertFuncs.Add(typeof (float).Name, (object sourceValue, out object castValue) =>
            {
                castValue = System.Convert.ToSingle((byte) sourceValue);
                return true;
            });
            ConvertFuncs.Add(typeof (long).Name, (object sourceValue, out object castValue) =>
            {
                castValue = System.Convert.ToInt64((byte) sourceValue);
                return true;
            });
            ConvertFuncs.Add(typeof (ulong).Name, (object sourceValue, out object castValue) =>
            {
                castValue = System.Convert.ToUInt64((byte) sourceValue);
                return true;
            });
            ConvertFuncs.Add(typeof (int).Name, (object sourceValue, out object castValue) =>
            {
                castValue = System.Convert.ToInt32((byte) sourceValue);
                return true;
            });
            ConvertFuncs.Add(typeof (uint).Name, (object sourceValue, out object castValue) =>
            {
                castValue = System.Convert.ToUInt32((byte) sourceValue);
                return true;
            });
            ConvertFuncs.Add(typeof (short).Name, (object sourceValue, out object castValue) =>
            {
                castValue = System.Convert.ToInt16((byte) sourceValue);
                return true;
            });
            ConvertFuncs.Add(typeof (ushort).Name, (object sourceValue, out object castValue) =>
            {
                castValue = System.Convert.ToUInt16((byte) sourceValue);
                return true;
            });
            ConvertFuncs.Add(typeof (char).Name, (object sourceValue, out object castValue) =>
            {
                castValue = System.Convert.ToChar((byte) sourceValue);
                return true;
            });
//            ConvertFuncs.Add(typeof (byte).Name, sourceValue => System.Convert.ToByte((byte)sourceValue));
            ConvertFuncs.Add(typeof (bool).Name, (object sourceValue, out object castValue) =>
            {
                castValue = (byte) sourceValue > 0;
                return true;
            });
            ConvertFuncs.Add(typeof (string).Name, (object sourceValue, out object castValue) =>
            {
                castValue = sourceValue.ToString();
                return true;
            });
        }

        public override object GetDefaultValue()
        {
            return (byte) 0;
        }
    }
}