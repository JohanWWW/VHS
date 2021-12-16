using System.Data.Common;

namespace VHS.Backend.Extensions
{
    internal static class DbConnectionExtensions
    {
        public static DbCommand CreateCommand(this DbConnection src, string commandText, System.Data.CommandType commandType = System.Data.CommandType.Text)
        {
            DbCommand cmd = src.CreateCommand();
            cmd.CommandText = commandText;
            cmd.CommandType = commandType;
            return cmd;
        }
    }
}
