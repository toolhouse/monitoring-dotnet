REM Builds the project.

nuget restore

pushd Toolhouse.Monitoring

msbuild ^
    /t:Clean,Build ^
    /p:Configuration=Release ^
    || (popd && exit /B 101)

popd
