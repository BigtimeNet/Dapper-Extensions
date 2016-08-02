IF (OBJECT_ID('Company') IS NOT NULL)
BEGIN
    DROP TABLE Company
END

CREATE TABLE Company (
	Id bigint NOT NULL IDENTITY(1,1),
	CompanyNm varchar(250) NULL,
	City varchar(250) NULL,
	St varchar(250) NULL,
	Zip varchar(250) NULL
)
