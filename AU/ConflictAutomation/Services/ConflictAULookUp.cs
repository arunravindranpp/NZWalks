using ConflictAutomation.Models;
using PACE;
using System.Data;
using System.Text;

namespace ConflictAutomation.Services
{
    public class ConflictAULookUp
    {
        public static List<LeagalExtensions> GetLeagalExtensions(AppConfigure config)
        {
            List<LeagalExtensions> list = new List<LeagalExtensions>();
            string getTemplateQuery = @"select ID,FromValue from KeywordGeneratorSubstringReplacements where keywordGeneratorType ='E'";
            try
            {
                using (var reader = EYSql.ExecuteReader(config.ConnectionString, CommandType.Text, getTemplateQuery,null
                                ))
                {
                    while(reader.Read())
                    {
                        list.Add(new LeagalExtensions
                        {
                            Id = reader.GetInt32("ID"),
                            Extensions = reader.GetString("FromValue")
                        });
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                LoggerInfo.LogException(ex);
            }
            return list;
        }
        public static List<SkipCountries> GetSkipCountries(AppConfigure config)
        {
            List<SkipCountries> list = new List<SkipCountries>();
            string getTemplateQuery = @"select Id,CountryCode,Country, SSLName from CAU_SkipCountries";
            try
            {
                System.Data.DataTable dt = EYSql.ExecuteDataset(config.ConnectionString, CommandType.Text, getTemplateQuery).Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(new SkipCountries
                    {
                        Id = dr.GetInt("Id"),
                        CountryCode = dr.GetString("CountryCode"),
                        Country = dr.GetString("Country"),
                        SSLName = dr.GetString("SSLName")
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerInfo.LogException(ex);
            }
            return list;
        }
    }
}
