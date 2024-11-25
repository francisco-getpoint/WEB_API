DROP	PROCEDURE	SP_SEL_API_PROF_USERS_LOGIN_LEE_PW
go
CREATE PROCEDURE	SP_SEL_API_PROF_USERS_LOGIN_LEE_PW
@PHRASE	CHAR(36)
,@USER	CHAR(15)
AS
BEGIN

DECLARE
@AUX_PHASH VARBINARY(MAX)
,@AUX_RESPUESTA CHAR(15)


	Select	@AUX_PHASH = PW
	From	API_PROF_USERS (NOLOCK)
	Where	1 = 1 
	And		USERNAME	=	@USER


	Select @AUX_RESPUESTA =  RTRIM(CONVERT(CHAR(15),DECRYPTBYPASSPHRASE(RTRIM(@PHRASE),@AUX_PHASH)))
	Select @AUX_RESPUESTA As Password

END

