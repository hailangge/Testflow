using Testflow.Data;

namespace Testflow.SlaveCore.Runner.Convertors
{
    internal class CharConvertor : ValueConvertorBase
    {
        protected override void InitializeConvertFuncs()
        {
            ConvertFuncs.Add(typeof(decimal).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToDecimal((char) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(double).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToDouble((char) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(float).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToSingle((char) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(long).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToInt64((char) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(ulong).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToUInt64((char) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(int).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToInt32((char) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(uint).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToUInt32((char) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(short).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToInt16((char) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(ushort).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = System.Convert.ToUInt16((char) sourceValue);
                    return true;
                });
//            ConvertFuncs.Add(typeof(char).Name, sourceValue => System.Convert.ToChar((char)sourceValue));
            ConvertFuncs.Add(typeof (byte).Name, (object sourceValue, out object castValue) =>
                {
                    if ((char)sourceValue > byte.MaxValue)
                    {
                        castValue = byte.MaxValue;
                        return false;
                    }
                    castValue = System.Convert.ToByte((char) sourceValue);
                    return true;
                });
            ConvertFuncs.Add(typeof(bool).Name, (object sourceValue, out object castValue) =>
                {
                    castValue = (char) sourceValue > 0;
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
            return (char) 0;
        }
    }
}