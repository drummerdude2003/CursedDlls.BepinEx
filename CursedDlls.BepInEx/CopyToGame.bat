@echo off
:: For people that want to modify how this works, the first parameter is the path to the dll, and the second is the path inside BepInEx (ex "plugins\CursedDll\")

:: TO FIX THE BEPINEX PATH:
:: Change this to your BepInEx path, and remember the trailing slash!
set bepinpath=C:\Program Files (x86)\Steam\steamapps\common\H3VR\BepInEx\

echo Trying to copy to %bepinpath%%2
IF exist "%bepinpath%" (
	IF NOT exist "%bepinpath%%2" ( mkdir "%bepinpath%%2" )
	IF %ERRORLEVEL% NEQ 0 (
		echo mkdir error code %ERRORLEVEL%
		exit /b %ERRORLEVEL%
	)
	copy /y %1 "%bepinpath%%2"
) ELSE ( echo Could not find the BepInEx folder, did you set your CopyToGame.bat correctly? )