using Testflow.Data;

namespace Testflow.SlaveCore.Runner.Convertors
{
    internal class FloatConvertor : ValueConvertorBase
    {
        private readonly string _format;
        public FloatConvertor(string format)
        {
            this._format = format;
        }

        protected override void InitializeConvertFuncs()
        {
            ConvertFuncs.Add(typeof(decimal).Name, (object sourceValue, out object castValue) =>
                {
                    if ((float)sourceValue > (float)decimal.MaxValue || (float)sourceValue < (float)decimal.MinValue)
                    {
                        castValue = decimal.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToDecimal((float) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(double).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToDouble((float) sourceValue);
                    return true;
                });
//            ConvertFuncs.Add(typeof(float).Name, sourceValue => System.Convert.ToSingle((float)sourceValue));
            ConvertFuncs.Add(typeof(long).Name, (object sourceValue, out object castValue) =>
                {
                    if ((float)sourceValue > long.MaxValue || (float)sourceValue < long.MinValue)
                    {
                        castValue = long.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToInt64((float) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(ulong).Name, (object sourceValue, out object castValue) =>
                {
                    if ((float)sourceValue > ulong.MaxValue || (float)sourceValue < ulong.MinValue)
                    {
                        castValue = ulong.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToUInt64((float) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(int).Name, (object sourceValue, out object castValue) =>
                {
                    if ((float)sourceValue > int.MaxValue || (float)sourceValue < int.MinValue)
                    {
                        castValue = int.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToInt32((float) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(uint).Name, (object sourceValue, out object castValue) =>
                {
                    if ((float)sourceValue > uint.MaxValue || (float)sourceValue < uint.MinValue)
                    {
                        castValue = uint.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToUInt32((float) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(short).Name, (object sourceValue, out object castValue) =>
                {
                    if ((float)sourceValue > short.MaxValue || (float)sourceValue < short.MinValue)
                    {
                        castValue = short.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToInt16((float) sourceValue);
                    return true;
                });

            ConvertFuncs.Add(typeof(ushort).Name, (object sourceValue, out object castValue) =>
                {
                    if ((float)sourceValue > ushort.MaxValue || (float)sourceValue < ushort.MinValue)
                    {
                        castValue = ushort.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToUInt16((float) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(char).Name, (object sourceValue, out object castValue) =>
                {
                    if ((float)sourceValue > char.MaxValue || (float)sourceValue < char.MinValue)
                    {
                        castValue = char.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToChar((float) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(byte).Name, (object sourceValue, out object castValue) =>
                {
                    if ((float)sourceValue > byte.MaxValue || (float)sourceValue < byte.MinValue)
                    {
                        castValue = byte.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToByte((float) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(bool).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = (float) sourceValue > 0;
                    return true;

                });
            ConvertFuncs.Add(typeof(string).Name,
                (object sourceValue, out object castValue) =>
                {
                    castValue = ((float) sourceValue).ToString(this._format);
                    return true;
                });
        }

        public override object GetDefaultValue()
        {
            return (float) 0;
        }
    }
}