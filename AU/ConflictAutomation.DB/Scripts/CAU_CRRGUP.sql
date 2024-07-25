DROP TABLE IF EXISTS CAU_FilteredServiceCodes
CREATE TABLE CAU_FilteredServiceCodes
(
    ServiceCode NVARCHAR(100) NULL,
	SORTServiceName NVARCHAR(1000) NULL
)

INSERT INTO CAU_FilteredServiceCodes(SORTServiceName)
SELECT 'Distressed Corporate Advisory' UNION
SELECT 'Distressed Supplier Advisory' UNION
SELECT 'Formal insolvency' UNION
SELECT 'Legal Entity Rationalization' UNION
SELECT 'Distressed Operational Restructuring' UNION
SELECT 'Creditor advisory' UNION
SELECT 'Chief Restructuring Officer (CRO)'