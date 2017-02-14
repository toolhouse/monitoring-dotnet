REM Builds the project's nuget package.

call build.cmd || exit /B 101

mkdir Dist

pushd Toolhouse.Monitoring
nuget pack -OutputDirectory ..\Dist Toolhouse.Monitoring.csproj || (popd && exit /B 201)

popd
