@echo on

dotnet publish .. -c Release -o ..\publish\win-x64 -r win-x64 /p:DefineConstants=WINDOWS /p:NativeLib=Shared /p:SelfContained=true

for %%I in (..\.) do set "ProjectName=%%~nI"

mkdir ..\release\win-x64\obs-plugins\64bit 2>nul
mkdir ..\release\win-x64\data\obs-plugins\%ProjectName%\locale 2>nul
del /F /S /Q ..\release\win-x64\obs-plugins\64bit\*
del /F /S /Q ..\release\win-x64\data\obs-plugins\%ProjectName%\locale\*
copy /Y ..\publish\win-x64\* ..\release\win-x64\obs-plugins\64bit\
copy /Y ..\locale\* ..\release\win-x64\data\obs-plugins\%ProjectName%\locale
rmdir /S /Q ..\publish
pause
