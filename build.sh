#!/bin/bash

if test `uname` = Darwin; then
    cachedir=~/Library/Caches/KBuild
else
    if [ -z $XDG_DATA_HOME ]; then
        cachedir=$HOME/.local/share
    else
        cachedir=$XDG_DATA_HOME;
    fi
fi
mkdir -p $cachedir

url=https://www.nuget.org/nuget.exe

if test ! -f $cachedir/nuget.exe; then
    wget -O $cachedir/nuget.exe $url 2>/dev/null || curl -o $cachedir/nuget.exe --location $url /dev/null
fi

if test ! -e .nuget; then
    mkdir .nuget
    cp $cachedir/nuget.exe .nuget/nuget.exe
fi

if test ! -d packages/KoreBuild; then
    mono .nuget/nuget.exe install KoreBuild -ExcludeVersion -o packages -nocache -pre
    mono .nuget/nuget.exe install Sake -version 0.2 -o packages -ExcludeVersion
fi

if ! type k > /dev/null 2>&1; then
    source packages/KoreBuild/build/kvm.sh
fi

if ! type k > /dev/null 2>&1; then
    kvm upgrade
fi

mono packages/Sake/tools/Sake.exe -I packages/KoreBuild/build -f makefile.shade "$@"
