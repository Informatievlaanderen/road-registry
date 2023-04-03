 #!/usr/bin/env bash
# /opt/mssql-tools/bin/sqlcmd -S 'tcp:mssql,1433' -U SA -P 'E@syP@ssw0rd' -l 65534 -Q 'DROP DATABASE [road-registry-events];' 
# /opt/mssql-tools/bin/sqlcmd -S 'tcp:mssql,1433' -U SA -P 'E@syP@ssw0rd' -l 65534 -Q 'DROP DATABASE [road-registry];' 
/opt/mssql-tools/bin/sqlcmd -S 'tcp:mssql,1433' -U SA -P 'E@syP@ssw0rd' -l 65534 -Q 'RESTORE DATABASE [road-registry-events] FROM DISK = "/var/lib/backup/road-registry-events.bak"'
/opt/mssql-tools/bin/sqlcmd -S 'tcp:mssql,1433' -U SA -P 'E@syP@ssw0rd' -l 65534 -Q 'RESTORE DATABASE [road-registry] FROM DISK = "/var/lib/backup/road-registry.bak"'