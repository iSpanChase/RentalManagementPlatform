ALTER TABLE [USER]
ADD is_operator_pending BIT NOT NULL CONSTRAINT DF_USERS_is_operator_pending DEFAULT(0);