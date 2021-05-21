using Testflow.Data;

namespace Testflow.SlaveCore.Runner.Convertors
{
    internal class DoubleConvertor : ValueConvertorBase
    {
        private readonly string _format;
        public DoubleConvertor(string format)
        {
            this._format = format;
        }

        protected override void InitializeConvertFuncs()
        {
            ConvertFuncs.Add(typeof(decimal).Name, (object sourceValue, out object castValue) =>
                {
                    if ((double)sourceValue > (double)decimal.MaxValue || (double)sourceValue < (double)decimal.MinValue)
                    {
                        castValue = decimal.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToDecimal((double) sourceValue);
                    return true;
                });
//            ConvertFuncs.Add(typeof(double).Name, sourceValue => System.Convert.ToDouble((double)sourceValue));
            ConvertFuncs.Add(typeof(float).Name, (object sourceValue, out object castValue) =>
                {
                    if ((double)sourceValue > float.MaxValue || (double)sourceValue < float.MinValue)
                    {
                        castValue = float.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToSingle((double) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(long).Name, (object sourceValue, out object castValue) =>
                {
                    if ((double)sourceValue > long.MaxValue || (double)sourceValue < long.MinValue)
                    {
                        castValue = long.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToInt64((double) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(ulong).Name, (object sourceValue, out object castValue) =>
                {
                    if ((double)sourceValue > ulong.MaxValue || (double)sourceValue < ulong.MinValue)
                    {
                        castValue = ulong.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToUInt64((double) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(int).Name, (object sourceValue, out object castValue) =>
                {
                    if ((double)sourceValue > int.MaxValue || (double)sourceValue < int.MinValue)
                    {
                        castValue = int.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToInt32((double) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(uint).Name, (object sourceValue, out object castValue) =>
                {
                    if ((double)sourceValue > uint.MaxValue || (double)sourceValue < uint.MinValue)
                    {
                        castValue = uint.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToUInt32((double) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(short).Name, (object sourceValue, out object castValue) =>
                {
                    if ((double)sourceValue > short.MaxValue || (double)sourceValue < short.MinValue)
                    {
                        castValue = short.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToInt16((double) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(ushort).Name, (object sourceValue, out object castValue) =>
                {
                    if ((double)sourceValue > ushort.MaxValue || (double)sourceValue < ushort.MinValue)
                    {
                        castValue = ushort.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToUInt16((double) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(char).Name, (object sourceValue, out object castValue) =>
                {
                    if ((double)sourceValue > char.MaxValue || (double)sourceValue < char.MinValue)
                    {
                        castValue = char.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToChar((double) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(byte).Name, (object sourceValue, out object castValue) =>
                {
                    if ((double)sourceValue > byte.MaxValue || (double)sourceValue < byte.MinValue)
                    {
                        castValue = byte.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToByte((double) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(bool).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = (double) sourceValue > 0;
                    return true;
                });
            ConvertFuncs.Add(typeof(string).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = ((double) sourceValue).ToString(this._format);
                    return true;
                });
        }

        public override object GetDefaultValue()
        {
            return (double) 0;
        }
    }
}