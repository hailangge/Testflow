namespace Testflow.SlaveCore.Runner.Convertors
{
    internal class DecimalConvertor : ValueConvertorBase
    {
        private readonly string _format;
        public DecimalConvertor(string format)
        {
            this._format = format;
        }

        protected override void InitializeConvertFuncs()
        {
//            ConvertFuncs.Add(typeof(decimal).Name, sourceValue => System.Convert.ToDecimal((decimal)sourceValue));
            ConvertFuncs.Add(typeof(double).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToDouble((decimal) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(float).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToSingle((decimal) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(long).Name, (object sourceValue, out object castValue) =>
                {
                    if ((decimal)sourceValue > long.MaxValue || (decimal)sourceValue < long.MinValue)
                    {
                        castValue = long.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToInt64((decimal) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(ulong).Name, (object sourceValue, out object castValue) =>
                {
                    if ((decimal)sourceValue > ulong.MaxValue || (decimal)sourceValue < ulong.MinValue)
                    {
                        castValue = ulong.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToUInt64((decimal) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(int).Name, (object sourceValue, out object castValue) =>
                {
                    if ((decimal)sourceValue > int.MaxValue || (decimal)sourceValue < int.MinValue)
                    {
                        castValue = int.MaxValue;
                        return false;
                    }
                    castValue =  System.Convert.ToInt32((decimal) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(uint).Name, (object sourceValue, out object castValue) =>
                {
                    if ((decimal)sourceValue > uint.MaxValue || (decimal)sourceValue < uint.MinValue)
                    {
                        castValue = uint.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToUInt32((decimal) sourceValue);
                    return true;

                });
            ConvertFuncs.Add(typeof(short).Name, (object sourceValue, out object castValue) =>
                {
                    if ((decimal)sourceValue > short.MaxValue || (decimal)sourceValue < short.MinValue)
                    {
                        castValue = short.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToInt16((decimal) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(ushort).Name, (object sourceValue, out object castValue) =>
            {
                if ((decimal)sourceValue > ushort.MaxValue || (decimal)sourceValue < ushort.MinValue)
                {
                    castValue = ushort.MaxValue;
                    return false;
                }
                castValue = System.Convert.ToUInt16((decimal)sourceValue);
                return true;
            });
            ConvertFuncs.Add(typeof(char).Name, (object sourceValue, out object castValue) =>
                {
                    if ((decimal) sourceValue > char.MaxValue || (decimal) sourceValue < char.MinValue)
                    {
                        castValue = char.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToChar((decimal) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(byte).Name, (object sourceValue, out object castValue) =>
                {
                    if ((decimal)sourceValue > byte.MaxValue || (decimal)sourceValue < byte.MinValue)
                    {
                        castValue = byte.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToByte((decimal)sourceValue);
                    return true;
                });

            ConvertFuncs.Add(typeof(bool).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = (decimal) sourceValue > 0;
                    return true;
                });
            ConvertFuncs.Add(typeof(string).Name, (object sourceValue, out object castValue) =>
                {

                    castValue = ((decimal) sourceValue).ToString(this._format);
                    return true;
                });
        }

        public override object GetDefaultValue()
        {
            return decimal.Zero;
        }
    }
}