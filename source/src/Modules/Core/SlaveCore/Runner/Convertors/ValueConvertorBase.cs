using System;
using System.Collections.Generic;
using Testflow.Data;
using Testflow.Usr;

namespace Testflow.SlaveCore.Runner.Convertors
{
    internal abstract class ValueConvertorBase
    {
        protected delegate bool ConvertFunction(object sourceValue, out object targetValue);

        protected Dictionary<string, ConvertFunction> ConvertFuncs { get; }

        protected ValueConvertorBase()
        {
            ConvertFuncs = new Dictionary<string, ConvertFunction>(20);
            InitializeConvertFuncs();
        }

        protected abstract void InitializeConvertFuncs();

        public abstract object GetDefaultValue();

        public bool TryCastValue(ITypeData targetType, object sourceValue, out object castValue)
        {
            return ConvertFuncs[targetType.Name](sourceValue, out castValue);
        }

        public bool TryCastValue(Type targetType, object sourceValue, out object castValue)
        {
            return ConvertFuncs[targetType.Name](sourceValue, out castValue);
        }

        public bool IsValidCastTarget(ITypeData targetType)
        {
            return ConvertFuncs.ContainsKey(targetType.Name);
        }

        public bool IsValidCastTarget(Type targetType)
        {
            return ConvertFuncs.ContainsKey(targetType.Name);
        }
    }
}