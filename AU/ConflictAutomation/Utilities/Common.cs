using ConflictAutomation.Models;
using FuzzySharp;
using System.Data;
using System.Reflection;

namespace ConflictAutomation.Services;

#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0028 // Simplify collection initialization
public class Common
{
    public static DataTable ToDataTable<T>(List<T> items)
    {
        DataTable dataTable = new DataTable(typeof(T).Name);

        //Get all the properties
        PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (PropertyInfo prop in Props)
        {
            //Defining type of data column gives proper data table 
            var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
            //Setting column names as Property names
            dataTable.Columns.Add(prop.Name, type);
        }
        foreach (T item in items)
        {
            var values = new object[Props.Length];
            for (int i = 0; i < Props.Length; i++)
            {
                //inserting property values to datatable rows
                values[i] = Props[i].GetValue(item, null);
            }
            dataTable.Rows.Add(values);
        }
        //put a breakpoint here and check datatable
        return dataTable;
    }

    public static string GetAutomationFilesPath()
    {
        string executingPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#pragma warning disable IDE0057 // Use range operator
        string automationFilesPath =  String.Concat(executingPath.Substring(0, executingPath.IndexOf("ConflictAutomation")), @"ConflictAutomation\AutomationFiles");
#pragma warning restore IDE0057 // Use range operator
        return automationFilesPath;
    }

    public static string GetMarkupCodes(SearchEntitiesResponseItemViewModel SERIVM)
    {
        List<string> lsMarkupCodes = new List<string>();

        if (SERIVM.BusinessRelationshipRestrictions > 0)
            lsMarkupCodes.Add("Business relationships restrictions");

        if (SERIVM.AcpaRequired == 1)
            lsMarkupCodes.Add("Audit Committee preapproval required for services"); 

        if (SERIVM.GcspApprovalRequired > 0)
            lsMarkupCodes.Add("GCSP approval required for services");
        
        if (SERIVM.Conflict)
            lsMarkupCodes.Add("Conflict"); 

        if(SERIVM.ConsultationRequired)
            lsMarkupCodes.Add("Consultation Required");

        if (SERIVM.LoanRestrictions >0)
            lsMarkupCodes.Add("Firm loan");
        
        if (SERIVM.ReverseRestriction)
            lsMarkupCodes.Add("Reverse restrictions");

        if (SERIVM.IsAuditClient)
            lsMarkupCodes.Add("Entity for which Audit Report is issued – Full scope of service restrictions");

        if (SERIVM.IsAttestAup)
            lsMarkupCodes.Add("Entity for which Attest/AUP Report is issued – Limited scope of service restrictions");

        if (SERIVM.IsSustainability)
            lsMarkupCodes.Add("Entity for which a sustainability assurance report is issued - requiring compliance with independence policy Parts 1, 2, 3 and 6 – Full scope of service restrictions");

        if (SERIVM.IsAuditClient == false && SERIVM.IsAttestAup == false && SERIVM.IsSustainability == false && SERIVM.ScopeOfService == 1)
            lsMarkupCodes.Add("Full scope of service restrictions");

        //Not needed. Ref. Bug 1019860: GIS Search Extract: extract missing restrictions in GIS
        //if (SERIVM.IsAuditClient == false && SERIVM.IsAttestAup == false && SERIVM.IsSustainability == false && SERIVM.ScopeOfService == 0)
        //    lsMarkupCodes.Add("Partial scope of service restrictions");

        if (!(SERIVM.IsAuditClient) && SERIVM.ScopeOfService > 0)
        {
            if (SERIVM.ScopeOfService == 2)
                lsMarkupCodes.Add("Partial scope of service restrictions");
        }

        //if (SERIVM.ReverseRestriction == true)
        //    lsMarkupCodes.Add("Reverse restrictions\");\r\n");

        if (SERIVM.IsSustainability)
            lsMarkupCodes.Add("Entity for which a sustainability assurance report is issued - requiring compliance with independence policy Parts 1, 2, 3 and 6 – Full scope of service restrictions");

        return string.Join(", ", lsMarkupCodes.Distinct().ToList());
    }

    public static int GetFuzzyPercentage(string sInputA, string sInputB)
    {
        return Fuzz.Ratio(sInputA.ToLower(), sInputB.ToLower());//return the fuzzy ratio.
    }
}
#pragma warning restore IDE0028 // Simplify collection initialization
#pragma warning restore IDE0090 // Use 'new(...)'
