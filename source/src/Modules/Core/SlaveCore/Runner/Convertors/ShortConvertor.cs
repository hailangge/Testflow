using Testflow.Data;

namespace Testflow.SlaveCore.Runner.Convertors
{
    internal class ShortConvertor : ValueConvertorBase
    {
        protected override void InitializeConvertFuncs()
        {
            ConvertFuncs.Add(typeof(decimal).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToDecimal((short) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(double).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToDouble((short) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(float).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToSingle((short) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(long).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToInt64((short) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(ulong).Name, (object sourceValue, out object castValue) =>
                {
                    if ((short)sourceValue < (short)ulong.MinValue)
                    {
                        castValue = ulong.MinValue;
                        return false;
                    }
                    castValue = System.Convert.ToUInt64((short) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(int).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToInt32((short) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(uint).Name, (object sourceValue, out object castValue) =>
                {
                    if ((short)sourceValue < (short)uint.MinValue)
                    {
                        castValue = uint.MinValue;
                        return false;
                    }
                    castValue = System.Convert.ToUInt32((short) sourceValue);
                    return true;
                });
//            ConvertFuncs.Add(typeof(short).Name, sourceValue => System.Convert.ToInt16((short)sourceValue));
            ConvertFuncs.Add(typeof(ushort).Name, (object sourceValue, out object castValue) =>
                {
                    if ((short)sourceValue < (short)ushort.MinValue)
                    {
                        castValue = ushort.MinValue;
                        return false;
                    }
                    castValue = System.Convert.ToUInt16((short) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(char).Name, (object sourceValue, out object castValue) =>
                {
                    if ((short)sourceValue < char.MinValue)
                    {
                        castValue = char.MinValue;
                        return false;
                    }
                    castValue = System.Convert.ToChar((short) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(byte).Name, (object sourceValue, out object castValue) =>
                {
                    if ((short)sourceValue > byte.MaxValue || (short)sourceValue < byte.MinValue)
                    {
                        castValue = byte.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToByte((short) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(bool).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = (short) sourceValue > 0;
                    return true;
                });
            ConvertFuncs.Add(typeof(string).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = sourceValue.ToString();
                    return true;
                });
        }

        public override object GetDefaultValue()
        {
            return (short) 0;
        }
    }
}