#!/bin/bash
# Start SQL Server in the background
/opt/mssql/bin/sqlservr &

# Start the import script
/usr/src/app/import-data.sh &

# Wait for the SQL Server process to exit
wait
