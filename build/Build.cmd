@echo off
setlocal
pushd %~dp0

cd /d "%THIS_DIR%..\src"

echo.
echo Build project as though it is from GitHub workflows, to test nupkg
::dotnet pack --configuration Release -p:NoWarn=1591 || goto :error
msbuild -noLogo -t:Clean;Pack -restore -p:Configuration=Release LpSolveDotNet.sln || goto :error

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
