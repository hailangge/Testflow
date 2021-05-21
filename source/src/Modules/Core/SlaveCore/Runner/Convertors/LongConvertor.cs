using Testflow.Data;

namespace Testflow.SlaveCore.Runner.Convertors
{
    internal class LongConvertor : ValueConvertorBase
    {
        protected override void InitializeConvertFuncs()
        {
            ConvertFuncs.Add(typeof(decimal).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToDecimal((long) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(double).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToDouble((long) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(float).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToSingle((long) sourceValue);
                    return true;
                });
//            ConvertFuncs.Add(typeof(long).Name, sourceValue => System.Convert.ToInt64((long)sourceValue));
            ConvertFuncs.Add(typeof(ulong).Name, (object sourceValue, out object castValue) =>
                {
                    if ((long)sourceValue < (long)ulong.MinValue)
                    {
                        castValue = ulong.MinValue;
                        return false;
                    }
                    castValue = System.Convert.ToUInt64((long) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(int).Name, (object sourceValue, out object castValue) =>
                {
                    if ((long)sourceValue > int.MaxValue || (long)sourceValue < int.MinValue)
                    {
                        castValue = int.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToInt32((long) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(uint).Name, (object sourceValue, out object castValue) =>
                {
                    if ((long)sourceValue > uint.MaxValue || (long)sourceValue < uint.MinValue)
                    {
                        castValue = uint.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToUInt32((long) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(short).Name, (object sourceValue, out object castValue) =>
                {
                    if ((long)sourceValue > short.MaxValue || (long)sourceValue < short.MinValue)
                    {
                        castValue = short.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToInt16((long) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(ushort).Name, (object sourceValue, out object castValue) =>
                {
                    if ((long)sourceValue > ushort.MaxValue || (long)sourceValue < ushort.MinValue)
                    {
                        castValue = ushort.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToUInt16((long) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(char).Name, (object sourceValue, out object castValue) =>
                {
                    if ((long)sourceValue > char.MaxValue || (long)sourceValue < char.MinValue)
                    {
                        castValue = char.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToChar((long) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(byte).Name, (object sourceValue, out object castValue) =>
                {
                    if ((long)sourceValue > byte.MaxValue || (long)sourceValue < byte.MinValue)
                    {
                        castValue = byte.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToByte((long) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(bool).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = (long) sourceValue > 0;
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
            return (long) 0;
        }
    }
}