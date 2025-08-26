@echo off
setlocal
set THIS_DIR=%~dp0
set DOCS_FOLDER=%THIS_DIR%output\docs
set DOCFXJSON=%THIS_DIR%..\docs\docfx.json

if "%1"=="full" (
    if exist "%DOCS_FOLDER%" (
        echo Removing old docs folder "%DOCS_FOLDER%"
        rmdir /s /q "%DOCS_FOLDER%"
        if exist "%DOCS_FOLDER%" (
            echo Failed to remove old docs folder "%DOCS_FOLDER%"
            exit /b 1
        )
    )

    echo Extracting metadata from source code...
    docfx metadata "%DOCFXJSON%"
)
echo Building documentation and serving it on http...
docfx build "%DOCFXJSON%" --serve
