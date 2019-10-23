#!/usr/bin/env bash
set -e

locale-gen nl_BE.UTF-8
PAKET_EXE=.paket/paket.exe
FAKE_EXE=packages/FAKE/tools/FAKE.exe

run() {
  if [ "$OS" != "Windows_NT" ]
  then
    mono "$@"
  else
    "$@"
  fi
}

run $PAKET_EXE restore
run $FAKE_EXE build.fsx "$@"
