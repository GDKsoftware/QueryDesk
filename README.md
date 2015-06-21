# QueryDesk

Application to maintain and execute useful queries for other application's databases.

Currently you can connect to and query MySQL and MSSQL databases.

Taskboard should be accessible using https://huboard.com/GDKsoftware/QueryDesk

Uses icons from the MICROSOFT VISUAL STUDIO 2012 IMAGE LIBRARY (http://www.microsoft.com/en-us/download/details.aspx?id=35825)

## HowToBuild

To get the most out of the QueryDesk application, especially in work environments where you can share your queries among your coworkers, you will need to store your various database settings and queries in a local MySQL database.

Steps:

* Create a centralized (non-public *) MySQL database and create the tables by running the script in `QueryDesk/create_appdb.sql`
* Change the App.config to your configuration needs
* Build the project in VS2013 (possible with the Express for Windows Desktop version)

\* passwords will be stored plaintext
