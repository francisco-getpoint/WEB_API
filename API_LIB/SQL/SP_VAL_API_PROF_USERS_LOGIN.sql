DROP	PROCEDURE	SP_VAL_API_PROF_USERS_LOGIN
go
CREATE PROCEDURE	SP_VAL_API_PROF_USERS_LOGIN
@PHRASE	CHAR(36)
,@USER	CHAR(15)
,@PW	CHAR(15)
AS
BEGIN
DECLARE
@AUX_PASSWORD CHAR(15)
,@AUX_PHASH VARCHAR(MAX)
,@AUX_RESPUESTA TINYINT

Select @AUX_RESPUESTA = 0


	Select	@AUX_PHASH = PW
	From	API_PROF_USERS (NOLOCK)
	Where	1 = 1 
	And		USERNAME	=	@USER
	if		@@ROWCOUNT = 0
	Begin
		goto SP_EXIT
	End

	IF RTRIM(CONVERT(CHAR(15),DECRYPTBYPASSPHRASE(RTRIM(@PHRASE),@AUX_PHASH))) = RTRIM(@PW)
	Begin
		Select @AUX_RESPUESTA = 1
	End

	SP_EXIT:
	Begin
		Select @AUX_RESPUESTA As RESPUESTA,UPPER(@USER) AS USERNAME
	End

END

