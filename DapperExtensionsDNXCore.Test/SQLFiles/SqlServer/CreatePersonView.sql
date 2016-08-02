IF (OBJECT_ID('PersonView') IS NOT NULL)
BEGIN
    DROP VIEW PersonView
END;

EXEC('CREATE VIEW PersonView AS 
	SELECT Person.Id, Person.FirstName, Person.LastName, Person.DateCreated, Person.Active, Person.CompanyId, Company.CompanyNm 
		FROM Person 
			LEFT OUTER JOIN Company ON Person.CompanyId=Company.Id')
