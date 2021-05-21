using System;
using Testflow.Usr;

namespace Testflow.SlaveCore.Runner.Convertors
{
    internal class DateTimeConvertor : ValueConvertorBase
    {
        protected override void InitializeConvertFuncs()
        {
            ConvertFuncs.Add(typeof(string).Name,
                (object sourceValue, out object castValue) =>
                {
                    castValue = ((DateTime) sourceValue).ToString(CommonConst.GlobalTimeFormat);
                    return true;
                });
        }

        public override object GetDefaultValue()
        {
            return DateTime.MinValue;
        }
    }
}