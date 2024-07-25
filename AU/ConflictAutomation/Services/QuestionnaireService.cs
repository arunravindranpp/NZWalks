using ConflictAutomation.Models;
using PACE;
using System.Data;
using System.Data.SqlClient;

namespace ConflictAutomation.Services
{
    public class QuestionnaireService
    {
        public class QuestionnaireList
        {
            public string QID { get; set; }
            public string ColumnName { get; set; }
            public string ColumnValue { get; set; }
            public int RowNumber { get; set; }
        }

        public void GetQuestionnaire(long? questionnaireId, CheckerQueue queue)
        {
            try
            {
                if(questionnaireId == 0 || questionnaireId ==null)
                {
                    return;
                }
                List<QuestionnaireList> list = new List<QuestionnaireList>();
                queue.questionnaireAdditionalParties = new List<QuestionnaireAdditionalParties>();
                DataTable dtAnswer = EYSql.ExecuteDataset(Program.PACEConnectionString, CommandType.Text,
                    @"SELECT ag.QUESTIONNAIRE_ID, ag.COLUMN_NAME,ag.COLUMN_VALUE, ag.ROW_NUMBER
                      FROM QMS_ANSWER_GRID ag
                      JOIN QMS_QUESTION_GRID qg
                      ON ag.QUESTION_GRID_ID = qg.QUESTION_GRID_ID
                      WHERE ag.Questionnaire_Id = @QID AND qg.DISPLAY_ON_QUESTIONNAIRE = 'CACC'   
                ", new SqlParameter("@QID", questionnaireId)).Tables[0];
                foreach (DataRow dr in dtAnswer.Rows)
                {
                    list.Add(new QuestionnaireList
                    {
                        QID = dr.GetString("QUESTIONNAIRE_ID"),
                        ColumnName = dr.GetString("COLUMN_NAME"),
                        ColumnValue = dr.GetString("COLUMN_VALUE"),
                        RowNumber = dr.GetInt("ROW_NUMBER")
                    });
                }
                var groupedData = list.OrderBy(q => q.RowNumber).GroupBy(q => q.RowNumber);
                foreach (var group in groupedData)
                {
                    QuestionnaireAdditionalParties additionalParties = new QuestionnaireAdditionalParties();
                    var name = group.FirstOrDefault(i => i.ColumnName == "Name");
                    if (name != null)
                    {
                        additionalParties.Name = name.ColumnValue;
                    }
                    var position = group.FirstOrDefault(i => i.ColumnName == "Position");
                    if (position != null)
                    {
                        additionalParties.Position = position.ColumnValue;
                    }
                    var otherInfo = group.FirstOrDefault(i => i.ColumnName == "Other Information (e.g., percentage ownership)");
                    if (otherInfo != null)
                    {
                        additionalParties.OtherInformation = otherInfo.ColumnValue;
                    }
                    queue.questionnaireAdditionalParties.Add(additionalParties);
                }
            }
            catch(Exception ex)
            {
                LoggerInfo.LogException(ex);
            }
        }
    }
}
