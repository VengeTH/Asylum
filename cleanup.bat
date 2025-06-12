@echo off
REM Remove large files from git history

echo Removing libburst-llvm-18.dylib from history...
git filter-branch --force --index-filter "git rm --cached --ignore-unmatch Assets/Scripts/Library/PackageCache/com.unity.burst@59eb6f11d242/.Runtime/libburst-llvm-18.dylib" --prune-empty --tag-name-filter cat -- --all

echo Removing libburst-llvm-19.dylib from history...
git filter-branch --force --index-filter "git rm --cached --ignore-unmatch Assets/Scripts/Library/PackageCache/com.unity.burst@59eb6f11d242/.Runtime/libburst-llvm-19.dylib" --prune-empty --tag-name-filter cat -- --all

REM Clean up and repack the repo
echo Cleaning up git history...
git reflog expire --expire=now --all
git gc --prune=now --aggressive

REM Force push to GitHub
echo Force pushing to GitHub...
git push --force

echo.
echo All done! If you see any errors about other large files, repeat the filter-branch step for those files.
pause