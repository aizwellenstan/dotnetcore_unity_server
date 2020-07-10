#!/bin/bash

cd Publish/osx-x64

dotnet App.dll --appId=1 --appType=AllServer --config=../../Config/StartConfig/LocalAllServer.txt --configOther=../../Config/{0}.txt

