using Testflow.Data;

namespace Testflow.SlaveCore.Runner.Convertors
{
    internal class IntConvertor : ValueConvertorBase
    {
        protected override void InitializeConvertFuncs()
        {
            ConvertFuncs.Add(typeof(decimal).Name,
                (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToDecimal((int) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(double).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToDouble((int) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(float).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToSingle((int) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(long).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToInt64((int) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(ulong).Name, (object sourceValue, out object castValue) =>
                {
                    if ((int)sourceValue < (int)ulong.MinValue)
                    {
                        castValue = ulong.MinValue;
                        return false;
                    }
                    castValue = System.Convert.ToUInt64((int) sourceValue);
                    return true;
                });
//            ConvertFuncs.Add(typeof(int).Name, sourceValue => System.Convert.ToInt32((int)sourceValue));
            ConvertFuncs.Add(typeof(uint).Name, (object sourceValue, out object castValue) =>
                {
                    if ((int)sourceValue < uint.MinValue)
                    {
                        castValue = uint.MinValue;
                        return false;
                    }
                    castValue = System.Convert.ToUInt32((int) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(short).Name, (object sourceValue, out object castValue) =>
                {
                    if ((int)sourceValue > short.MaxValue || (int)sourceValue < short.MinValue)
                    {
                        castValue = short.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToInt16((int) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(ushort).Name, (object sourceValue, out object castValue) =>
                {
                    if ((int)sourceValue > ushort.MaxValue || (int)sourceValue < ushort.MinValue)
                    {
                        castValue = ushort.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToUInt16((int) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(char).Name, (object sourceValue, out object castValue) =>
                {
                    if ((int)sourceValue > char.MaxValue || (int)sourceValue < char.MinValue)
                    {
                        castValue = char.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToChar((int) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(byte).Name, (object sourceValue, out object castValue) =>
                {
                    if ((int)sourceValue > byte.MaxValue || (int)sourceValue < byte.MinValue)
                    {
                        castValue = byte.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToByte((int) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(bool).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = (int) sourceValue > 0;
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
            return 0;
        }
    }
}