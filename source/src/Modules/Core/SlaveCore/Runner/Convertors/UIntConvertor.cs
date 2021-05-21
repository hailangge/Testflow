using Testflow.Data;

namespace Testflow.SlaveCore.Runner.Convertors
{
    internal class UIntConvertor : ValueConvertorBase
    {
        protected override void InitializeConvertFuncs()
        {
            ConvertFuncs.Add(typeof(decimal).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToDecimal((uint) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(double).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToDouble((uint) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(float).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToSingle((uint) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(long).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToInt64((uint) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(ulong).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToUInt64((uint) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(int).Name, (object sourceValue, out object castValue) =>
                {
                    if ((uint)sourceValue > int.MaxValue)
                    {
                        castValue = int.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToInt32((uint) sourceValue);
                    return true;
                });
//            ConvertFuncs.Add(typeof(uint).Name, sourceValue => System.Convert.ToUInt32((uint)sourceValue));
            ConvertFuncs.Add(typeof(short).Name, (object sourceValue, out object castValue) =>
                {
                    if ((uint)sourceValue > short.MaxValue)
                    {
                        castValue = short.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToInt16((uint) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(ushort).Name, (object sourceValue, out object castValue) =>
                {
                    if ((uint)sourceValue > ushort.MaxValue)
                    {
                        castValue = ushort.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToUInt16((uint) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(char).Name, (object sourceValue, out object castValue) =>
                {
                    if ((uint)sourceValue > char.MaxValue)
                    {
                        castValue = char.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToChar((uint) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(byte).Name, (object sourceValue, out object castValue) =>
                {
                    if ((uint)sourceValue > byte.MaxValue)
                    {
                        castValue = byte.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToByte((uint) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(bool).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = (uint) sourceValue > 0;
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
            return (uint) 0;
        }
    }
}