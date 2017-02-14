REM Builds the project.

pushd Toolhouse.Monitoring

msbuild || (popd && exit /B 101)

popd
