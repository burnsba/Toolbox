CREATE LOGIN [domain\user] FROM WINDOWS

EXEC sp_addsrvrolemember @loginame='domain\user', @rolename = 'sysadmin'

-- more info http://blogs.ameriteach.com/chris-randall/2009/12/11/sql-server-2008-forgot-to-add-an-administrator-account.html
