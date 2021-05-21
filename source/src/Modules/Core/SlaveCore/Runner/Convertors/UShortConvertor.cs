using Testflow.Data;

namespace Testflow.SlaveCore.Runner.Convertors
{
    internal class UShortConvertor : ValueConvertorBase
    {
        protected override void InitializeConvertFuncs()
        {
            ConvertFuncs.Add(typeof(decimal).Name,
                (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToDecimal((ushort) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(double).Name,
                (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToDouble((ushort) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(float).Name,
                (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToSingle((ushort) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(long).Name,
                (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToInt64((ushort) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(ulong).Name,
                (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToUInt64((ushort) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(int).Name,
                (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToInt32((ushort) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(uint).Name,
                (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToUInt32((ushort) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(short).Name,
                (object sourceValue, out object castValue) =>
                {
                    if ((ushort)sourceValue > short.MaxValue)
                    {
                        castValue = short.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToInt16((ushort) sourceValue);
                    return true;
                });
//            ConvertFuncs.Add(typeof(ushort).Name, sourceValue => System.Convert.ToUInt16((ushort)sourceValue));
            ConvertFuncs.Add(typeof(char).Name,
                (object sourceValue, out object castValue) =>
                {
                    if ((ushort)sourceValue > char.MaxValue)
                    {
                        castValue = char.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToChar((ushort) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(byte).Name,
                (object sourceValue, out object castValue) =>
                {
                    if ((ushort)sourceValue > byte.MaxValue)
                    {
                        castValue = byte.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToByte((ushort) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(bool).Name,
                (object sourceValue, out object castValue) =>
                {
                    castValue = (ushort) sourceValue > 0;
                    return true;
                });
            ConvertFuncs.Add(typeof(string).Name,
                (object sourceValue, out object castValue) =>
                {
                    castValue = sourceValue.ToString();
                    return true;
                });
        }

        public override object GetDefaultValue()
        {
            return (ushort) 0;
        }
    }
}