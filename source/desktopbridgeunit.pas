unit desktopbridgeunit;

interface

// Experimental Petz 5 bridge. No game pointers ever leave the drawing thread.
// Opt in with DESKTOPPETZ_PIPE=1 before launching the game.
procedure DesktopBridgePublish(const Snapshot: AnsiString);
procedure DesktopBridgeStop;

implementation

uses Windows, SysUtils, Classes, SyncObjs;

type
  TBridgeWorker = class(TThread)
  protected
    procedure Execute; override;
  end;

var
  Gate: TCriticalSection = nil;
  Worker: TBridgeWorker = nil;
  Pending: AnsiString = '';
  HasPending: Boolean = False;
  Stopping: Boolean = False;

procedure TBridgeWorker.Execute;
var
  Pipe: THandle;
  Text: AnsiString;
  Written: DWORD;
  Connected: BOOL;
begin
  while not Terminated do
  begin
    Pipe := CreateNamedPipeA('\\.\pipe\DesktopPetz.State.v1',
      PIPE_ACCESS_OUTBOUND, PIPE_TYPE_BYTE or PIPE_READMODE_BYTE or PIPE_WAIT,
      1, 65536, 65536, 1000, nil);
    if Pipe = INVALID_HANDLE_VALUE then
    begin
      Sleep(500);
      Continue;
    end;
    try
      Connected := ConnectNamedPipe(Pipe, nil);
      if not Connected then Connected := GetLastError = ERROR_PIPE_CONNECTED;
      if not Connected then Continue;
      while not Terminated do
      begin
        Text := '';
        Gate.Enter;
        try
          if HasPending then
          begin
            Text := Pending;
            HasPending := False;
          end;
        finally
          Gate.Leave;
        end;
        if Text = '' then
        begin
          Sleep(50);
          Continue;
        end;
        Text := Text + #10;
        if not WriteFile(Pipe, Text[1], Length(Text), Written, nil) or
           (Written <> DWORD(Length(Text))) then Break;
      end;
    finally
      DisconnectNamedPipe(Pipe);
      CloseHandle(Pipe);
    end;
  end;
end;

procedure DesktopBridgePublish(const Snapshot: AnsiString);
var
  Enabled: array[0..7] of Char;
begin
  if GetEnvironmentVariable('DESKTOPPETZ_PIPE', Enabled, Length(Enabled)) <> 1 then Exit;
  if Enabled[0] <> '1' then Exit;
  if Length(Snapshot) > 60000 then Exit;
  if Gate = nil then
  begin
    Gate := TCriticalSection.Create;
    Worker := TBridgeWorker.Create(False);
    Worker.FreeOnTerminate := False;
  end;
  Gate.Enter;
  try
    if not Stopping then
    begin
      Pending := Snapshot;
      HasPending := True;
    end;
  finally
    Gate.Leave;
  end;
end;

procedure DesktopBridgeStop;
begin
  // The worker may be blocked in ConnectNamedPipe or WriteFile.
  // Do not block the host process on shutdown; terminate on process exit.
  if Gate <> nil then
  begin
    Gate.Enter;
    try
      Stopping := True;
      Pending := '';
      HasPending := False;
    finally
      Gate.Leave;
    end;
  end;
end;

end.
