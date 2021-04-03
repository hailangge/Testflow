namespace Testflow.Utility
{
    internal static class UtilityConstants
    {
        internal const int DefaultDispatchInterval = 1;
        internal const int DefaultEntityCount = 8;
        internal const string I18nName = "LoggerI18N";
        internal const string MessengerName = "MessengerI18N";
        internal const string UtilsName = "Utils";

        /// <summary>
        /// 中文国际化语言名
        /// </summary>
        public const string ChineseName = "zh-CN";

        /// <summary>
        /// 英文国际化语言名
        /// </summary>
        public const string EnglishName = "en-US";

        /// <summary>
        /// 英文资源文件后缀
        /// </summary>
        public const string EnglishResourceName = "en";

        #region 表达式相关常量

        public const string ExpI18nName = "expression";

        public const string ArgNameFormat = "ARG{0}";
        public const string ArgNamePrefix = "ARG";
        public const string ArgNamePattern = "(?:ARG|EXP)\\d+";
        public const string ExpPlaceHolderPattern = "EXP\\d+";
        public const string ExpPlaceHolderFormat = "EXP{0}";
        public const string ExpNamePrefix = "EXP";
        public const string SingleArgPattern = "^ARG\\d+$";
        public const string SingleExpPattern = "^EXP\\d+$";
        public const string NumericPattern = "^(?:\\+|-)?(?:\\d+(?:\\.\\d+)?|0[xX][0-9a-fA-F]+|\\d+(?:\\.\\d+)?[Ee][\\+-]?\\d+)?$";
        public const string SciNumericPattern = "\\d+(?:\\.\\d+)?[Ee][\\+-]?\\d+";
        public const string StringPattern = "^(\"|')(.*)\\1$";
        public const string BoolPattern = "^(?:[Tt]rue|[Ff]alse)$";
        public const string OperatorPlaceHolderRegex = @"\{\d+\}";
        // 运算符左侧有元素的模式
        public const string LeftValuePattern = "^\\{\\d+\\}.+";
        // 运算符右侧有元素的模式
        public const string RightValuePattern = ".+\\{\\d+\\}$";

        #endregion
    }
}