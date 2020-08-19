#!/bin/bash
#/*******************************************************************************
# *Author:  Kien Truong
# *Program: Flower
#******************************************************************************/

echo First remove old binary files
rm *.dll
rm *.exe

echo View the list of source files
ls -l

echo Compile Flower.cs to create the file: Flower.dll
mcs -target:library -r:System.Drawing.dll -r:System.Windows.Forms.dll -out:Flower.dll Flower.cs

echo Link the previously created dll file to create an executable file.
mcs -r:System -r:System.Windows.Forms -r:Flower.dll -out:FlowerApp.exe main.cs

echo View the list of files in the current folder
ls -l

echo Run the Flower program.
./FlowerApp.exe

echo The script has terminated.
