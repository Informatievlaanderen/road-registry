 #!/usr/bin/env bash
/opt/mssql/bin/sqlservr &
MSSQL_PID=$!
/opt/mssql-tools/bin/sqlcmd -S 'tcp:localhost,1433' -U SA -P 'E@syP@ssw0rd' -Q 'IF NOT EXISTS (SELECT * FROM [SYS].[DATABASES] WHERE [Name] = N'\''RoadRegistry'\'') BEGIN CREATE DATABASE [RoadRegistry] END;ALTER DATABASE [RoadRegistry] SET ALLOW_SNAPSHOT_ISOLATION ON;ALTER DATABASE [RoadRegistry] SET READ_COMMITTED_SNAPSHOT ON;'
wait $MSSQL_PID
