using Testflow.Data;

namespace Testflow.SlaveCore.Runner.Convertors
{
    internal class ULongConvertor : ValueConvertorBase
    {
        protected override void InitializeConvertFuncs()
        {
            ConvertFuncs.Add(typeof(decimal).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToDecimal((ulong) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(double).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToDouble((ulong) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(float).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToSingle((ulong) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(long).Name, (object sourceValue, out object castValue) =>
                {
                    if ((ulong)sourceValue > long.MaxValue)
                    {
                        castValue = long.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToInt64((ulong) sourceValue);
                    return true;
                });
//            ConvertFuncs.Add(typeof(ulong).Name, sourceValue => System.Convert.ToUInt64((ulong)sourceValue));
            ConvertFuncs.Add(typeof(int).Name, (object sourceValue, out object castValue) =>
                {
                    if ((ulong)sourceValue > int.MaxValue)
                    {
                        castValue = int.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToInt32((ulong) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(uint).Name, (object sourceValue, out object castValue) =>
                {
                    if ((ulong)sourceValue > uint.MaxValue)
                    {
                        castValue = uint.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToUInt32((ulong) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(short).Name, (object sourceValue, out object castValue) =>
                {
                    if ((ulong)sourceValue > (ulong)short.MaxValue)
                    {
                        castValue = short.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToInt16((ulong) sourceValue);
                    return true;
                });

            ConvertFuncs.Add(typeof(ushort).Name, (object sourceValue, out object castValue) =>
                {
                    if ((ulong)sourceValue > ushort.MaxValue)
                    {
                        castValue = ushort.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToUInt16((ulong) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(char).Name, (object sourceValue, out object castValue) =>
                {
                    if ((ulong)sourceValue > char.MaxValue)
                    {
                        castValue = char.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToChar((ulong) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(byte).Name, (object sourceValue, out object castValue) =>
                {
                    if ((ulong)sourceValue > byte.MaxValue)
                    {
                        castValue = byte.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToByte((ulong) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(bool).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = (ulong) sourceValue > 0;
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
            return (ulong) 0;
        }
    }
}