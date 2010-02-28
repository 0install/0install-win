[Code]
function ModPathDir(): TArrayOfString;
var
	Dir:	TArrayOfString;
begin
	setArrayLength(Dir, 1)
	Dir[0] := ExpandConstant('{app}');
	Result := Dir;
end;

procedure ModPath();
var
	oldpath:	String;
	newpath:	String;
	pathArr:	TArrayOfString;
	aExecFile:	String;
	aExecArr:	TArrayOfString;
	i, d:		Integer;
	pathdir:	TArrayOfString;
begin

	// Get array of new directories and act on each individually
	pathdir := ModPathDir();
	for d := 0 to GetArrayLength(pathdir)-1 do begin

		// Modify WinNT path
		if UsingWinNT() = true then begin

			// Get current path, split into an array
			RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment', 'Path', oldpath);
			oldpath := oldpath + ';';
			i := 0;
			while (Pos(';', oldpath) > 0) do begin
				SetArrayLength(pathArr, i+1);
				pathArr[i] := Copy(oldpath, 0, Pos(';', oldpath)-1);
				oldpath := Copy(oldpath, Pos(';', oldpath)+1, Length(oldpath));
				i := i + 1;

				// Check if current directory matches app dir
				if pathdir[d] = pathArr[i-1] then begin
					// if uninstalling, remove dir from path
					if IsUninstaller() = true then begin
						continue;
					// if installing, abort because dir was already in path
					end else begin
						abort;
					end;
				end;

				// Add current directory to new path
				if i = 1 then begin
					newpath := pathArr[i-1];
				end else begin
					newpath := newpath + ';' + pathArr[i-1];
				end;
			end;

			// Append app dir to path if not already included
			if IsUninstaller() = false then
				newpath := newpath + ';' + pathdir[d];

			// Write new path
			RegWriteStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment', 'Path', newpath);

		end;

		// Write file to flag modifypath was selected
		//   Workaround since IsTaskSelected() cannot be called at uninstall and AppName and AppId cannot be "read" in Code section
		if IsUninstaller() = false then
			SaveStringToFile(ExpandConstant('{app}') + '\uninsTasks.txt', WizardSelectedTasks(False), False);
	end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
	if CurStep = ssPostInstall then
		if IsTaskSelected('modifypath') then
			ModPath();
end;

function NeedRestart(): Boolean;
begin
	if IsTaskSelected('modifypath') and not UsingWinNT() then begin
		Result := True;
	end else begin
		Result := False;
	end;
end;
