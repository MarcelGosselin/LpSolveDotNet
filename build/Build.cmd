@echo off
setlocal
pushd %~dp0

cd "%THIS_DIR%..\src\LpSolveDotNet"

echo.
echo Build project
::dotnet pack --configuration Release -p:NoWarn=1591 || goto :error
msbuild -t:Clean;Pack -restore || goto :error

::if [%1]==[push] (
::	echo.
::	echo Push package to NuGet
::	nuget push "%OUTPUT_DIR%\LpSolveDotNet.%TARGET_VERSION%.nupkg" -Source https://www.nuget.org || goto :error
::)
goto :end

:error
echo.
echo **********************************
echo Build failed, see above for errors
echo **********************************
echo.
popd
exit /b 1

:end
popd
exit /b 0
