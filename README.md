# QueryDesk

Application to maintain and execute useful queries for other application's databases.

Currently you can connect to and query MySQL, MSSQL and SQLite databases.

If you don't want to build QueryDesk yourself, you can download a setup (unsigned) from http://gdksoftware.nl/QueryDesk/setup.exe

# Features
* SQL Syntax highlighting
* Autocomplete tables and fields (using ctrl+space)
* Parameterization of queries to be filled in when running the queries
* Feeding query results to a HTTP URL

## How to build

With Visual Studio 2015, building the QueryDesk project will automatically download the required libraries without any configuration.

## Configuration

### Local use

* By default QueryDesk uses an SQLite database that needs no setup.

### Shared/remote use

To get the most out of the QueryDesk application, especially in work environments where you can share your queries among your coworkers, you can store your various database settings and queries in a MySQL database.

* Create a centralized (non-public *) MySQL database and create the tables by running the script in `QueryDesk/create_appdb.sql`
* Edit QueryDesk.exe.config at the section appSettings and remove the tag containing the key connectionsqlite
* Add a new tag \<add key="connection" value="Server=127.0.0.1;Database=querydesk;Uid=querydesk;Pwd=magicpassword;" /\>

### Libraries used

_Uses some icons from the MICROSOFT VISUAL STUDIO 2012 IMAGE LIBRARY (http://www.microsoft.com/en-us/download/details.aspx?id=35825)_

* ICSharpCode.AvalonEdit
* Mysql.Data
* System.Data.SQLite
