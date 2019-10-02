 #!/usr/bin/env bash
/opt/mssql/bin/sqlservr & \
/opt/mssql-tools/bin/sqlcmd -S 'tcp:localhost,1433' -U SA -P 'E@syP@ssw0rd' -Q 'CREATE DATABASE [RoadRegistry];ALTER DATABASE [RoadRegistry] SET ALLOW_SNAPSHOT_ISOLATION ON;ALTER DATABASE [RoadRegistry] SET READ_COMMITTED_SNAPSHOT ON' & \
tail -f /dev/null
