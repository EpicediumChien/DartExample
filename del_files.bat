@echo off

setlocal

SET CurDir=%CD%

PUSHD %~DP0 & cd /d "%~dp0"
%1 %2
mshta vbscript:createobject("shell.application").shellexecute("%~s0","goto :runas","","runas",1)(window.close)&goto :eof
:runas



RD /S /Q "_BIN"
del /Q /F /s "obj"
del /Q /F /s "bin"

endlocal
exit
