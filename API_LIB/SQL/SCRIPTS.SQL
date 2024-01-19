/*
select * from API_PROF_USERS
--DELETE FROM API_PROF_USERS

insert into API_PROF_USERS 
Select 
'ADMIN',
CAST('' AS varbinary(max)),
'ADMINISTRADOR','','I-TEC','','demo@demo.cl',1

--update API_PROF_USERS set PW = EncryptByPassPhrase('6D25788D423D4773B00A48EEE2617C77', 'itec.2020' ) where USERNAME = 'ADMIN          '
--select convert(varchar,DecryptByPassPhrase('6D25788D423D4773B00A48EEE2617C77',PW))from API_PROF_USERS

EXEC SP_VAL_API_PROF_USERS_LOGIN '6D25788D423D4773B00A48EEE2617C77','ADMIN','itec.2020'
*/




