@echo off

if [%1] == [true] GOTO VERIFY 

powershell ./ApplyGPLBoilerplate.ps1

GOTO END

:VERIFY

powershell ./ApplyGPLBoilerplate.ps1 -verifyOnly -verbose
GOTO END


:END

