
INSERT INTO CAU_NonClientSideSORT
SELECT NULL, 'Corporate Sell Side – Due Diligence' UNION 
SELECT NULL, 'Corporate Sell Side – other (non-DD) services' UNION 
SELECT NULL, 'Transaction Law - Corporate Sell Side' UNION 
SELECT NULL, 'Transaction Law - Private Equity Sell Side' UNION 
SELECT NULL, 'Private Capital Sell Side' UNION 
SELECT NULL, 'Sell-side/vendor Commercial diligence' UNION 
SELECT NULL, 'Sell-Side M&A Advisory Services' UNION 
SELECT NULL, 'Sell-Side Carve-Out Operational Advice' UNION 
SELECT NULL, 'Debt / Equity Offering Support' UNION 
SELECT NULL, 'Exit Readiness Services' UNION 
SELECT NULL, 'Pre- IPO Readiness' UNION 
SELECT NULL, 'Seller Advisory' UNION 
SELECT NULL, 'Seller Assistance' UNION 
SELECT NULL, 'Seller Due Diligence Reporting' UNION 
SELECT NULL, 'Transaction Law - Bankruptcy Restructuring' UNION 
SELECT NULL, 'Transaction Law - Non-Bankruptcy Restructuring (advice to distressed company)' UNION 
SELECT NULL, 'Transaction Law - Non-Bankruptcy Restructuring (advice to distressed company)' UNION 
SELECT NULL, 'Transaction Law - Refinancing Services' UNION 
SELECT NULL, 'Sell-side/vendor Commercial diligence' UNION 
SELECT NULL, 'Capital Markets Lead Advisory Services' UNION 
SELECT NULL, 'Capital&debt advisory' UNION 
SELECT NULL, 'Sell-Side M&A Advisory Services ' UNION 
SELECT NULL, 'New Market Investment Strategy' UNION 
SELECT NULL, 'Creditor Advisory' UNION 
SELECT NULL, 'Distressed Corporate Advisory' UNION 
SELECT NULL, 'Distressed Supplier Advisory' UNION 
SELECT NULL, 'Formal Insolvency' UNION 
SELECT NULL, 'Legal Entity Rationalization' UNION 
SELECT NULL, 'Operational Assessment and Improvement' UNION 
SELECT NULL, 'Non-Bankruptcy Restructuring (advice to creditors)' UNION 
SELECT NULL, 'Transaction Law - Non-Bankruptcy Restructuring (advice to creditors)' UNION 
SELECT NULL, 'Anti-fraud Assessment' UNION 
SELECT NULL, 'Anti-fraud Improvement Services' UNION 
SELECT NULL, 'Discovery ' UNION 
SELECT NULL, 'Dispute Services' UNION 
SELECT NULL, 'Fact-Based Forensic IT Data Analytics (Retrospective)' UNION 
SELECT NULL, 'Fact-Based IT Forensic Investigations' UNION 
SELECT NULL, 'Forensic Contract Risk Services' UNION 
SELECT NULL, 'Forensic Data Analytics' UNION 
SELECT NULL, 'Insurance Claims Services - Channel 1' UNION 
SELECT NULL, 'Insurance Claims Services - Channel 2' UNION 
SELECT NULL, 'Investigations' UNION 
SELECT NULL, 'Whistleblower Program Advisory Services' UNION 
SELECT NULL, 'Whistleblower Program Services (Operate)' UNION 
SELECT NULL, 'Third Party Diligence and Integrity Assessments' UNION 

select distinct ServiceCode, SORTServiceName from WF_ServicesConfiguration where SortServiceName like '%Restructuring%'

UNION

select distinct ServiceCode, SORTServiceName from WF_ServicesConfiguration where SortServiceName like '%Sell%'

----------------------------------------------------------------
DROP TABLE CAU_NonClientSideSORT_ServiceIDs
CREATE TABLE CAU_NonClientSideSORT_ServiceIDs
(ServiceID NVARCHAR(540)
,IsFiltered BIT DEFAULT 0)

INSERT INTO CAU_NonClientSideSORT_ServiceIDs(ServiceID)
SELECT '1069' UNION
SELECT '11212' UNION
SELECT '1046' UNION
SELECT '17474' UNION
SELECT '11208' UNION
SELECT '11175' UNION
SELECT '1027' UNION
SELECT '1023' UNION
SELECT '1025' UNION
SELECT '1022' UNION
SELECT '1018' UNION
SELECT '11692' UNION
SELECT '11746' UNION
SELECT '11213' UNION
SELECT '11747' UNION
SELECT '11177' UNION
SELECT '17456' UNION
SELECT '13719' UNION
SELECT '13718' UNION
SELECT '13720' UNION
SELECT '1034' UNION
SELECT '11196' UNION
SELECT '11513' UNION
SELECT '1033' UNION
SELECT '11124' UNION
SELECT '11127' UNION
SELECT '11129' UNION
SELECT '1030' UNION
SELECT '1032' UNION
SELECT '1067' UNION
SELECT '1019' UNION
SELECT '1065' UNION
SELECT '11176' UNION
SELECT '11512' UNION
SELECT '13841' UNION
SELECT '11184' UNION
SELECT '11802' UNION
SELECT '1021' UNION
SELECT '1020' UNION
SELECT '950' UNION
SELECT '11178' UNION
SELECT '11261' UNION
SELECT '1041' UNION
SELECT '11201' UNION
SELECT '11200' UNION
SELECT '17413' UNION
SELECT '11130' UNION
SELECT '17605' UNION
SELECT '11262' UNION
SELECT '1024' UNION
SELECT '11745' UNION
SELECT '11748' UNION
SELECT '11749' UNION
SELECT '11750' UNION
SELECT '11511' UNION
SELECT '11170' UNION
SELECT '11174' UNION
SELECT '11162' UNION
SELECT '1068' UNION
SELECT '11514' UNION
SELECT '11150' UNION
SELECT '11149' UNION
SELECT '11360' UNION
SELECT '11152' UNION
SELECT '17606' UNION
SELECT '11190' UNION
SELECT '1064' UNION
SELECT '1035' UNION
SELECT '1036' 

GO

UPDATE CAU_NonClientSideSORT_ServiceIDs
SET IsFiltered = 1 
WHERE ServiceID in ('11177','11127','11129','11176','11178','1027')
GO