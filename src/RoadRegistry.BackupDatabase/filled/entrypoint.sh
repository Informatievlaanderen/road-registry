 #!/usr/bin/env bash
 /opt/mssql-tools/bin/sqlcmd -S 'tcp:mssql,1433' -U SA -P 'E@syP@ssw0rd' -l 65534 -Q 'RESTORE DATABASE [road-registry-events] FROM DISK = "/var/lib/backup/database_backup.bak" WITH MOVE "road-registry-events-schemacopy-prod" TO "/var/opt/mssql/data/RoadRegistry1.mdf", MOVE "road-registry-events-schemacopy-prod_log" TO "/var/opt/mssql/data/RoadRegistry.ldf"' \
 && /opt/mssql-tools/bin/sqlcmd -S 'tcp:mssql,1433' -U SA -P 'E@syP@ssw0rd' -l 65534 -Q 'DROP DATABASE [RoadRegistry];' \
 && /opt/mssql-tools/bin/sqlcmd -S 'tcp:mssql,1433' -U SA -P 'E@syP@ssw0rd' -l 65534 -Q 'ALTER DATABASE [road-registry-events] MODIFY NAME = [RoadRegistry];' \
