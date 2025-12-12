#!/bin/bash
# Add mssql-tools to PATH
export PATH="$PATH:/opt/mssql-tools/bin"

# Wait for SQL Server to start
for i in {1..50};
do
    sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q "SELECT 1" > /dev/null 2>&1
    if [ $? -eq 0 ]
    then
        echo "SQL Server is ready."
        break
    else
        echo "Not ready yet..."
        sleep 1
    fi
done

# Run Scripts
echo "Running CreateDatabase.sql..."
sqlcmd -S localhost -U sa -P $SA_PASSWORD -i /usr/src/app/db/CreateDatabase.sql

echo "Running StoredProcedures.sql..."
sqlcmd -S localhost -U sa -P $SA_PASSWORD -i /usr/src/app/db/StoredProcedures.sql

echo "Running UpdateDatabase_VerificationLog.sql..."
sqlcmd -S localhost -U sa -P $SA_PASSWORD -i /usr/src/app/db/UpdateDatabase_VerificationLog.sql

echo "Running UpdateDatabase_CrossSeta.sql..."
sqlcmd -S localhost -U sa -P $SA_PASSWORD -i /usr/src/app/db/UpdateDatabase_CrossSeta.sql

echo "Running UpdateSchema_AddRegistrationFields.sql..."
sqlcmd -S localhost -U sa -P $SA_PASSWORD -i /usr/src/app/db/UpdateSchema_AddRegistrationFields.sql

echo "Running InsertSampleData_CrossSeta.sql..."
sqlcmd -S localhost -U sa -P $SA_PASSWORD -i /usr/src/app/db/InsertSampleData_CrossSeta.sql

echo "Database initialization completed."
