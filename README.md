# SqlDependency with C#
## Database instalation script:

```
USE TestBase;
GO
```

Create the table that will have the sql dependency running:
```
CREATE TABLE [dbo].[tbLastLineChange](
	[ChangeId] [int] IDENTITY(1,1) NOT NULL,
	[GameNum] [int] NULL,
	[PeriodNumber] [int] NULL,
	[Store] [int] NULL
) ON [PRIMARY]
GO
```

Creates the queue for sql depedency
```
CREATE QUEUE SQLDependencyQueue;
GO
```

 Creates the Broker service used on the queue
```
CREATE SERVICE SQLDependencyService ON QUEUE SQLDependencyQueue; 
GO
```

Enables Broker to be use in the database
```
ALTER DATABASE TestBase SET ENABLE_BROKER with rollback immediate;
GO
```


create user for schema ownership
```
CREATE USER SqlDependencySchemaOwner WITHOUT LOGIN;
GO
```

create schema for SqlDependency ojbects
```
CREATE SCHEMA SqlDependency AUTHORIZATION SqlDependencySchemaOwner;
GO
```

set the default schema of minimally privileged user to SqlDependency
```
ALTER USER TestBaseUser WITH DEFAULT_SCHEMA = SqlDependency;
```

grant user control permissions on SqlDependency schema
```
GRANT CONTROL ON SCHEMA::SqlDependency TO TestBaseUser;
```

grant user impersonate permissions on SqlDependency schema owner
```
GRANT IMPERSONATE ON USER::SqlDependencySchemaOwner TO TestBaseUser;
GO
```

grant database permissions needed to create and use SqlDependency objects
```
GRANT CREATE PROCEDURE TO TestBaseUser;
GRANT CREATE QUEUE TO TestBaseUser;
GRANT CREATE SERVICE TO TestBaseUser;
GRANT REFERENCES ON
    CONTRACT::[http://schemas.microsoft.com/SQL/Notifications/PostQueryNotification] TO TestBaseUser;
GRANT VIEW DEFINITION TO TestBaseUser;
GRANT SELECT to TestBaseUser;
GRANT SUBSCRIBE QUERY NOTIFICATIONS TO TestBaseUser;
GRANT RECEIVE ON QueryNotificationErrorsQueue TO TestBaseUser;
```
