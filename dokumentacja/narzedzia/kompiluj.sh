#!/bin/sh
# Kompiluje dokumentację w kontenerze TeX Live (uruchamiać z katalogu dokumentacja/).
exec podman run --rm -v "$PWD":/w:Z -w /w docker.io/texlive/texlive:latest-small sh -c '
  tlmgr update --self >/dev/null 2>&1
  tlmgr install courier helvetic >/dev/null 2>&1
  for i in 1 2 3; do pdflatex -interaction=nonstopmode -halt-on-error dokumentacja.tex || exit 1; done'
