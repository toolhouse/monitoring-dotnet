REM Builds the project's nuget package.
@echo off

call build.cmd || exit /B 101

rmdir /S /Q Dist
mkdir Dist

pushd Toolhouse.Monitoring
nuget pack ^
    -OutputDirectory ..\Dist ^
    -Prop Configuration=Release ^
    Toolhouse.Monitoring.csproj ^
    || (popd && exit /B 201)

pushd Dist
for %%f in (*.nupkg) do (
    set pkg=%%f
) || (popd && exit /B 301)

echo Package built. To push to nuget, run the following:
echo nuget push Dist\%pkg% -Source https://www.nuget.org/api/v2/package
popd
