 #!/usr/bin/env bash
/opt/mssql/bin/sqlservr &
MSSQL_PID=$!
/opt/mssql-tools/bin/sqlcmd -S 'tcp:localhost,1433' -U SA -P 'E@syP@ssw0rd' -Q 'IF NOT EXISTS (SELECT * FROM [SYS].[DATABASES] WHERE [Name] = N'\''RoadRegistry'\'') BEGIN CREATE DATABASE [RoadRegistry] END;'
wait $MSSQL_PID
