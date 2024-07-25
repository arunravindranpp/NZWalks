CREATE OR ALTER PROC [dbo].[CAU_Summary]                              
 @CheckID BIGINT                              
AS                              
                              
--EXEC CAU_Summary '1412582' '1412640'                              
--EXEC CAU_Summary '1412636'                              
--DECLARE @CheckID BIGINT                              
--SET @CheckID = '1412640'                              
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED       
      
DECLARE @CheckType NVARCHAR(50)                              
SET @CheckType = 'NonPursuit'                              
                              
SELECT @CheckType = ConflictCheckType FROM WF_ConflictChecks WHERE ConflictCheckType = 'Pursuit' AND ConflictCheckID = @CheckID                              
                              
IF(@CheckType = 'NonPursuit')                              
BEGIN                              
DECLARE @AssessmentID BIGINT                              
SELECT @AssessmentID = AssessmentID FROM WF_ConflictChecks WHERE ConflictCheckID = @CheckID                               
                          
DROP TABLE IF EXISTS #C_Services_TEMP                          
SELECT DISTINCT  CS.AssessmentID,                             
             CASE WHEN CS.ServiceName = '' THEN CS.ServiceCode + ' - ' + CS.SORTServiceName ELSE
                CS.ServiceCode + ' - ' + CS.ServiceName + ' / ' + CS.SORTServiceName END as [Services],                          
    CS.SORTServiceName,                    
                SERVICE_LINE_DESCR as ServiceLineName,                           
                (CASE WHEN isnull(M.MercurySSLLabel, '') = '' THEN SSL.SUB_SERVICE_LINE_DESCR ELSE M.MercurySSLLabel END) AS SubServiceLineName,                          
                ISNULL(CL.COUNTRY_DESCRIPTION,CLM.COUNTRY_DESCRIPTION) as CountryName , CS.IsPrimary,  WC.SORTSODetailID                                    
                INTO #C_Services_TEMP                          
    FROM WF_Services CS       JOIN WF_Assessments WA on CS.AssessmentID = WA.AssessmentID                    
                LEFT JOIN vwWFServiceLineSubServiceLine SSL on CS.SubServiceLineCode = SSL.SUB_SERVICE_LINE_CODE                           
                LEFT JOIN WF_ServicesConfiguration WC on WC.ServiceConfigurationid = CS.ServiceConfigurationid                          
    LEFT JOIN vwCOUNTRY_LIST CL on CL.COUNTRY_CODE = WC.CountryCode                          
    LEFT JOIN vwCOUNTRY_LIST CLM on CLM.COUNTRY_CODE = WA.CountryCode                      
                LEFT JOIN WF_CountryConfiguration MCC on MCC.CountryCode =  WC.CountryCode and MCC.MercuryAcceptance = 1                          
                LEFT JOIN WF_MercurySSLLabel M on M.ServiceLineCode = SSL.SERVICE_LINE_CODE and M.SubServiceLineCode = SSL.SUB_SERVICE_LINE_CODE and MCC.CountryConfigurationID is not null                          
                WHERE CS.AssessmentID = @AssessmentID                          
                              
                          
DROP TABLE IF EXISTS #C_Services_SubTemp                               
CREATE TABLE #C_Services_SubTemp(AssessmentID BIGINT, [Services] NVARCHAR(MAX), SubServiceLineName NVARCHAR(MAX))                          
INSERT INTO #C_Services_SubTemp(AssessmentID, [Services], SubServiceLineName)                          
SELECT AssessmentID, STRING_AGG([Services] + '(Primary)', ' | ') as [Services] , STRING_AGG(  ServiceLineName  + ' / ' + SubServiceLineName + '(Primary)', ' | ') AS [SSL]                          
FROM #C_Services_TEMP WHERE IsPrimary = 1 GROUP BY AssessmentID                             
                      
INSERT INTO #C_Services_SubTemp(AssessmentID, [Services], SubServiceLineName)                          
SELECT AssessmentID, STRING_AGG([Services], ' | ') as [Services] , STRING_AGG(  ServiceLineName  + ' / ' + SubServiceLineName, ' | ') AS [SSL]                          
FROM #C_Services_TEMP WHERE IsPrimary = 0 GROUP BY AssessmentID                           
                      DROP TABLE IF EXISTS #C_Services                               
CREATE TABLE #C_Services(AssessmentID BIGINT, [Services] NVARCHAR(MAX), SubServiceLineName NVARCHAR(MAX), CountryName NVARCHAR(MAX),      
IsUKI BIT DEFAULT 0, IsClientSide BIT, IsCRRGUP BIT DEFAULT 0)                           
INSERT INTO #C_Services(AssessmentID, [Services], SubServiceLineName)                          
SELECT AssessmentID, STRING_AGG([Services], ' | ') as [Services] , STRING_AGG(  SubServiceLineName, ' | ') AS [SSL]                          
FROM #C_Services_SubTemp GROUP BY AssessmentID                            
                          
UPDATE A                          
SET A.CountryName = B.CountryName                          
FROM #C_Services A JOIN #C_Services_TEMP B ON A.AssessmentID = B.AssessmentID                        
                    
DROP TABLE IF EXISTS #C_IsUKI                            
CREATE TABLE #C_IsUKI(AssessmentID BIGINT)                    
INSERT INTO #C_IsUKI                    
SELECT A.AssessmentID                    
FROM #C_Services A JOIN #C_Services_TEMP B ON A.AssessmentID = B.AssessmentID                     
JOIN CAU_NonClientSideSORT_ServiceIDs C ON B.SORTSODetailID = C.ServiceID  AND C.IsFiltered = 1                   
WHERE B.CountryName IN('United Kingdom','Ireland') AND C.IsFiltered = 1                      
                  
DROP TABLE IF EXISTS #C_IsClientSide                           
CREATE TABLE #C_IsClientSide(AssessmentID BIGINT)                    
INSERT INTO #C_IsClientSide                    
SELECT A.AssessmentID                    
FROM #C_Services A JOIN #C_Services_TEMP B ON A.AssessmentID = B.AssessmentID                     
JOIN CAU_NonClientSideSORT_ServiceIDs C ON B.SORTSODetailID = C.ServiceID                    
                    
UPDATE A                          
SET A.IsUKI = 1                         
FROM #C_Services A JOIN #C_IsUKI B ON A.AssessmentID = B.AssessmentID                        
                  
UPDATE A                          
SET A.IsClientSide = 0                         
FROM #C_Services A JOIN #C_IsClientSide B ON A.AssessmentID = B.AssessmentID               
      
UPDATE A        
SET A.IsCRRGUP = 1        
FROM #C_Services A JOIN #C_Services_TEMP B ON A.AssessmentID = B.AssessmentID         
JOIN CAU_CRRGUPSearch C ON B.SORTSODetailID = C.ServiceID        
                              
SELECT C.ConflictCheckID, C.ConflictCheckType, C.AssessmentID, C.ClientName, C.Region, CS.CountryName, A.Tag, A.EngagementName,                               
CS.SubServiceLineName, CASE WHEN C.ConflictPrivacySetting= 'Level 2' THEN 'Yes' ELSE 'No' END AS Confidential, CS.[Services] ,       
A.ServicesDetailedDescription, CS.IsUKI, CS.IsCRRGUP                               
FROM WF_ConflictChecks C                               
JOIN WF_Assessments A ON C.AssessmentID = A.AssessmentID                          
LEFT JOIN #C_Services CS ON CS.AssessmentID = A.AssessmentID                              
LEFT JOIN vwGHRDB_Employees G ON C.ConflictCheckerGUI = G.GUI AND G.PrimaryEmail = 1                              
WHERE C.ConflictCheckID = @CheckID                         
                  
                              
--Entity Info                    
DROP TABLE IF EXISTS #C_AssessmentType                    
CREATE TABLE #C_AssessmentType(AssessmentID BIGINT, AssessmentTypeCode NVARCHAR(100))                  
INSERT INTO #C_AssessmentType                  
SELECT WA.AssessmentID, WA.AssessmentTypeCode                   
FROM WF_Assessments WA WHERE WA.AssessmentID = @AssessmentID                       
                  
 DROP TABLE IF EXISTS #EntityInfo                      
CREATE TABLE #EntityInfo(OrderNo INT IDENTITY(1,1), EntityName NVARCHAR(500), [Role] NVARCHAR(1000),                       
Country NVARCHAR(200), DUNSNumber NVARCHAR(50), GISID NVARCHAR(50), MDMID NVARCHAR(50), AdditionalPartyID BIGINT, AdditionalComments NVARCHAR(MAX),                 
IsFinscan BIT DEFAULT 1, IsClientSide BIT, PerformResearch BIT DEFAULT 1, CountryCode NVARCHAR(20))                      
                      
INSERT INTO #EntityInfo (EntityName, Role, Country, DUNSNumber, GISID, MDMID, AdditionalPartyID,AdditionalComments, IsClientSide, CountryCode)                      
SELECT DISTINCT ClientName as EntityName, 'Main Client | ' + ISNULL(AG.Role,'') AS Role, CL.COUNTRY_DESCRIPTION AS Country, DUNSNumber, GISClientID as 'GISID',                
A.MDMClientID   ,1 , APGComments, CS.IsClientSide, A.CountryCode                       
FROM WF_Assessments A             
LEFT JOIN WF_APGRoles AG ON AG.APGRoleID = A.MCAdditionalRole  AND ISNULL(A.MCAdditionalRole,'') <> '' 
LEFT JOIN vwCOUNTRY_LIST CL ON A.CountryCode = CL.COUNTRY_CODE                     
LEFT JOIN #C_Services CS ON CS.AssessmentID = A.AssessmentID                   
WHERE A.AssessmentID = @AssessmentID         
  
INSERT INTO #EntityInfo (EntityName, Role, Country, DUNSNumber, GISID, MDMID, AdditionalPartyID,AdditionalComments, IsClientSide, CountryCode)                              
SELECT DISTINCT GUPName as EntityName, 'GUP of Main Client | ' + ISNULL(AG.Role,'') AS Role, '' AS Country, GUPDUNSNumber, GUPGISClientID as 'GISID' , '' ,2  , '',                 
CS.IsClientSide, ''                          
FROM WF_Assessments A                           
LEFT JOIN WF_APGRoles AG ON AG.APGRoleID = A.MCAdditionalRole  AND ISNULL(A.MCAdditionalRole,'') <> '' 
LEFT JOIN #C_Services CS ON CS.AssessmentID = A.AssessmentID                   
WHERE ISNULL(A.DUNSNumber,'') <> ISNULL(A.GUPDUNSNumber,'') AND ISNULL(A.GUPName,'') <>'' AND A.AssessmentID = @AssessmentID                         
            
UPDATE A            
SET AdditionalComments ='Main client transferred from summary page'            
FROM #EntityInfo A  WHERE Role ='Main Client' and AdditionalComments = ''            
            
UPDATE A                  
SET IsFinscan = 0                   
FROM #EntityInfo A JOIN #C_AssessmentType B ON 1 = 1                  
WHERE B.AssessmentTypeCode IN ('CC/EC','CC/EA','EA')   AND A.Role = 'Main Client'               
                      
INSERT INTO #EntityInfo (EntityName, Role, Country, DUNSNumber, GISID, MDMID, AdditionalPartyID, AdditionalComments, IsClientSide, CountryCode)                         
SELECT DISTINCT EntityName, Role, CL.COUNTRY_DESCRIPTION AS Country, DUNSNumber, GISID as 'GISID', AP.MDMClientID    , AP.AdditionalPartyID,                       
CASE WHEN AP.AdditionalComments <> '' AND AP.Comments <> '' THEN  AP.Comments +' - ' + AP.AdditionalComments             
     WHEN AP.AdditionalComments <> '' AND AP.Comments = ''  THEN  AP.AdditionalComments             
ELSE AP.Comments END , CS.IsClientSide, AP.CountryCode                      
FROM WF_AdditionalParties AP                               
LEFT JOIN WF_APGRoles APR ON AP.APGRoleID = APR.APGRoleID                                
LEFT JOIN vwCOUNTRY_LIST CL ON AP.CountryCode = CL.COUNTRY_CODE                      
LEFT JOIN #C_Services CS ON CS.AssessmentID = AP.FolderID                   
WHERE FolderID = @AssessmentID   ORDER BY AP.AdditionalPartyID                              
                      
INSERT INTO #EntityInfo (EntityName, Role, Country, DUNSNumber, GISID, AdditionalPartyID, IsClientSide, CountryCode)                            
SELECT DISTINCT GUPEntityName as EntityName, 'GUP of '+ APR.Role as Role, '' AS Country, AP.GUPDUNSNumber,           
GUPGISID as 'GISID',AP.AdditionalPartyID , CS.IsClientSide, ''  --Tejal come back on APG GUP country                              
FROM WF_AdditionalParties AP                                
LEFT JOIN WF_APGRoles APR ON AP.APGRoleID = APR.APGRoleID                                
LEFT JOIN #C_Services CS ON CS.AssessmentID = AP.FolderID                       
LEFT JOIN #EntityInfo EI ON EI.EntityName = AP.GUPEntityName AND EI.DUNSNumber = AP.GUPDUNSNumber                       
WHERE   (ISNULL(AP.GUPEntityName,'') <> '' )                      
 AND FolderID = @AssessmentID                       
AND EI.EntityName IS NULL AND EI.DUNSNumber IS NULL                       
ORDER BY AP.AdditionalPartyID                      
                  
UPDATE A                  
SET A.IsClientSide = 0                   
FROM #EntityInfo A JOIN #C_Services B ON 1=1                  
WHERE B.IsClientSide = 0                  
                  
UPDATE A                          
SET A.IsClientSide = 1                         
FROM #EntityInfo A JOIN CAU_ClientSideRoles B ON A.[Role] = B.RoleName                        
WHERE A.IsClientSide IS NULL                  
                  
UPDATE A                        
SET A.IsClientSide = 0                         
FROM #EntityInfo A                   
WHERE A.IsClientSide IS NULL                  
                
UPDATE A                
SET PerformResearch = 0                
FROM #EntityInfo A                
JOIN CAU_APGRolesToBlockResearch B ON A.Role = B.RoleName                 
WHERE A.IsClientSide = 0           
                
UPDATE A                
SET PerformResearch = 0                
FROM #EntityInfo A                
JOIN CAU_APGRolesToBlockResearch B ON A.Role = 'GUP of '+ B.RoleName                 
WHERE A.IsClientSide = 0                
              
 --select * from #EntityInfo order by AdditionalPartyID,1                  
          
DROP TABLE IF EXISTS #EntityInfo_Final     
SELECT MIN(OrderNo) as OrderNo, t.EntityName,         
  STUFF((SELECT DISTINCT ',' + Role FROM #EntityInfo EIP         
 WHERE EIP.EntityName= t.EntityName AND EIP.DUNSNumber = t.DUNSNumber and EIP.MDMID =t.MDMID      
 and EIP.GISID = t.GISID FOR XML PATH (''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '') as Role,      
  t.Country AS Country,        
  t.DUNSNumber, t.GISID, t.MDMID, MIN(AdditionalPartyID) as AdditionalPartyID,       
  STUFF((SELECT DISTINCT ',' + AdditionalComments FROM #EntityInfo EIP         
 WHERE EIP.EntityName= t.EntityName AND EIP.DUNSNumber = t.DUNSNumber and EIP.MDMID =t.MDMID      
 and EIP.GISID = t.GISID FOR XML PATH (''), TYPE  
        ).value('.', 'VARCHAR(MAX)'), 1, 1, ''  
    ) as AdditionalComments,          
  MAX(CASE WHEN IsFinscan=1 THEN 1 ELSE 0 END) IsFinscan, MIN(CASE WHEN IsClientSide=1 THEN 1 ELSE 0 END) IsClientSide,           
  MAX(CASE WHEN PerformResearch=1 THEN 1 ELSE 0 END) PerformResearch,      
  t.CountryCode AS CountryCode      
 INTO #EntityInfo_Final    
FROM #EntityInfo t           
GROUP BY t.EntityName, t.DUNSNumber, t.GISID, t.MDMID , t.Country, t.CountryCode         
ORDER BY 8,1     
        
UPDATE A        
SET A.Role = B.Role        
FROM #EntityInfo_Final A JOIN #EntityInfo B        
ON A.OrderNo = B.OrderNo WHERE ISNULL(A.role,'') = ''        
        
--UPDATE A        
--SET A.Country = B.Country, A.CountryCode = B.CountryCode        
--FROM #EntityInfo_Final A JOIN #EntityInfo B        
--ON A.OrderNo = B.OrderNo WHERE ISNULL(A.Country,'') = ''        
        
UPDATE A        
SET A.AdditionalComments = B.AdditionalComments        
FROM #EntityInfo_Final A JOIN #EntityInfo B        
ON A.OrderNo = B.OrderNo WHERE ISNULL(A.AdditionalComments,'') = ''        
        
UPDATE A        
SET A.DUNSNumber = B.DUNSNumber        
FROM #EntityInfo_Final A JOIN #EntityInfo B        
ON A.OrderNo = B.OrderNo WHERE ISNULL(A.DUNSNumber,'') = ''        
        
UPDATE A        
SET A.GISID = B.GISID        
FROM #EntityInfo_Final A JOIN #EntityInfo B        
ON A.OrderNo = B.OrderNo WHERE ISNULL(A.GISID,'') = ''        
        
UPDATE A        
SET A.MDMID = B.MDMID        
FROM #EntityInfo_Final A JOIN #EntityInfo B        
ON A.OrderNo = B.OrderNo WHERE ISNULL(A.MDMID,'') = ''        
        
SELECT * FROM #EntityInfo_Final order by AdditionalPartyID,1        
                                
--Questionnaire                              
DROP TABLE IF EXISTS #Questionnaire                               
SELECT DISTINCT                               
 ISNULL(REPLACE(dbo.fn_rp_CleanString(CASE WHEN LEN(I.[VALUE]) > 2000 THEN SUBSTRING(I.[VALUE], 1, 1997) + '...' ELSE I.[VALUE] END), '&amp', '&'), '') AS Answer,                                    
 COALESCE(I.[ROW_NUMBER], IA.[ROW_NUMBER], -1) RowNumber,                              
 ISNULL(REPLACE(dbo.fn_rp_CleanString(CASE WHEN LEN(ie.[VALUE]) > 2000 THEN SUBSTRING(ie.[VALUE], 1, 1997) + '...' ELSE ie.[VALUE] END), '&amp', '&'), '') AS TriggeredAnswer,                              
-- ISNULL(dbo.fn_rp_CleanString(CASE WHEN LEN(ia.[Value]) > 2000 THEN SUBSTRING(ia.[Value], 1, 1997) + '...' ELSE ia.[Value] END), '') AS AttachmentName,                              
-- COALESCE(g.column_name_unique_number,s.selection_unique_number, q.question_number, '') AS UniqueID,                              
 WA.AssessmentID,                              
 Q.Question_ID,                              
 Q.Question_Number as QuestionNumber,                              
 ISNULL(s.SELECTION_UNIQUE_NUMBER, '') AS AnswerNumber,                              
 I.QMS_ANSWER_INPUTS_ID,                              
 CASE WHEN ia.[VALUE] IS NOT NULL THEN N'Attachment Uploaded' ELSE ISNULL(dbo.fn_rp_CleanString(CASE WHEN LEN(i.ANSWER_ELABORATION) > 2000 THEN SUBSTRING(i.ANSWER_ELABORATION, 1, 1997) + '...' ELSE i.ANSWER_ELABORATION END), '') END AS Explanation        
 
    
INTO #Questionnaire                              
FROM                               
  WF_Assessments WA                               
 JOIN QMS_QUESTIONNAIRE_VISIBILITY v on WA.questionnaireid = v.questionnaire_id                              
 JOIN qms_question q on v.question_id = q.question_id                              
 JOIN CAU_Questions CAU ON q.QUESTION_NUMBER = CAU.QuestionNumber                              
 JOIN qms_answer_inputs i on                               
  v.questionnaire_id = i.questionnaire_id                              
  AND v.question_id = i.question_id                              
  AND I.QUESTION_TRIGGER_ID is null                               
  AND I.QUESTION_BPM_TRIGGER_ID is null                              
  AND i.input_type <> 'Attachment'                              
 LEFT JOIN QMS_QUESTION_GRID G on                               
  I.QUESTION_GRID_ID = G.QUESTION_GRID_ID and i.input_type = 'grid'                              
 LEFT JOIN                               
  (                              
   SELECT SELECTION, QUESTION_ID, MAX(SELECTION_UNIQUE_NUMBER) AS SELECTION_UNIQUE_NUMBER                              
   FROM QMS_QUESTION_SELECTION GROUP BY SELECTION, QUESTION_ID                              
  ) s on I.QUESTION_ID = s.QUESTION_ID                                  
  AND i.input_type = 'selection'                               
  AND i.value = s.selection                              
 LEFT JOIN (                              
  qms_question_trigger te                               
  JOIN qms_answer_inputs ie on                               
   te.question_trigger_id = ie.question_trigger_id                              
   AND ie.answer_elaboration is not null                              
 ) ON                              
  i.question_id = te.triggered_by_question_id                              
  AND i.value = te.expected_value                              
  AND trigger_title not like 'trigger%'                               
  AND TRIGGER_TYPE_CODE = 'Elaborate'                              
  AND v.questionnaire_id = ie.questionnaire_id                    
 LEFT JOIN qms_answer_inputs ia on                               
  q.question_id = ia.question_id                               
  AND WA.questionnaireid = ia.questionnaire_id                                
  AND ia.input_type='Attachment'                               
 where WA.AssessmentID = @AssessmentID                              
                              
 DROP TABLE IF EXISTS #Title                              
  SELECT Title, ISNULL(Answer, 'N/A') AS Answer, ISNULL(TriggeredAnswer, 'N/A') as Explanation, AssessmentID,A.QuestionNumber INTO #Title                              
 FROM CAU_Questions A  JOIN #Questionnaire B ON A.QuestionNumber = B.QuestionNumber                              
                              
                              
 SELECT distinct A.Title, ISNULL(B.Answer, 'N/A') AS Answer, ISNULL(B.Explanation, 'N/A') as Explanation,B.QuestionNumber,AssessmentID, @CheckID                              
 FROM CAU_Questions A LEFT JOIN #Title B ON A.Title = B.Title                              
END                              
                              
IF(@CheckType = 'Pursuit')                              
BEGIN                              
DROP TABLE IF EXISTS #CP_Services_TEMP                          
SELECT DISTINCT CC.ConflictCheckID,     
CASE WHEN CS.ServiceName = '' THEN CS.ServiceCode + ' - ' + CS.SORTServiceName ELSE
    CS.ServiceCode + ' - ' + CS.ServiceName + ' / ' + CS.SORTServiceName  END as [Services],                       
    CS.SORTServiceName ,                    
                COALESCE(SERVICE_LINE_DESCR, CC.ServiceLineName) as ServiceLineName,                           
                (CASE WHEN isnull(M.MercurySSLLabel, '') = '' THEN SSL.SUB_SERVICE_LINE_DESCR ELSE M.MercurySSLLabel END) AS SubServiceLineName,                          
                CL.COUNTRY_DESCRIPTION as CountryName  , CC.ServiceLineName as IsPrimary,  WC.SORTSODetailID                                      
                INTO #CP_Services_TEMP                          
    FROM WF_ConflictChecks CC LEFT JOIN WF_ConflictCheckServices CS ON CS.ConflictCheckID = CC.ConflictCheckID                        
                LEFT JOIN vwWFServiceLineSubServiceLine SSL on CS.SubServiceLineCode = SSL.SUB_SERVICE_LINE_CODE                           
                LEFT JOIN WF_ServicesConfiguration WC on WC.ServiceConfigurationid = CS.ServiceConfigurationid                     
    LEFT JOIN vwCOUNTRY_LIST CL on CL.COUNTRY_CODE = WC.CountryCode                          
                LEFT JOIN WF_CountryConfiguration MCC on MCC.CountryCode =  WC.CountryCode and MCC.MercuryAcceptance = 1                          
                LEFT JOIN WF_MercurySSLLabel M on M.ServiceLineCode = SSL.SERVICE_LINE_CODE and M.SubServiceLineCode = SSL.SUB_SERVICE_LINE_CODE and MCC.CountryConfigurationID is not null                          
                WHERE CC.ConflictCheckid=@CheckID                          
                          
DROP TABLE IF EXISTS #CP_Services_SubTemp1                               
CREATE TABLE #CP_Services_SubTemp1(ConflictCheckID BIGINT, [Services] NVARCHAR(MAX), SubServiceLineName NVARCHAR(MAX), ServiceLineName NVARCHAR(MAX), IsPrimary NVARCHAR(MAX))                          
INSERT INTO #CP_Services_SubTemp1                      
SELECT ConflictCheckID, CASE WHEN [Services] IS NOT NULL THEN  [Services]  + '(Primary)' END AS [Services] ,                         
CASE WHEN SubServiceLineName IS NOT NULL THEN  ServiceLineName  + ' / ' + SubServiceLineName + '(Primary)' ELSE ServiceLineName + '(Primary)' END AS [SSL],                      
ServiceLineName, IsPrimary                      
FROM #CP_Services_TEMP WHERE IsPrimary = ServiceLineName                       
                          
DROP TABLE IF EXISTS #CP_Services_SubTemp                               
CREATE TABLE #CP_Services_SubTemp(ConflictCheckID BIGINT, [Services] NVARCHAR(MAX), SubServiceLineName NVARCHAR(MAX))                          
INSERT INTO #CP_Services_SubTemp(ConflictCheckID, [Services], SubServiceLineName)                          
SELECT ConflictCheckID, STRING_AGG([Services], ' | ') as [Services] , STRING_AGG(  SubServiceLineName , ' | ') AS [SSL]                          
FROM #CP_Services_SubTemp1 GROUP BY ConflictCheckID                                 
                      
INSERT INTO #CP_Services_SubTemp(ConflictCheckID, [Services], SubServiceLineName)                          
SELECT ConflictCheckID, STRING_AGG([Services], ' | ') as [Services] , STRING_AGG(  ServiceLineName  + ' / ' + SubServiceLineName, ' | ') AS [SSL]                          
FROM #CP_Services_TEMP WHERE IsPrimary <> ServiceLineName GROUP BY ConflictCheckID                       
                      
DROP TABLE IF EXISTS #CP_Services                               
CREATE TABLE #CP_Services(ConflictCheckID BIGINT, [Services] NVARCHAR(MAX), SubServiceLineName NVARCHAR(200), CountryName NVARCHAR(250),       
IsUKI BIT DEFAULT 0, IsClientSide BIT, IsCRRGUP BIT DEFAULT 0)                           
INSERT INTO #CP_Services(ConflictCheckID, [Services], SubServiceLineName)                          
SELECT ConflictCheckID, STRING_AGG([Services], ' | ') as [Services] , STRING_AGG(  SubServiceLineName, ' | ') AS [SSL]                          
FROM #CP_Services_SubTemp GROUP BY ConflictCheckID                           
                          
UPDATE A                          
SET A.CountryName = B.CountryName                          
FROM #CP_Services A JOIN #CP_Services_TEMP B ON A.ConflictCheckID = B.ConflictCheckID                          
                    
DROP TABLE IF EXISTS #CP_IsUKI                            
CREATE TABLE #CP_IsUKI(ConflictCheckID BIGINT)                    
INSERT INTO #CP_IsUKI                    
SELECT A.ConflictCheckID                    
FROM #CP_Services A JOIN #CP_Services_TEMP B ON A.ConflictCheckID = B.ConflictCheckID                     
JOIN CAU_NonClientSideSORT_ServiceIDs C ON B.SORTSODetailID = C.ServiceID  AND C.IsFiltered = 1                        
WHERE B.CountryName IN('United Kingdom','Ireland') AND C.IsFiltered = 1                          
                  
                  
DROP TABLE IF EXISTS #CP_IsClientSide                            
CREATE TABLE #CP_IsClientSide(ConflictCheckID BIGINT)                    
INSERT INTO #CP_IsClientSide                    
SELECT A.ConflictCheckID                    
FROM #CP_Services A JOIN #CP_Services_TEMP B ON A.ConflictCheckID = B.ConflictCheckID                     
JOIN CAU_NonClientSideSORT_ServiceIDs C ON B.SORTSODetailID = C.ServiceID                 
                    
UPDATE A                          
SET A.IsUKI = 1                         
FROM #CP_Services A JOIN #CP_IsUKI B ON A.ConflictCheckID = B.ConflictCheckID                      
                  
UPDATE A                          
SET A.IsClientSide = 0                         
FROM #CP_Services A JOIN #CP_IsClientSide B ON A.ConflictCheckID = B.ConflictCheckID           
      
UPDATE A        
SET A.IsCRRGUP = 1        
FROM #CP_Services A JOIN #CP_Services_TEMP B ON A.ConflictCheckID = B.ConflictCheckID         
JOIN CAU_CRRGUPSearch C ON B.SORTSODetailID = C.ServiceID        
                              
DROP TABLE IF EXISTS #MainPursuitGrid                  
SELECT C.ConflictCheckID, C.ConflictCheckType, C.AssessmentID, C.ClientName, C.Region, ISNULL(CL.Country_Description, CP.CountryName) as CountryName, '' as Tag, '' as EngagementName,                               
CP.SubServiceLineName, CASE WHEN C.ConflictPrivacySetting= 'Level 2' THEN 'Yes' ELSE 'No' END AS Confidential, CP.[Services] ,       
C.ServicesDetailedDescription , CP.IsUKI, CP.IsCRRGUP                         
INTO #MainPursuitGrid                  
FROM WF_ConflictChecks C                               
LEFT JOIN vwCOUNTRY_LIST CL ON C.CountryCode = CL.COUNTRY_CODE                              
LEFT JOIN #CP_Services CP ON CP.ConflictCheckID = C.ConflictCheckID                              
LEFT JOIN vwGHRDB_Employees G ON C.ConflictCheckerGUI = G.GUI AND G.PrimaryEmail = 1                              
WHERE C.ConflictCheckID = @CheckID                              
                  
SELECT * FROM #MainPursuitGrid                              
                              
--Entity Info                            
DROP TABLE IF EXISTS #EntityInfo_P                      
CREATE TABLE #EntityInfo_P(OrderNo INT IDENTITY(1,1), EntityName NVARCHAR(500), [Role] NVARCHAR(1000),                       
Country NVARCHAR(200), DUNSNumber NVARCHAR(50), GISID NVARCHAR(50), MDMID NVARCHAR(50), AdditionalPartyID BIGINT, AdditionalComments NVARCHAR(MAX),                 
IsFinscan BIT DEFAULT 1, IsClientSide BIT, PerformResearch BIT DEFAULT 1, CountryCode NVARCHAR(20))                         
                      
INSERT INTO #EntityInfo_P (EntityName, Role, Country, DUNSNumber, GISID, MDMID,AdditionalPartyID,AdditionalComments,IsClientSide,CountryCode)                      
                      
SELECT DISTINCT EntityName, Role, CL.COUNTRY_DESCRIPTION AS Country, DUNSNumber, GISID, AP.MDMClientID, AdditionalPartyID,                       
CASE WHEN AP.AdditionalComments <> '' AND AP.Comments <> '' THEN  AP.Comments +' - ' + AP.AdditionalComments             
     WHEN AP.AdditionalComments <> '' AND AP.Comments = ''  THEN  AP.AdditionalComments             
ELSE AP.Comments END, CS.IsClientSide, AP.CountryCode                       
FROM WF_AdditionalParties AP                               
LEFT JOIN WF_APGRoles APR ON AP.APGRoleID = APR.APGRoleID                                
LEFT JOIN vwCOUNTRY_LIST CL ON AP.CountryCode = CL.COUNTRY_CODE               
LEFT JOIN #CP_Services CS ON CS.ConflictCheckID = AP.FolderID                   
WHERE FolderID = @CheckID AND FolderType ='Pursuit' ORDER BY AP.AdditionalPartyID ASC                          
                      
INSERT INTO #EntityInfo_P (EntityName, Role, Country, DUNSNumber,GISID,MDMID, AdditionalPartyID,IsClientSide, CountryCode)                              
SELECT DISTINCT  GUPEntityName, 'GUP of '+ APR.Role , '' AS Country, GUPDUNSNumber, GUPGISID , '', AP.AdditionalPartyID,           
CS.IsClientSide, ''  --Tejal come back on APG GUP country                              
FROM WF_AdditionalParties AP                               
LEFT JOIN WF_APGRoles APR ON AP.APGRoleID = APR.APGRoleID                                
--LEFT JOIN vwGFIS_CLIENT C ON AP.GUPDUNSNumber = C.DUNS_NUMBER                              
--LEFT JOIN vwCOUNTRY_LIST CL ON C.COUNTRY_CODE = CL.COUNTRY_CODE                             
LEFT JOIN #EntityInfo_P EI ON EI.EntityName = AP.GUPEntityName AND EI.DUNSNumber = AP.GUPDUNSNumber                   
LEFT JOIN #CP_Services CS ON CS.ConflictCheckID = AP.FolderID                   
WHERE FolderType ='Pursuit' AND (ISNULL(AP.GUPDUNSNumber,'') <> '' OR  ISNULL(AP.GUPEntityName,'') <> '')                      
 AND FolderID = @CheckID                            
AND EI.EntityName IS NULL AND EI.DUNSNumber IS NULL                       
 ORDER BY AP.AdditionalPartyID ASC                        
                  
                  
UPDATE A                          
SET A.IsClientSide = 1                         
FROM #EntityInfo_P A JOIN CAU_ClientSideRoles B ON A.[Role] = B.RoleName                        
WHERE A.IsClientSide IS NULL                  
                  
UPDATE A                          
SET A.IsClientSide = 0                         
FROM #EntityInfo_P A                   
WHERE A.IsClientSide IS NULL                  
                  
UPDATE A                  
SET A.IsFinscan = 0                  
FROM #EntityInfo_P A                  
WHERE A.IsClientSide = 1                  
                  
UPDATE A                  
SET A.IsFinscan = 1                  
FROM #EntityInfo_P A JOIN #MainPursuitGrid B ON 1=1 AND B.CountryName = 'United States of America'                  
WHERE A.Role LIKE '%Main Client'                  
                  
UPDATE A                  
SET A.IsFinscan = 1                  
FROM #EntityInfo_P A JOIN #MainPursuitGrid B ON 1=1 AND B.Region = 'Oceania'                  
WHERE A.Role LIKE '%Main Client'                  
              
UPDATE A                  
SET A.IsFinscan = 1                  
FROM #EntityInfo_P A JOIN #MainPursuitGrid B ON 1=1 AND B.CountryName = 'United States of America'                  
WHERE A.IsClientSide = 1                  
                  
UPDATE A                  
SET A.IsFinscan = 1                  
FROM #EntityInfo_P A JOIN #MainPursuitGrid B ON 1=1 AND B.Region = 'Oceania'                  
WHERE A.IsClientSide = 1                 
                
UPDATE A                
SET PerformResearch = 0                
FROM #EntityInfo_P A                
JOIN CAU_APGRolesToBlockResearch B ON A.Role = B.RoleName                 
WHERE A.IsClientSide = 0                
                
UPDATE A                
SET PerformResearch = 0                
FROM #EntityInfo_P A                
JOIN CAU_APGRolesToBlockResearch B ON A.Role = 'GUP of '+ B.RoleName                 
WHERE A.IsClientSide = 0                
              
UPDATE A                
SET Country = NULL                
FROM #EntityInfo_P A               
        WHERE Country = ''              
 --select * from #EntityInfo_P order by AdditionalPartyID,1                        
        
DROP TABLE IF EXISTS #EntityInfo_P_Final             
SELECT MIN(OrderNo) as OrderNo, t.EntityName,             
  STUFF((SELECT DISTINCT ',' + Role FROM #EntityInfo_P EIP             
 WHERE EIP.EntityName= t.EntityName AND EIP.DUNSNumber = t.DUNSNumber and EIP.MDMID =t.MDMID          
 and EIP.GISID = t.GISID FOR XML PATH (''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '') as Role,        
 t.Country AS Country,            
  t.DUNSNumber, t.GISID, t.MDMID, MIN(AdditionalPartyID) as AdditionalPartyID,           
STUFF((SELECT DISTINCT ',' + AdditionalComments FROM #EntityInfo_P EIP             
 WHERE EIP.EntityName= t.EntityName AND EIP.DUNSNumber = t.DUNSNumber and EIP.MDMID =t.MDMID          
 and EIP.GISID = t.GISID FOR XML PATH (''), TYPE      
        ).value('.', 'VARCHAR(MAX)'), 1, 1, ''      
    ) as AdditionalComments,            
  MAX(CASE WHEN IsFinscan=1 THEN 1 ELSE 0 END) IsFinscan, MIN(CASE WHEN IsClientSide=1 THEN 1 ELSE 0 END) IsClientSide,               
  MAX(CASE WHEN PerformResearch=1 THEN 1 ELSE 0 END) PerformResearch ,          
  t.CountryCode AS CountryCode          
 INTO #EntityInfo_P_Final        
FROM #EntityInfo_P t               
GROUP BY t.EntityName, t.DUNSNumber, t.GISID, t.MDMID, t.Country, t.CountryCode              
ORDER BY 8,1              
        
UPDATE A        
SET A.Role = B.Role        
FROM #EntityInfo_P_Final A JOIN #EntityInfo_P B        
ON A.OrderNo = B.OrderNo WHERE ISNULL(A.role,'') = ''        
        
--UPDATE A        
--SET A.Country = B.Country, A.CountryCode = B.CountryCode        
--FROM #EntityInfo_P_Final A JOIN #EntityInfo_P B        
--ON A.OrderNo = B.OrderNo WHERE ISNULL(A.Country,'') = ''        
        
UPDATE A        
SET A.AdditionalComments = B.AdditionalComments        
FROM #EntityInfo_P_Final A JOIN #EntityInfo_P B        
ON A.OrderNo = B.OrderNo WHERE ISNULL(A.AdditionalComments,'') = ''        
        
UPDATE A        
SET A.DUNSNumber = B.DUNSNumber        
FROM #EntityInfo_P_Final A JOIN #EntityInfo_P B        
ON A.OrderNo = B.OrderNo WHERE ISNULL(A.DUNSNumber,'') = ''        
        
UPDATE A        
SET A.GISID = B.GISID        
FROM #EntityInfo_P_Final A JOIN #EntityInfo_P B        
ON A.OrderNo = B.OrderNo WHERE ISNULL(A.GISID,'') = ''        
        
UPDATE A        
SET A.MDMID = B.MDMID        
FROM #EntityInfo_P_Final A JOIN #EntityInfo_P B        
ON A.OrderNo = B.OrderNo WHERE ISNULL(A.MDMID,'') = ''        
        
SELECT * FROM #EntityInfo_P_Final order by AdditionalPartyID,1        
                              
--Questionnaire                              
DROP TABLE IF EXISTS #PursuitQuestionnaire                               
CREATE TABLE #PursuitQuestionnaire                              
(                              
  QuestionNumber NVARCHAR(40),                              
  Answer NVARCHAR(MAX),                              
  Explanation NVARCHAR(MAX),                              
  AssessmentID BIGINT,                              
  ConflictcheckID BIGINT                              
)                              
                              
INSERT INTO #PursuitQuestionnaire                              
SELECT '597835' as QuestionNumber, CASE WHEN HostileQuestion = '0' THEN 'No' WHEN HostileQuestion = '1' THEN 'Yes' ELSE 'N/A' END as Answer,                               
'N/A' as Explanation, 0 as AssessmentID, ConflictCheckID FROM WF_ConflictChecks WHERE ConflictcheckID =@CheckID   
UNION  
SELECT '999' as QuestionNumber, CASE WHEN AuctionQuestion = '0' THEN 'No' WHEN AuctionQuestion = '1' THEN 'Yes' ELSE 'N/A' END as Answer,                               
'N/A' as Explanation, 0 as AssessmentID, ConflictCheckID FROM WF_ConflictChecks WHERE ConflictcheckID =@CheckID                              
                              
DROP TABLE IF EXISTS #PursuitTitle                              
  SELECT Title, ISNULL(Answer, 'N/A') AS Answer, ISNULL(Explanation, 'N/A') as Explanation, AssessmentID,A.QuestionNumber INTO #PursuitTitle                              
 FROM CAU_Questions A  JOIN #PursuitQuestionnaire B ON A.QuestionNumber = B.QuestionNumber                              
                              
 SELECT DISTINCT A.Title, ISNULL(B.Answer, 'N/A') AS Answer, ISNULL(B.Explanation, 'N/A') as Explanation, B.QuestionNumber,AssessmentID, @CheckID                              
 FROM CAU_Questions A LEFT JOIN #PursuitTitle B ON A.Title = B.Title                              
                              
END 