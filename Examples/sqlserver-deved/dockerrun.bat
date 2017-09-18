docker run -d -p 1433:1433 -e sa_password=Password1! -e ACCEPT_EULA=Y -v C:/temp/:C:/temp/ -e attach_dbs="[{'dbName':'PlantOrders','dbFiles':['C:\\temp\\PlantOrders.mdf','C:\\temp\\PlantOrders_log.ldf']}]" --name sqlsvr microsoft/mssql-server-windows-developer

