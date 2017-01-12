#!/bin/bash
if test "$OS" = "Windows_NT"
then
  # use Windows .NET FW
  .paket/paket.bootstrapper.exe
  exit_code=$?
  if [ $exit_code -ne 0 ]; then
  	exit $exit_code
  fi

  .paket/paket.exe restore
  exit_code=$?
  if [ $exit_code -ne 0 ]; then
  	exit $exit_code
  fi
  
  packages/build/FAKE/tools/FAKE.exe $@ --fsiargs build.fsx 
else
  # use dotnet
  export DOTNET_SDK_URL="https://go.microsoft.com/fwlink/?LinkID=809129"
  export DOTNET_INSTALL_DIR="$PWD/.dotnetcli"
  mkdir $DOTNET_INSTALL_DIR
  curl -L $DOTNET_SDK_URL -o dotnet_package
  tar -xvzf dotnet_package -C $DOTNET_INSTALL_DIR
  export PATH="$DOTNET_INSTALL_DIR:$PATH"

  dotnet .paket/paket.bootstrapper.exe
  exit_code=$?
  if [ $exit_code -ne 0 ]; then
  	exit $exit_code
  fi

  dotnet .paket/paket.exe restore
  exit_code=$?
  if [ $exit_code -ne 0 ]; then
  	exit $exit_code
  fi

  dotnet packages/build/FAKE/tools/FAKE.exe $@ --fsiargs build.fsx 
fi