using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConflictAutomation.Models
{
    public static class DataRowExtensions
    {
        public static string GetString(this DataRow dr, string fieldName)
        {
            if (dr[fieldName] == DBNull.Value) return string.Empty;

            return dr[fieldName].ToString();
        }

        public static int GetInt(this DataRow dr, string fieldName)
        {
            return dr.Field<int?>(fieldName) ?? 0;
        }

        public static long GetLong(this DataRow dr, string fieldName)
        {
            return dr.Field<long?>(fieldName) ?? 0;
        }

        public static short GetShort(this DataRow dr, string fieldName)
        {
            return dr.Field<short?>(fieldName) ?? 0;
        }

        public static DateTime GetDateTime(this DataRow dr, string fieldName)
        {
            return dr.Field<DateTime?>(fieldName) ?? DateTime.MinValue;
        }

        public static bool GetBool(this DataRow dr, string fieldName)
        {
            return dr.Field<bool?>(fieldName) ?? false;
        }

    }
}