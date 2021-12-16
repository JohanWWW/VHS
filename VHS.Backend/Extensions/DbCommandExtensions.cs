using System.Data.Common;

namespace VHS.Backend.Extensions
{
    internal static class DbCommandExtensions
    {
        private const char ID_PREFIX = '$';

        public static DbCommand AddParameter(this DbCommand src, string key, object value)
        {
            DbParameter parameter = src.CreateParameter();
            parameter.ParameterName = key[0] is not ID_PREFIX ? $"{ID_PREFIX}{key}" : key;
            parameter.Value = value;

            src.Parameters.Add(parameter);

            return src;
        }
    }
}
