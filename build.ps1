# Initialises the build system (which declares three global build functions, build, clean and rebuild) then starts a build.
# The real work of the build system is defined in .build\build.ps1 and .build\_init.ps1.
& "$PsScriptRoot\.build\_init.ps1"
Rebuild
