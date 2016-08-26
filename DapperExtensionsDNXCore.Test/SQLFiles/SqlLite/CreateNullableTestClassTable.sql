﻿CREATE TABLE NullableTestClass (
	Id INTEGER PRIMARY KEY,
	StringVal NVARCHAR(100) NOT NULL,
	StringVal2 NVARCHAR(100),
	IntVal INTEGER NOT NULL,
	NullableIntVal INTEGER,
	DTVal NVARCHAR(100),
	NullableDTVal NVARCHAR(100) NULL,
	EnumVal NVARCHAR(20),
	NullableEnumVal NVARCHAR(20) NULL
)