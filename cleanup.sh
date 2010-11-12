#!/bin/sh
#Removes compilation artifacts and other temporary files.
cd ${0%%/*}

#Clear binaries (leave Documentation and Portable intact)
rm -rf build/Backend
rm -rf build/Frontend
rm -rf build/Tools
rm -rf build/Setup

#Clear object cache
rm -rf vs/Central.Cli/obj
rm -rf vs/Central.WinForms/obj
rm -rf vs/Central.Wpf/obj
rm -rf vs/Central.Gtk/obj
rm -rf vs/Injector.Cli/obj
rm -rf vs/Injector.WinForms/obj
rm -rf vs/Store.Service/obj
rm -rf vs/Store.Management.Cli/obj
rm -rf vs/Store.Management.WinForms/obj
rm -rf vs/Common/obj
rm -rf vs/Common.Wpf/obj
rm -rf vs/Common.Gtk/obj
rm -rf vs/DownloadBroker/obj
rm -rf vs/Injector/obj
rm -rf vs/Model/obj
rm -rf vs/MyApps/obj
rm -rf vs/Store/obj
rm -rf vs/Test.Common/obj
rm -rf vs/Test.Backend/obj
rm -rf vs/Test.Frontend/obj
rm -rf vs/Test.Tools/obj
rm -rf vs/Publish/obj
rm -rf vs/Publish.Cli/obj
rm -rf vs/Publish.WinForms/obj
rm -rf vs/Modeling/obj
