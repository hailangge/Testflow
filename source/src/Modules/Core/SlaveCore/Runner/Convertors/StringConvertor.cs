using System;
using System.Text.RegularExpressions;
using Testflow.CoreCommon;
using Testflow.Data;
using Testflow.Usr;

namespace Testflow.SlaveCore.Runner.Convertors
{
    internal class StringConvertor : ValueConvertorBase
    {
        /// <summary>
        /// 十六进制正则表达式
        /// </summary>
        private readonly Regex _hexRegex;

        /// <summary>
        /// 十六进制正则表达式
        /// </summary>
        private readonly Regex _octRegex;

        /// <summary>
        /// 十六进制正则表达式
        /// </summary>
        private readonly Regex _binRegex;

        private const int LongBitCount = 64;
        private const int IntBitCount = 32;
        private const int ShortBitCount = 16;
        private const int CharBitCount = 16;
        private const int ByteBitCount = 8;

        private const int HexCharBitCount = 4;
        private const int OctCharBitCount = 3;
        private const int BinCharBitCount = 1;

        public StringConvertor() : base()
        {
            this._hexRegex = new Regex("^0[xX][0-9a-fA-F]+$", RegexOptions.Compiled);
            this._octRegex = new Regex("^0[oO]([0-7]+)$", RegexOptions.Compiled);
            this._binRegex = new Regex("^0[bB][01]+$", RegexOptions.Compiled);
        }

        protected override void InitializeConvertFuncs()
        {
            ConvertFuncs.Add(typeof(decimal).Name, (object sourceValue, out object castValue) =>
            {
                decimal value;
                bool parseSuccess = false;
                parseSuccess = decimal.TryParse((string)sourceValue, out value);
                if (parseSuccess)
                {
                    castValue = value;
                    return true;
                }
                object castULongValue = 0;
                parseSuccess = ConvertFuncs[typeof(ulong).Name](sourceValue, out castULongValue);
                castValue = Convert.ToDecimal(castULongValue);
                return parseSuccess;
            });
            ConvertFuncs.Add(typeof(double).Name, (object sourceValue, out object castValue) =>
            {
                double value;
                bool parseSuccss = false;
                parseSuccss = double.TryParse((string)sourceValue, out value);
                if (parseSuccss)
                {
                    castValue = value;
                    return true;
                }
                object castULongValue = 0;
                parseSuccss = ConvertFuncs[typeof(ulong).Name](sourceValue, out castULongValue);
                castValue = Convert.ToDouble(castULongValue);
                return parseSuccss;
            });
            ConvertFuncs.Add(typeof(float).Name, (object sourceValue, out object castValue) => {
                float value;
                bool parseSuccss = false;
                parseSuccss = float.TryParse((string)sourceValue, out value);
                if (parseSuccss)
                {
                    castValue = value;
                    return true;
                }
                object castULongValue = 0;
                parseSuccss = ConvertFuncs[typeof(ulong).Name](sourceValue, out castULongValue);
                castValue = Convert.ToSingle(castULongValue);
                return parseSuccss;
            });
            ConvertFuncs.Add(typeof(long).Name, (object sourceValue, out object castValue) =>
            {
                long value;
                bool parseSuccss = false;
                string sourceValueStr = (string) sourceValue;
                parseSuccss = long.TryParse(sourceValueStr, out value);
                if (parseSuccss)
                {
                    castValue = value;
                    return true;
                }
                castValue = (long)0;
                int fromBase = 0;
                if (this._hexRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, LongBitCount, HexCharBitCount)) return false;
                    fromBase = 16;
                }
                else if (this._octRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, LongBitCount, OctCharBitCount, sourceValueStr[2]))
                        return false;
                    fromBase = 8;
                }
                else if (this._binRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, LongBitCount, BinCharBitCount)) return false;
                    fromBase = 2;
                }
                else
                {
                    return false;
                }
                sourceValueStr = sourceValueStr.Substring(2, sourceValueStr.Length - 2);
                castValue = Convert.ToInt64(sourceValueStr, fromBase);
                return true;
            });
            ConvertFuncs.Add(typeof(ulong).Name, (object sourceValue, out object castValue) => {
                ulong value;
                bool parseSuccss = false;
                string sourceValueStr = (string)sourceValue;
                parseSuccss = ulong.TryParse(sourceValueStr, out value);
                if (parseSuccss)
                {
                    castValue = value;
                    return true;
                }
                castValue = (ulong)0;
                int fromBase = 0;
                if (this._hexRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, LongBitCount, HexCharBitCount)) return false;
                    fromBase = 16;
                }
                else if (this._octRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, LongBitCount, OctCharBitCount, sourceValueStr[2]))
                        return false;
                    fromBase = 8;
                }
                else if (this._binRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, LongBitCount, BinCharBitCount)) return false;
                    fromBase = 2;
                }
                else
                {
                    return false;
                }
                sourceValueStr = sourceValueStr.Substring(2, sourceValueStr.Length - 2);
                castValue = Convert.ToUInt64(sourceValueStr, fromBase);
                return true;
            });
            ConvertFuncs.Add(typeof(int).Name, (object sourceValue, out object castValue) => {
                int value;
                bool parseSuccss = false;
                string sourceValueStr = (string)sourceValue;
                parseSuccss = int.TryParse(sourceValueStr, out value);
                if (parseSuccss)
                {
                    castValue = value;
                    return true;
                }
                castValue = (int)0;
                int fromBase = 0;
                if (this._hexRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, IntBitCount, HexCharBitCount)) return false;
                    fromBase = 16;
                }
                else if (this._octRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, IntBitCount, OctCharBitCount, sourceValueStr[2]))
                        return false;
                    fromBase = 8;
                }
                else if (this._binRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, IntBitCount, BinCharBitCount)) return false;
                    fromBase = 2;
                }
                else
                {
                    return false;
                }
                sourceValueStr = sourceValueStr.Substring(2, sourceValueStr.Length - 2);
                castValue = Convert.ToInt32(sourceValueStr, fromBase);
                return true;
            });
            ConvertFuncs.Add(typeof(uint).Name, (object sourceValue, out object castValue) => {
                uint value;
                bool parseSuccss = false;
                string sourceValueStr = (string)sourceValue;
                parseSuccss = uint.TryParse(sourceValueStr, out value);
                if (parseSuccss)
                {
                    castValue = value;
                    return true;
                }
                castValue = (uint)0;
                int fromBase = 0;
                if (this._hexRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, IntBitCount, HexCharBitCount)) return false;
                    fromBase = 16;
                }
                else if (this._octRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, IntBitCount, OctCharBitCount, sourceValueStr[2]))
                        return false;
                    fromBase = 8;
                }
                else if (this._binRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, IntBitCount, BinCharBitCount)) return false;
                    fromBase = 2;
                }
                else
                {
                    return false;
                }
                sourceValueStr = sourceValueStr.Substring(2, sourceValueStr.Length - 2);
                castValue = Convert.ToUInt32(sourceValueStr, fromBase);
                return true;
            });
            ConvertFuncs.Add(typeof(short).Name, (object sourceValue, out object castValue) => {
                short value;
                bool parseSuccss = false;
                string sourceValueStr = (string)sourceValue;
                parseSuccss = short.TryParse(sourceValueStr, out value);
                if (parseSuccss)
                {
                    castValue = value;
                    return true;
                }
                castValue = (short)0;
                int fromBase = 0;
                if (this._hexRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, ShortBitCount, HexCharBitCount)) return false;
                    fromBase = 16;
                }
                else if (this._octRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, ShortBitCount, OctCharBitCount, sourceValueStr[2]))
                        return false;
                    fromBase = 8;
                }
                else if (this._binRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, ShortBitCount, BinCharBitCount)) return false;
                    fromBase = 2;
                }
                else
                {
                    return false;
                }
                sourceValueStr = sourceValueStr.Substring(2, sourceValueStr.Length - 2);
                castValue = Convert.ToInt16(sourceValueStr, fromBase);
                return true;
            });
            ConvertFuncs.Add(typeof(ushort).Name, (object sourceValue, out object castValue) => {
                ushort value;
                bool parseSuccss = false;
                string sourceValueStr = (string)sourceValue;
                parseSuccss = ushort.TryParse(sourceValueStr, out value);
                if (parseSuccss)
                {
                    castValue = value;
                    return true;
                }
                castValue = (ushort)0;
                int fromBase = 0;
                if (this._hexRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, ShortBitCount, HexCharBitCount)) return false;
                    fromBase = 16;
                }
                else if (this._octRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, ShortBitCount, OctCharBitCount, sourceValueStr[2]))
                        return false;
                    fromBase = 8;
                }
                else if (this._binRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, ShortBitCount, BinCharBitCount)) return false;
                    fromBase = 2;
                }
                else
                {
                    return false;
                }
                sourceValueStr = sourceValueStr.Substring(2, sourceValueStr.Length - 2);
                castValue = Convert.ToUInt16(sourceValueStr, fromBase);
                return true;
            });
            ConvertFuncs.Add(typeof(char).Name, (object sourceValue, out object castValue) => {
                char value;
                bool parseSuccss = false;
                string sourceValueStr = (string)sourceValue;
                parseSuccss = char.TryParse(sourceValueStr, out value);
                if (parseSuccss)
                {
                    castValue = value;
                    return true;
                }
                ushort numValue;
                parseSuccss = ushort.TryParse(sourceValueStr, out numValue);
                if (parseSuccss)
                {
                    castValue = (char) numValue;
                    return true;
                }
                castValue = (char)0;
                int fromBase = 0;
                if (this._hexRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, CharBitCount, HexCharBitCount)) return false;
                    fromBase = 16;
                }
                else if (this._octRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, CharBitCount, OctCharBitCount, sourceValueStr[2]))
                        return false;
                    fromBase = 8;
                }
                else if (this._binRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, CharBitCount, BinCharBitCount)) return false;
                    fromBase = 2;
                }
                else
                {
                    return false;
                }
                sourceValueStr = sourceValueStr.Substring(2, sourceValueStr.Length - 2);
                castValue = (char) Convert.ToUInt16(sourceValueStr, fromBase);
                return true;
            });
            ConvertFuncs.Add(typeof (byte).Name, (object sourceValue, out object castValue) => {
                byte value;
                bool parseSuccss = false;
                string sourceValueStr = (string)sourceValue;
                parseSuccss = byte.TryParse(sourceValueStr, out value);
                if (parseSuccss)
                {
                    castValue = value;
                    return true;
                }
                castValue = (byte)0;
                int fromBase = 0;
                if (this._hexRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, ByteBitCount, HexCharBitCount)) return false;
                    fromBase = 16;
                }
                else if (this._octRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, ByteBitCount, OctCharBitCount, sourceValueStr[2]))
                        return false;
                    fromBase = 8;
                }
                else if (this._binRegex.IsMatch(sourceValueStr))
                {
                    if (IsBitCountExceed(sourceValueStr.Length, ByteBitCount, BinCharBitCount)) return false;
                    fromBase = 2;
                }
                else
                {
                    return false;
                }
                sourceValueStr = sourceValueStr.Substring(2, sourceValueStr.Length - 2);
                castValue = Convert.ToByte(sourceValueStr, fromBase);
                return true;
            });
            ConvertFuncs.Add(typeof(bool).Name, (object sourceValue, out object castValue) => {
                string sourceValueStr = ((string)sourceValue).Trim();
                if ("true".Equals(sourceValueStr, StringComparison.OrdinalIgnoreCase))
                {
                    castValue = true;
                    return true;
                }
                if ("false".Equals(sourceValueStr, StringComparison.OrdinalIgnoreCase))
                {
                    castValue = false;
                    return true;
                }
                castValue = sourceValue;
                return false;
            });
            ConvertFuncs.Add(typeof(DateTime).Name, (object sourceValue, out object castValue) =>
            {
                DateTime value;
                bool parseSuccss = false;
                parseSuccss = DateTime.TryParse((string)sourceValue, out value);
                castValue = value;
                return parseSuccss;
            });
//            ConvertFuncs.Add(typeof(string).Name, sourceValue => sourceValue.ToString());
        }

        public override object GetDefaultValue()
        {
            return "";
        }

        /// <summary>
        /// 返回当前字符串表示的十六进制、八进制、二进制是否超过目标类型的长度
        /// </summary>
        private bool IsBitCountExceed(int stringLength, int maxBitCount, int singleCharBitCount, char firstChar = '\0')
        {
            // 对于maxBitCount是singleCharBitCount整数倍的情况直接判断位数即可判断是否超越范围
            if (maxBitCount%singleCharBitCount == 0)
            {
                return (stringLength - 2) * singleCharBitCount > maxBitCount;
            }
            // 如果SingleCharBitCount不能被maxBitCount整除，但是整体长度小于最大位长度，则肯定没超过上限
            if ((stringLength - 2) * singleCharBitCount < maxBitCount)
            {
                return false;
            }
            if ((stringLength - 2) * singleCharBitCount > maxBitCount + singleCharBitCount - 1)
            {
                return true;
            }
            int maxDigitInFirstChar = (1 << maxBitCount%singleCharBitCount) - 1;
            // 不考虑firstChar是非数值的情况，因为没有超过10的进制会走到这个分支
            return firstChar > maxDigitInFirstChar + 48;
        }
    }
}