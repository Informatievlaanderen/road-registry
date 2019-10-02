 #!/usr/bin/env bash
/opt/mssql/bin/sqlservr & \
/opt/mssql-tools/bin/sqlcmd -S 'tcp:localhost,1433' -U SA -P 'E@syP@ssw0rd' -Q 'RESTORE DATABASE LegacyRoadRegistry FROM DISK = "/var/lib/backup/legacydb.bak" WITH MOVE "MRBWegen_01" TO "/var/opt/mssql/data/LegacyRoadRegistry1.mdf", MOVE "MRBWegen_02" TO "/var/opt/mssql/data/LegacyRoadRegistry2.ndf", MOVE "MRBWegen_03" TO "/var/opt/mssql/data/LegacyRoadRegistry3.ndf", MOVE "MRBWegen_04" TO "/var/opt/mssql/data/LegacyRoadRegistry4.ndf", MOVE "MRBWegen_log" TO "/var/opt/mssql/data/LegacyRoadRegistry.ldf"' & \
tail -f /dev/null
