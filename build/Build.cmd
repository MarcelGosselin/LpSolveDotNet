@echo off
setlocal
if [%1]==[] (
	echo.
	echo Missing Version Number as 1st parameter
	echo.
	goto :error
)
if /i [%2]==[push] (
	set SHOULD_PUSH=1
) else (
	set SHOULD_PUSH=
)
set THIS_DIR=%~dp0
set OUTPUT_DIR=%THIS_DIR%output
set TARGET_VERSION=%~1

if exist "%OUTPUT_DIR%" (
	echo.
	echo Remove "%OUTPUT_DIR%"
	rmdir /s /q "%OUTPUT_DIR%" || goto :error
)

echo.
echo Create "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%" || goto :error

::TODO update AssemblyVersion number
echo.
echo Build project
msbuild "%THIS_DIR%..\src\LpSolveDotNet\LpSolveDotNet.csproj" /p:Configuration=Release "/p:OutputPath=%OUTPUT_DIR%" || goto :error

echo.
echo Build NuGet package
nuget pack "%THIS_DIR%LpSolveDotNet.nuspec" -Version "%TARGET_VERSION%" -OutputDirectory "%OUTPUT_DIR%" || goto :error

if [%SHOULD_PUSH%]==[1] (
	echo.
	echo Push package to NuGet
	echo nuget push "%OUTPUT_DIR%\LpSolveDotNet.%TARGET_VERSION%.nupkg" -Source nuget.org || goto :error
)
goto :end

:error
exit /b 1

:end
exit /b 0
